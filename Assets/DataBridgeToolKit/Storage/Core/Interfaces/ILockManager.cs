using System;
using System.Threading;
using System.Threading.Tasks;


namespace DataBridgeToolKit.Storage.Core.Interfaces
{
    public interface ILockManager : IDisposable
    {
        Task<IDisposable> AcquireLockAsync(string key, TimeSpan timeout, CancellationToken token);
    }
}

