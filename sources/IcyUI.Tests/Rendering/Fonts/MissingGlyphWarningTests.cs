using Icy.Configuration;
using Icy.Rendering.Fonts;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;

namespace Icy.Tests.Rendering.Fonts
{
    /// <summary>
    /// A character no loaded font has is reported once (ICY-009) instead of vanishing silently.
    /// </summary>
    public class MissingGlyphWarningTests
    {
        // U+E000 is a private-use character: no font ships it.
        private const string Text = "A\uE000";

        private static (IcyConfiguration Configuration, CapturingLoggerFactory Log) Create()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .ConfigureAssets()
                .AddBasicFontSupport()
                .Build();
            configuration.Fonts.ImportFont(configuration.Assets.DefaultAssetContext, "Resources/Airfool.otf");
            return (configuration, log);
        }

        [Fact]
        public void MissingCharacter_IsReportedOnce_WithItsFamilyAndCodepoint()
        {
            var (configuration, log) = Create();
            IFont font = configuration.Fonts.ResolveTextFont("Airfool", 16, FontStyle.Regular)!;

            font.MeasureString(Text, default);
            font.MeasureString(Text, default);

            var warning = Assert.Single(log.Entries, e => e.EventId.Id == 1005);
            Assert.Contains("Airfool", warning.Message, StringComparison.Ordinal);
            Assert.Contains("U+E000", warning.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void SupportedText_LogsNothing()
        {
            var (configuration, log) = Create();
            IFont font = configuration.Fonts.ResolveTextFont("Airfool", 16, FontStyle.Regular)!;

            font.MeasureString("A", default);

            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 1005);
        }
    }
}
