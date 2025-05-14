using DataBridgeToolKit.Serialization.Core.Enums;
using DataBridgeToolKit.Serialization.Core.Interfaces;
using System.Xml;

namespace DataBridgeToolKit.Serialization.Implementations.Options
{
    public sealed class XmlSerializationOptions : ISerializationOptions
    {
        public SerializationFormat Format => SerializationFormat.Xml;

        private readonly XmlWriterSettings _writerSettings;
        private readonly XmlReaderSettings _readerSettings;

        public XmlSerializationOptions(XmlWriterSettings writerSettings = null, XmlReaderSettings readerSettings = null)
        {
            _writerSettings = writerSettings ?? new XmlWriterSettings
            {
                Indent = true,
                Async = true,
                Encoding = new System.Text.UTF8Encoding(false)
            };

            _readerSettings = readerSettings ?? new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
        }

        public XmlWriterSettings GetWriterSettings()
        {
            return _writerSettings.Clone();
        }

        public XmlReaderSettings GetReaderSettings()
        {
            return _readerSettings.Clone();
        }

        public ISerializationOptions Clone()
        {
            var clonedWriterSettings = _writerSettings.Clone();
            var clonedReaderSettings = _readerSettings.Clone();

            return new XmlSerializationOptions(clonedWriterSettings, clonedReaderSettings);
        }
    }
}