using DataBridgeToolKit.Storage.Core.Interfaces;
using DataBridgeToolKit.Storage.Implementations;
using DataBridgeToolKit.Storage.Options;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataBridgeToolKit.Storage.Core.Factories.Tests
{
    [TestFixture]
    public class FileSystemFactoryTests
    {
        private string _tempDirectory;
        private LocalStorageProviderOptions _options;

        [SetUp]
        public void SetUp()
        {
            // Create a unique temporary directory for each test
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            // Configure LocalStorageProviderOptions
            _options = new LocalStorageProviderOptions
            {
                BasePath = _tempDirectory,
                BufferSize = 4096,
                LockTimeout = TimeSpan.FromMilliseconds(500),
                LockCleanupInterval = TimeSpan.FromSeconds(1),
                LockInactiveTimeout = TimeSpan.FromSeconds(2),
                UseWriteThrough = false
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Release the file system instance
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Delete the temporary directory and its contents
            if (Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error deleting temporary directory '{_tempDirectory}': {ex}");
                }
            }
        }

        #region GetOrCreateFileSystem Tests

        [Test]
        [Description("Should create a new SecureFileSystem instance on the first call to GetOrCreateFileSystem")]
        public void GetOrCreateFileSystem_FirstCall_CreatesNewFileSystem()
        {
            IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");
            Assert.IsInstanceOf<SecureFileSystem>(fs, "Should return a SecureFileSystem instance");
        }

        [Test]
        [Description("Multiple calls to GetOrCreateFileSystem with the same BasePath should return the same instance and correctly manage the reference count")]
        public void GetOrCreateFileSystem_MultipleCalls_SameInstance()
        {
            IFileSystem fs1 = FileSystemFactory.GetOrCreateFileSystem(_options);
            IFileSystem fs2 = FileSystemFactory.GetOrCreateFileSystem(_options);

            Assert.AreSame(fs1, fs2, "Should return the same IFileSystem instance");
        }

        [Test]
        [Description("Multiple calls to GetOrCreateFileSystem with different BasePaths should return different instances")]
        public void GetOrCreateFileSystem_DifferentBasePath_DifferentInstances()
        {
            // Create a second temporary directory
            string secondTempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(secondTempDir);

            var secondOptions = new LocalStorageProviderOptions
            {
                BasePath = secondTempDir,
                BufferSize = 4096,
                LockTimeout = TimeSpan.FromMilliseconds(500),
                LockCleanupInterval = TimeSpan.FromSeconds(1),
                LockInactiveTimeout = TimeSpan.FromSeconds(2),
                UseWriteThrough = false
            };

            try
            {
                IFileSystem fs1 = FileSystemFactory.GetOrCreateFileSystem(_options);
                IFileSystem fs2 = FileSystemFactory.GetOrCreateFileSystem(secondOptions);

                Assert.IsNotNull(fs1, "Should return a non-null IFileSystem instance");
                Assert.IsNotNull(fs2, "Should return a non-null IFileSystem instance");
                Assert.AreNotSame(fs1, fs2, "Different BasePaths should return different IFileSystem instances");
            }
            finally
            {
                // Clean up the second temporary directory
                FileSystemFactory.ReleaseFileSystem(secondOptions.BasePath);
                if (Directory.Exists(secondTempDir))
                {
                    Directory.Delete(secondTempDir, true);
                }
            }
        }

        [Test]
        [Description("Calling GetOrCreateFileSystem with null options should throw ArgumentNullException")]
        public void GetOrCreateFileSystem_NullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                FileSystemFactory.GetOrCreateFileSystem(null);
            }, "Should throw ArgumentNullException when options are null");
        }


        #endregion

        #region ReleaseFileSystem Tests

        [Test]
        [Description("Calling ReleaseFileSystem should correctly release resources when the reference count drops to zero")]
        public void ReleaseFileSystem_ReferenceCountZero_DisposesFileSystem()
        {
            IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");

            // Release the file system once, reference count drops from 1 to 0, should trigger Dispose
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Attempt to get the file system again, should create a new instance
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
            Assert.AreNotSame(fs, fsNew, "A new IFileSystem instance should be created after the reference count drops to zero");
        }

        [Test]
        [Description("Calling ReleaseFileSystem multiple times should correctly manage the reference count and only dispose when the count reaches zero")]
        public void ReleaseFileSystem_MultipleReferences_DisposesOnlyWhenReferenceCountZero()
        {
            IFileSystem fs1 = FileSystemFactory.GetOrCreateFileSystem(_options);
            IFileSystem fs2 = FileSystemFactory.GetOrCreateFileSystem(_options);

            Assert.AreSame(fs1, fs2, "Should return the same IFileSystem instance");

            // Release once, reference count drops from 2 to 1, should not trigger Dispose
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Release again, reference count drops from 1 to 0, should trigger Dispose
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Attempt to get the file system again, should create a new instance
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
            Assert.AreNotSame(fs1, fsNew, "A new IFileSystem instance should be created after the reference count drops to zero");
        }

        [Test]
        [Description("Calling ReleaseFileSystem on a non-existent BasePath should not throw an exception")]
        public void ReleaseFileSystem_NonExistentBasePath_DoesNotThrow()
        {
            string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Assert.DoesNotThrow(() =>
            {
                FileSystemFactory.ReleaseFileSystem(nonExistentPath);
            }, "Should not throw an exception for a non-existent BasePath");
        }

        #endregion

        #region Thread Safety Tests

        [Test]
        [Description("Concurrent calls to GetOrCreateFileSystem and ReleaseFileSystem should be thread-safe")]
        public void FileSystemFactory_ConcurrentAccess_ThreadSafe()
        {
            int threadCount = 10;
            int iterations = 100;
            Task[] tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
                        Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");

                        // Randomly release the file system
                        FileSystemFactory.ReleaseFileSystem(_options.BasePath);
                    }
                });
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks);

            // Finally, release any remaining references
            // Since each thread performed 'iterations' GetOrCreate and Release, the reference count should be zero
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Ensure a new instance can be created
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        [Description("Calling GetOrCreateFileSystem and immediately releasing should be handled correctly")]
        public void GetOrCreateAndRelease_Immediately_Succeeds()
        {
            IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");

            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Ensure a new instance can be created
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
            Assert.AreNotSame(fs, fsNew, "A new IFileSystem instance should be created after release");
        }

        [Test]
        [Description("Calling GetOrCreateFileSystem multiple times and releasing multiple times should ensure no residual references")]
        public void GetOrCreateMultipleAndReleaseMultiple_NoResidualReferences()
        {
            int createCount = 5;
            for (int i = 0; i < createCount; i++)
            {
                IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
                Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");
            }

            // Release the same number of times
            for (int i = 0; i < createCount; i++)
            {
                Assert.DoesNotThrow(() =>
                {
                    FileSystemFactory.ReleaseFileSystem(_options.BasePath);
                }, "Releasing should not throw an exception");
            }

            // Ensure the reference count is zero and a new instance can be created
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
            Assert.AreNotSame(fsNew, null, "Should create a new IFileSystem instance");
        }

        #endregion

        #region IDisposable Tests

        [Test]
        [Description("Ensure that SecureFileSystem instances are correctly disposed after ReleaseFileSystem is called")]
        public void ReleaseFileSystem_DisposesSecureFileSystem()
        {
            IFileSystem fs = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fs, "Should return a non-null IFileSystem instance");

            // Release the file system
            FileSystemFactory.ReleaseFileSystem(_options.BasePath);

            // Since SecureFileSystem does not expose its Dispose state, indirectly verify by checking if a new instance is created
            IFileSystem fsNew = FileSystemFactory.GetOrCreateFileSystem(_options);
            Assert.IsNotNull(fsNew, "Should return a non-null IFileSystem instance");
            Assert.AreNotSame(fs, fsNew, "A new IFileSystem instance should be created after disposal");
        }

        #endregion
    }
}
