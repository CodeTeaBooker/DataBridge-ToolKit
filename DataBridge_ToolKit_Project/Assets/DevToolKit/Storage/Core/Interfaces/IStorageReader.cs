using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Storage.Core.Interfaces
{
    public interface IStorageReader
    {
        Task<byte[]> ReadAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
