using System;
using System.Threading;
using System.Threading.Tasks;


namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface IFileSystem : IDisposable
    {
        Task<byte[]> ReadFileAsync(string fullPath, int bufferSize, CancellationToken token);
        Task<byte[]> ReadFileAsync(string basePath, string fileName, string fileExtension, int bufferSize, CancellationToken token);
        Task WriteFileAsync(string fullPath, byte[] data, int bufferSize, CancellationToken token);
        Task WriteFileAsync(string basePath, string fileName, string fileExtension, byte[] data, int bufferSize, CancellationToken token);

        string GetFileName(string fullPath);
        bool FileExists(string fullPath);
        void DeleteFile(string fullPath);
        string GetDirectoryName(string fullPath);

        bool DirectoryExists(string directoryPath);
        void CreateDirectory(string directoryPath);
        void DeleteDirectory(string directoryPath, bool recursive);
        bool IsDirectoryEmpty(string directoryPath);
    }
}

