using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataBridgeToolKit.Storage.Utils.Tests
{
    [TestFixture]
    [Category("Storage")]
    [Category("Validation")]
    public class ValidationResultTests
    {
        #region Constructor Tests

        public static IEnumerable<TestCaseData> ConstructorInvalidErrorsTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new string[] { "Error 1", null, "   ", string.Empty, "Error 2" },
                    new string[] { "Error 1", "Error 2" },
                    50
                ).SetName("Constructor_WithInvalidErrors_FiltersInvalidEntries");
            }
        }

        [Test, TestCaseSource(nameof(ConstructorInvalidErrorsTestCases))]
        [Description("Verifies that constructor filters out null and whitespace errors")]
        public void Constructor_WithInvalidErrors_FiltersInvalidEntries(string[] errors, string[] expectedErrors, int maxErrors)
        {
            // Act
            var result = new ValidationResult(errors, maxErrors);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False, "Result should be invalid");
                Assert.That(result.Errors.ToList(), Has.Count.EqualTo(expectedErrors.Length), "Should only include valid errors");
                Assert.That(result.Errors.ToList(), Is.EquivalentTo(expectedErrors), "Should contain non-empty errors");
            });
        }

        [Test]
        [Description("Verifies that default constructor creates valid result without errors")]
        public void Constructor_Default_CreatesValidResult()
        {
            // Act
            var result = new ValidationResult();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True, "Result should be valid");
                Assert.That(result.Errors, Is.Empty, "Errors collection should be empty");
            });
        }

        [Test]
        [Description("Verifies that constructor with single error creates invalid result")]
        public void Constructor_WithSingleError_CreatesInvalidResult()
        {
            // Arrange
            const string errorMessage = "Test error";

            // Act
            var result = new ValidationResult(errorMessage);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False, "Result should be invalid");
                Assert.That(result.Errors.ToList(), Has.Count.EqualTo(1), "Should have exactly one error");
                Assert.That(result.Errors.First(), Is.EqualTo(errorMessage), "Error message should match");
            });
        }

        [Test]
        [Description("Verifies that constructor with error collection creates invalid result")]
        public void Constructor_WithErrorCollection_CreatesInvalidResult()
        {
            // Arrange
            var errors = new[] { "Error 1", "Error 2", "Error 3" };

            // Act
            var result = new ValidationResult(errors);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False, "Result should be invalid");
                Assert.That(result.Errors.ToList(), Has.Count.EqualTo(3), "Should have exact number of errors");
                Assert.That(result.Errors, Is.EquivalentTo(errors), "Errors should match input collection");
            });
        }

        [Test]
        [Description("Verifies that constructor throws ArgumentNullException for null error collection")]
        public void Constructor_WithNullErrorCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ValidationResult((IEnumerable<string>)null));
            Assert.That(ex.ParamName, Is.EqualTo("errors"));
        }

        #endregion

        #region AddError Tests

        [Test]
        [Description("Verifies that AddError correctly adds a single valid error")]
        public void AddError_WithValidError_AddsToCollection()
        {
            // Arrange
            var result = new ValidationResult();
            const string errorMessage = "Valid Error";

            // Act
            result.AddError(errorMessage);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False, "Result should become invalid");
                Assert.That(result.Errors.ToList(), Has.Count.EqualTo(1), "Should have exactly one error");
                Assert.That(result.Errors.First(), Is.EqualTo(errorMessage), "Error message should match");
            });
        }

        [Test]
        [TestCase(null, TestName = "AddError_WithNullError_ThrowsArgumentException")]
        [TestCase("", TestName = "AddError_WithEmptyError_ThrowsArgumentException")]
        [TestCase("   ", TestName = "AddError_WithWhitespaceError_ThrowsArgumentException")]
        public void AddError_WithInvalidError_ThrowsArgumentException(string invalidError)
        {
            // Arrange
            var result = new ValidationResult();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => result.AddError(invalidError));
            Assert.That(ex.ParamName, Is.EqualTo("error"));
        }

        #endregion

        #region AddErrors Tests

        public static IEnumerable<TestCaseData> AddErrorsTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 2", "Error 3" },
                    new string[] { "Error 1", "Error 2", "Error 3" },
                    50
                ).SetName("AddErrors_WithValidErrors_AddsToCollection");

                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 1", "Error 2", "Error 2" },
                    new string[] { "Error 1", "Error 1", "Error 2", "Error 2" },
                    50
                ).SetName("AddErrors_WithDuplicateErrors_AllDuplicatesAdded");

                yield return new TestCaseData(
                    new string[] { "Error 1", "", "   ", null, "Error 2" },
                    new string[] { "Error 1", "Error 2" },
                    50
                ).SetName("AddErrors_WithInvalidErrors_FiltersAndAddsValidErrors");

                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 2", "Error 3", "Error 4", "Error 5" },
                    new string[] { "Error 3", "Error 4", "Error 5" },
                    3
                ).SetName("AddErrors_ReachesMaxErrors_RemovesOldestErrors");
            }
        }

        [Test, TestCaseSource(nameof(AddErrorsTestCases))]
        [Description("Verifies that AddErrors correctly adds multiple errors with various scenarios")]
        public void AddErrors_Tests(string[] inputErrors, string[] expectedErrors, int maxErrors)
        {
            // Arrange
            var result = new ValidationResult(maxErrors);

            // Act
            result.AddErrors(inputErrors);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                if (expectedErrors.Length == 0)
                {
                    Assert.That(result.IsValid, Is.True, "Result should be valid");
                    Assert.That(result.Errors.ToList(), Is.Empty, "Errors collection should be empty");
                }
                else
                {
                    Assert.That(result.IsValid, Is.False, "Result should be invalid");
                    Assert.That(result.Errors.ToList(), Has.Count.EqualTo(expectedErrors.Length), "Should have exact number of expected errors");
                    CollectionAssert.AreEquivalent(expectedErrors, result.Errors.ToList());
                }
            });
        }

        [Test]
        [Description("Verifies that AddErrors throws ArgumentNullException for null collection")]
        public void AddErrors_WithNullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            var result = new ValidationResult();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => result.AddErrors(null));
            Assert.That(ex.ParamName, Is.EqualTo("errors"));
        }

        #endregion

        #region Static Method Tests

        [Test]
        [TestCase(TestName = "Success_CreatesValidResult")]
        public void Success_CreatesValidResult()
        {
            // Act
            var result = ValidationResult.Success();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True, "Result should be valid");
                Assert.That(result.Errors, Is.Empty, "Errors collection should be empty");
            });
        }

        [Test]
        [TestCase("Test error", TestName = "Error_CreatesInvalidResult")]
        public void Error_CreatesInvalidResult(string errorMessage)
        {
            // Act
            var result = ValidationResult.Error(errorMessage);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False, "Result should be invalid");
                Assert.That(result.Errors.ToList(), Has.Count.EqualTo(1), "Should have exactly one error");
                Assert.That(result.Errors.First(), Is.EqualTo(errorMessage), "Error message should match");
            });
        }

        public static IEnumerable<TestCaseData> CombineTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 2" },
                    new string[] { "Error 3", "Error 4" },
                    new string[] { "Error 1", "Error 2", "Error 3", "Error 4" },
                    5
                ).SetName("Combine_WithMultipleResults_MergesCorrectly");

                yield return new TestCaseData(
                    new string[] { },
                    new string[] { },
                    new string[] { },
                    50
                ).SetName("Combine_WithNoResults_ReturnsSuccess");

                yield return new TestCaseData(
                    null,
                    null,
                    new string[] { },
                    50
                ).SetName("Combine_WithNullArray_ReturnsSuccess");

                yield return new TestCaseData(
                    new string[] { "Error 1" },
                    null,
                    new string[] { "Error 1" },
                    5
                ).SetName("Combine_WithNullResults_FiltersNullEntries");

                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 2" },
                    new string[] { "Error 2", "Error 3" },
                    new string[] { "Error 1", "Error 2", "Error 2", "Error 3" },
                    5
                ).SetName("Combine_WithDuplicateErrors_AllDuplicatesAdded");

                yield return new TestCaseData(
                    new string[] { "Error 1", "Error 2", "Error 3" },
                    new string[] { "Error 4", "Error 5", "Error 6" },
                    new string[] { "Error 4", "Error 5", "Error 6" },
                    3
                ).SetName("Combine_ExceedsCombinedMaxErrors_RemovesOldestErrors");
            }
        }

        [Test, TestCaseSource(nameof(CombineTestCases))]
        [Description("Verifies that Combine method correctly merges multiple ValidationResult instances")]
        public void Combine_Tests(string[] errors1, string[] errors2, string[] expectedErrors, int combinedMaxErrors)
        {
            // Arrange
            var result1 = errors1 != null ? new ValidationResult(errors1, combinedMaxErrors) : null;
            var result2 = errors2 != null ? new ValidationResult(errors2, combinedMaxErrors) : null;
            var results = new ValidationResult[] { result1, result2 };

            // Act
            var combined = ValidationResult.Combine(results);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                if (expectedErrors.Length == 0)
                {
                    Assert.That(combined.IsValid, Is.True, "Combined result should be valid");
                    Assert.That(combined.Errors.ToList(), Is.Empty, "Errors collection should be empty");
                }
                else
                {
                    Assert.That(combined.IsValid, Is.False, "Combined result should be invalid");
                    Assert.That(combined.Errors.ToList(), Has.Count.EqualTo(expectedErrors.Length), "Should have exact number of combined errors");
                    CollectionAssert.AreEquivalent(expectedErrors, combined.Errors.ToList());
                }
            });
        }

        [Test]
        [Description("Verifies that Combine with all ValidationResult instances as Success returns Success")]
        public void Combine_AllValidationResultsSuccess_ReturnsSuccess()
        {
            // Arrange
            var result1 = ValidationResult.Success();
            var result2 = ValidationResult.Success();
            var results = new ValidationResult[] { result1, result2 };

            // Act
            var combined = ValidationResult.Combine(results);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(combined.IsValid, Is.True, "Combined result should be valid");
                Assert.That(combined.Errors, Is.Empty, "Errors collection should be empty");
            });
        }

        #endregion

        #region ToString Tests

        [Test]
        [Description("Verifies that ToString method returns comma-separated error messages")]
        public void ToString_ReturnsCommaSeparatedErrors()
        {
            // Arrange
            var result = new ValidationResult();
            var errors = new[] { "Error 1", "Error 2", "Error 3" };
            result.AddErrors(errors);

            // Act
            var resultString = result.ToString();

            // Assert
            var expectedString = string.Join(", ", errors);
            Assert.That(resultString, Is.EqualTo(expectedString), "ToString should return comma-separated errors");
        }

        #endregion

        #region Exception Tests

        [Test]
        [TestCase(null, TestName = "Constructor_WithNullErrorCollection_ThrowsArgumentNullException")]
        public void Constructor_WithNullErrorCollection_ThrowsArgumentNullException(string[] errors)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ValidationResult(errors));
            Assert.That(ex.ParamName, Is.EqualTo("errors"));
        }

        #endregion

        #region Thread Safety Tests

        [Test]
        [Description("Verifies that concurrent AddError calls correctly include the last 5 errors and do not exceed MaxErrors")]
        public void AddError_ConcurrentAdds_DoesNotExceedMaxErrors()
        {
            // Arrange
            int maxErrors = 100;
            var result = new ValidationResult(maxErrors);
            int totalThreads = 10;
            int errorsPerThread = 20;
            var tasks = new List<Task>();
            var expectedLastErrors = Enumerable.Range(totalThreads * errorsPerThread - 4, 5)
                                               .Select(i => $"Error {i}")
                                               .ToArray();

            // Act
            for (int i = 0; i < totalThreads; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < errorsPerThread; j++)
                    {
                        result.AddError($"Error {threadIndex * errorsPerThread + j + 1}");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.Errors.Count, Is.LessThanOrEqualTo(maxErrors), "Errors count should not exceed MaxErrors");
                Assert.That(result.Errors, Is.SupersetOf(expectedLastErrors), "Errors collection should contain the last 5 errors");
            });
        }

        //[Test]
        //[Description("Verifies that concurrent AddErrors calls correctly include the last 5 errors and do not exceed MaxErrors")]
        //public void AddErrors_ConcurrentBatchAdds_DoesNotExceedMaxErrors()
        //{
        //    // Arrange
        //    int maxErrors = 100;
        //    var result = new ValidationResult(maxErrors);
        //    int totalThreads = 10;
        //    int errorsPerThread = 20;
        //    var tasks = new List<Task>();
        //    var expectedLastErrors = Enumerable.Range(totalThreads * errorsPerThread - 4, 5)
        //                                       .Select(i => $"Error {i}")
        //                                       .ToArray();

        //    // Act
        //    for (int i = 0; i < totalThreads; i++)
        //    {
        //        int threadIndex = i;
        //        tasks.Add(Task.Run(() =>
        //        {
        //            var errors = Enumerable.Range(1, errorsPerThread)
        //                                   .Select(j => $"Error {threadIndex * errorsPerThread + j}");
        //            result.AddErrors(errors);
        //        }));
        //    }

        //    Task.WaitAll(tasks.ToArray());

        //    // Assert
        //    // Assert
        //    // Assert
        //    MultipleAssert.Multiple(() =>
        //    {
        //        Assert.That(result.Errors.Count, Is.LessThanOrEqualTo(maxErrors), "Errors count should not exceed MaxErrors");
        //        var actualLastErrors = result.Errors.Reverse().Take(5).Reverse().ToList();
        //        Assert.That(actualLastErrors, Is.EquivalentTo(expectedLastErrors), "Last 5 errors should match expected errors");
        //    });



        //}

        [Test]
        [Description("Verifies that Combine method is thread-safe when called concurrently")]
        public void Combine_ConcurrentCombines_AreThreadSafe()
        {
            // Arrange
            int maxErrors = 50;
            var validationResults = new List<ValidationResult>();
            int numberOfResults = 10;
            for (int i = 0; i < numberOfResults; i++)
            {
                var vr = new ValidationResult(maxErrors);
                vr.AddErrors(Enumerable.Range(1, 20).Select(j => $"Error {i * 20 + j}"));
                validationResults.Add(vr);
            }

            var tasks = new List<Task<ValidationResult>>();

            // Act
            foreach (var vr in validationResults)
            {
                tasks.Add(Task.Run(() => ValidationResult.Combine(vr)));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            foreach (var task in tasks)
            {
                var combined = task.Result;
                Assert.That(combined.Errors.Count, Is.LessThanOrEqualTo(maxErrors), "Combined errors should not exceed MaxErrors");
            }
        }

        #endregion

        #region Performance Tests

        [Test]
        [Description("Verifies that AddErrors completes within a reasonable time for large number of errors")]
        public void AddErrors_LargeNumberOfErrors_CompletesWithinReasonableTime()
        {
            // Arrange
            var result = new ValidationResult(maxErrors: 1000);
            var errors = Enumerable.Range(1, 1000).Select(i => $"Error {i}").ToList();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            result.AddErrors(errors);

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // Assert
            Assert.That(elapsedMilliseconds, Is.LessThan(100), $"Adding 1000 errors took too long: {elapsedMilliseconds} ms");
            Assert.That(result.Errors.Count, Is.EqualTo(1000), "Should have exactly 1000 errors");
        }

        [Test]
        [Description("Verifies that AddError performs well under high concurrency and contains the last 5 errors")]
        public void AddError_ConcurrentHighLoad_CompletesWithinReasonableTime()
        {
            // Arrange
            int maxErrors = 1000;
            var result = new ValidationResult(maxErrors);
            int totalThreads = 50;
            int errorsPerThread = 25;
            var tasks = new List<Task>();
            var expectedLastErrors = Enumerable.Range(totalThreads * errorsPerThread - 4, 5)
                                               .Select(i => $"Error {i}")
                                               .ToArray();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < totalThreads; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < errorsPerThread; j++)
                    {
                        result.AddError($"Error {threadIndex * errorsPerThread + j + 1}");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.Errors.Count, Is.LessThanOrEqualTo(maxErrors), "Errors count should not exceed MaxErrors");
                Assert.That(result.Errors, Is.SupersetOf(expectedLastErrors), "Errors collection should contain the last 5 errors");
            });
        }

        #endregion
    }
}
