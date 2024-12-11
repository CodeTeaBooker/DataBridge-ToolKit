using DevToolkit.Serialization.Core.Enums;
using DevToolkit.Serialization.Core.Interfaces;
using MessagePack;

namespace DevToolkit.Serialization.Implementations.Options
{
    public sealed class MsgPackSerializationOptions : ISerializationOptions
    {
        public SerializationFormat Format => SerializationFormat.MsgPack;

        private readonly MessagePackSerializerOptions _settings;

        public int SmallDataThreshold { get; }
        public int InitialBufferSize { get; }
        public int MaxDataSize { get; }

        public MsgPackSerializationOptions(
            MessagePackSerializerOptions settings = null,
            int smallDataThreshold = 85000,
            int initialBufferSize = 256,
            int maxDataSize = 100 * 1024 * 1024) 
        {
            SmallDataThreshold = smallDataThreshold;
            InitialBufferSize = initialBufferSize;
            MaxDataSize = maxDataSize;
            _settings = (settings ?? MessagePackSerializerOptions.Standard)
                .WithSecurity(MessagePackSecurity.TrustedData);
        }

        public MessagePackSerializerOptions GetSettings()
        {
            return _settings;
        }

        public ISerializationOptions Clone()
        {
            return new MsgPackSerializationOptions(
                _settings,
                SmallDataThreshold,
                InitialBufferSize,
                MaxDataSize);
        }
    }
}
