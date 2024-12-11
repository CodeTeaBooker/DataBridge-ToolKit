using DevToolkit.Storage.Utils;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IPathValidator
    {
        ValidationResult ValidatePath(string path);
        ValidationResult ValidateFileName(string fileName);
        ValidationResult ValidateExtension(string extension);
    }
}
