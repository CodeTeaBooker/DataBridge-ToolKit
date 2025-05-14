using DataBridgeToolKit.Serialization.Core.Enums;
using DataBridgeToolKit.Serialization.Core.Factories;
using DataBridgeToolKit.Serialization.Core.Interfaces;
using DataBridgeToolKit.Storage.Core.Enums;
using DataBridgeToolKit.Storage.Core.Factories;
using DataBridgeToolKit.Storage.Core.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace DataBridgeToolKit.Services.Implementations
{
    public class DataStorageService<T> : IDisposable
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IDataConverter<T> _dataConverter;
        private readonly IStorageWriter _storageWriter;
        private readonly IStorageReader _storageReader;
        private bool _disposed;


        public DataStorageService(StorageProviderType storageType, SerializationFormat serializationFormat)
        : this(CreateDefaultOptions(storageType), serializationFormat)
        {
        }

        public DataStorageService(IStorageProviderOptions storageProviderOptions, SerializationFormat serializationFormat)
        {
            if (storageProviderOptions == null)
                throw new ArgumentNullException(nameof(storageProviderOptions));

            var storageProviderFactory = new StorageProviderFactory();
            _storageProvider = storageProviderFactory.CreateProvider(storageProviderOptions);

            _storageWriter = _storageProvider.CreateWriter();
            _storageReader = _storageProvider.CreateReader();

            var serializationsOptionsFactory = new SerializationOptionsFactory();
            var serializationOptions = serializationsOptionsFactory.CreateOptions(serializationFormat);
            var dataConverterFactory = new DataConverterFactory();
            _dataConverter = dataConverterFactory.CreateConverter<T>(serializationOptions);
        }

        private static IStorageProviderOptions CreateDefaultOptions(StorageProviderType storageType)
        {
            var storageProviderOptionsFactory = new StorageProviderOptionsFactory();
            return storageProviderOptionsFactory.CreateOptions(storageType);
        }

        public async Task SaveAsync(string fileName, T data, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var bytes = _dataConverter.ToBytes(data);
            string finalFileName = Path.ChangeExtension(fileName, _dataConverter.FileExtension);
            await _storageWriter.WriteAsync(finalFileName, bytes, token);
        }


        public async Task<T> LoadAsync(string fileName, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            string finalFileName = Path.ChangeExtension(fileName, _dataConverter.FileExtension);
            var bytes = await _storageReader.ReadAsync(finalFileName, token);
            return _dataConverter.FromBytes(bytes);
        }

        public Task<bool> ExistsAsync(string fileName, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            string finalFileName = Path.ChangeExtension(fileName, _dataConverter.FileExtension);
            return _storageReader.ExistsAsync(finalFileName, token);
        }

        public Task DeleteAsync(string fileName, CancellationToken token = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            string finalFileName = Path.ChangeExtension(fileName, _dataConverter.FileExtension);
            return _storageWriter.DeleteAsync(finalFileName, token);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DataStorageService<T>));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    _storageProvider?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
