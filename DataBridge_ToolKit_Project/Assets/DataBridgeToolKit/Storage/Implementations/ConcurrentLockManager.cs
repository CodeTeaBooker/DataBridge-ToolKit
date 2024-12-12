using DataBridgeToolKit.Storage.Core.Exceptions;
using DataBridgeToolKit.Storage.Core.Interfaces;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataBridgeToolKit.Storage.Implementations
{
   

    public sealed class ConcurrentLockManager : ILockManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, LockWrapper> _locks;
        private readonly TimeSpan _defaultTimeout;
        private readonly TimeSpan _inactiveTimeout;
        private readonly Timer _cleanupTimer;
        private volatile int _disposed;
        private const int MaxKeyLength = 2048;

        public ConcurrentLockManager(
            TimeSpan? defaultTimeout = null,
            TimeSpan? cleanupInterval = null,
            TimeSpan? inactiveTimeout = null)
        {
            _locks = new ConcurrentDictionary<string, LockWrapper>(StringComparer.Ordinal);
            _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(1);
            var interval = cleanupInterval ?? TimeSpan.FromMinutes(10);
            _inactiveTimeout = inactiveTimeout ?? TimeSpan.FromMinutes(30);

            _cleanupTimer = new Timer(
                CleanupInactiveLocks,
                null,
                interval,
                interval);
        }

        private sealed class LockWrapper : IDisposable
        {
            private readonly AsyncLock _lock;
            private volatile int _refCount;
            private volatile int _disposed;
            private readonly long _createdTimeTicks;
            private long _lastAccessTimeTicks;
            private long _acquisitionCount;
            private long _timeoutCount;

            public LockWrapper()
            {
                _lock = new AsyncLock();
                var now = DateTime.UtcNow.Ticks;
                _createdTimeTicks = now;
                _lastAccessTimeTicks = now;
            }

            public DateTime LastAccessTime => new DateTime(Interlocked.Read(ref _lastAccessTimeTicks));
            public long AcquisitionCount => Interlocked.Read(ref _acquisitionCount);
            public long TimeoutCount => Interlocked.Read(ref _timeoutCount);

            public bool IsInactive(TimeSpan inactiveTimeout)
            {
                return DateTime.UtcNow - LastAccessTime > inactiveTimeout;
            }

            public async Task<IDisposable> AcquireLockAsync(TimeSpan timeout, CancellationToken token, TimeSpan inactiveTimeout)
            {
                if (_disposed == 1)
                    throw new ObjectDisposedException(nameof(LockWrapper));

                Interlocked.Increment(ref _refCount);
                Interlocked.Exchange(ref _lastAccessTimeTicks, DateTime.UtcNow.Ticks);

                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    cts.CancelAfter(timeout);

                    var lockHandle = await _lock.LockAsync(cts.Token).ConfigureAwait(false);
                    Interlocked.Increment(ref _acquisitionCount);
                    return lockHandle;
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Increment(ref _timeoutCount);
                    DecrementRefCount();
                    throw;
                }
                catch
                {
                    DecrementRefCount();
                    throw;
                }
            }

            public int DecrementRefCount()
            {
                return Interlocked.Decrement(ref _refCount);
            }

            public bool CanBeRemoved(TimeSpan inactiveTimeout)
            {
                return _refCount == 0 && IsInactive(inactiveTimeout);
            }

            //public LockStatistics GetStatistics()
            //{
            //    return new LockStatistics(
            //        AcquisitionCount,
            //        TimeoutCount,
            //        DateTime.UtcNow - new DateTime(_createdTimeTicks),
            //        _refCount,
            //        DateTime.UtcNow - LastAccessTime);
            //}

            public void Dispose()
            {
                Interlocked.Exchange(ref _disposed, 1);
            }
        }

        public async Task<IDisposable> AcquireLockAsync(
            string key,
            TimeSpan timeout,
            CancellationToken token = default)
        {
            ValidateState(key);

            var effectiveTimeout = timeout == TimeSpan.Zero ? _defaultTimeout : timeout;
            var wrapper = _locks.GetOrAdd(key, _ => new LockWrapper());

            try
            {
                var lockHandle = await wrapper.AcquireLockAsync(effectiveTimeout, token, _inactiveTimeout).ConfigureAwait(false);
                return new LockReleaser(this, key, wrapper, lockHandle);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                throw new TimeoutException($"Failed to acquire lock for key '{key}' within {effectiveTimeout.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                throw new LockAcquisitionException($"Error acquiring lock for key '{key}'", ex);
            }
        }

        //public LockStatistics GetLockStatistics(string key)
        //{
        //    if (_locks.TryGetValue(key, out var wrapper))
        //    {
        //        return wrapper.GetStatistics();
        //    }
        //    return null;
        //}

        private sealed class LockReleaser : IDisposable
        {
            private readonly ConcurrentLockManager _manager;
            private readonly string _key;
            private readonly LockWrapper _wrapper;
            private readonly IDisposable _lockHandle;
            private volatile int _disposedFlag;

            public LockReleaser(
                ConcurrentLockManager manager,
                string key,
                LockWrapper wrapper,
                IDisposable lockHandle)
            {
                _manager = manager;
                _key = key;
                _wrapper = wrapper;
                _lockHandle = lockHandle;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposedFlag, 1) == 1) return;

                try
                {
                    _lockHandle.Dispose();
                    var refCount = _wrapper.DecrementRefCount();
                    if (refCount == 0 && _wrapper.CanBeRemoved(_manager._inactiveTimeout))
                    {
                        if (_manager._locks.TryRemove(_key, out var removed) && removed == _wrapper)
                        {
                            removed.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error disposing lock for key '{_key}': {ex}");
                    throw;
                }
            }
        }

        private void CleanupInactiveLocks(object state)
        {
            if (_disposed == 1) return;

            try
            {
                foreach (var kvp in _locks)
                {
                    var wrapper = kvp.Value;
                    if (wrapper.CanBeRemoved(_inactiveTimeout))
                    {
                        if (_locks.TryRemove(kvp.Key, out var removed))
                        {
                            removed.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during cleanup: {ex}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateState(string key)
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(nameof(ConcurrentLockManager));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (key.Length > MaxKeyLength)
                throw new ArgumentException($"Key exceeds maximum length of {MaxKeyLength}", nameof(key));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            _cleanupTimer.Dispose();

            foreach (var kvp in _locks)
            {
                if (_locks.TryRemove(kvp.Key, out var wrapper))
                {
                    wrapper.Dispose();
                }
            }
        }

        //public class LockStatistics
        //{
        //    public long AcquisitionCount { get; }
        //    public long TimeoutCount { get; }
        //    public TimeSpan Age { get; }
        //    public int CurrentRefCount { get; }
        //    public TimeSpan IdleTime { get; }

        //    public LockStatistics(
        //        long acquisitionCount,
        //        long timeoutCount,
        //        TimeSpan age,
        //        int refCount,
        //        TimeSpan idleTime)
        //    {
        //        AcquisitionCount = acquisitionCount;
        //        TimeoutCount = timeoutCount;
        //        Age = age;
        //        CurrentRefCount = refCount;
        //        IdleTime = idleTime;
        //    }
        //}
    }
}