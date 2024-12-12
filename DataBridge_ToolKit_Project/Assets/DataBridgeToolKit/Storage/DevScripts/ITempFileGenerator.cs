namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface ITempFileGenerator
    {
        string GenerateSecureTempPath(string originalPath);
    }
}
