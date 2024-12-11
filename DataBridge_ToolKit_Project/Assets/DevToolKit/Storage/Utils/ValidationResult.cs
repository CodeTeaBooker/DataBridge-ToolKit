using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DevToolkit.Storage.Utils
{
 
    public sealed class ValidationResult : IDisposable
    {
        private const int DefaultMaxErrors = 50;
        private readonly ConcurrentQueue<string> _errors = new ConcurrentQueue<string>();
        private readonly int _maxErrors;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _isDisposed;

        public bool IsValid => _errors.IsEmpty;

        public IReadOnlyCollection<string> Errors
        {
            get
            {
                ThrowIfDisposed();
                _lock.EnterReadLock();
                try
                {
                    return _errors.ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public int MaxErrors => _maxErrors;

        public ValidationResult(int maxErrors = DefaultMaxErrors)
        {
            if (maxErrors <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maxErrors),
                    maxErrors,
                    "Max errors must be greater than zero."
                );

            _maxErrors = maxErrors;
        }

        public ValidationResult(IEnumerable<string> errors, int maxErrors = DefaultMaxErrors)
            : this(maxErrors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            AddErrors(errors);
        }

        public ValidationResult(string error)
            : this(new[] { error }) { }

        
        public void AddError(string error)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));

            _lock.EnterWriteLock();
            try
            {
                // Ensure the queue does not exceed MaxErrors
                while (_errors.Count >= _maxErrors)
                {
                    _errors.TryDequeue(out _);
                }
                _errors.Enqueue(error);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        
        public void AddErrors(IEnumerable<string> errors)
        {
            ThrowIfDisposed();
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            // Pre-filter invalid data outside the lock
            var validErrors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
            if (!validErrors.Any())
                return;

            _lock.EnterWriteLock();
            try
            {
                foreach (var error in validErrors)
                {
                    // Ensure the queue does not exceed MaxErrors
                    while (_errors.Count >= _maxErrors)
                    {
                        _errors.TryDequeue(out _);
                    }
                    _errors.Enqueue(error);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static ValidationResult Success() => new ValidationResult();

        public static ValidationResult Error(string error) => new ValidationResult(error);

      
        public static ValidationResult Combine(params ValidationResult[] results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            if (!results.Any(r => r != null && !r.IsValid))
                return Success();

            int combinedMaxErrors = results.Where(r => r != null).Max(r => r.MaxErrors);
            var combined = new ValidationResult(combinedMaxErrors);

            IEnumerable<string> GetAllErrors()
            {
                foreach (var result in results.Where(r => r != null && !r.IsValid))
                {
                    foreach (var error in result.Errors)
                    {
                        yield return error;
                    }
                }
            }

            combined.AddErrors(GetAllErrors());
            return combined;
        }

        public override string ToString()
        {
            ThrowIfDisposed();
            _lock.EnterReadLock();
            try
            {
                return string.Join(", ", _errors);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            _lock.EnterWriteLock();
            try
            {
                while (_errors.TryDequeue(out _)) { }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

     
        public IReadOnlyList<string> GetLatestErrors(int count)
        {
            ThrowIfDisposed();
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            _lock.EnterReadLock();
            try
            {
                return _errors.Skip(Math.Max(0, _errors.Count - count))
                    .ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _lock.Dispose();
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ValidationResult));
        }
    }
}

