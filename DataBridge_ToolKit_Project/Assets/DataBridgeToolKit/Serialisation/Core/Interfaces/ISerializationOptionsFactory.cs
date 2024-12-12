using DevToolkit.Serialization.Core.Enums;


namespace DevToolkit.Serialization.Core.Interfaces
{
    public interface ISerializationOptionsFactory
    {
        ISerializationOptions CreateOptions(SerializationFormat format);
    }
}

