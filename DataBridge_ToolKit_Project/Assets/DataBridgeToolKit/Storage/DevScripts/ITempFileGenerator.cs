namespace DevToolkit.Storage.Core.Interfaces
{
    public interface ITempFileGenerator
    {
        string GenerateSecureTempPath(string originalPath);
    }
}
