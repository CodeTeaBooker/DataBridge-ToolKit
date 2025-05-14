using DataBridgeToolKit.Serialization.Core.Enums;
using DataBridgeToolKit.Serialization.Core.Interfaces;
using Newtonsoft.Json;
using System;

namespace DataBridgeToolKit.Serialization.Implementations.Options
{
    public sealed class JsonSerializationOptions : ISerializationOptions
    {
        public SerializationFormat Format => SerializationFormat.Json;

        private readonly JsonSerializerSettings _settings;

        public JsonSerializationOptions(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.None
            };
        }

        public JsonSerializerSettings GetSettings()
        {
            return CloneSettings(_settings);
        }

        public ISerializationOptions Clone()
        {
            try
            {
                return new JsonSerializationOptions(CloneSettings(_settings));
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to clone JsonSerializerSettings.", ex);
            }
        }

        private static JsonSerializerSettings CloneSettings(JsonSerializerSettings settings)
        {
            var settingsJson = JsonConvert.SerializeObject(settings);
            return JsonConvert.DeserializeObject<JsonSerializerSettings>(settingsJson);
        }

    }
}

