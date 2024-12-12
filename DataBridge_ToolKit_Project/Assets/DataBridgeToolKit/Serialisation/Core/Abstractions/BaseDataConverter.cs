using DevToolkit.Serialization.Core.Exceptions;
using DevToolkit.Serialization.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Serialization.Core.Abstractions
{
    public abstract class BaseDataConverter<TData, TOptions> : IDataConverter<TData>
        where TOptions : class, ISerializationOptions
    {
        public abstract string ContentType { get; }
        public abstract string FileExtension { get; }

        protected readonly TOptions DefaultOptions;
        protected readonly string TypeFullName;

        protected BaseDataConverter(TOptions options)
        {
            DefaultOptions = options?.Clone() as TOptions
                ?? throw new ArgumentNullException(nameof(options));
            TypeFullName = typeof(TData).FullName;
        }

        public byte[] ToBytes(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            return SerializeToBytes(data);
        }

        public TData FromBytes(byte[] data)
        {
            ValidateInputData(data);
            return DeserializeFromBytes(data);
        }

        public async Task<byte[]> ToBytesAsync(TData data, CancellationToken token)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            token.ThrowIfCancellationRequested();
            return await SerializeToBytesAsync(data, token).ConfigureAwait(false);
        }

        public async Task<TData> FromBytesAsync(byte[] data, CancellationToken token)
        {
            ValidateInputData(data);
            token.ThrowIfCancellationRequested();
            return await DeserializeFromBytesAsync(data, token).ConfigureAwait(false);
        }

        protected abstract byte[] SerializeToBytes(TData data);
        protected abstract TData DeserializeFromBytes(byte[] data);
        protected abstract Task<byte[]> SerializeToBytesAsync(TData data, CancellationToken token);
        protected abstract Task<TData> DeserializeFromBytesAsync(byte[] data, CancellationToken token);

        protected virtual void ValidateInputData(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Input data cannot be null or empty.");
        }

        protected SerializationException CreateSerializationException(Exception ex, string operation)
        {
            return new SerializationException(
                $"Failed to {operation} object of type {TypeFullName}. Error: {ex.Message}",
                ex);
        }

        protected DeserializationException CreateDeserializationException(Exception ex, int dataLength)
        {
            return new DeserializationException(
                $"Failed to deserialize data to type {TypeFullName}. Data length: {dataLength} bytes. Error: {ex.Message}",
                ex);
        }

        protected DeserializationException CreateDeserializationException(string message)
        {
            return new DeserializationException(message);
        }
    }
}