using System;
using System.Threading;
using System.Threading.Tasks;


namespace DevToolkit.Storage.Core.Interfaces
{
    public interface ILockManager : IDisposable
    {
        Task<IDisposable> AcquireLockAsync(string key, TimeSpan timeout, CancellationToken token);
    }
}

