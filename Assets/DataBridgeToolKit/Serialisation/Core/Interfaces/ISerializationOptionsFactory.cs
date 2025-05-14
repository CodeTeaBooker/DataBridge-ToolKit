using DataBridgeToolKit.Serialization.Core.Enums;


namespace DataBridgeToolKit.Serialization.Core.Interfaces
{
    public interface ISerializationOptionsFactory
    {
        ISerializationOptions CreateOptions(SerializationFormat format);
    }
}

