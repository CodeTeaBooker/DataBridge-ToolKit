using DevToolkit.Storage.Core.Enums;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IStorageProviderOptions
    {
        StorageProviderType ProviderType { get; }
        void Validate();
        IStorageProviderOptions Clone();
    }
}