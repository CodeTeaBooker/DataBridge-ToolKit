using DevToolkit.Serialization.Core.Abstractions;
using DevToolkit.Serialization.Implementations.Options;
using MessagePack;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Serialization.Implementations.Converters
{
    public class MsgPackDataConverter<TData> : BaseDataConverter<TData, MsgPackSerializationOptions>
    {
        private const string ContentTypeValue = "application/x-msgpack";
        private const string FileExtensionValue = ".msgpack";

        public override string ContentType => ContentTypeValue;
        public override string FileExtension => FileExtensionValue;

        private sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
        {
            private byte[] _rentedBuffer;
            private int _written;

            public PooledBufferWriter(int initialCapacity)
            {
                _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
                _written = 0;
            }

            public void Advance(int count)
            {
                _written += count;
            }

            public Memory<byte> GetMemory(int sizeHint = 0)
            {
                EnsureCapacity(sizeHint);
                return _rentedBuffer.AsMemory(_written);
            }

            public Span<byte> GetSpan(int sizeHint = 0)
            {
                EnsureCapacity(sizeHint);
                return _rentedBuffer.AsSpan(_written);
            }

            private void EnsureCapacity(int sizeHint)
            {
                if (sizeHint == 0)
                    sizeHint = 1;

                if (_written + sizeHint > _rentedBuffer.Length)
                {
                    var newSize = Math.Max(_rentedBuffer.Length * 2, _written + sizeHint);
                    var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
                    Buffer.BlockCopy(_rentedBuffer, 0, newBuffer, 0, _written);

                    var oldBuffer = _rentedBuffer;
                    _rentedBuffer = newBuffer;
                    ArrayPool<byte>.Shared.Return(oldBuffer);
                }
            }

            public byte[] ToArray()
            {
                byte[] result = new byte[_written];
                Buffer.BlockCopy(_rentedBuffer, 0, result, 0, _written);
                return result;
            }

            public void Dispose()
            {
                if (_rentedBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(_rentedBuffer);
                    _rentedBuffer = null;
                }
            }
        }

        public MsgPackDataConverter(MsgPackSerializationOptions options)
            : base(options)
        {
        }

        protected override void ValidateInputData(byte[] data)
        {
            base.ValidateInputData(data);
            if (data.Length > DefaultOptions.MaxDataSize)
            {
                throw CreateDeserializationException(
                    $"Input data size ({data.Length} bytes) exceeds maximum allowed size ({DefaultOptions.MaxDataSize} bytes).");
            }
        }

        protected override byte[] SerializeToBytes(TData data)
        {
            try
            {
                using var bufferWriter = new PooledBufferWriter(DefaultOptions.InitialBufferSize);
                MessagePackSerializer.Serialize(bufferWriter, data, DefaultOptions.GetSettings());
                return bufferWriter.ToArray();
            }
            catch (Exception ex) when (ex is MessagePackSerializationException || ex is InvalidOperationException)
            {
                throw CreateSerializationException(ex, "serialize");
            }
        }

        protected override async Task<byte[]> SerializeToBytesAsync(TData data, CancellationToken token)
        {
            try
            {
                using var stream = new MemoryStream(DefaultOptions.InitialBufferSize);
                await MessagePackSerializer.SerializeAsync(stream, data, DefaultOptions.GetSettings(), token)
                    .ConfigureAwait(false);
                return stream.ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is MessagePackSerializationException || ex is InvalidOperationException)
            {
                throw CreateSerializationException(ex, "serialize async");
            }
        }

        protected override TData DeserializeFromBytes(byte[] data)
        {
            try
            {
                ReadOnlyMemory<byte> memory = data;
                return MessagePackSerializer.Deserialize<TData>(memory, DefaultOptions.GetSettings());
            }
            catch (Exception ex) when (ex is MessagePackSerializationException || ex is InvalidOperationException)
            {
                throw CreateDeserializationException(ex, data.Length);
            }
        }

        protected override async Task<TData> DeserializeFromBytesAsync(byte[] data, CancellationToken token)
        {
            try
            {
                if (data.Length <= DefaultOptions.SmallDataThreshold)
                {
                    return DeserializeFromBytes(data);
                }

                using var stream = new MemoryStream(data, writable: false);
                return await MessagePackSerializer.DeserializeAsync<TData>(
                    stream,
                    DefaultOptions.GetSettings(),
                    token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is MessagePackSerializationException || ex is InvalidOperationException)
            {
                throw CreateDeserializationException(ex, data.Length);
            }
        }
    }
}