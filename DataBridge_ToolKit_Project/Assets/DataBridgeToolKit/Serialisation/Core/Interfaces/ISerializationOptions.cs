using DevToolkit.Serialization.Core.Enums;

namespace DevToolkit.Serialization.Core.Interfaces
{
    public interface ISerializationOptions
    {
        SerializationFormat Format { get; }


        ISerializationOptions Clone();
    }
}

