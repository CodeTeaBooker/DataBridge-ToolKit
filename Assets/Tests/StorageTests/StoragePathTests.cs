using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBridgeToolKit.Storage.Utils.Tests
{
    [TestFixture]
    [Category("StoragePath")]
    public class StoragePathTests
    {
        private const string TestOriginalKey = "test/originalKey";
        private const string TestNormalizedKey = "test/normalizedKey";
        private const string TestFullPath = "C:/test/fullPath";

        #region Constructor Tests

        [Test]
        [Description("Verifies that constructor initializes with valid parameters")]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(storagePath.OriginalKey, Is.EqualTo(TestOriginalKey));
                Assert.That(storagePath.NormalizedKey, Is.EqualTo(TestNormalizedKey));
                Assert.That(storagePath.FullPath, Is.EqualTo(TestFullPath));
                Assert.That(storagePath.IsValid, Is.True);
                Assert.That(storagePath.ValidationErrors, Is.Empty);
            });
        }

        [Test]
        [Description("Verifies that constructor throws ArgumentNullException for null OriginalKey")]
        public void Constructor_WithNullOriginalKey_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new StoragePath(null, TestNormalizedKey, TestFullPath));
            Assert.That(ex.ParamName, Is.EqualTo("originalKey"));
        }

        [Test]
        [Description("Verifies that constructor initializes with validation errors")]
        public void Constructor_WithValidationErrors_ShouldInitializeWithErrors()
        {
            var errors = new[] { "Error 1", "Error 2" };

            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, errors);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(storagePath.IsValid, Is.False);
                Assert.That(storagePath.ValidationErrors, Is.EquivalentTo(errors));
            });
        }

        [Test]
        [Description("Verifies that constructor filters out null, empty, and whitespace validation errors")]
        public void Constructor_WithInvalidValidationErrors_ShouldFilterInvalidErrors()
        {
            var errors = new[] { "Error 1", "", " ", null, "Error 2" };
            var expected = new[] { "Error 1", "Error 2" };

            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, errors);

            Assert.That(storagePath.ValidationErrors, Is.EquivalentTo(expected));
        }

        #endregion

        #region AddValidationError Tests

        [Test]
        [Description("Verifies that AddValidationError adds a valid error")]
        public void AddValidationError_WithValidError_ShouldAddError()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);

            storagePath.AddValidationError("Test Error");

            MultipleAssert.Multiple(() =>
            {
                Assert.That(storagePath.IsValid, Is.False);
                Assert.That(storagePath.ValidationErrors, Contains.Item("Test Error"));
            });
        }

        [Test]
        [Description("Verifies that AddValidationError throws ArgumentException for null, empty, or whitespace errors")]
        public void AddValidationError_WithInvalidError_ShouldThrowArgumentException()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);

            Assert.Throws<ArgumentException>(() => storagePath.AddValidationError(string.Empty));
            Assert.Throws<ArgumentException>(() => storagePath.AddValidationError(null));
            Assert.Throws<ArgumentException>(() => storagePath.AddValidationError("  "));
        }

        #endregion

        #region AddValidationErrors Tests

        [Test]
        [Description("Verifies that AddValidationErrors adds multiple valid errors")]
        public void AddValidationErrors_WithValidErrors_ShouldAddAllErrors()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);
            var errors = new[] { "Error 1", "Error 2" };

            storagePath.AddValidationErrors(errors);

            Assert.That(storagePath.ValidationErrors, Is.EquivalentTo(errors));
        }

        [Test]
        [Description("Verifies that AddValidationErrors filters out null, empty, and whitespace errors")]
        public void AddValidationErrors_WithInvalidErrors_ShouldFilterErrors()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);
            var errors = new[] { "Error 1", "", "  ", null, "Error 2" };
            var expected = new[] { "Error 1", "Error 2" };

            storagePath.AddValidationErrors(errors);

            Assert.That(storagePath.ValidationErrors, Is.EquivalentTo(expected));
        }

        #endregion

        #region Static Methods Tests

        [Test]
        [Description("Verifies that Invalid method with a single error creates an invalid StoragePath")]
        public void Invalid_WithSingleError_ShouldCreateInvalidPath()
        {
            var error = "Test Error";

            var storagePath = StoragePath.Invalid(TestOriginalKey, error);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(storagePath.IsValid, Is.False);
                Assert.That(storagePath.ValidationErrors, Contains.Item(error));
            });
        }

        [Test]
        [Description("Verifies that Invalid method with multiple errors creates an invalid StoragePath")]
        public void Invalid_WithMultipleErrors_ShouldCreateInvalidPath()
        {
            var errors = new[] { "Error 1", "Error 2" };

            var storagePath = StoragePath.Invalid(TestOriginalKey, errors);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(storagePath.ValidationErrors, Is.EquivalentTo(errors));
            });
        }

        #endregion

        #region Copy Method Tests

        [Test]
        [Description("Verifies that Copy creates an identical copy of the StoragePath")]
        public void Copy_ShouldCreateIdenticalCopy()
        {
            var original = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, new[] { "Error 1" });

            var copy = original.Copy();

            MultipleAssert.Multiple(() =>
            {
                Assert.That(copy, Is.Not.SameAs(original));
                Assert.That(copy.ValidationErrors, Is.EquivalentTo(original.ValidationErrors));
            });
        }

        #endregion

        #region ToValidationResult Tests

        [Test]
        [Description("Verifies that ToValidationResult creates a ValidationResult with correct errors")]
        public void ToValidationResult_ShouldCreateValidationResultWithErrors()
        {
            var errors = new[] { "Error 1", "Error 2" };
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, errors);

            var validationResult = storagePath.ToValidationResult();

            MultipleAssert.Multiple(() =>
            {
                Assert.That(validationResult.IsValid, Is.False);
                Assert.That(validationResult.Errors, Is.EquivalentTo(errors));
            });
        }

        #endregion

        #region Resource Management Tests

        [Test]
        [Description("Verifies that Dispose releases resources and prevents further usage")]
        public void Dispose_ShouldReleaseResources()
        {
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath);

            storagePath.Dispose();

            Assert.Throws<ObjectDisposedException>(() => storagePath.AddValidationError("Error"));
        }

        #endregion

        #region ToString Tests

        [Test]
        [Description("Verifies that ToString returns a correct string representation of the StoragePath")]
        public void ToString_ShouldReturnCorrectRepresentation()
        {
            var errors = new[] { "Error 1", "Error 2" };
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, errors);

            var result = storagePath.ToString();

            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain($"Original='{TestOriginalKey}'"));
                Assert.That(result, Does.Contain($"Normalized='{TestNormalizedKey}'"));
                Assert.That(result, Does.Contain($"Full='{TestFullPath}'"));
                Assert.That(result, Does.Contain("Valid=False"));
                Assert.That(result, Does.Contain("Error 1"));
                Assert.That(result, Does.Contain("Error 2"));
            });
        }

        #endregion

        #region Thread Safety Tests

        [Test]
        [Description("Verifies that concurrent AddValidationError calls are handled correctly")]
        public void AddValidationError_ConcurrentCalls_ShouldNotLoseErrors()
        {
            // Arrange
            int totalThreads = 10;
            int errorsPerThread = 20;
            int maxErrors = totalThreads * errorsPerThread;
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, maxErrors: maxErrors);

            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < totalThreads; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < errorsPerThread; j++)
                    {
                        storagePath.AddValidationError($"Error {threadIndex * errorsPerThread + j}");
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(storagePath.ValidationErrors.Count, Is.EqualTo(maxErrors), "All errors should be retained without loss.");
        }


        [Test]
        [Description("Verifies that concurrent AddValidationErrors calls handle multiple collections correctly")]
        public void AddValidationErrors_ConcurrentCalls_ShouldNotLoseErrors()
        {
            // Arrange
            int totalThreads = 10;
            int errorsPerBatch = 20;
            int maxErrors = totalThreads * errorsPerBatch;
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, maxErrors: maxErrors);
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < totalThreads; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    var errors = Enumerable.Range(1, errorsPerBatch).Select(x => $"Error {threadIndex * errorsPerBatch + x}");
                    storagePath.AddValidationErrors(errors);
                }));
            }
            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(storagePath.ValidationErrors.Count, Is.EqualTo(maxErrors), "Not all errors were retained.");
        }


        #endregion

        #region Performance Tests

        [Test]
        [Description("Verifies that adding 10,000 validation errors completes in reasonable time")]
        public void AddValidationError_PerformanceTest_ShouldCompleteQuickly()
        {
            // Arrange
            int totalErrors = 10_000;
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, maxErrors: totalErrors);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < totalErrors; i++)
            {
                storagePath.AddValidationError($"Error {i}");
            }

            stopwatch.Stop();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000),
                    $"Adding {totalErrors} errors took too long: {stopwatch.ElapsedMilliseconds} ms");
                Assert.That(storagePath.ValidationErrors.Count, Is.EqualTo(totalErrors),
                    "The total number of errors added does not match the expected value");
            });
        }


        [Test]
        [Description("Verifies that adding 10,000 errors from multiple threads completes in reasonable time")]
        public void AddValidationError_ConcurrentPerformanceTest_ShouldCompleteQuickly()
        {
            // Arrange
            int totalThreads = 10;
            int errorsPerThread = 1_000;
            int totalErrors = totalThreads * errorsPerThread;
            var storagePath = new StoragePath(TestOriginalKey, TestNormalizedKey, TestFullPath, maxErrors: totalErrors);
            var tasks = new List<Task>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < totalThreads; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < errorsPerThread; j++)
                    {
                        storagePath.AddValidationError($"Error {threadIndex * errorsPerThread + j}");
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            stopwatch.Stop();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1500),
                    $"Concurrent adding of {totalErrors} errors took too long: {stopwatch.ElapsedMilliseconds} ms");
                Assert.That(storagePath.ValidationErrors.Count, Is.EqualTo(totalErrors),
                    "The total number of errors added does not match the expected value");
            });
        }



        #endregion
    }
}
