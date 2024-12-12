using DataBridgeToolKit.Storage.Core.Enums;

namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface IStorageProviderOptionsFactory
    {
        IStorageProviderOptions CreateOptions(StorageProviderType providerType);
    }
}


