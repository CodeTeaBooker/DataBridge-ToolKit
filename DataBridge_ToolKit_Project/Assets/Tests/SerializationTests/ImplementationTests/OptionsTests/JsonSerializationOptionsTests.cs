using DataBridgeToolKit.Tests.Core.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace DataBridgeToolKit.Serialization.Implementations.Options.Tests
{
    [TestFixture]
    [Category("Serialization")]
    public class JsonSerializationOptionsTests
    {
        private JsonSerializerSettings _customSettings;

        [SetUp]
        public void SetUp()
        {
            _customSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        [TearDown]
        public void TearDown()
        {
            _customSettings = null;
        }

        #region Constructor Tests

        [Test]
        [Description("Verifies that constructor with custom settings initializes correctly")]
        public void Constructor_WithCustomSettings_InitializesCorrectly()
        {
            // Arrange & Act
            var options = new JsonSerializationOptions(_customSettings);
            var resultSettings = options.GetSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(resultSettings, Is.Not.Null);
                Assert.That(resultSettings.Formatting, Is.EqualTo(_customSettings.Formatting));
                Assert.That(resultSettings.NullValueHandling, Is.EqualTo(_customSettings.NullValueHandling));
                Assert.That(resultSettings.DateFormatHandling, Is.EqualTo(_customSettings.DateFormatHandling));
                Assert.That(resultSettings.ReferenceLoopHandling, Is.EqualTo(_customSettings.ReferenceLoopHandling));
            });
        }

        [Test]
        [Description("Verifies that constructor with null settings initializes with default values")]
        public void Constructor_WithNullSettings_InitializesWithDefaults()
        {
            // Arrange & Act
            var options = new JsonSerializationOptions(null);
            var settings = options.GetSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(settings, Is.Not.Null, "Settings should not be null");
                Assert.That(settings.Formatting, Is.EqualTo(Formatting.None),
                    "Default formatting should be None");
                Assert.That(settings.NullValueHandling, Is.EqualTo(NullValueHandling.Include),
                    "Default null value handling should be Include");
                Assert.That(settings.ReferenceLoopHandling, Is.EqualTo(ReferenceLoopHandling.Ignore),
                    "Default reference loop handling should be Ignore");
                Assert.That(settings.TypeNameHandling, Is.EqualTo(TypeNameHandling.None),
                    "Default type name handling should be None");
            });
        }

        #endregion

        #region Clone Tests

        [Test]
        [Description("Verifies that Clone creates a deep copy with identical settings")]
        public void Clone_CreatesDeepCopyWithIdenticalSettings()
        {
            // Arrange
            var original = new JsonSerializationOptions(_customSettings);

            // Act
            var cloned = original.Clone() as JsonSerializationOptions;
            var originalSettings = original.GetSettings();
            var clonedSettings = cloned.GetSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(cloned, Is.Not.Null);
                Assert.That(cloned, Is.Not.SameAs(original),
                    "Cloned instance should be a different object");
                Assert.That(clonedSettings, Is.Not.SameAs(originalSettings),
                    "Settings should be cloned, not referenced");
                Assert.That(clonedSettings.Formatting, Is.EqualTo(originalSettings.Formatting));
                Assert.That(clonedSettings.NullValueHandling, Is.EqualTo(originalSettings.NullValueHandling));
                Assert.That(clonedSettings.DateFormatHandling, Is.EqualTo(originalSettings.DateFormatHandling));
                Assert.That(clonedSettings.ReferenceLoopHandling, Is.EqualTo(originalSettings.ReferenceLoopHandling));
            });
        }

        [Test]
        [Description("Verifies that modifying cloned settings does not affect original")]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            // Arrange
            var original = new JsonSerializationOptions(_customSettings);
            var cloned = original.Clone() as JsonSerializationOptions;

            // Act - Modify cloned settings
            var clonedSettings = cloned.GetSettings();
            clonedSettings.Formatting = clonedSettings.Formatting == Formatting.None
                ? Formatting.Indented
                : Formatting.None;

            // Assert
            var originalSettings = original.GetSettings();
            Assert.That(originalSettings.Formatting, Is.Not.EqualTo(clonedSettings.Formatting),
                "Original settings should remain unchanged when clone is modified");
        }

        [Test]
        [Description("Verifies that Clone with complex settings works correctly")]
        public void Clone_WithComplexSettings_ClonesCorrectly()
        {
            // Arrange
            var complexSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                FloatFormatHandling = FloatFormatHandling.DefaultValue
            };
            var original = new JsonSerializationOptions(complexSettings);

            // Act
            var cloned = original.Clone() as JsonSerializationOptions;

            // Assert
            var clonedSettings = cloned.GetSettings();
            MultipleAssert.Multiple(() =>
            {
                Assert.That(cloned, Is.Not.Null);
                Assert.That(clonedSettings.Formatting, Is.EqualTo(complexSettings.Formatting));
                Assert.That(clonedSettings.DateFormatHandling, Is.EqualTo(complexSettings.DateFormatHandling));
                Assert.That(clonedSettings.DefaultValueHandling, Is.EqualTo(complexSettings.DefaultValueHandling));
                Assert.That(clonedSettings.NullValueHandling, Is.EqualTo(complexSettings.NullValueHandling));
                Assert.That(clonedSettings.ReferenceLoopHandling, Is.EqualTo(complexSettings.ReferenceLoopHandling));
                Assert.That(clonedSettings.PreserveReferencesHandling,
                    Is.EqualTo(complexSettings.PreserveReferencesHandling));
                Assert.That(clonedSettings.TypeNameHandling, Is.EqualTo(complexSettings.TypeNameHandling));
                Assert.That(clonedSettings.MetadataPropertyHandling,
                    Is.EqualTo(complexSettings.MetadataPropertyHandling));
                Assert.That(clonedSettings.DateParseHandling, Is.EqualTo(complexSettings.DateParseHandling));
                Assert.That(clonedSettings.FloatFormatHandling, Is.EqualTo(complexSettings.FloatFormatHandling));
            });
        }

        #endregion

        #region GetSettings Tests

        [Test]
        [Description("Verifies that GetSettings returns correct settings instance")]
        public void GetSettings_ReturnsCorrectInstance()
        {
            // Arrange
            var options = new JsonSerializationOptions(_customSettings);

            // Act
            var settings = options.GetSettings();

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Is.TypeOf<JsonSerializerSettings>());
        }

        [Test]
        [Description("Verifies that GetSettings returns new instance each time")]
        public void GetSettings_ReturnsNewInstance()
        {
            // Arrange
            var options = new JsonSerializationOptions(_customSettings);

            // Act
            var firstCall = options.GetSettings();
            var secondCall = options.GetSettings();

            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(firstCall, Is.Not.Null);
                Assert.That(secondCall, Is.Not.Null);
                Assert.That(secondCall, Is.Not.SameAs(firstCall),
                    "Multiple calls to GetSettings should return different instances");
                Assert.That(secondCall.Formatting, Is.EqualTo(firstCall.Formatting),
                    "Settings values should be identical");
                Assert.That(secondCall.NullValueHandling, Is.EqualTo(firstCall.NullValueHandling),
                    "Settings values should be identical");
                Assert.That(secondCall.DateFormatHandling, Is.EqualTo(firstCall.DateFormatHandling),
                    "Settings values should be identical");
            });
        }

        #endregion

        #region Error Handling Tests

        [Test]
        [Description("Verifies that Clone handles serialization errors correctly")]
        public void Clone_WithSerializationError_ThrowsInvalidOperationException()
        {
            // Arrange
            var problematicSettings = new JsonSerializerSettings
            {
                Converters = new[] { new ProblemJsonConverter() }
            };
            var options = new JsonSerializationOptions(problematicSettings);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => options.Clone());
            Assert.That(ex.Message, Does.Contain("Failed to clone JsonSerializerSettings"));
        }

        private class ProblemJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => throw new Exception("Forced error");
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer) => throw new NotImplementedException();
            public override void WriteJson(JsonWriter writer, object value,
                JsonSerializer serializer) => throw new NotImplementedException();
        }

        #endregion
    }
}