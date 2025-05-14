using DataBridgeToolKit.Serialization.Core.Enums;
using DataBridgeToolKit.Serialization.Implementations.Options;
using NUnit.Framework;
using System;

namespace DataBridgeToolKit.Serialization.Core.Factories.Tests
{
    [TestFixture]
    public class SerializationOptionsFactoryTests
    {
        private JsonSerializationOptions _defaultJsonOptions;
        private XmlSerializationOptions _defaultXmlOptions;
        private MsgPackSerializationOptions _defaultMsgPackOptions;
        private SerializationOptionsFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _defaultJsonOptions = new JsonSerializationOptions();
            _defaultXmlOptions = new XmlSerializationOptions();
            _defaultMsgPackOptions = new MsgPackSerializationOptions();

            _factory = new SerializationOptionsFactory(_defaultJsonOptions, _defaultXmlOptions, _defaultMsgPackOptions);
        }

        #region Constructor Tests

        [Test]
        [Description("Verifies the constructor correctly uses default serialization options.")]
        public void Constructor_WithNullOptions_UsesDefaultInstances()
        {
            var factory = new SerializationOptionsFactory();
            Assert.That(factory.CreateOptions(SerializationFormat.Json), Is.Not.Null);
            Assert.That(factory.CreateOptions(SerializationFormat.Xml), Is.Not.Null);
            Assert.That(factory.CreateOptions(SerializationFormat.MsgPack), Is.Not.Null);
        }

        [Test]
        [Description("Verifies the constructor uses provided custom serialization options.")]
        public void Constructor_WithCustomOptions_UsesProvidedInstances()
        {
            var jsonOptions = new JsonSerializationOptions();
            var xmlOptions = new XmlSerializationOptions();
            var msgPackOptions = new MsgPackSerializationOptions();

            var factory = new SerializationOptionsFactory(jsonOptions, xmlOptions, msgPackOptions);

            var createdJsonOptions = (JsonSerializationOptions)factory.CreateOptions(SerializationFormat.Json);
            var createdXmlOptions = (XmlSerializationOptions)factory.CreateOptions(SerializationFormat.Xml);
            var createdMsgPackOptions = (MsgPackSerializationOptions)factory.CreateOptions(SerializationFormat.MsgPack);

            Assert.That(createdJsonOptions, Is.Not.Null);
            Assert.That(createdXmlOptions, Is.Not.Null);
            Assert.That(createdMsgPackOptions, Is.Not.Null);
        }

        #endregion

        #region CreateOptions Tests

        [Test]
        [Description("Verifies CreateOptions returns the correct cloned object for Json format.")]
        public void CreateOptions_WithJsonFormat_ReturnsJsonOptionsClone()
        {
            var options = _factory.CreateOptions(SerializationFormat.Json);
            Assert.That(options, Is.InstanceOf<JsonSerializationOptions>());
            Assert.That(options, Is.Not.SameAs(_defaultJsonOptions));
        }

        [Test]
        [Description("Verifies CreateOptions returns the correct cloned object for Xml format.")]
        public void CreateOptions_WithXmlFormat_ReturnsXmlOptionsClone()
        {
            var options = _factory.CreateOptions(SerializationFormat.Xml);
            Assert.That(options, Is.InstanceOf<XmlSerializationOptions>());
            Assert.That(options, Is.Not.SameAs(_defaultXmlOptions));
        }

        [Test]
        [Description("Verifies CreateOptions returns the correct cloned object for MsgPack format.")]
        public void CreateOptions_WithMsgPackFormat_ReturnsMsgPackOptionsClone()
        {
            var options = _factory.CreateOptions(SerializationFormat.MsgPack);
            Assert.That(options, Is.InstanceOf<MsgPackSerializationOptions>());
            Assert.That(options, Is.Not.SameAs(_defaultMsgPackOptions));
        }

        [Test]
        [Description("Verifies CreateOptions throws an ArgumentException for invalid formats.")]
        public void CreateOptions_WithInvalidFormat_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _factory.CreateOptions((SerializationFormat)999);
            });
        }

        #endregion

        #region Edge Case Tests

        [Test]
        [Description("Verifies that clones of default options are independent.")]
        public void CreateOptions_ClonedOptions_AreIndependent()
        {
            var jsonOptions = (JsonSerializationOptions)_factory.CreateOptions(SerializationFormat.Json);
            var newJsonOptions = (JsonSerializationOptions)_factory.CreateOptions(SerializationFormat.Json);

            Assert.That(jsonOptions, Is.Not.SameAs(newJsonOptions));
        }

        [Test]
        [Description("Verifies the cloning logic of the CreateOptions method.")]
        public void CreateOptions_ClonedObjects_HaveSeparateMemory()
        {
            var originalOptions = _factory.CreateOptions(SerializationFormat.Json);
            var clonedOptions = _factory.CreateOptions(SerializationFormat.Json);

            Assert.That(originalOptions, Is.Not.Null);
            Assert.That(clonedOptions, Is.Not.Null);
            Assert.That(originalOptions, Is.Not.SameAs(clonedOptions));
        }


        #endregion
    }
}
