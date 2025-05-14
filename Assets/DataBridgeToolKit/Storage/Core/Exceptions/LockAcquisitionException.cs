using System;

namespace DataBridgeToolKit.Storage.Core.Exceptions
{
    public class LockAcquisitionException : Exception
    {
        public LockAcquisitionException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
