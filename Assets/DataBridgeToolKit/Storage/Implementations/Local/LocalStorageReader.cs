using DataBridgeToolKit.Storage.Core.Abstractions;
using DataBridgeToolKit.Storage.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace DataBridgeToolKit.Storage.Implementations
{
    public class LocalStorageReader : LocalStorageBase, IStorageReader
    {
        public LocalStorageReader(IFileSystem fileSystem, string basePath, bool createDirectoryIfNotExist,
          bool cleanupEmptyDirectories, int maxCleanupDepth, long maxFileSize)
          : base(fileSystem, basePath, createDirectoryIfNotExist, cleanupEmptyDirectories, maxCleanupDepth, maxFileSize)
        {
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfFileSystemNull();

            var fullPath = BuildStoragePath(key);
            ValidatePath(fullPath);

            return Task.FromResult(FileSystem.FileExists(fullPath));
        }

        public async Task<byte[]> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfFileSystemNull();

            var fullPath = BuildStoragePath(key);
            ValidatePath(fullPath);

            return await FileSystem.ReadFileAsync(
                fullPath,
                0,
                cancellationToken);
        }
    }
}

