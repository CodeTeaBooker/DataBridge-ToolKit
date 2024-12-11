using System;
using System.Collections.Generic;

namespace DevToolkit.Storage.Utils
{
    public sealed class StoragePath : IEquatable<StoragePath>, IDisposable
    {
        private ValidationResult _validationResult;
        private bool _isDisposed;

        public string OriginalKey { get; }

        public string NormalizedKey { get; }

        public string FullPath { get; }

        public bool IsValid
        {
            get
            {
                ThrowIfDisposed();
                return _validationResult == null || _validationResult.IsValid;
            }
        }

        public IReadOnlyCollection<string> ValidationErrors
        {
            get
            {
                ThrowIfDisposed();
                return _validationResult?.Errors ?? Array.Empty<string>();
            }
        }

        public StoragePath(
            string originalKey, 
            string normalizedKey, 
            string fullPath,
            int maxErrors = 50)
        {
            OriginalKey = originalKey ?? throw new ArgumentNullException(nameof(originalKey));
            NormalizedKey = normalizedKey;
            FullPath = fullPath;
            _validationResult = new ValidationResult(maxErrors);
        }

        public StoragePath(
            string originalKey, 
            string normalizedKey, 
            string fullPath, 
            IEnumerable<string> validationErrors,
            int maxErrors = 50)
            : this(originalKey, normalizedKey, fullPath, maxErrors)
        {
            if (validationErrors == null)
                throw new ArgumentNullException(nameof(validationErrors));

            _validationResult.AddErrors(validationErrors);
        }

        public void AddValidationError(string error)
        {
            ThrowIfDisposed();
            _validationResult.AddError(error);
        }

        public void AddValidationErrors(IEnumerable<string> errors)
        {
            ThrowIfDisposed();
            _validationResult.AddErrors(errors);
        }

        public ValidationResult ToValidationResult()
        {
            ThrowIfDisposed();
            return _validationResult ?? ValidationResult.Success();
        }

        public static StoragePath Invalid(string originalKey, string error)
        {
            return Invalid(originalKey, new[] { error });
        }

        public static StoragePath Invalid(string originalKey, IEnumerable<string> errors)
        {
            return new StoragePath(originalKey, null, null, errors);
        }

        public static StoragePath Invalid(string originalKey, string normalizedKey, string fullPath, IEnumerable<string> errors)
        {
            return new StoragePath(originalKey, normalizedKey, fullPath, errors);
        }

        public StoragePath Copy()
        {
            ThrowIfDisposed();
            return new StoragePath(OriginalKey, NormalizedKey, FullPath, ValidationErrors);
        }

        public override string ToString()
        {
            ThrowIfDisposed();
            var errors = IsValid ? string.Empty : $", Errors=[{string.Join(", ", ValidationErrors)}]";
            return $"StoragePath[Original='{OriginalKey}', Normalized='{NormalizedKey}', Full='{FullPath}', Valid={IsValid}{errors}]";
        }

        public bool Equals(StoragePath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(OriginalKey, other.OriginalKey) &&
                   string.Equals(NormalizedKey, other.NormalizedKey) &&
                   string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj)
        {
            return obj is StoragePath other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OriginalKey, NormalizedKey, FullPath);
        }

        public static bool operator ==(StoragePath left, StoragePath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StoragePath left, StoragePath right)
        {
            return !(left == right);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _validationResult?.Dispose();
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(StoragePath));
        }
    }
}
