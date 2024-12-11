using DevToolkit.Storage.Core.Enums;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IStorageProviderOptionsFactory
    {
        IStorageProviderOptions CreateOptions(StorageProviderType providerType);
    }
}


