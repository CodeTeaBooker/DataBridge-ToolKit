# DataBridge-ToolKit

A modular Unity toolkit for data serialization and storage. Currently supports JSON, MessagePack, and XML serialization formats with extensibility for custom formats. Features local storage implementation, with network and cloud storage solutions planned for future releases. Provides a unified interface for data management in Unity applications.

## Features

- Core DataStorageService
  - Single unified interface for all data operations
  - Easy format switching through SerializationFormat enum
  - Future storage type switching through StorageProviderType enum
  - Application logic remains unchanged when switching formats or storage types
- Multiple Serialization Formats Support
  - JSON
  - XML
  - MessagePack
- Flexible Storage Options
  - Local Storage (Implemented)
  - Network Storage (Planned)
  - Cloud Storage (Planned)
- Secure and Reliable File System Implementation
  - Atomic write operations to prevent data corruption
  - Thread-safe concurrent operations
  - File locking mechanism
  - Error recovery and rollback mechanisms
- Performance Optimizations
  - Configurable buffer strategy
  - Size-based file handling optimizations
  - WriteThrough option support
- Asynchronous API Design
- Unified Error Handling and Logging System

## System Requirements

- Unity 2022.3 or higher
- NuGetForUnity v4.1.1

## Installation

1. First, ensure [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) is installed in your project
2. Install the following dependencies via NuGet:
   - MessagePack v3.0.300
   - AsyncEx v5.1.2
   - Newtonsoft.Json v13.0.3

## Quick Start

### Data Model Definition
First, define your data model:

```csharp
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
```

### Using Default Configuration
Example using default configuration for quick start:

```csharp
// Create test data
var testData = new StorageData
{
    Name = "LocalTest",
    Description = "Local storage test",
    ID = 1
};

// Initialize storage service with default configuration using local storage and XML serialization
using var storageService = new DataStorageService<StorageData>(
    StorageProviderType.Local, 
    SerializationFormat.Xml);

try
{
    // Save data
    await storageService.SaveAsync("ExampleData", testData);
    Debug.Log("Data saved successfully.");

    // Check if file exists
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
```

### Using Custom Configuration
For more granular control, use custom configuration:

```csharp
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

// Initialize storage service with custom configuration
using var storageService = new DataStorageService<StorageData>(
    localStorageOptions, 
    SerializationFormat.MsgPack);

try
{
    // Save data
    await storageService.SaveAsync("TestData", testData);
    Debug.Log("Data saved successfully.");

    // Check if file exists
    bool exists = await storageService.ExistsAsync("TestData");
    Debug.Log($"File exists: {exists}");

    // Load data
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
```

## Project Structure

- **Core**
  - Abstractions: Base abstract classes
  - Interfaces: Interface definitions
  - Enums: Enumeration definitions
  - Exceptions: Custom exception classes
- **Implementations**
  - Converters: Data converter implementations
  - Options: Configuration option classes
  - Providers: Storage provider implementations

## Dependencies

- [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) v4.1.1
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp) v3.0.300
- [AsyncEx](https://github.com/StephenCleary/AsyncEx.git) v5.1.2
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) v13.0.3

## Development Roadmap

| Status | Feature |
|:------:|---------|
| ✅ | Local storage implementation |
| ✅ | Serialization format compression support |
| ⏳ | Custom serialization format support |
| ⏳ | Network storage implementation |
| ⏳ | Cloud storage implementation |
| ⏳ | Encryption support |

## Contributing

Issues and Pull Requests are welcome.

## License

Licensed under the MIT License. For full details, see the `LICENSE` file in the repository.
