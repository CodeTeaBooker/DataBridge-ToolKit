using DataBridgeToolKit.Storage.Core.Factories;
using DataBridgeToolKit.Storage.Core.Interfaces;
using DataBridgeToolKit.Storage.Options;
using System;

namespace DataBridgeToolKit.Storage.Implementations
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly LocalStorageProviderOptions _options;
        private bool _disposed = false;

        public LocalStorageProvider(LocalStorageProviderOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _fileSystem = FileSystemFactory.GetOrCreateFileSystem(options);

            if (_options.CreateDirectoryIfNotExist)
            {
                _fileSystem.CreateDirectory(_options.BasePath);
            }
        }

        public IStorageReader CreateReader()
        {
            ThrowIfDisposed();
            return new LocalStorageReader(_fileSystem, _options.BasePath, _options.CreateDirectoryIfNotExist,
                _options.CleanupEmptyDirectories, _options.MaxCleanupDepth, _options.MaxFileSize);
        }

        public IStorageWriter CreateWriter()
        {
            ThrowIfDisposed();
            return new LocalStorageWriter(_fileSystem, _options.BasePath, _options.CreateDirectoryIfNotExist,
                _options.CleanupEmptyDirectories, _options.MaxCleanupDepth, _options.MaxFileSize);
        }


        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LocalStorageProvider));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    FileSystemFactory.ReleaseFileSystem(_options.BasePath);
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}


