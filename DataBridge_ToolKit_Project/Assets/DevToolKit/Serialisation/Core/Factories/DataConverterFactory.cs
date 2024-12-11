using DevToolkit.Serialization.Core.Enums;
using DevToolkit.Serialization.Core.Interfaces;
using DevToolkit.Serialization.Implementations.Converters;
using DevToolkit.Serialization.Implementations.Options;
using System;

namespace DevToolkit.Serialization.Core.Factories
{
    public class DataConverterFactory : IDataConverterFactory
    {
        public IDataConverter<TData> CreateConverter<TData>(ISerializationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return options.Format switch
            {
                SerializationFormat.Json => CreateJsonConverter<TData>(ValidateAndCastOptions<JsonSerializationOptions>(options)),
                SerializationFormat.Xml => CreateXmlConverter<TData>(ValidateAndCastOptions<XmlSerializationOptions>(options)),
                SerializationFormat.MsgPack => CreateMsgPackConverter<TData>(ValidateAndCastOptions<MsgPackSerializationOptions>(options)),
                _ => throw new ArgumentException($"Unsupported serialization format: {options.Format}", nameof(options.Format))
            };
        }

        protected virtual IDataConverter<TData> CreateJsonConverter<TData>(JsonSerializationOptions options)
        {
            return new JsonDataConverter<TData>(options);
        }

        protected virtual IDataConverter<TData> CreateXmlConverter<TData>(XmlSerializationOptions options)
        {
            return new XmlDataConverter<TData>(options);
        }

        protected virtual IDataConverter<TData> CreateMsgPackConverter<TData>(MsgPackSerializationOptions options)
        {
            return new MsgPackDataConverter<TData>(options);
        }

        private static TOptions ValidateAndCastOptions<TOptions>(ISerializationOptions options)
            where TOptions : class, ISerializationOptions
        {
            if (options is not TOptions typedOptions)
            {
                throw new ArgumentException(
                    $"Invalid options type. Expected {typeof(TOptions).Name}, but got {options.GetType().Name}",
                    nameof(options));
            }

            return typedOptions;
        }
    }
}