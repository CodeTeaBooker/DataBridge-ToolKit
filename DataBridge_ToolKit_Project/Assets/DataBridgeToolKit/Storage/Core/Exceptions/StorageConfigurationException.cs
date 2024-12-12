using System;

namespace DevToolkit.Storage.Core.Exceptions
{
    public class StorageConfigurationException : Exception
    {
        public StorageConfigurationException(string message) : base(message) { }
        public StorageConfigurationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}