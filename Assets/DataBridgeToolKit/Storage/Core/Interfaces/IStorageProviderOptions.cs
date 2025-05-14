using DataBridgeToolKit.Storage.Core.Enums;

namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface IStorageProviderOptions
    {
        StorageProviderType ProviderType { get; }
        void Validate();
        IStorageProviderOptions Clone();
    }
}