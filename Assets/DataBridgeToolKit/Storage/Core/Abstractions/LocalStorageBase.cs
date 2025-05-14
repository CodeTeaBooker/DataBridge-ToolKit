using DataBridgeToolKit.Storage.Core.Interfaces;
using System;
using System.IO;
using System.Security;



namespace DataBridgeToolKit.Storage.Core.Abstractions
{
    public abstract class LocalStorageBase
    {
        protected readonly IFileSystem FileSystem;
        protected readonly string BasePath;
        protected readonly bool CreateDirectoryIfNotExist;
        protected readonly bool CleanupEmptyDirectories;
        protected readonly int MaxCleanupDepth;
        protected readonly long MaxFileSize;

        protected LocalStorageBase(IFileSystem fileSystem, string basePath, bool createDirectoryIfNotExist,
        bool cleanupEmptyDirectories, int maxCleanupDepth, long maxFileSize)
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentException("Base path cannot be empty", nameof(basePath));
            if (maxFileSize <= 0)
                throw new ArgumentException("MaxFileSize must be greater than 0", nameof(maxFileSize));

            BasePath = basePath;
            CreateDirectoryIfNotExist = createDirectoryIfNotExist;
            CleanupEmptyDirectories = cleanupEmptyDirectories;
            MaxCleanupDepth = maxCleanupDepth;
            MaxFileSize = maxFileSize;
        }

        protected string BuildStoragePath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Key contains invalid characters", nameof(key));

            return Path.Combine(BasePath, key);
        }

        protected void ValidatePath(string fullPath)
        {
            var normalizedPath = Path.GetFullPath(fullPath);
            var normalizedBasePath = Path.GetFullPath(BasePath);

            if (!normalizedPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
                throw new SecurityException("Access to path outside of base directory is not allowed.");
        }

        protected void ThrowIfFileSystemNull()
        {
            if (FileSystem == null)
                throw new ObjectDisposedException(nameof(LocalStorageBase));
        }
    }

}
