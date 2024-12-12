using DataBridgeToolKit.Storage.Core.Interfaces;
using DataBridgeToolKit.Storage.Options;
using DataBridgeToolKit.Storage.Implementations;
using System;
using System.Collections.Concurrent;

namespace DataBridgeToolKit.Storage.Core.Factories
{
    public static class FileSystemFactory
    {
        private static readonly ConcurrentDictionary<string, FileSystemWrapper> _fileSystems
            = new ConcurrentDictionary<string, FileSystemWrapper>(StringComparer.OrdinalIgnoreCase);

        private class FileSystemWrapper : IDisposable
        {
            public IFileSystem FileSystem { get; }
            private int _referenceCount;
            private readonly object _lock = new object();

            public FileSystemWrapper(IFileSystem fileSystem)
            {
                FileSystem = fileSystem;
                _referenceCount = 1;
            }

            public void IncrementReference()
            {
                lock (_lock)
                {
                    _referenceCount++;
                }
            }

            public bool DecrementReference()
            {
                lock (_lock)
                {
                    _referenceCount--;
                    return _referenceCount <= 0;
                }
            }

            public void Dispose()
            {
                if (FileSystem is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public static IFileSystem GetOrCreateFileSystem(LocalStorageProviderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "Options cannot be null.");

            bool created = false;
            var wrapper = _fileSystems.GetOrAdd(options.BasePath, _ =>
            {
                created = true;
                return CreateNewFileSystemWrapper(options);
            });

            if (!created)
            {
                wrapper.IncrementReference();
            }

            return wrapper.FileSystem;
        }


        private static FileSystemWrapper CreateNewFileSystemWrapper(LocalStorageProviderOptions options)
        {
            var lockManager = new ConcurrentLockManager(
                options.LockTimeout,
                options.LockCleanupInterval,
                options.LockInactiveTimeout);

            var fileSystem = new SecureFileSystem(
                lockManager,
                options.BufferSize,
                options.LockTimeout,
                options.UseWriteThrough);

            return new FileSystemWrapper(fileSystem);
        }

        public static void ReleaseFileSystem(string basePath)
        {
            if (_fileSystems.TryGetValue(basePath, out var wrapper))
            {
                if (wrapper.DecrementReference())
                {
                    if (_fileSystems.TryRemove(basePath, out var removedWrapper))
                    {
                        removedWrapper.Dispose();
                    }
                }
            }
        }
    }
}