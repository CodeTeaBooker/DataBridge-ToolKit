using DevToolkit.Serialization.Core.Enums;

namespace DevToolkit.Serialization.Core.Interfaces
{
    public interface IDataConverterFactory
    {
        IDataConverter<TData> CreateConverter<TData>(ISerializationOptions options);
    }
}


