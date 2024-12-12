//using DevToolkit.Serialization.Core.Exceptions;
//using DevToolkit.Serialization.Implementations.Options;
//using DevToolkit.Tests.Core.Utils;
//using MessagePack;
//using NUnit.Framework;
//using System;
//using System.Collections;
//using System.Linq;
//using System.Threading;
//using UnityEngine.TestTools;

//namespace DevToolkit.Serialization.Implementations.Converters.Tests
//{
//    [TestFixture]
//    [Category("Serialization")]
//    [Category("MessagePack")]
//    public class MsgPackDataConverterTests
//    {
//        private MsgPackDataConverter<TestData> _converter;
//        private MsgPackSerializationOptions _options;
//        private TestData _testData;
//        private TestDataBuilder _builder;
//        private readonly DateTime _fixedDate = new DateTime(2024, 1, 1);

//        [OneTimeSetUp]
//        public void OneTimeSetUp()
//        {
//            _builder = new TestDataBuilder(_fixedDate);
//        }

//        [SetUp]
//        public void SetUp()
//        {
//            var msgPackOptions = MessagePackSerializerOptions.Standard
//                .WithCompression(MessagePackCompression.Lz4BlockArray);
//            _options = new MsgPackSerializationOptions(msgPackOptions);
//            _converter = new MsgPackDataConverter<TestData>(_options);
//            _testData = _builder.Build();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _converter = null;
//            _options = null;
//            _testData = null;
//        }

//        #region Constructor Tests

//        [Test]
//        [Description("Verifies that the converter is properly initialized with valid options")]
//        public void Constructor_WithValidOptions_InitializesConverter()
//        {
//            // Arrange
//            var customOptions = new MsgPackSerializationOptions(
//                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.None));

//            // Act
//            var converter = new MsgPackDataConverter<TestData>(customOptions);

//            // Assert
//            MultipleAssert.Multiple(() =>
//            {
//                Assert.That(converter, Is.Not.Null);
//                Assert.That(converter.ContentType, Is.EqualTo("application/x-msgpack"));
//                Assert.That(converter.FileExtension, Is.EqualTo(".msgpack"));
//            });
//        }

//        [Test]
//        [Description("Verifies that constructor throws ArgumentNullException when options are null")]
//        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            var ex = Assert.Throws<ArgumentNullException>(() => new MsgPackDataConverter<TestData>(null));
//            Assert.That(ex.ParamName, Is.EqualTo("options"));
//        }

//        #endregion

//        #region Synchronous Serialization Tests

//        [TestCase(1, "Test Name", Description = "Small data")]
//        [TestCase(2, "A very long name that exceeds the typical size", Description = "Medium data")]
//        [TestCase(3, "An extremely long name that definitely exceeds the buffer size and tests our handling of large data streams", Description = "Large data")]
//        public void ToBytes_WithDifferentDataSizes_ReturnsCorrectBytes(int id, string name)
//        {
//            // Arrange
//            var data = _builder
//                .WithId(id)
//                .WithName(name)
//                .Build();

//            // Act
//            var resultBytes = _converter.ToBytes(data);
//            var deserializedData = MessagePackSerializer.Deserialize<TestData>(resultBytes, _options.GetSettings());

//            // Assert
//            MultipleAssert.Multiple(() =>
//            {
//                Assert.That(resultBytes, Is.Not.Null);
//                Assert.That(deserializedData.Id, Is.EqualTo(data.Id));
//                Assert.That(deserializedData.Name, Is.EqualTo(data.Name));
//                Assert.That(deserializedData.CreatedDate, Is.EqualTo(data.CreatedDate));
//                Assert.That(deserializedData.Numbers, Is.EqualTo(data.Numbers));
//            });
//        }

//        [Test]
//        [Description("Verifies that serializing null data throws ArgumentNullException")]
//        public void ToBytes_WithNullData_ThrowsArgumentNullException()
//        {
//            // Arrange
//            TestData nullData = null;

//            // Act & Assert
//            var ex = Assert.Throws<ArgumentNullException>(() => _converter.ToBytes(nullData));
//            Assert.That(ex.ParamName, Is.EqualTo("data"));
//        }

//        [Test]
//        [Description("Verifies that serializing data with circular reference throws SerializationException")]
//        public void ToBytes_WithCircularReference_ThrowsSerializationException()
//        {
//            // Arrange
//            var circularData = _builder.Build();
//            circularData.Reference = circularData; // Create circular reference

//            // Act & Assert
//            var ex = Assert.Throws<SerializationException>(() => _converter.ToBytes(circularData));
//            Assert.That(ex.Message, Does.Contain("Failed to serialize"));
//        }

//        [Test]
//        [Description("Verifies that large data exceeding default buffer size is serialized correctly")]
//        public void ToBytes_WithLargeData_SerializesCorrectly()
//        {
//            // Arrange
//            var largeData = _builder
//                .WithName(new string('a', 10000))
//                .WithNumbers(Enumerable.Range(1, 10000).ToArray())
//                .Build();

//            // Act
//            var resultBytes = _converter.ToBytes(largeData);
//            var deserializedData = MessagePackSerializer.Deserialize<TestData>(resultBytes, _options.GetSettings());

//            // Assert
//            MultipleAssert.Multiple(() =>
//            {
//                Assert.That(resultBytes.Length, Is.GreaterThan(4096),
//                    "Large data should produce bytes larger than default buffer size");
//                Assert.That(deserializedData.Name, Is.EqualTo(largeData.Name));
//                Assert.That(deserializedData.Numbers, Is.EqualTo(largeData.Numbers));
//            });
//        }

//        #endregion

//        #region Synchronous Deserialization Tests

//        [Test]
//        [Description("Verifies that valid bytes are correctly deserialized to object")]
//        public void FromBytes_WithValidBytes_ReturnsDeserializedObject()
//        {
//            // Arrange
//            var bytes = _converter.ToBytes(_testData);

//            // Act
//            var result = _converter.FromBytes(bytes);

//            // Assert
//            MultipleAssert.Multiple(() =>
//            {
//                Assert.That(result, Is.Not.Null);
//                Assert.That(result.Id, Is.EqualTo(_testData.Id));
//                Assert.That(result.Name, Is.EqualTo(_testData.Name));
//                Assert.That(result.CreatedDate, Is.EqualTo(_testData.CreatedDate));
//                Assert.That(result.Numbers, Is.EqualTo(_testData.Numbers));
//            });
//        }

//        [Test]
//        [Description("Verifies that deserializing null bytes throws ArgumentNullException")]
//        public void FromBytes_WithNullBytes_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            var ex = Assert.Throws<ArgumentNullException>(() => _converter.FromBytes(null));
//            Assert.That(ex.ParamName, Is.EqualTo("data"));
//        }

//        [Test]
//        [Description("Verifies that deserializing empty bytes throws DeserializationException")]
//        public void FromBytes_WithEmptyBytes_ThrowsDeserializationException()
//        {
//            // Arrange
//            var emptyBytes = Array.Empty<byte>();

//            // Act & Assert
//            var ex = Assert.Throws<ArgumentNullException>(() => _converter.FromBytes(emptyBytes));
//            Assert.That(ex.Message, Does.Contain("Input data cannot be null or empty"));
//        }

//        [Test]
//        [Description("Verifies that deserializing invalid bytes throws DeserializationException")]
//        public void FromBytes_WithInvalidBytes_ThrowsDeserializationException()
//        {
//            // Arrange
//            var invalidBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

//            // Act & Assert
//            var ex = Assert.Throws<DeserializationException>(() => _converter.FromBytes(invalidBytes));
//            Assert.That(ex.Message, Does.Contain("Failed to deserialize"));
//            Assert.That(ex.Message, Does.Contain($"Data length: {invalidBytes.Length} bytes"));
//        }

//        #endregion

//        #region Asynchronous Serialization Tests

//        [UnityTest]
//        [Description("Verifies that valid data is correctly serialized to bytes asynchronously")]
//        public IEnumerator ToBytesAsync_WithValidData_ReturnsCorrectBytes()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Act
//                var resultBytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);
//                var expectedBytes = MessagePackSerializer.Serialize(_testData, _options.GetSettings());

//                // Assert
//                MultipleAssert.Multiple(() =>
//                {
//                    Assert.That(resultBytes, Is.Not.Null);
//                    Assert.That(resultBytes, Is.EqualTo(expectedBytes));
//                });
//            });
//        }

//        [UnityTest]
//        [Description("Verifies that async serialization can be cancelled")]
//        public IEnumerator ToBytesAsync_WithCancellation_ThrowsOperationCanceledException()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Arrange
//                using var cts = new CancellationTokenSource();
//                cts.Cancel();

//                // Act & Assert
//                await AsyncAssert.ThrowsAsync<OperationCanceledException>(
//                    async () => await _converter.ToBytesAsync(_testData, cts.Token));
//            });
//        }

//        [UnityTest]
//        [Description("Verifies that async serialization of large data completes successfully")]
//        public IEnumerator ToBytesAsync_WithLargeData_CompletesSuccessfully()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Arrange
//                var largeData = _builder
//                    .WithName(new string('a', 10000))
//                    .WithNumbers(Enumerable.Range(1, 10000).ToArray())
//                    .Build();

//                // Act
//                var bytes = await _converter.ToBytesAsync(largeData, CancellationToken.None);

//                // Assert
//                Assert.That(bytes.Length, Is.GreaterThan(4096),
//                    "Large data should produce bytes larger than default buffer size");
//            });
//        }

//        #endregion

//        #region Asynchronous Deserialization Tests

//        [UnityTest]
//        [Description("Verifies that valid bytes are correctly deserialized to object asynchronously")]
//        public IEnumerator FromBytesAsync_WithValidBytes_ReturnsDeserializedObject()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Arrange
//                var bytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);

//                // Act
//                var result = await _converter.FromBytesAsync(bytes, CancellationToken.None);

//                // Assert
//                Assert.That(result, Is.EqualTo(_testData));
//            });
//        }

//        [UnityTest]
//        [Description("Verifies that async deserialization of invalid bytes throws DeserializationException")]
//        public IEnumerator FromBytesAsync_WithInvalidBytes_ThrowsDeserializationException()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Arrange
//                var invalidBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

//                // Act & Assert
//                var ex = await AsyncAssert.ThrowsAsync<DeserializationException>(
//                    async () => await _converter.FromBytesAsync(invalidBytes, CancellationToken.None));
//                Assert.That(ex.Message, Does.Contain("Failed to deserialize"));
//                Assert.That(ex.Message, Does.Contain($"Data length: {invalidBytes.Length} bytes"));
//            });
//        }

//        [UnityTest]
//        [Description("Verifies that async deserialization handles cancellation correctly")]
//        public IEnumerator FromBytesAsync_WithCancellation_ThrowsOperationCanceledException()
//        {
//            yield return AsyncTestUtilities.RunAsyncTest(async () =>
//            {
//                // Arrange
//                var bytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);
//                using var cts = new CancellationTokenSource();
//                cts.Cancel();

//                // Act & Assert
//                await AsyncAssert.ThrowsAsync<OperationCanceledException>(
//                    async () => await _converter.FromBytesAsync(bytes, cts.Token));
//            });
//        }

//        #endregion

//        #region Helper Classes

//        [MessagePackObject]
//        public class TestData
//        {
//            [Key(0)]
//            public int Id { get; set; }

//            [Key(1)]
//            public string Name { get; set; }

//            [Key(2)]
//            public DateTime CreatedDate { get; set; }

//            [Key(3)]
//            public int[] Numbers { get; set; }

//            [Key(4)]
//            public TestData Reference { get; set; }

//            public override bool Equals(object obj)
//            {
//                if (obj is TestData other)
//                {
//                    return Id == other.Id &&
//                           Name == other.Name &&
//                           CreatedDate.Equals(other.CreatedDate) &&
//                           (Numbers?.SequenceEqual(other.Numbers) ?? other.Numbers == null);
//                }
//                return false;
//            }

//            public override int GetHashCode()
//            {
//                return HashCode.Combine(Id, Name, CreatedDate, Numbers);
//            }
//        }

//        public class TestDataBuilder
//        {
//            private TestData _data;
//            private readonly DateTime _fixedDate;

//            public TestDataBuilder(DateTime fixedDate)
//            {
//                _fixedDate = fixedDate;
//                Reset();
//            }

//            private void Reset()
//            {
//                _data = new TestData
//                {
//                    Id = 1,
//                    Name = "Test Data",
//                    CreatedDate = _fixedDate,
//                    Numbers = new[] { 1, 2, 3, 4, 5 }
//                };
//            }

//            public TestDataBuilder WithId(int id)
//            {
//                _data.Id = id;
//                return this;
//            }

//            public TestDataBuilder WithName(string name)
//            {
//                _data.Name = name;
//                return this;
//            }

//            public TestDataBuilder WithNumbers(int[] numbers)
//            {
//                _data.Numbers = numbers;
//                return this;
//            }

//            public TestData Build()
//            {
//                var result = _data;
//                Reset();
//                return result;
//            }
//        }

//        #endregion
//    }
//}

using DataBridgeToolKit.Serialization.Core.Exceptions;
using DataBridgeToolKit.Serialization.Implementations.Options;
using DataBridgeToolKit.Tests.Core.Utils;
using MessagePack;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace DataBridgeToolKit.Serialization.Implementations.Converters.Tests
{
    [TestFixture]
    [Category("Serialization")]
    [Category("MessagePack")]
    public class MsgPackDataConverterTests
    {
        private MsgPackDataConverter<TestData> _converter;
        private MsgPackSerializationOptions _options;
        private TestDataBuilder _builder;
        private readonly DateTime _fixedDate = new DateTime(2024, 1, 1);

        private const int SmallDataSize = 100;
        private const int MediumDataSize = 1000;
        private const int LargeDataSize = 10000;
        private const int DefaultTimeout = 5000; // 5 seconds

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _builder = new TestDataBuilder(_fixedDate);
        }

        [SetUp]
        public void SetUp()
        {
            var msgPackOptions = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray);
            _options = new MsgPackSerializationOptions(
                msgPackOptions,
                maxDataSize: LargeDataSize * 4);
            _converter = new MsgPackDataConverter<TestData>(_options);
        }

        [TearDown]
        public void TearDown()
        {
            _converter = null;
            _options = null;
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidOptions_InitializesConverter()
        {
            // Arrange & Act
            var customOptions = new MsgPackSerializationOptions(
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.None));
            var converter = new MsgPackDataConverter<TestData>(customOptions);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(converter, Is.Not.Null);
                Assert.That(converter.ContentType, Is.EqualTo("application/x-msgpack"));
                Assert.That(converter.FileExtension, Is.EqualTo(".msgpack"));
            });
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new MsgPackDataConverter<TestData>(null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        #endregion

        #region Validation Tests

        [Test]
        public void ValidateInputData_WithOversizedData_ThrowsDeserializationException()
        {
            // Arrange
            var oversizedData = new byte[_options.MaxDataSize + 1];

            // Act & Assert
            var ex = Assert.Throws<DeserializationException>(() =>
                _converter.FromBytes(oversizedData));
            Assert.That(ex.Message, Does.Contain("exceeds maximum allowed size"));
        }

        [Test]
        public void ValidateInputData_WithZeroBytes_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyData = Array.Empty<byte>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _converter.FromBytes(emptyData));
            Assert.That(ex.Message, Does.Contain("Input data cannot be null or empty"));
        }

        [TestCase(SmallDataSize)]
        [TestCase(MediumDataSize)]
        public void ValidateInputData_WithValidSizes_DoesNotThrow(int size)
        {
            // Arrange
            var testData = _builder.WithLargeData(size).Build();
            var validBytes = _converter.ToBytes(testData);

            // Act & Assert
            Assert.DoesNotThrow(() => _converter.FromBytes(validBytes),
                $"Should not throw for valid MessagePack data of size {validBytes.Length} bytes");
        }

        [Test]
        public void ValidateInputData_ExceedingMaxSize_ThrowsDeserializationException()
        {
            // Arrange
            var testData = _builder.WithLargeData(_options.MaxDataSize + 1000).Build();
            var largeBytes = _converter.ToBytes(testData);

            // Act & Assert
            var ex = Assert.Throws<DeserializationException>(() =>
                _converter.FromBytes(largeBytes));
            Assert.That(ex.Message, Does.Contain("exceeds maximum allowed size"));
        }

        [Test]
        public void ValidateInputData_WithInvalidData_ThrowsDeserializationException()
        {
            // Arrange
            var invalidData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

            // Act & Assert
            var ex = Assert.Throws<DeserializationException>(() =>
                _converter.FromBytes(invalidData));
            Assert.That(ex.Message, Does.Contain("Failed to deserialize"));
        }

        [Test]
        public void ValidateInputData_WithEmptyArray_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyData = Array.Empty<byte>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _converter.FromBytes(emptyData));
            Assert.That(ex.Message, Does.Contain("Input data cannot be null or empty"));
        }

        #endregion

        #region Synchronous Serialization Tests

        [TestCase(SmallDataSize, TestName = "Small data")]
        [TestCase(MediumDataSize, TestName = "Medium data")]
        [TestCase(LargeDataSize, TestName = "Large data")]
        public void ToBytes_WithDifferentDataSizes_SerializesCorrectly(int size)
        {
            // Arrange
            var testData = _builder
                .WithName(new string('a', size))
                .WithNumbers(Enumerable.Range(1, size).ToArray())
                .Build();

            // Act
            var resultBytes = _converter.ToBytes(testData);
            var deserializedData = _converter.FromBytes(resultBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(deserializedData.Name.Length, Is.EqualTo(testData.Name.Length));
                Assert.That(deserializedData.Numbers.Length, Is.EqualTo(testData.Numbers.Length));
                Assert.That(deserializedData, Is.EqualTo(testData));
            });
        }

        [Test]
        public void ToBytes_WithNullData_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                _converter.ToBytes(null));
            Assert.That(ex.ParamName, Is.EqualTo("data"));
        }

        [Test]
        public void ToBytes_WithCircularReference_ThrowsSerializationException()
        {
            // Arrange
            var circularData = _builder.Build();
            circularData.Reference = circularData;

            // Act & Assert
            var ex = Assert.Throws<SerializationException>(() =>
                _converter.ToBytes(circularData));
            Assert.That(ex.Message, Does.Contain("Failed to serialize"));
        }

        #endregion

        #region Asynchronous Serialization Tests

        private async Task TestDataSerialization(int size)
        {
            // Arrange
            var testData = _builder.WithLargeData(size).Build();
            using var cts = new CancellationTokenSource(DefaultTimeout);

            // Act
            var resultBytes = await _converter.ToBytesAsync(testData, cts.Token);
            var deserializedData = await _converter.FromBytesAsync(resultBytes, cts.Token);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(deserializedData.Id, Is.EqualTo(testData.Id));
                Assert.That(deserializedData.Name, Is.EqualTo(testData.Name));
                Assert.That(deserializedData.Numbers, Is.EqualTo(testData.Numbers));
                Assert.That(deserializedData.CreatedDate, Is.EqualTo(testData.CreatedDate));
            });
        }

        [UnityTest]
        public IEnumerator ToBytesAsync_WithSmallData_SerializesCorrectly()
        {
            yield return AsyncTestUtilities.RunAsyncTest(() => TestDataSerialization(SmallDataSize));
        }

        [UnityTest]
        public IEnumerator ToBytesAsync_WithMediumData_SerializesCorrectly()
        {
            yield return AsyncTestUtilities.RunAsyncTest(() => TestDataSerialization(MediumDataSize));
        }

        [UnityTest]
        public IEnumerator ToBytesAsync_WithCancellation_ThrowsOperationCanceledException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var testData = _builder.WithLargeData(LargeDataSize).Build();
               
                using var cts = new CancellationTokenSource();
                cts.Cancel(); 

                // Act & Assert
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(async () =>
                    await _converter.ToBytesAsync(testData, cts.Token)
                );
            });
        }

        #endregion

        [UnityTest]
        public IEnumerator FromBytesAsync_WithTimeout_ThrowsOperationCanceledException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var testData = _builder.WithLargeData(SmallDataSize).Build();
                var bytes = await _converter.ToBytesAsync(testData, CancellationToken.None);

                using var cts = new CancellationTokenSource();
                cts.Cancel(); 

                // Act & Assert
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(async () =>
                    await _converter.FromBytesAsync(bytes, cts.Token)
                );
            });
        }

        #region Performance Tests

        [Test]
        [TestCase(SmallDataSize)]
        [TestCase(MediumDataSize)]
        [TestCase(LargeDataSize)]
        public void PerformanceTest_Serialization(int size)
        {
            // Arrange
            var testData = _builder.WithLargeData(size).Build();
            var stopwatch = new Stopwatch();
            const int iterations = 100;

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                _converter.ToBytes(testData);
            }
            stopwatch.Stop();

            // Assert
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.That(averageMs, Is.LessThan(100), // 100ms per operation
                $"Average serialization time for size {size}: {averageMs}ms");
        }

        [Test]
        public void ConcurrentOperations_ShouldNotInterfere()
        {
            // Arrange
            const int concurrentTasks = 10;
            var testData = _builder.WithLargeData(MediumDataSize).Build();
            var errors = new ConcurrentBag<Exception>();

            // Act
            Parallel.For(0, concurrentTasks, i =>
            {
                try
                {
                    var bytes = _converter.ToBytes(testData);
                    var deserialized = _converter.FromBytes(bytes);
                    Assert.That(deserialized, Is.EqualTo(testData));
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            });

            // Assert
            Assert.That(errors, Is.Empty, "Concurrent operations should not cause errors");
        }

        #endregion

        #region Helper Classes

        [MessagePackObject]
        public class TestData
        {
            [Key(0)]
            public int Id { get; set; }

            [Key(1)]
            public string Name { get; set; }

            [Key(2)]
            public DateTime CreatedDate { get; set; }

            [Key(3)]
            public int[] Numbers { get; set; }

            [Key(4)]
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

            public TestDataBuilder WithLargeData(int size)
            {
                return WithName(new string('a', size))
                       .WithNumbers(Enumerable.Range(1, size).ToArray());
            }

            public TestData Build()
            {
                var result = _data;
                Reset();
                return result;
            }
        }

        #endregion
    }
}