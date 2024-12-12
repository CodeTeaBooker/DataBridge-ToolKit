using DataBridgeToolKit.Tests.Core.Utils;
using MessagePack;
using NUnit.Framework;

namespace DataBridgeToolKit.Serialization.Implementations.Options.Tests
{
    [TestFixture]
    [Category("Serialization")]
    [Category("MessagePack")]
    public class MsgPackSerializationOptionsTests
    {
        private MessagePackSerializerOptions _customSettings;

        [SetUp]
        public void SetUp()
        {
            _customSettings = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(MessagePack.Resolvers.StandardResolver.Instance);
        }

        [TearDown]
        public void TearDown()
        {
            _customSettings = null;
        }

        #region Constructor Tests

        [Test]
        [Description("Verifies constructor initializes with provided custom settings and properties")]
        public void Constructor_WithCustomSettingsAndProperties_InitializesCorrectly()
        {
            int smallDataThreshold = 50000;
            int initialBufferSize = 512;
            int maxDataSize = 50 * 1024 * 1024;

            var options = new MsgPackSerializationOptions(
                _customSettings,
                smallDataThreshold,
                initialBufferSize,
                maxDataSize);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(options.SmallDataThreshold, Is.EqualTo(smallDataThreshold));
                Assert.That(options.InitialBufferSize, Is.EqualTo(initialBufferSize));
                Assert.That(options.MaxDataSize, Is.EqualTo(maxDataSize));
                Assert.That(options.GetSettings(), Is.EqualTo(_customSettings));
            });
        }

        [Test]
        [Description("Verifies constructor with null settings initializes with defaults")]
        public void Constructor_WithNullSettings_InitializesWithDefaults()
        {
            var options = new MsgPackSerializationOptions(null);

            MultipleAssert.Multiple(() =>
            {
                Assert.That(options.GetSettings().Compression, Is.EqualTo(MessagePackSerializerOptions.Standard.Compression));
                Assert.That(options.GetSettings().Resolver, Is.EqualTo(MessagePackSerializerOptions.Standard.Resolver));
            });
        }

        #endregion

        #region Clone Tests

        [Test]
        [Description("Verifies Clone creates an independent copy with identical settings")]
        public void Clone_CreatesDeepCopyWithIdenticalProperties()
        {
            var original = new MsgPackSerializationOptions(
                _customSettings,
                50000,
                512,
                50 * 1024 * 1024);

            var cloned = original.Clone() as MsgPackSerializationOptions;

            MultipleAssert.Multiple(() =>
            {
                Assert.That(cloned, Is.Not.Null);
                Assert.That(cloned, Is.Not.SameAs(original));
                Assert.That(cloned.SmallDataThreshold, Is.EqualTo(original.SmallDataThreshold));
                Assert.That(cloned.InitialBufferSize, Is.EqualTo(original.InitialBufferSize));
                Assert.That(cloned.MaxDataSize, Is.EqualTo(original.MaxDataSize));
            });
        }

        [Test]
        [Description("Verifies that modifying clone settings does not affect original")]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            var original = new MsgPackSerializationOptions(_customSettings, 85000, 256, 100 * 1024 * 1024);
            var cloned = original.Clone() as MsgPackSerializationOptions;

            var modifiedSettings = cloned.GetSettings().WithCompression(MessagePackCompression.None);

            Assert.That(original.GetSettings().Compression, Is.Not.EqualTo(modifiedSettings.Compression));
        }

        [Test]
        [Description("Verifies Clone with complex settings works correctly")]
        public void Clone_WithComplexSettings_ClonesCorrectly()
        {
            var complexSettings = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(MessagePack.Resolvers.StandardResolver.Instance)
                .WithAllowAssemblyVersionMismatch(true)
                .WithOmitAssemblyVersion(true);

            var original = new MsgPackSerializationOptions(complexSettings);

            var cloned = original.Clone() as MsgPackSerializationOptions;

            MultipleAssert.Multiple(() =>
            {
                Assert.That(cloned.GetSettings().Compression, Is.EqualTo(complexSettings.Compression));
                Assert.That(cloned.GetSettings().Resolver, Is.EqualTo(complexSettings.Resolver));
            });
        }

        #endregion

        #region GetSettings Tests

        [Test]
        [Description("Verifies GetSettings returns correct settings instance")]
        public void GetSettings_ReturnsCorrectInstance()
        {
            var options = new MsgPackSerializationOptions(_customSettings);
            Assert.That(options.GetSettings(), Is.TypeOf<MessagePackSerializerOptions>());
        }

        #endregion

        #region Compression Settings Tests

        [Test]
        [Description("Verifies that compression settings are properly configured")]
        public void CompressionSettings_AreProperlyConfigured()
        {
            var compressionTypes = new[]
            {
                MessagePackCompression.None,
                MessagePackCompression.Lz4Block,
                MessagePackCompression.Lz4BlockArray
            };

            foreach (var compression in compressionTypes)
            {
                var settings = MessagePackSerializerOptions.Standard.WithCompression(compression);
                var options = new MsgPackSerializationOptions(settings);

                Assert.That(options.GetSettings().Compression, Is.EqualTo(compression));
            }
        }

        #endregion
    }
}
