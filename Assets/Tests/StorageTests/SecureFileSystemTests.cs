using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;


namespace DataBridgeToolKit.Storage.Implementations.Tests
{
    [TestFixture]
    public class SecureFileSystemTests
    {
        #region Test Setup
        private SecureFileSystem _fileSystem;
        private ConcurrentLockManager _lockManager;
        private string _tempDirectory;
        private const int DefaultBufferSize = 4096;
        private const int DefaultTimeout = 500;

        [SetUp]
        public void SetUp()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            _lockManager = new ConcurrentLockManager(
                defaultTimeout: TimeSpan.FromMilliseconds(DefaultTimeout),
                cleanupInterval: TimeSpan.FromSeconds(1),
                inactiveTimeout: TimeSpan.FromSeconds(2));

            _fileSystem = new SecureFileSystem(
                _lockManager,
                defaultBufferSize: DefaultBufferSize,
                lockTimeout: TimeSpan.FromMilliseconds(DefaultTimeout),
                useWriteThrough: false);
        }

        [TearDown]
        public void TearDown()
        {
            _lockManager.Dispose();

            if (Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error cleaning up temp directory: {ex}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private async Task<byte[]> GenerateRandomDataAsync(int size)
        {
            byte[] data = new byte[size];
            await Task.Run(() => new System.Random().NextBytes(data));
            return data;
        }

        private string GetTempFilePath(string prefix = "test")
        {
            return Path.Combine(_tempDirectory, $"{prefix}_{Guid.NewGuid()}.tmp");
        }

        private async Task<string> CreateTestFileAsync(int size)
        {
            string path = GetTempFilePath();
            byte[] data = await GenerateRandomDataAsync(size);
            await File.WriteAllBytesAsync(path, data);
            return path;
        }

        #endregion

        #region Read Tests

        [UnityTest]
        public IEnumerator ReadFileAsync_ExistingFile_ReturnsCorrectData()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                byte[] expectedData = await GenerateRandomDataAsync(1024);
                string filePath = GetTempFilePath();
                await File.WriteAllBytesAsync(filePath, expectedData);

                // Act
                byte[] actualData = await _fileSystem.ReadFileAsync(filePath, DefaultBufferSize, CancellationToken.None);

                // Assert
                CollectionAssert.AreEqual(expectedData, actualData, "Read data should match written data");
            });
        }

        [UnityTest]
        public IEnumerator ReadFileAsync_NullPath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
               await _fileSystem.ReadFileAsync(null, DefaultBufferSize, CancellationToken.None));

            });

        }

        [UnityTest]
        public IEnumerator ReadFileAsync_EmptyPath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                await _fileSystem.ReadFileAsync(string.Empty, DefaultBufferSize, CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator ReadFileAsync_NonexistentFile_ThrowsFileNotFoundException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                string nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.txt");
                await AsyncAssert.ThrowsAsync<FileNotFoundException>(async () =>
                    await _fileSystem.ReadFileAsync(nonExistentPath, DefaultBufferSize, CancellationToken.None));
            });



        }

        [UnityTest]
        public IEnumerator ReadFileAsync_CancelledOperation_ThrowsOperationCanceledException()
        {
            LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("Error reading file at path .*"));

            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();
                string filePath = GetTempFilePath();
                File.WriteAllText(filePath, "test");

                await AsyncAssert.ThrowsAsync<OperationCanceledException>(async () =>
                    await _fileSystem.ReadFileAsync(filePath, DefaultBufferSize, cts.Token));
            });
        }


        [UnityTest]
        public IEnumerator ReadFileAsync_WithBasePathAndFileName_ReturnsCorrectData()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                byte[] expectedData = await GenerateRandomDataAsync(1024);
                string fileName = "testfile";
                string extension = ".dat";
                await File.WriteAllBytesAsync(Path.Combine(_tempDirectory, fileName + extension), expectedData);

                // Act
                byte[] actualData = await _fileSystem.ReadFileAsync(
                    _tempDirectory,
                    fileName,
                    extension,
                    DefaultBufferSize,
                    CancellationToken.None);

                // Assert
                CollectionAssert.AreEqual(expectedData, actualData, "Read data should match written data");
            });
        }

        [UnityTest]
        public IEnumerator ReadFileAsync_WithBasePathAndFileName_NullBasePath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                    await _fileSystem.ReadFileAsync(
                        null,
                        "fileName",
                        ".txt",
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator ReadFileAsync_WithBasePathAndFileName_NullFileName_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                    await _fileSystem.ReadFileAsync(
                        _tempDirectory,
                        null,
                        ".txt",
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator ReadFileAsync_WithBasePathAndFileName_NonExistentFile_ThrowsFileNotFoundException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<FileNotFoundException>(async () =>
                    await _fileSystem.ReadFileAsync(
                        _tempDirectory,
                        "nonexistent",
                        ".txt",
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        #endregion

        #region Write Tests

        [UnityTest]
        public IEnumerator WriteFileAsync_ValidData_WritesSuccessfully()
        {

            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                string filePath = GetTempFilePath();
                byte[] data = await GenerateRandomDataAsync(1024);

                // Act
                await _fileSystem.WriteFileAsync(filePath, data, DefaultBufferSize, CancellationToken.None);

                // Assert
                byte[] writtenData = await File.ReadAllBytesAsync(filePath);
                CollectionAssert.AreEqual(data, writtenData, "Written data should match original data");
            });

        }

        [UnityTest]
        public IEnumerator WriteFileAsync_NullPath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                byte[] data = new byte[10];
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                     await _fileSystem.WriteFileAsync(null, data, DefaultBufferSize, CancellationToken.None));
            });

        }

        [UnityTest]
        public IEnumerator WriteFileAsync_NullData_ThrowsArgumentNullException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                string path = GetTempFilePath();
                await AsyncAssert.ThrowsAsync<ArgumentNullException>(async () =>
                    await _fileSystem.WriteFileAsync(path, null, DefaultBufferSize, CancellationToken.None));
            });
        }


        [UnityTest]
        public IEnumerator WriteFileAsync_EmptyPath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                byte[] data = new byte[10];
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                    await _fileSystem.WriteFileAsync(string.Empty, data, DefaultBufferSize, CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_ConcurrentWrites_MaintainsDataIntegrity()
        {

            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                string filePath = GetTempFilePath();
                const int writeCount = 10;
                var tasks = new List<Task>();
                var writtenData = new List<byte[]>();

                // Act
                for (int i = 0; i < writeCount; i++)
                {
                    byte[] data = await GenerateRandomDataAsync(1024);
                    writtenData.Add(data);
                    tasks.Add(_fileSystem.WriteFileAsync(filePath, data, DefaultBufferSize, CancellationToken.None));
                }

                await Task.WhenAll(tasks);

                byte[] finalData = await File.ReadAllBytesAsync(filePath);
                CollectionAssert.AreEqual(writtenData[writtenData.Count - 1], finalData,
                    "Final file content should match last written data");
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_WithBasePathAndFileName_WritesSuccessfully()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                byte[] data = await GenerateRandomDataAsync(1024);
                string fileName = "testwrite";
                string extension = ".dat";
                string expectedPath = Path.Combine(_tempDirectory, fileName + extension);

                // Act
                await _fileSystem.WriteFileAsync(
                    _tempDirectory,
                    fileName,
                    extension,
                    data,
                    DefaultBufferSize,
                    CancellationToken.None);

                // Assert
                Assert.IsTrue(File.Exists(expectedPath), "File should exist at the expected path");
                byte[] writtenData = await File.ReadAllBytesAsync(expectedPath);
                CollectionAssert.AreEqual(data, writtenData, "Written data should match original data");
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_WithBasePathAndFileName_NullBasePath_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                byte[] data = await GenerateRandomDataAsync(1024);
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                    await _fileSystem.WriteFileAsync(
                        null,
                        "fileName",
                        ".txt",
                        data,
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_WithBasePathAndFileName_NullFileName_ThrowsArgumentException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                byte[] data = await GenerateRandomDataAsync(1024);
                await AsyncAssert.ThrowsAsync<ArgumentException>(async () =>
                    await _fileSystem.WriteFileAsync(
                        _tempDirectory,
                        null,
                        ".txt",
                        data,
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_WithBasePathAndFileName_NullData_ThrowsArgumentNullException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                await AsyncAssert.ThrowsAsync<ArgumentNullException>(async () =>
                    await _fileSystem.WriteFileAsync(
                        _tempDirectory,
                        "fileName",
                        ".txt",
                        null,
                        DefaultBufferSize,
                        CancellationToken.None));
            });
        }

        [UnityTest]
        public IEnumerator WriteFileAsync_WithBasePathAndFileName_CreatesDirectoryIfNotExists()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                string subDir = "subdir";
                string basePath = Path.Combine(_tempDirectory, subDir);
                byte[] data = await GenerateRandomDataAsync(1024);
                string fileName = "testfile";
                string extension = ".dat";

                // Act
                await _fileSystem.WriteFileAsync(
                    basePath,
                    fileName,
                    extension,
                    data,
                    DefaultBufferSize,
                    CancellationToken.None);

                // Assert
                string expectedPath = Path.Combine(basePath, fileName + extension);
                Assert.IsTrue(Directory.Exists(basePath), "Directory should be created");
                Assert.IsTrue(File.Exists(expectedPath), "File should exist at the expected path");
                byte[] writtenData = await File.ReadAllBytesAsync(expectedPath);
                CollectionAssert.AreEqual(data, writtenData, "Written data should match original data");
            });
        }

        #endregion

        #region Directory Operations Tests

        [Test]
        public void CreateDirectory_ValidPath_CreatesDirectory()
        {
            // Arrange
            string dirPath = Path.Combine(_tempDirectory, "testDir");

            // Act
            _fileSystem.CreateDirectory(dirPath);

            // Assert
            Assert.IsTrue(Directory.Exists(dirPath), "Directory should be created");
        }

        [Test]
        public void CreateDirectory_NullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _fileSystem.CreateDirectory(null));
        }

        [Test]
        public void DeleteDirectory_ExistingDirectory_DeletesSuccessfully()
        {
            // Arrange
            string dirPath = Path.Combine(_tempDirectory, "deleteDir");
            Directory.CreateDirectory(dirPath);
            File.WriteAllText(Path.Combine(dirPath, "test.txt"), "test");

            // Act
            _fileSystem.DeleteDirectory(dirPath, true);

            // Assert
            Assert.IsFalse(Directory.Exists(dirPath), "Directory should be deleted");
        }

        [Test]
        public void IsDirectoryEmpty_EmptyDirectory_ReturnsTrue()
        {
            // Arrange
            string dirPath = Path.Combine(_tempDirectory, "emptyDir");
            Directory.CreateDirectory(dirPath);

            // Act & Assert
            Assert.IsTrue(_fileSystem.IsDirectoryEmpty(dirPath));
        }

        [Test]
        public void IsDirectoryEmpty_NonEmptyDirectory_ReturnsFalse()
        {
            // Arrange
            string dirPath = Path.Combine(_tempDirectory, "nonEmptyDir");
            Directory.CreateDirectory(dirPath);
            File.WriteAllText(Path.Combine(dirPath, "test.txt"), "test");

            // Act & Assert
            Assert.IsFalse(_fileSystem.IsDirectoryEmpty(dirPath));
        }

        #endregion

        #region Path Operation Tests
        [Test]
        public void GetDirectoryName_ValidPath_ReturnsCorrectName()
        {
            // Arrange
            string path = Path.Combine(_tempDirectory, "subdir", "file.txt");

            // Act
            string dirName = _fileSystem.GetDirectoryName(path);

            // Assert
            Assert.AreEqual(Path.Combine(_tempDirectory, "subdir"), dirName);
        }

        [Test]
        public void GetFileName_ValidPath_ReturnsCorrectName()
        {
            // Arrange
            string path = Path.Combine(_tempDirectory, "subdir", "file.txt");

            // Act
            string fileName = _fileSystem.GetFileName(path);

            // Assert
            Assert.AreEqual("file.txt", fileName);
        }
        #endregion

        #region Performance Tests

        [UnityTest]
        public IEnumerator ReadWritePerformance_LargeFile_CompletesWithinTimeout()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                const int fileSize = 100 * 1024 * 1024; // 100MB
                const int timeoutSeconds = 30;
                byte[] data = await GenerateRandomDataAsync(fileSize);
                string filePath = GetTempFilePath();

                // Act & Assert - Write
                var writeStopwatch = System.Diagnostics.Stopwatch.StartNew();
                await _fileSystem.WriteFileAsync(filePath, data, DefaultBufferSize, CancellationToken.None);
                writeStopwatch.Stop();

                Assert.Less(writeStopwatch.ElapsedMilliseconds, timeoutSeconds * 1000,
                    $"Write operation should complete within {timeoutSeconds} seconds");

                // Act & Assert - Read
                var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
                byte[] readData = await _fileSystem.ReadFileAsync(filePath, DefaultBufferSize, CancellationToken.None);
                readStopwatch.Stop();

                Assert.Less(readStopwatch.ElapsedMilliseconds, timeoutSeconds * 1000,
                    $"Read operation should complete within {timeoutSeconds} seconds");
                CollectionAssert.AreEqual(data, readData, "Read data should match written data");
            });

        }
        #endregion

        #region Edge Cases

        [UnityTest]
        public IEnumerator WriteFileAsync_ZeroByteFile_HandlesCorrectly()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                string filePath = GetTempFilePath();
                byte[] emptyData = new byte[0];

                // Act
                await _fileSystem.WriteFileAsync(filePath, emptyData, DefaultBufferSize, CancellationToken.None);

                // Assert
                Assert.IsTrue(File.Exists(filePath), "File should be created");
                Assert.AreEqual(0, new FileInfo(filePath).Length, "File should be empty");

            });
        }


        [UnityTest]
        public IEnumerator WriteFileAsync_VerySmallBufferSize_WorksCorrectly()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                const int smallBufferSize = 1;
                string filePath = GetTempFilePath();
                byte[] data = await GenerateRandomDataAsync(1024);

                // Act
                await _fileSystem.WriteFileAsync(filePath, data, smallBufferSize, CancellationToken.None);

                // Assert
                byte[] readData = await File.ReadAllBytesAsync(filePath);
                CollectionAssert.AreEqual(data, readData, "Written data should match original data");
            });
        }
        #endregion

        #region Cleanup Tests
        [UnityTest]
        public IEnumerator WriteFileAsync_FailedWrite_CleanupsTempFile()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                string filePath = GetTempFilePath();
                string tempFilePath = filePath + ".tmp";
                byte[] data = await GenerateRandomDataAsync(1024);
                var cts = new CancellationTokenSource();

                // Act
                try
                {
                    var writeTask = _fileSystem.WriteFileAsync(filePath, data, DefaultBufferSize, cts.Token);
                    cts.Cancel();
                    await writeTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected exception
                }

                // Assert
                Assert.IsFalse(File.Exists(tempFilePath), "Temporary file should be cleaned up");
            });
        }

        #endregion

    }
}

