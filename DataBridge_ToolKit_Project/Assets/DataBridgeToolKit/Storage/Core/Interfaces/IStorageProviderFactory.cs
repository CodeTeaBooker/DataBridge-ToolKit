namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface IStorageProviderFactory
    {
        IStorageProvider CreateProvider(IStorageProviderOptions options);
    }
}