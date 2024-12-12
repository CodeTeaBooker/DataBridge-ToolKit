using DataBridgeToolKit.Storage.Utils;

namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface IPathResolver
    {
        StoragePath ResolvePath(string key, string extension);
    }
}
