using DataBridgeToolKit.Serialization.Core.Enums;

namespace DataBridgeToolKit.Serialization.Core.Interfaces
{
    public interface IDataConverterFactory
    {
        IDataConverter<TData> CreateConverter<TData>(ISerializationOptions options);
    }
}


