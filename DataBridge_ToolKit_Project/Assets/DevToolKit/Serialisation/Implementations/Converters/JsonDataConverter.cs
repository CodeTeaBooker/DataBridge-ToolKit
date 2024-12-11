using DevToolkit.Serialization.Core.Abstractions;
using DevToolkit.Serialization.Core.Exceptions;
using DevToolkit.Serialization.Implementations.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevToolkit.Serialization.Implementations.Converters
{
    public class JsonDataConverter<TData> : BaseDataConverter<TData, JsonSerializationOptions>
    {
        public override string ContentType => "application/json";
        public override string FileExtension => ".json";

        private readonly JsonSerializer _serializer;

        public JsonDataConverter(JsonSerializationOptions options)
            : base(options)
        {

            _serializer = JsonSerializer.Create(DefaultOptions.GetSettings());
        }

        protected override byte[] SerializeToBytes(TData data)
        {
            try
            {
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);

                _serializer.Serialize(jsonWriter, data);
                return Encoding.UTF8.GetBytes(stringWriter.ToString());
            }
            catch (JsonException ex)
            {
                throw new SerializationException(
                    $"Failed to serialize object of type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new SerializationException(
                    $"Invalid operation during serialization of object of type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
        }

        protected override TData DeserializeFromBytes(byte[] data)
        {
            try
            {
                var json = Encoding.UTF8.GetString(data);
                using var stringReader = new StringReader(json);
                using var jsonReader = new JsonTextReader(stringReader);

                return _serializer.Deserialize<TData>(jsonReader);
            }
            catch (JsonException ex)
            {
                throw new DeserializationException(
                    $"Failed to deserialize data to type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new DeserializationException(
                    $"Invalid operation during deserialization to type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
        }

        protected override async Task<byte[]> SerializeToBytesAsync(TData data, CancellationToken token)
        {
            try
            {
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);

                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    _serializer.Serialize(jsonWriter, data);
                }, token);

                await jsonWriter.FlushAsync(token).ConfigureAwait(false);
                return Encoding.UTF8.GetBytes(stringWriter.ToString());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new SerializationException(
                    $"Failed to serialize object of type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new SerializationException(
                    $"Invalid operation during serialization of object of type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
        }

        protected override async Task<TData> DeserializeFromBytesAsync(byte[] data, CancellationToken token)
        {
            try
            {
                var json = Encoding.UTF8.GetString(data);
                using var stringReader = new StringReader(json);
                using var jsonReader = new JsonTextReader(stringReader);

                if (await jsonReader.ReadAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    return await Task.Run(() => _serializer.Deserialize<TData>(jsonReader), token);
                }
                else
                {
                    throw new DeserializationException("No content to deserialize.");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new DeserializationException(
                    $"Failed to deserialize data to type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new DeserializationException(
                    $"Invalid operation during deserialization to type {typeof(TData).FullName}. Error: {ex.Message}", ex);
            }
        }
    }
}