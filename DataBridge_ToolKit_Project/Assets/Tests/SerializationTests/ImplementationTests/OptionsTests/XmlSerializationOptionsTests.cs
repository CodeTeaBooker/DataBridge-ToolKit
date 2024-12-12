using DataBridgeToolKit.Tests.Core.Utils;
using NUnit.Framework;
using System.Text;
using System.Xml;

namespace DataBridgeToolKit.Serialization.Implementations.Options.Tests
{
    [TestFixture]
    [Category("Serialization")]
    public class XmlSerializationOptionsTests
    {
        private XmlWriterSettings _customWriterSettings;
        private XmlReaderSettings _customReaderSettings;

        [SetUp]
        public void SetUp()
        {
            _customWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.Unicode,
                Async = true,
                OmitXmlDeclaration = true
            };

            _customReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                Async = true,
                DtdProcessing = DtdProcessing.Prohibit
            };
            _customReaderSettings.XmlResolver = null;
        }

        [TearDown]
        public void TearDown()
        {
            _customWriterSettings = null;
            _customReaderSettings = null;
        }

        #region Constructor Tests

        [Test]
        [Description("Verifies that constructor with custom settings initializes correctly")]
        public void Constructor_WithCustomSettings_InitializesCorrectly()
        {
            // Arrange & Act
            var options = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);
            var resultWriterSettings = options.GetWriterSettings();
            var resultReaderSettings = options.GetReaderSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultWriterSettings, Is.Not.Null);
                Assert.That(resultReaderSettings, Is.Not.Null);

                // Writer settings assertions
                Assert.That(resultWriterSettings.Indent, Is.EqualTo(_customWriterSettings.Indent));
                Assert.That(resultWriterSettings.Encoding, Is.EqualTo(_customWriterSettings.Encoding));
                Assert.That(resultWriterSettings.Async, Is.EqualTo(_customWriterSettings.Async));
                Assert.That(resultWriterSettings.OmitXmlDeclaration, Is.EqualTo(_customWriterSettings.OmitXmlDeclaration));

                // Reader settings assertions
                Assert.That(resultReaderSettings.IgnoreWhitespace, Is.EqualTo(_customReaderSettings.IgnoreWhitespace));
                Assert.That(resultReaderSettings.IgnoreComments, Is.EqualTo(_customReaderSettings.IgnoreComments));
                Assert.That(resultReaderSettings.Async, Is.EqualTo(_customReaderSettings.Async));
                Assert.That(resultReaderSettings.DtdProcessing, Is.EqualTo(_customReaderSettings.DtdProcessing));
            });
        }

        [Test]
        [Description("Verifies that constructor with null settings initializes with default values")]
        public void Constructor_WithNullSettings_InitializesWithDefaults()
        {
            // Arrange & Act
            var options = new XmlSerializationOptions(null, null);
            var writerSettings = options.GetWriterSettings();
            var readerSettings = options.GetReaderSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                // Writer settings default values
                Assert.That(writerSettings, Is.Not.Null, "Writer settings should not be null");
                Assert.That(writerSettings.Indent, Is.True, "Default indent should be True");
                Assert.That(writerSettings.Async, Is.True, "Default async should be True");
                Assert.That(writerSettings.Encoding, Is.EqualTo(new UTF8Encoding(false)),
                    "Default encoding should be UTF8 without BOM");

                // Reader settings default values
                Assert.That(readerSettings, Is.Not.Null, "Reader settings should not be null");
                Assert.That(readerSettings.Async, Is.True, "Default async should be True");
                Assert.That(readerSettings.IgnoreWhitespace, Is.True, "Default ignore whitespace should be True");
                Assert.That(readerSettings.DtdProcessing, Is.EqualTo(DtdProcessing.Prohibit),
                    "Default DTD processing should be Prohibit");
            });
        }

        #endregion

        #region Clone Tests

        [Test]
        [Description("Verifies that Clone creates a deep copy with identical settings")]
        public void Clone_CreatesDeepCopyWithIdenticalSettings()
        {
            // Arrange
            var original = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);

            // Act
            var cloned = original.Clone() as XmlSerializationOptions;
            var originalWriterSettings = original.GetWriterSettings();
            var originalReaderSettings = original.GetReaderSettings();
            var clonedWriterSettings = cloned.GetWriterSettings();
            var clonedReaderSettings = cloned.GetReaderSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(cloned, Is.Not.Null);
                Assert.That(cloned, Is.Not.SameAs(original),
                    "Cloned instance should be a different object");

                // Writer settings assertions
                Assert.That(clonedWriterSettings, Is.Not.SameAs(originalWriterSettings),
                    "Writer settings should be cloned, not referenced");
                Assert.That(clonedWriterSettings.Indent, Is.EqualTo(originalWriterSettings.Indent));
                Assert.That(clonedWriterSettings.Encoding, Is.EqualTo(originalWriterSettings.Encoding));
                Assert.That(clonedWriterSettings.Async, Is.EqualTo(originalWriterSettings.Async));

                // Reader settings assertions
                Assert.That(clonedReaderSettings, Is.Not.SameAs(originalReaderSettings),
                    "Reader settings should be cloned, not referenced");
                Assert.That(clonedReaderSettings.IgnoreWhitespace, Is.EqualTo(originalReaderSettings.IgnoreWhitespace));
                Assert.That(clonedReaderSettings.DtdProcessing, Is.EqualTo(originalReaderSettings.DtdProcessing));
                Assert.That(clonedReaderSettings.Async, Is.EqualTo(originalReaderSettings.Async));
            });
        }

        [Test]
        [Description("Verifies that modifying cloned settings does not affect original")]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            // Arrange
            var original = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);
            var cloned = original.Clone() as XmlSerializationOptions;

            // Act - Modify cloned settings
            var clonedWriterSettings = cloned.GetWriterSettings();
            clonedWriterSettings.Indent = !clonedWriterSettings.Indent;

            var clonedReaderSettings = cloned.GetReaderSettings();
            clonedReaderSettings.IgnoreWhitespace = !clonedReaderSettings.IgnoreWhitespace;

            // Assert
            var originalWriterSettings = original.GetWriterSettings();
            var originalReaderSettings = original.GetReaderSettings();

            MultipleAssert.Multiple(() =>
            {
                Assert.That(originalWriterSettings.Indent, Is.Not.EqualTo(clonedWriterSettings.Indent),
                    "Original writer settings should remain unchanged when clone is modified");
                Assert.That(originalReaderSettings.IgnoreWhitespace, Is.Not.EqualTo(clonedReaderSettings.IgnoreWhitespace),
                    "Original reader settings should remain unchanged when clone is modified");
            });
        }

        #endregion

        #region GetSettings Tests

        [Test]
        [Description("Verifies that GetWriterSettings returns correct settings instance")]
        public void GetWriterSettings_ReturnsCorrectInstance()
        {
            // Arrange
            var options = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);

            // Act
            var settings = options.GetWriterSettings();

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Is.TypeOf<XmlWriterSettings>());
        }

        [Test]
        [Description("Verifies that GetReaderSettings returns correct settings instance")]
        public void GetReaderSettings_ReturnsCorrectInstance()
        {
            // Arrange
            var options = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);

            // Act
            var settings = options.GetReaderSettings();

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Is.TypeOf<XmlReaderSettings>());
        }

        [Test]
        [Description("Verifies that each call to GetWriterSettings returns new instance")]
        public void GetWriterSettings_ReturnsNewInstance()
        {
            // Arrange
            var options = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);

            // Act
            var firstCall = options.GetWriterSettings();
            var secondCall = options.GetWriterSettings();

            // Assert
            Assert.That(secondCall, Is.Not.SameAs(firstCall),
                "Multiple calls to GetWriterSettings should return different instances");
        }

        [Test]
        [Description("Verifies that each call to GetReaderSettings returns new instance")]
        public void GetReaderSettings_ReturnsNewInstance()
        {
            // Arrange
            var options = new XmlSerializationOptions(_customWriterSettings, _customReaderSettings);

            // Act
            var firstCall = options.GetReaderSettings();
            var secondCall = options.GetReaderSettings();

            // Assert
            Assert.That(secondCall, Is.Not.SameAs(firstCall),
                "Multiple calls to GetReaderSettings should return different instances");
        }

        [Test]
        [Description("Verifies that XML security settings are properly configured")]
        public void SecuritySettings_AreProperlyConfigured()
        {
            // Arrange & Act
            var options = new XmlSerializationOptions();
            var readerSettings = options.GetReaderSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(readerSettings.DtdProcessing, Is.EqualTo(DtdProcessing.Prohibit),
                    "DTD processing should be prohibited for security");
            });
        }

        #endregion
    }
}