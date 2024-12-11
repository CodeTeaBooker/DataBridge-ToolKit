using DevToolkit.Serialization.Core.Enums;
using DevToolkit.Services.Implementations;
using DevToolkit.Storage.Options;
using MessagePack;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DataStorageCustomConfigurationExample : MonoBehaviour
{
    private void Start()
    {
        // Execute the local storage configuration example
        _ = ExecuteLocalStorageExampleAsync();

        // Placeholder: ExecuteNetworkStorageExampleAsync is not implemented yet and is planned
        _ = ExecuteNetworkStorageExampleAsync();

        // Placeholder: ExecuteCloudStorageExampleAsync is not implemented yet and is planned
        _ = ExecuteCloudStorageExampleAsync();
    }

    private async Task ExecuteLocalStorageExampleAsync()
    {
        // Create local storage configuration
        var localStorageOptions = new LocalStorageProviderOptions
        {
            BasePath = Path.Combine(Application.persistentDataPath, "GameData"),
            BufferSize = 32 * 1024,  // 32KB buffer
            MaxFileSize = 50 * 1024 * 1024,  // Maximum file size 50MB
            LockTimeout = TimeSpan.FromSeconds(15),
            LockCleanupInterval = TimeSpan.FromMinutes(5),
            LockInactiveTimeout = TimeSpan.FromMinutes(15),
            UseWriteThrough = true,  // Write directly to disk without using system cache
            CreateDirectoryIfNotExist = true,
            CleanupEmptyDirectories = true,
            MaxCleanupDepth = 3
        };

        // Create test data
        var testData = new StorageData
        {
            Name = "LocalTest",
            Description = "Local storage test",
            ID = 2
        };

        // Use DataStorageService with custom configuration
        using var storageService = new DataStorageService<StorageData>(localStorageOptions, SerializationFormat.MsgPack);

        try
        {
            // Save data
            await storageService.SaveAsync("TestData", testData);
            Debug.Log("Data saved successfully.");

            // Check if the file exists
            bool exists = await storageService.ExistsAsync("TestData");
            Debug.Log($"File exists: {exists}");

            // Read data
            var loadedData = await storageService.LoadAsync("TestData");
            Debug.Log($"Loaded data: {loadedData}");

            // Delete data
            await storageService.DeleteAsync("TestData");
            Debug.Log("Data deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Storage operation failed: {ex.Message}");
        }
    }

    private async Task ExecuteNetworkStorageExampleAsync()
    {
        Debug.LogWarning("Network storage is not yet implemented. Please wait for future updates.");

        // Create network storage configuration
        var networkStorageOptions = new NetworkStorageProviderOptions
        {
            ServerUrl = "https://storage.myserver.com",
            Username = "gameUser",
            Password = "securePassword",
            ConnectionTimeout = TimeSpan.FromSeconds(10),
            OperationTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 3
        };

        // Create example data
        var networkTestData = new StorageData
        {
            Name = "NetworkTest",
            Description = "Network storage test",
            ID = 1
        };

        // Use DataStorageService with network configuration
        using var networkStorageService = new DataStorageService<StorageData>(networkStorageOptions, SerializationFormat.Json);

        try
        {
            // Save data
            await networkStorageService.SaveAsync("NetworkData", networkTestData);
            Debug.Log("Data saved successfully to network storage.");

            // Check if the file exists
            bool exists = await networkStorageService.ExistsAsync("NetworkData");
            Debug.Log($"File exists in network storage: {exists}");

            // Read data
            var loadedData = await networkStorageService.LoadAsync("NetworkData");
            Debug.Log($"Loaded data from network: {loadedData}");

            // Delete data
            await networkStorageService.DeleteAsync("NetworkData");
            Debug.Log("Data deleted successfully from network storage.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Network storage operation failed: {ex.Message}");
        }
    }

    private async Task ExecuteCloudStorageExampleAsync()
    {
        Debug.LogWarning("Cloud storage is not yet implemented. Please wait for future updates.");

        // Create cloud storage configuration
        var cloudStorageOptions = new CloudStorageProviderOptions
        {
            CloudProvider = "Azure",
            AccessKeyId = "YOUR_ACCESS_KEY",
            SecretAccessKey = "YOUR_SECRET_KEY",
            Region = "europa-west-1",
            BucketName = "game-saves",
            Timeout = TimeSpan.FromMinutes(3)
        };

        // Create example data
        var cloudTestData = new StorageData
        {
            Name = "CloudTest",
            Description = "Cloud storage test",
            ID = 1
        };

        // Use DataStorageService with cloud configuration
        using var cloudStorageService = new DataStorageService<StorageData>(cloudStorageOptions, SerializationFormat.MsgPack);

        try
        {
            // Save data
            await cloudStorageService.SaveAsync("CloudData", cloudTestData);
            Debug.Log("Data saved successfully to cloud storage.");

            // Check if the file exists
            bool exists = await cloudStorageService.ExistsAsync("CloudData");
            Debug.Log($"File exists in cloud storage: {exists}");

            // Read data
            var loadedData = await cloudStorageService.LoadAsync("CloudData");
            Debug.Log($"Loaded data from cloud: {loadedData}");

            // Delete data
            await cloudStorageService.DeleteAsync("CloudData");
            Debug.Log("Data deleted successfully from cloud storage.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Cloud storage operation failed: {ex.Message}");
        }
    }
}

[Serializable]
[MessagePackObject]
public class StorageData
{
    [Key(0)]
    public string Name { get; set; }

    [Key(1)]
    public string Description { get; set; }

    [Key(2)]
    public int ID { get; set; }

    public override string ToString()
    {
        return $"StorageData: Name = {Name}, Description = {Description}, ID = {ID}";
    }
}
