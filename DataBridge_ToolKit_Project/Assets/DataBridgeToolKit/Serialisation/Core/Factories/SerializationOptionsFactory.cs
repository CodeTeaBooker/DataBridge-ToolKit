using DataBridgeToolKit.Serialization.Core.Enums;
using DataBridgeToolKit.Serialization.Core.Interfaces;
using DataBridgeToolKit.Serialization.Implementations.Options;



namespace DataBridgeToolKit.Serialization.Core.Factories
{
    public class SerializationOptionsFactory : ISerializationOptionsFactory
    {
        private readonly JsonSerializationOptions _jsonOptions;
        private readonly XmlSerializationOptions _xmlOptions;
        private readonly MsgPackSerializationOptions _msgPackOptions;

        public SerializationOptionsFactory(
            JsonSerializationOptions jsonOptions = null,
            XmlSerializationOptions xmlOptions = null,
            MsgPackSerializationOptions msgPackOptions = null)
        {
            _jsonOptions = jsonOptions ?? new JsonSerializationOptions();
            _xmlOptions = xmlOptions ?? new XmlSerializationOptions();
            _msgPackOptions = msgPackOptions ?? new MsgPackSerializationOptions();
        }

        public ISerializationOptions CreateOptions(SerializationFormat format)
        {
            return format switch
            {
                SerializationFormat.Json => _jsonOptions.Clone(),
                SerializationFormat.Xml => _xmlOptions.Clone(),
                SerializationFormat.MsgPack => _msgPackOptions.Clone(),
                _ => throw new System.ArgumentException($"Unsupported format: {format}")
            };
        }
    }

}
