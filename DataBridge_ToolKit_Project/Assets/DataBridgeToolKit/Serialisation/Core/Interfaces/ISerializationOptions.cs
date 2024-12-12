using DataBridgeToolKit.Serialization.Core.Enums;

namespace DataBridgeToolKit.Serialization.Core.Interfaces
{
    public interface ISerializationOptions
    {
        SerializationFormat Format { get; }


        ISerializationOptions Clone();
    }
}

