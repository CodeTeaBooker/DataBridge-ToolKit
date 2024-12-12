using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IStorageWriter
    {  
        Task WriteAsync(string key, byte[] data, CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    }

}
