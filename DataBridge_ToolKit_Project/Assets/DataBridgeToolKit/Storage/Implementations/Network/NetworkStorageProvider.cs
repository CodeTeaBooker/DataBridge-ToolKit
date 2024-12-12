using DevToolkit.Storage.Core.Interfaces;
using System;


namespace DevToolkit.Storage.Implementations
{
    public class NetworkStorageProvider : IStorageProvider
    {
        public IStorageReader CreateReader()
        {
            throw new NotImplementedException();
        }

        public IStorageWriter CreateWriter()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
