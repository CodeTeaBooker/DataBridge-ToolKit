using DevToolkit.Storage.Core.Enums;
using DevToolkit.Storage.Core.Exceptions;
using DevToolkit.Storage.Core.Interfaces;
using System;

namespace DevToolkit.Storage.Options
{
    public class LocalStorageProviderOptions : IStorageProviderOptions
    {
        public StorageProviderType ProviderType => StorageProviderType.Local;

        public string BasePath { get; set; }
        public bool CreateDirectoryIfNotExist { get; set; } = true;
        public bool CleanupEmptyDirectories { get; set; } = true;
        public int MaxCleanupDepth { get; set; } = 5;
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

        public int BufferSize { get; set; } = 81920;
        public bool UseWriteThrough { get; set; } = false;

        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan LockCleanupInterval { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan LockInactiveTimeout { get; set; } = TimeSpan.FromMinutes(30);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BasePath))
                throw new StorageConfigurationException("BasePath cannot be empty");

            if (BufferSize <= 0)
                throw new StorageConfigurationException("BufferSize must be greater than 0");

            if (MaxFileSize <= 0)
                throw new StorageConfigurationException("MaxFileSize must be greater than 0");

            if (LockTimeout <= TimeSpan.Zero)
                throw new StorageConfigurationException("LockTimeout must be greater than 0");

            if (LockCleanupInterval <= TimeSpan.Zero)
                throw new StorageConfigurationException("LockCleanupInterval must be greater than 0");

            if (LockInactiveTimeout <= TimeSpan.Zero)
                throw new StorageConfigurationException("LockInactiveTimeout must be greater than 0");

            if (MaxCleanupDepth < 0)
                throw new StorageConfigurationException("MaxCleanupDepth cannot be negative");
        }

        public IStorageProviderOptions Clone() => new LocalStorageProviderOptions
        {
            BasePath = this.BasePath,
            BufferSize = this.BufferSize,
            MaxFileSize = this.MaxFileSize,
            LockTimeout = this.LockTimeout,
            LockCleanupInterval = this.LockCleanupInterval,
            LockInactiveTimeout = this.LockInactiveTimeout,
            UseWriteThrough = this.UseWriteThrough,
            CreateDirectoryIfNotExist = this.CreateDirectoryIfNotExist,
            CleanupEmptyDirectories = this.CleanupEmptyDirectories,
            MaxCleanupDepth = this.MaxCleanupDepth
        };
    }
}