using DataBridgeToolKit.Storage.Core.Enums;
using DataBridgeToolKit.Storage.Core.Exceptions;
using DataBridgeToolKit.Storage.Core.Interfaces;
using System;

namespace DataBridgeToolKit.Storage.Options
{
    public class CloudStorageProviderOptions : IStorageProviderOptions
    {
        public StorageProviderType ProviderType => StorageProviderType.Cloud;

        public string CloudProvider { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Region { get; set; }
        public string BucketName { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CloudProvider))
                throw new StorageConfigurationException("CloudProvider cannot be empty");
            if (string.IsNullOrWhiteSpace(AccessKeyId))
                throw new StorageConfigurationException("AccessKeyId cannot be empty");
            if (string.IsNullOrWhiteSpace(SecretAccessKey))
                throw new StorageConfigurationException("SecretAccessKey cannot be empty");
            if (string.IsNullOrWhiteSpace(Region))
                throw new StorageConfigurationException("Region cannot be empty");
            if (string.IsNullOrWhiteSpace(BucketName))
                throw new StorageConfigurationException("BucketName cannot be empty");
            if (Timeout <= TimeSpan.Zero)
                throw new StorageConfigurationException("Timeout must be greater than 0");
        }

        public IStorageProviderOptions Clone() => new CloudStorageProviderOptions
        {
            CloudProvider = this.CloudProvider,
            AccessKeyId = this.AccessKeyId,
            SecretAccessKey = this.SecretAccessKey,
            Region = this.Region,
            BucketName = this.BucketName,
            Timeout = this.Timeout
        };
    }
}
