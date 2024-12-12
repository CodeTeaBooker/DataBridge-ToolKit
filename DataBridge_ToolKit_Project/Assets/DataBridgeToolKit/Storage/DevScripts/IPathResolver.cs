using DevToolkit.Storage.Utils;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IPathResolver
    {
        StoragePath ResolvePath(string key, string extension);
    }
}
