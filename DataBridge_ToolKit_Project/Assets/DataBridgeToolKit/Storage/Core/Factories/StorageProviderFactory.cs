using DataBridgeToolKit.Storage.Core.Enums;
using DataBridgeToolKit.Storage.Core.Exceptions;
using DataBridgeToolKit.Storage.Core.Interfaces;
using DataBridgeToolKit.Storage.Implementations;
using DataBridgeToolKit.Storage.Options;
using System;



namespace DataBridgeToolKit.Storage.Core.Factories
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public IStorageProvider CreateProvider(IStorageProviderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            return options.ProviderType switch
            {
                StorageProviderType.Local => CreateLocalStorageProvider((LocalStorageProviderOptions)options),
                StorageProviderType.Network => CreateNetworkStorageProvider((NetworkStorageProviderOptions)options),
                StorageProviderType.Cloud => CreateCloudStorageProvider((CloudStorageProviderOptions)options),
                _ => throw new StorageConfigurationException($"Unsupported storage provider type: {options.ProviderType}")
            };
        }

        private IStorageProvider CreateLocalStorageProvider(LocalStorageProviderOptions options)
        {
            return new LocalStorageProvider(options);
        }

        private IStorageProvider CreateNetworkStorageProvider(NetworkStorageProviderOptions options)
        {
            throw new NotImplementedException("Network storage provider is not implemented yet.");
        }

        private IStorageProvider CreateCloudStorageProvider(CloudStorageProviderOptions options)
        {
            throw new NotImplementedException("Cloud storage provider is not implemented yet.");
        }
    }
}

