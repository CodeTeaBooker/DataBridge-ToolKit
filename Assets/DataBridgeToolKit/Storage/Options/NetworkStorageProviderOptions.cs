using DataBridgeToolKit.Storage.Core.Enums;
using DataBridgeToolKit.Storage.Core.Exceptions;
using DataBridgeToolKit.Storage.Core.Interfaces;
using System;

namespace DataBridgeToolKit.Storage.Options
{
    public class NetworkStorageProviderOptions : IStorageProviderOptions
    {
        public StorageProviderType ProviderType => StorageProviderType.Network;

        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public int MaxRetries { get; set; } = 3;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
                throw new StorageConfigurationException("ServerUrl cannot be empty");
            if (string.IsNullOrWhiteSpace(Username))
                throw new StorageConfigurationException("Username cannot be empty");
            if (string.IsNullOrWhiteSpace(Password))
                throw new StorageConfigurationException("Password cannot be empty");
            if (ConnectionTimeout <= TimeSpan.Zero)
                throw new StorageConfigurationException("ConnectionTimeout must be greater than 0");
            if (OperationTimeout <= TimeSpan.Zero)
                throw new StorageConfigurationException("OperationTimeout must be greater than 0");
            if (MaxRetries < 0)
                throw new StorageConfigurationException("MaxRetries cannot be negative");
        }

        public IStorageProviderOptions Clone() => new NetworkStorageProviderOptions
        {
            ServerUrl = this.ServerUrl,
            Username = this.Username,
            Password = this.Password,
            ConnectionTimeout = this.ConnectionTimeout,
            OperationTimeout = this.OperationTimeout,
            MaxRetries = this.MaxRetries
        };
    }
}