using System;

namespace DataBridgeToolKit.Serialization.Core.Exceptions
{
    /// <summary>
    /// Base exception for serialization-related errors.
    /// </summary>
    public class SerializationException : Exception
    {
        public SerializationException(string message) : base(message) { }

        public SerializationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}