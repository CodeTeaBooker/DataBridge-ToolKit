using System;

namespace DataBridgeToolKit.Serialization.Core.Exceptions
{
    /// <summary>
    /// Exception for errors encountered during deserialization.
    /// </summary>
    public class DeserializationException : SerializationException
    {
        public DeserializationException(string message) : base(message) { }

        public DeserializationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}