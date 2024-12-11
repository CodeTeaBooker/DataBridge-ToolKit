using DevToolkit.Storage.Core.Enums;
using DevToolkit.Storage.Core.Exceptions;
using DevToolkit.Storage.Core.Interfaces;
using DevToolkit.Storage.Implementations;
using DevToolkit.Storage.Options;
using System;



namespace DevToolkit.Storage.Core.Factories
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

