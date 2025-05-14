using DataBridgeToolKit.Serialization.Core.Exceptions;
using DataBridgeToolKit.Serialization.Implementations.Options;
using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using UnityEngine.TestTools;

namespace DataBridgeToolKit.Serialization.Implementations.Converters.Tests
{
    [TestFixture]
    [Category("Serialization")]
    [Category("XML")]
    public class XmlDataConverterTests
    {
        private XmlDataConverter<TestData> _converter;
        private XmlSerializationOptions _options;
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
            _options = new XmlSerializationOptions(CreateDefaultWriterSettings(), CreateDefaultReaderSettings());
            _converter = new XmlDataConverter<TestData>(_options);
            _testData = _builder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _converter = null;
            _options = null;
            _testData = null;
        }

        private XmlWriterSettings CreateDefaultWriterSettings() => new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(false),
            Async = true
        };

        private XmlReaderSettings CreateDefaultReaderSettings() => new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        #region Constructor Tests

        [Test]
        [Description("Verifies that the converter is properly initialized with valid options")]
        public void Constructor_WithValidOptions_InitializesConverter()
        {
            // Arrange & Act
            var converter = new XmlDataConverter<TestData>(_options);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(converter, Is.Not.Null);
                Assert.That(converter.ContentType, Is.EqualTo("application/xml"));
                Assert.That(converter.FileExtension, Is.EqualTo(".xml"));
            });
        }

        [Test]
        [Description("Verifies that constructor throws ArgumentNullException when options are null")]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new XmlDataConverter<TestData>(null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        #endregion

        #region ContentType Tests

        [Test]
        [Description("Verifies ContentType and FileExtension return correct values")]
        public void ContentType_ReturnsCorrectValue()
        {
            MultipleAssert.Multiple(() =>
            {
                Assert.That(_converter.ContentType, Is.EqualTo("application/xml"), "ContentType should be application/xml");
                Assert.That(_converter.FileExtension, Is.EqualTo(".xml"), "FileExtension should be .xml");
            });
        }

        #endregion

        #region Synchronous Serialization Tests

        [Test]
        [Description("Verifies that valid data is correctly serialized to bytes")]
        public void ToBytes_WithValidData_ReturnsCorrectBytes()
        {
            // Act
            var resultBytes = _converter.ToBytes(_testData);
            var resultXml = Encoding.UTF8.GetString(resultBytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultBytes, Is.Not.Null);
                Assert.That(resultXml, Does.Contain($"<Id>{_testData.Id}</Id>"));
                Assert.That(resultXml, Does.Contain($"<Name>{_testData.Name}</Name>"));
                Assert.That(resultXml, Does.Contain("<CreatedDate>"));
                Assert.That(resultXml, Does.Contain("<Numbers>"));
            });
        }

        [Test]
        [Description("Verifies that serializing empty object works correctly")]
        public void ToBytes_WithEmptyObject_SerializesCorrectly()
        {
            // Arrange
            var emptyData = _builder
                .WithName("")
                .WithNumbers(Array.Empty<int>())
                .Build();

            // Act
            var bytes = _converter.ToBytes(emptyData);
            var resultXml = Encoding.UTF8.GetString(bytes);

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(bytes.Length, Is.GreaterThan(0));
                Assert.That(resultXml, Does.Contain("<Name />"));
                Assert.That(resultXml, Does.Contain("<Numbers />"));
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
            Assert.That(ex.ParamName, Is.EqualTo("data"));
        }

        [Test]
        [Description("Verifies that serialization handles special XML characters correctly")]
        public void ToBytes_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var specialData = _builder
                .WithName("Test & Data < > \" '")
                .Build();

            // Act
            var resultBytes = _converter.ToBytes(specialData);
            var resultXml = Encoding.UTF8.GetString(resultBytes);

            // Assert
            Assert.That(resultXml, Does.Contain("Test &amp; Data &lt; &gt; \" '"));
        }

        #endregion

        #region Synchronous Deserialization Tests

        [Test]
        [Description("Verifies that valid bytes are correctly deserialized to object")]
        public void FromBytes_WithValidBytes_ReturnsDeserializedObject()
        {
            // Arrange
            var bytes = _converter.ToBytes(_testData);

            // Act
            var result = _converter.FromBytes(bytes);

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
            Assert.That(ex.ParamName, Is.EqualTo("data"), "Parameter name should be 'data'");
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
        [Description("Verifies that deserializing invalid XML throws DeserializationException")]
        public void FromBytes_WithInvalidXml_ThrowsDeserializationException()
        {
            // Arrange
            var invalidXmlBytes = Encoding.UTF8.GetBytes("<invalid>");

            // Act & Assert
            var ex = Assert.Throws<DeserializationException>(() => _converter.FromBytes(invalidXmlBytes));
            Assert.That(ex.Message, Does.Contain("Invalid operation during deserialization"));
        }

        #endregion

        #region Asynchronous Serialization Tests

        [UnityTest]
        [Description("Verifies that valid data is correctly serialized to bytes asynchronously")]
        public IEnumerator ToBytesAsync_WithValidData_ReturnsCorrectBytes()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Act
                var resultBytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);
                var resultXml = Encoding.UTF8.GetString(resultBytes);

                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(resultBytes, Is.Not.Null, "Result bytes should not be null");
                    Assert.That(resultXml, Does.Contain($"<Id>{_testData.Id}</Id>"));
                    Assert.That(resultXml, Does.Contain($"<Name>{_testData.Name}</Name>"));
                    Assert.That(resultXml, Does.Contain("<CreatedDate>"));
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
                    async () => await _converter.ToBytesAsync(_testData, cts.Token),
                    "Operation should be canceled");
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
                Assert.That(ex.ParamName, Is.EqualTo("data"), "Parameter name should be 'data'");
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
                var bytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);

                // Act
                var result = await _converter.FromBytesAsync(bytes, CancellationToken.None);

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
                var bytes = await _converter.ToBytesAsync(_testData, CancellationToken.None);
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                // Act & Assert
                await AsyncAssert.ThrowsAsync<OperationCanceledException>(
                    async () => await _converter.FromBytesAsync(bytes, cts.Token),
                    "Operation should be canceled");
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

        [UnityTest]
        [Description("Verifies that async deserialization of invalid XML throws DeserializationException")]
        public IEnumerator FromBytesAsync_WithInvalidXml_ThrowsDeserializationException()
        {
            yield return AsyncTestUtilities.RunAsyncTest(async () =>
            {
                // Arrange
                var invalidXml = "<Invalid>";
                var bytes = Encoding.UTF8.GetBytes(invalidXml);

                // Act & Assert
                await AsyncAssert.ThrowsAsync<DeserializationException>(
                    async () => await _converter.FromBytesAsync(bytes, CancellationToken.None),
                    "Error during async deserialization");
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