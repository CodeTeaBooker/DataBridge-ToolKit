using DevToolkit.Serialization.Core.Abstractions;
using DevToolkit.Serialization.Core.Exceptions;
using DevToolkit.Serialization.Implementations.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DevToolkit.Serialization.Implementations.Converters
{
    public sealed class XmlDataConverter<TData> : BaseDataConverter<TData, XmlSerializationOptions>
    {
        public override string ContentType => "application/xml";
        public override string FileExtension => ".xml";

        private readonly XmlSerializer _serializer;

        public XmlDataConverter(XmlSerializationOptions options)
            : base(options)
        {
            _serializer = new XmlSerializer(typeof(TData));
        }

        protected override byte[] SerializeToBytes(TData data)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var xmlWriter = XmlWriter.Create(memoryStream, DefaultOptions.GetWriterSettings());

                _serializer.Serialize(xmlWriter, data);
                xmlWriter.Flush();

                return memoryStream.ToArray();
            }
            catch (InvalidOperationException ex)
            {
                throw new SerializationException(
                    $"Invalid operation during serialization of type {typeof(TData).FullName}.", ex);
            }
            catch (XmlException ex)
            {
                throw new SerializationException(
                    $"XML error during serialization of type {typeof(TData).FullName}.", ex);
            }
        }

        protected override TData DeserializeFromBytes(byte[] data)
        {
            try
            {
                using var memoryStream = new MemoryStream(data);
                using var xmlReader = XmlReader.Create(memoryStream, DefaultOptions.GetReaderSettings());

                return xmlReader.MoveToContent() != XmlNodeType.None
                    ? (TData)_serializer.Deserialize(xmlReader)
                    : throw new DeserializationException("No content to deserialize.");
            }
            catch (InvalidOperationException ex)
            {
                throw new DeserializationException(
                    $"Invalid operation during deserialization to type {typeof(TData).FullName}.", ex);
            }
            catch (XmlException ex)
            {
                throw new DeserializationException(
                    $"XML parsing error during deserialization to type {typeof(TData).FullName}.", ex);
            }
        }

        protected override async Task<byte[]> SerializeToBytesAsync(TData data, CancellationToken token)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var xmlWriter = XmlWriter.Create(memoryStream, DefaultOptions.GetWriterSettings());

                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    _serializer.Serialize(xmlWriter, data);
                }, token).ConfigureAwait(false);

                await xmlWriter.FlushAsync().ConfigureAwait(false);
                return memoryStream.ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or IOException)
            {
                throw new SerializationException(
                    $"Error during async serialization of type {typeof(TData).FullName}.", ex);
            }
        }

        protected override async Task<TData> DeserializeFromBytesAsync(byte[] data, CancellationToken token)
        {
            try
            {
                using var memoryStream = new MemoryStream(data);
                using var xmlReader = XmlReader.Create(memoryStream, DefaultOptions.GetReaderSettings());

                if (await xmlReader.MoveToContentAsync().ConfigureAwait(false) != XmlNodeType.None)
                {
                    return await Task.Run(() =>
                    {
                        token.ThrowIfCancellationRequested();
                        return (TData)_serializer.Deserialize(xmlReader);
                    }, token).ConfigureAwait(false);
                }

                throw new DeserializationException("No content to deserialize.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or IOException)
            {
                throw new DeserializationException(
                    $"Error during async deserialization to type {typeof(TData).FullName}.", ex);
            }
        }
    }
}