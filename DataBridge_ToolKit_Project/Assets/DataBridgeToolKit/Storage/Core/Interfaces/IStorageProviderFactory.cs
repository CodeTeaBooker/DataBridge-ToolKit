namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IStorageProviderFactory
    {
        IStorageProvider CreateProvider(IStorageProviderOptions options);
    }
}