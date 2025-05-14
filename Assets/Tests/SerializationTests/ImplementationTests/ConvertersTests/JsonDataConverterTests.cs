using DataBridgeToolKit.Serialization.Core.Exceptions;
using DataBridgeToolKit.Serialization.Implementations.Options;
using DataBridgeToolKit.Tests.Core.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine.TestTools;

namespace DataBridgeToolKit.Serialization.Implementations.Converters.Tests
{
    [TestFixture]
    [Category("Serialization")]
    public class JsonDataConverterTests
    {
        private JsonDataConverter<TestData> _converter;
        private JsonSerializationOptions _options;
        private TestData _testData;
        private TestDataBuilder _builder;
        private readonly DateTime _fixedDate = new DateTime(2024, 1, 1);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _builder = new TestDataBuilder(_fixedDate);
        }

        [SetUp]
        public void SetUp()
        {
            _options = new JsonSerializationOptions(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 10,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
            _converter = new JsonDataConverter<TestData>(_options);
            _testData = _builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _converter = null;
            _options = null;
            _testData = null;
        }

        #region Constructor Tests

        [Test]
        [Description("Verifies that the converter is properly initialized with valid options")]
        public void Constructor_WithValidOptions_InitializesConverter()
        {
            // Arrange & Act
            var converter = new JsonDataConverter<TestData>(_options);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(converter, Is.Not.Null);
                Assert.That(converter.ContentType, Is.EqualTo("application/json"));
                Assert.That(converter.FileExtension, Is.EqualTo(".json"));
            });
        }

        [Test]
        [Description("Verifies that constructor throws ArgumentNullException when options are null")]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(
                () => new JsonDataConverter<TestData>(null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        #endregion

        #region Property Validation Tests

        [Test]
        [Description("Verifies that ContentType and FileExtension properties return correct values")]
        public void Properties_ReturnCorrectValues()
        {
            // Arrange & Act
            var contentType = _converter.ContentType;
            var fileExtension = _converter.FileExtension;

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(contentType, Is.EqualTo("application/json"));
                Assert.That(fileExtension, Is.EqualTo(".json"));
            });
        }

        #endregion

        #region Synchronous Serialization Tests

        [Test]
        [Description("Verifies that valid data is correctly serialized to bytes")]
        public void ToBytes_WithValidData_ReturnsCorrectBytes()
        {
            // Arrange
            var expectedJson = JsonConvert.SerializeObject(_testData, _options.GetSettings());
            var expectedBytes = Encoding.UTF8.GetBytes(expectedJson);

            // Act
            var resultBytes = _converter.ToBytes(_testData);
            var resultJson = Encoding.UTF8.GetString(resultBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultBytes, Is.Not.Null);
                Assert.That(resultJson, Is.EqualTo(expectedJson));
            });
        }

        [Test]
        [Description("Verifies that serializing null data throws ArgumentNullException")]
        public void ToBytes_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            TestData nullData = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => _converter.ToBytes(nullData));
        }

        [Test]
        [Description("Verifies that circular references are handled correctly")]
        public void ToBytes_WithCircularReference_HandlesCorrectly()
        {
            // Arrange
            var circularData = _builder
                .WithName("Circular")
                .Build();

            circularData.Reference = circularData;

            // Act
            var resultBytes = _converter.ToBytes(circularData);
            var resultJson = Encoding.UTF8.GetString(resultBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultBytes, Is.Not.Null);
                Assert.That(resultJson, Does.Not.Contain("Reference"),
                    "When ReferenceLoopHandling is set to Ignore, circular references should be excluded from serialization");
                Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<TestData>(resultJson),
                    "The JSON should be valid and deserializable");
            });
        }

        [Test]
        [Description("Verifies that circular references can be preserved with appropriate settings")]
        public void ToBytes_WithCircularReference_PreservesReferenceWhenConfigured()
        {
            // Arrange
            var preserveReferencesSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var options = new JsonSerializationOptions(preserveReferencesSettings);
            var converter = new JsonDataConverter<TestData>(options);

            var circularData = _builder
                .WithName("Circular")
                .Build();
            circularData.Reference = circularData;

            // Act
            var resultBytes = converter.ToBytes(circularData);
            var resultJson = Encoding.UTF8.GetString(resultBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultBytes, Is.Not.Null);
                Assert.That(resultJson, Does.Contain("Reference"),
                    "With PreserveReferencesHandling enabled, circular references should be included");
                Assert.That(resultJson, Does.Contain("$id"),
                    "JSON should contain reference identifiers");
                var deserialized = JsonConvert.DeserializeObject<TestData>(resultJson, preserveReferencesSettings);
                Assert.That(deserialized.Reference, Is.SameAs(deserialized),
                    "Circular reference should be preserved after deserialization");
            });
        }


        [Test]
        [Category("Performance")]
        [Description("Verifies that serializing large data completes within timeout")]
        public void ToBytes_WithLargeData_CompletesWithinTimeout()
        {
            // Arrange
            var largeData = _builder
                .WithName(new string('a', 1000))
                .WithNumbers(Enumerable.Range(1, 10000).ToArray())
                .Build();

            // Act & Assert
            Assert.That(() =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _converter.ToBytes(largeData);
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }, Is.LessThan(1000), "Serialization should complete within 1 second");
        }

        #endregion

        #region Synchronous Deserialization Tests

        [Test]
        [Description("Verifies that valid bytes are correctly deserialized to object")]
        public void FromBytes_WithValidBytes_ReturnsDeserializedObject()
        {
            // Arrange
            var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_testData, _options.GetSettings()));

            // Act
            var result = _converter.FromBytes(jsonBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(_testData.Id));
                Assert.That(result.Name, Is.EqualTo(_testData.Name));
                Assert.That(result.CreatedDate, Is.EqualTo(_testData.CreatedDate));
                Assert.That(result.Numbers, Is.EqualTo(_testData.Numbers));
            });
        }

        [Test]
        [Description("Verifies that deserializing null bytes throws ArgumentNullException")]
        public void FromBytes_WithNullBytes_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => _converter.FromBytes(null));
        }

        [Test]
        [Description("Verifies that deserializing empty bytes throws DeserializationException")]
        public void FromBytes_WithEmptyBytes_ThrowsDeserializationException()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => _converter.FromBytes(emptyBytes));
            Assert.That(ex.Message, Does.Contain("Input data cannot be null or empty"));
        }

        [Test]
        [Description("Verifies that deserializing invalid JSON throws DeserializationException")]
        public void FromBytes_WithInvalidJson_ThrowsDeserializationException()
        {
            // Arrange
            var invalidJsonBytes = Encoding.UTF8.GetBytes("invalid json");

            // Act & Assert
            var ex = Assert.Throws<DeserializationException>(() => _converter.FromBytes(invalidJsonBytes));
            Assert.That(ex.Message, Does.Contain("Failed to deserialize"));
        }

        #endregion

        #region Asynchronous Serialization Tests

        [UnityTest]
        [Description("Verifies that valid data is correctly serialized to bytes asynchronously")]
        public IEnumerator ToBytesAsync_WithValidData_ReturnsCorrectBytes()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var expectedJson = JsonConvert.SerializeObject(_testData, _options.GetSettings());

                // Act
                var resultBytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);
                var resultJson = Encoding.UTF8.GetString(resultBytes);

                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(resultBytes, Is.Not.Null);
                    Assert.That(resultJson, Is.EqualTo(expectedJson));
                });
            });
        }

        [UnityTest]
        [Description("Verifies that async serialization can be cancelled")]
        public IEnumerator ToBytesAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                // Act & Assert
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(
                    async () => await _converter.ToBytesAsync(_testData, cts.Token));
            });
        }

        [UnityTest]
        [Description("Verifies that async serialization of null data throws ArgumentNullException")]
        public IEnumerator ToBytesAsync_WithNullData_ThrowsArgumentNullException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                TestData nullData = null;

                // Act & Assert
                var ex = await AsyncAssert.ThrowsAsync<ArgumentNullException>(
                    async () => await _converter.ToBytesAsync(nullData, CancellationToken.None));
                Assert.That(ex.ParamName, Is.EqualTo("data"));
            });
        }

        #endregion

        #region Asynchronous Deserialization Tests

        [UnityTest]
        [Description("Verifies that valid bytes are correctly deserialized to object asynchronously")]
        public IEnumerator FromBytesAsync_WithValidBytes_ReturnsDeserializedObject()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_testData, _options.GetSettings()));

                // Act
                var result = await _converter.FromBytesAsync(jsonBytes, CancellationToken.None);

                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result.Id, Is.EqualTo(_testData.Id));
                    Assert.That(result.Name, Is.EqualTo(_testData.Name));
                    Assert.That(result.CreatedDate, Is.EqualTo(_testData.CreatedDate));
                    Assert.That(result.Numbers, Is.EqualTo(_testData.Numbers));
                });
            });
        }

        [UnityTest]
        [Description("Verifies that async deserialization can be cancelled")]
        public IEnumerator FromBytesAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_testData));
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                // Act & Assert
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(
                    async () => await _converter.FromBytesAsync(jsonBytes, cts.Token));
            });
        }

        [UnityTest]
        [Description("Verifies that async deserialization of null bytes throws ArgumentNullException")]
        public IEnumerator FromBytesAsync_WithNullBytes_ThrowsArgumentNullException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Act & Assert
                var ex = await AsyncAssert.ThrowsAsync<ArgumentNullException>(
                    async () => await _converter.FromBytesAsync(null, CancellationToken.None));
                Assert.That(ex.ParamName, Is.EqualTo("data"));
            });
        }

        #endregion

        #region Helper Classes

        public class TestDataBuilder
        {
            private TestData _data;
            private readonly DateTime _fixedDate;

            public TestDataBuilder(DateTime fixedDate)
            {
                _fixedDate = fixedDate;
                Reset();
            }

            private void Reset()
            {
                _data = new TestData
                {
                    Id = 1,
                    Name = "Test Data",
                    CreatedDate = _fixedDate,
                    Numbers = new[] { 1, 2, 3, 4, 5 }
                };
            }

            public TestDataBuilder WithId(int id)
            {
                _data.Id = id;
                return this;
            }

            public TestDataBuilder WithName(string name)
            {
                _data.Name = name;
                return this;
            }

            public TestDataBuilder WithNumbers(int[] numbers)
            {
                _data.Numbers = numbers;
                return this;
            }

            public TestDataBuilder WithReference(TestData reference)
            {
                _data.Reference = reference;
                return this;
            }

            public TestData Build()
            {
                var result = _data;
                Reset();
                return result;
            }
        }

        public class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
            public int[] Numbers { get; set; }
            public TestData Reference { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is TestData other)
                {
                    return Id == other.Id &&
                           Name == other.Name &&
                           CreatedDate.Equals(other.CreatedDate) &&
                           (Numbers?.SequenceEqual(other.Numbers) ?? other.Numbers == null);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Name, CreatedDate, Numbers);
            }
        }

        #endregion
    }
}