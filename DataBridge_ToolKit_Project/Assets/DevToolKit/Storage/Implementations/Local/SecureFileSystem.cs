using DevToolkit.Storage.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace DevToolkit.Storage.Implementations
{
    public class SecureFileSystem : IFileSystem
    {
        private readonly ILockManager _lockManager;
        private readonly TimeSpan _lockTimeout;
        private readonly int _defaultBufferSize;
        private readonly bool _useWriteThrough;
        private bool _disposed = false;

        public SecureFileSystem(
            ILockManager lockManager,
            int defaultBufferSize = 81920,
            TimeSpan? lockTimeout = null,
            bool useWriteThrough = false)
        {
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            _defaultBufferSize = defaultBufferSize > 0 ? defaultBufferSize : 81920;
            _lockTimeout = lockTimeout ?? TimeSpan.FromSeconds(5);
            _useWriteThrough = useWriteThrough;
        }

        private void LogError(string message, Exception ex = null, [CallerMemberName] string memberName = "")
        {
            if (ex == null)
                UnityEngine.Debug.LogError($"[{memberName}] {message}");
            else
                UnityEngine.Debug.LogError($"[{memberName}] {message}\nException: {ex}");
        }

        #region IDisposable Support

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SecureFileSystem));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                   
                    if (_lockManager is IDisposable disposableLockManager)
                    {
                        disposableLockManager.Dispose();
                    }
                }

                
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Read File

        public Task<byte[]> ReadFileAsync(string basePath, string fileName, string fileExtension, int bufferSize, CancellationToken token)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentException("basePath cannot be null or empty.", nameof(basePath));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName cannot be null or empty.", nameof(fileName));

            string fullPath = Path.Combine(basePath, fileName + fileExtension);
            return ReadFileAsync(fullPath, bufferSize, token);
        }

        public async Task<byte[]> ReadFileAsync(string fullPath, int bufferSize, CancellationToken token)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            bufferSize = ValidateBufferSize(bufferSize);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("The specified file does not exist.", fullPath);

            try
            {
                using (await _lockManager.AcquireLockAsync(fullPath, _lockTimeout, token))
                {
                    if (!File.Exists(fullPath))
                        throw new FileNotFoundException("The specified file does not exist.", fullPath);

                    using var fileStream = new FileStream(
                        fullPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize,
                        FileOptions.Asynchronous | FileOptions.SequentialScan
                    );

                    using var memoryStream = new MemoryStream();
                    var buffer = new byte[bufferSize];
                    int bytesRead;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await memoryStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error reading file at path {fullPath}", ex);
                throw;
            }
        }

        #endregion

        #region Write File

        public Task WriteFileAsync(string basePath, string fileName, string fileExtension, byte[] data, int bufferSize, CancellationToken token)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentException("basePath cannot be null or empty.", nameof(basePath));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName cannot be null or empty.", nameof(fileName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string fullPath = Path.Combine(basePath, fileName + fileExtension);

            return WriteFileAsync(fullPath, data, bufferSize, token);
        }

        public async Task WriteFileAsync(string fullPath, byte[] data, int bufferSize, CancellationToken token)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            bufferSize = ValidateBufferSize(bufferSize);
            string tempFile = CreateTempFilePath(fullPath);

            try
            {
                using (await _lockManager.AcquireLockAsync(fullPath, _lockTimeout, token))
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    CleanupTempFile(tempFile);

                    var fileOptions = FileOptions.Asynchronous;
                    if (_useWriteThrough)
                        fileOptions |= FileOptions.WriteThrough;

                    using (var fileStream = new FileStream(
                        tempFile,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize,
                        fileOptions))
                    {
                        int offset = 0;
                        while (offset < data.Length)
                        {
                            token.ThrowIfCancellationRequested();

                            int bytesRemaining = data.Length - offset;
                            int bytesToWrite = Math.Min(bufferSize, bytesRemaining);

                            await fileStream.WriteAsync(data, offset, bytesToWrite, token).ConfigureAwait(false);
                            offset += bytesToWrite;
                        }

                        await fileStream.FlushAsync(token).ConfigureAwait(false);
                    }
                    ReplaceFile(tempFile, fullPath);
                }
            }
            catch (OperationCanceledException)
            {
                CleanupTempFile(tempFile);
                throw;
            }
            catch (Exception ex)
            {
                CleanupTempFile(tempFile);
                LogError($"Failed to write file at path {fullPath}", ex);
                throw;
            }
        }


        #endregion

        #region File Operations

        public bool FileExists(string fullPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            return File.Exists(fullPath);
        }

        public void DeleteFile(string fullPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            try
            {
                using (_lockManager.AcquireLockAsync(fullPath, _lockTimeout, CancellationToken.None).Result)
                {
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }
            catch (IOException ex)
            {
                LogError($"Error deleting file at path {fullPath}", ex);
                throw new IOException($"Error deleting file at path {fullPath}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError($"Access denied when deleting file at path {fullPath}", ex);
                throw new UnauthorizedAccessException($"Access denied when deleting file at path {fullPath}", ex);
            }
        }

        public string GetDirectoryName(string fullPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            return Path.GetDirectoryName(fullPath);
        }

        public string GetFileName(string fullPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(fullPath));

            return Path.GetFileName(fullPath);
        }

        #endregion

        #region Directory Operations

        public bool DirectoryExists(string directoryPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("directoryPath cannot be null or empty.", nameof(directoryPath));

            return Directory.Exists(directoryPath);
        }

        public void CreateDirectory(string directoryPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("directoryPath cannot be null or empty.", nameof(directoryPath));

            Directory.CreateDirectory(directoryPath);
        }

        public void DeleteDirectory(string directoryPath, bool recursive)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("directoryPath cannot be null or empty.", nameof(directoryPath));

            try
            {
                Directory.Delete(directoryPath, recursive);
            }
            catch (IOException ex)
            {
                LogError($"Error deleting directory at path {directoryPath}", ex);
                throw new IOException($"Error deleting directory at path {directoryPath}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError($"Access denied when deleting directory at path {directoryPath}", ex);
                throw new UnauthorizedAccessException($"Access denied when deleting directory at path {directoryPath}", ex);
            }
        }

        public bool IsDirectoryEmpty(string directoryPath)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("directoryPath cannot be null or empty.", nameof(directoryPath));

            try
            {
                return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError($"Access denied when checking if directory is empty at path {directoryPath}", ex);
                throw new UnauthorizedAccessException($"Access denied when checking if directory is empty at path {directoryPath}", ex);
            }
            catch (IOException ex)
            {
                LogError($"Error accessing directory at path {directoryPath}", ex);
                throw new IOException($"Error accessing directory at path {directoryPath}", ex);
            }
        }

        #endregion

        #region Private Helpers


        private int ValidateBufferSize(int bufferSize)
        {
            return bufferSize <= 0 ? _defaultBufferSize : bufferSize;
        }

        private string CreateTempFilePath(string originalPath)
        {
            return Path.Combine(
                Path.GetDirectoryName(originalPath) ?? string.Empty,
                Path.GetFileName(originalPath) + ".tmp");
        }

        private void ReplaceFile(string sourceFile, string destinationFile)
        {
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }
            File.Move(sourceFile, destinationFile);
        }

        private void CleanupTempFile(string tempFile)
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete temp file '{tempFile}'", ex);
            }
        }

        #endregion

    }
}
