using DevToolkit.Serialization.Core.Enums;
using DevToolkit.Services.Implementations;
using DevToolkit.Storage.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DataStorageLocalCustomConfigurationExample : MonoBehaviour
{
    [SerializeField]
    private string _directoryName = "GameData";

    [SerializeField]
    private string _fileName = "TestData";

    [SerializeField]
    private SerializationFormat _serializationFormat;

    private StorageData _testData;

    private DataStorageService<StorageData> _storageService;
    
    void Start()
    {
        InitializeStorageService();
        InitializeTestData();
    }

    private void InitializeTestData()
    {
        _testData = new StorageData
        {
            Name = "LocalTest",
            Description = "Local storage test",
            ID = 2
        };
    }


    private void InitializeStorageService()
    {
        var localStorageOptions = new LocalStorageProviderOptions
        {
            BasePath = Path.Combine(Application.dataPath, _directoryName),
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

        _storageService = new DataStorageService<StorageData>(localStorageOptions, _serializationFormat);
    }

    public void SaveData()
    {
        _ = SaveDataAsync();
    }

    private async Task SaveDataAsync()
    {
        try
        {
            await _storageService.SaveAsync(_fileName, _testData);
            Debug.Log("Data saved successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data: {ex.Message}");
        }
    }


    public void LoadData()
    {
        _ = LoadDataAsync();
    }


    private async Task LoadDataAsync()
    {
        try
        {
            var loadedData = await _storageService.LoadAsync(_fileName);
            Debug.Log($"Loaded data: {loadedData}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load data: {ex.Message}");
        }
    }

    public void CheckDataExists()
    {
        _ = CheckDataExistsAsync();
    }


    private async Task CheckDataExistsAsync()
    {
        try
        {
            bool exists = await _storageService.ExistsAsync(_fileName);
            Debug.Log($"File exists: {exists}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check data existence: {ex.Message}");
        }
    }


    public void DeleteData()
    {
        _ = DeleteDataAsync();
    }

    private async Task DeleteDataAsync()
    {
        try
        {
            await _storageService.DeleteAsync(_fileName);
            Debug.Log("Data deleted successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete data: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        _storageService?.Dispose();
    }

}



#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DataStorageLocalCustomConfigurationExample))]
public class DataStorageLocalCustomConfigurationExampleEditor: UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DataStorageLocalCustomConfigurationExample dataStorageLocalCustomConfigurationExample
            = (DataStorageLocalCustomConfigurationExample)target;
        DrawDefaultInspector();

        if(GUILayout.Button("Save Data"))
        {
            dataStorageLocalCustomConfigurationExample.SaveData();
        }

        if (GUILayout.Button("Check Data Exisits"))
        {
            dataStorageLocalCustomConfigurationExample.CheckDataExists();
        }

        if (GUILayout.Button("Load Data"))
        {
            dataStorageLocalCustomConfigurationExample.LoadData();
        }

        if (GUILayout.Button("Delete Data"))
        {
            dataStorageLocalCustomConfigurationExample.DeleteData();
        }
    }
}



#endif
