using DataBridgeToolKit.Storage.Core.Enums;
using DataBridgeToolKit.Storage.Core.Interfaces;
using DataBridgeToolKit.Storage.Options;
using System;
using System.IO;

namespace DataBridgeToolKit.Storage.Core.Factories
{
    public class StorageProviderOptionsFactory : IStorageProviderOptionsFactory
    {
        private readonly LocalStorageProviderOptions _localOptions;
        private readonly NetworkStorageProviderOptions _networkOptions;
        private readonly CloudStorageProviderOptions _cloudOptions;


        public StorageProviderOptionsFactory(
                    Action<LocalStorageProviderOptions> configureLocal = null,
                    Action<NetworkStorageProviderOptions> configureNetwork = null,
                    Action<CloudStorageProviderOptions> configureCloud = null)
        {
           
            _localOptions = CreateDefaultLocalOptions();
            _networkOptions = CreateDefaultNetworkOptions();
            _cloudOptions = CreateDefaultCloudOptions();

           
            configureLocal?.Invoke(_localOptions);
            configureNetwork?.Invoke(_networkOptions);
            configureCloud?.Invoke(_cloudOptions);

          
            _localOptions.Validate();
            _networkOptions.Validate();
            _cloudOptions.Validate();
        }

        public IStorageProviderOptions CreateOptions(StorageProviderType providerType)
        {
            return providerType switch
            {
                StorageProviderType.Local => _localOptions.Clone(),
                StorageProviderType.Network => _networkOptions.Clone(),
                StorageProviderType.Cloud => _cloudOptions.Clone(),
                _ => throw new System.ArgumentException($"Unsupported provider type: {providerType}")
            };
        }

        private static LocalStorageProviderOptions CreateDefaultLocalOptions()
        {
            return new LocalStorageProviderOptions
            {
                BasePath = Path.Combine(UnityEngine.Application.persistentDataPath, "Storage"),
                BufferSize = GetOptimalBufferSize(),
                MaxFileSize = 100 * 1024 * 1024, // 100MB
                LockTimeout = TimeSpan.FromSeconds(30),
                LockCleanupInterval = TimeSpan.FromMinutes(10),
                LockInactiveTimeout = TimeSpan.FromMinutes(30),
                UseWriteThrough = false,
                CreateDirectoryIfNotExist = true,
                CleanupEmptyDirectories = true,
                MaxCleanupDepth = 5
            };
        }

        private static NetworkStorageProviderOptions CreateDefaultNetworkOptions()
        {
            return new NetworkStorageProviderOptions
            {
                ServerUrl = "https://example.serverUrl.com/api/",
                Username = "username",
                Password = "password",
                ConnectionTimeout = TimeSpan.FromSeconds(30),
                OperationTimeout = TimeSpan.FromMinutes(5),
                MaxRetries = 3
            };
        }

        private static CloudStorageProviderOptions CreateDefaultCloudOptions()
        {
            return new CloudStorageProviderOptions
            {
                CloudProvider = "Azure",
                AccessKeyId = "your-access-key-id",
                SecretAccessKey = "your-secret-access-key",
                Region = "europe-west-1",
                BucketName = "example-bucket",
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        private static int GetOptimalBufferSize()
        {
#if UNITY_EDITOR
            return 81920; // 80KB
#elif UNITY_ANDROID
    // Android 
    return 32 * 1024; // 32KB
#elif UNITY_IOS
    // iOS 
    return 64 * 1024; // 64KB
#else
    // default
    return 81920; // 80KB
#endif
        }

    }
}

