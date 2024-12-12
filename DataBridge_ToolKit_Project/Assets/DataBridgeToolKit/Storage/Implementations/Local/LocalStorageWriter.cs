using DevToolkit.Storage.Core.Abstractions;
using DevToolkit.Storage.Core.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Storage.Implementations
{
    public class LocalStorageWriter : LocalStorageBase, IStorageWriter
    {

        public LocalStorageWriter(IFileSystem fileSystem, string basePath, bool createDirectoryIfNotExist,
         bool cleanupEmptyDirectories, int maxCleanupDepth, long maxFileSize)
         : base(fileSystem, basePath, createDirectoryIfNotExist, cleanupEmptyDirectories, maxCleanupDepth, maxFileSize)
        {
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfFileSystemNull();

            var fullPath = BuildStoragePath(key);
            ValidatePath(fullPath);

            FileSystem.DeleteFile(fullPath);

            if (CleanupEmptyDirectories)
            {
                var directory = Path.GetDirectoryName(fullPath);
                CleanupEmptyParentDirectories(directory);
            }
            return Task.CompletedTask;
        }

        public async Task WriteAsync(string key, byte[] data, CancellationToken cancellationToken = default)
        {
            ThrowIfFileSystemNull();

            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length > MaxFileSize)
                throw new ArgumentException($"Data size exceeds maximum allowed size of {MaxFileSize} bytes");

            var fullPath = BuildStoragePath(key);
            ValidatePath(fullPath);

            var directory = Path.GetDirectoryName(fullPath);
            if (CreateDirectoryIfNotExist && !string.IsNullOrEmpty(directory))
            {
                FileSystem.CreateDirectory(directory);
            }

            await FileSystem.WriteFileAsync(
                fullPath,
                data,
                0,
                cancellationToken);
        }

        private void CleanupEmptyParentDirectories(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return;

            var current = directory;
            var baseDir = BasePath;
            var depth = 0;


            while (!string.IsNullOrEmpty(current) &&
                   current.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) &&
                   depth < MaxCleanupDepth)
            {

                if (FileSystem.IsDirectoryEmpty(current))
                {
                    FileSystem.DeleteDirectory(current, false);
                    current = Path.GetDirectoryName(current);
                    depth++;
                }
                else
                {
                    break;
                }
            }
        }
    }

}
