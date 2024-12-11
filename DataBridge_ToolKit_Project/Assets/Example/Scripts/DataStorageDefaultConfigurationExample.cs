using DevToolkit.Serialization.Core.Enums;
using DevToolkit.Services.Implementations;
using DevToolkit.Storage.Core.Enums;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class DataStorageDefaultConfigurationExample : MonoBehaviour
{
    private void Start()
    {
        // Execute the default storage example asynchronously
        _ = ExecuteDefaultStorageExampleAsync();
    }

    private async Task ExecuteDefaultStorageExampleAsync()
    {
        // Create test data
        var testData = new StorageData
        {
            Name = "LocalTest",
            Description = "Local storage test",
            ID = 1
        };

        // Initialize DataStorageService with default (local) configuration and XML serialization format
        using var storageService = new DataStorageService<StorageData>(StorageProviderType.Local, SerializationFormat.Xml);

        try
        {
            // Save data
            await storageService.SaveAsync("ExampleData", testData);
            Debug.Log("Data saved successfully.");

            // Check if the file exists
            bool exists = await storageService.ExistsAsync("ExampleData");
            Debug.Log($"File exists: {exists}");

            // Load data
            var loadedData = await storageService.LoadAsync("ExampleData");
            Debug.Log($"Loaded data: {loadedData}");

            // Delete data
            await storageService.DeleteAsync("ExampleData");
            Debug.Log("Data deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Storage operation failed: {ex.Message}");
        }
    }
}