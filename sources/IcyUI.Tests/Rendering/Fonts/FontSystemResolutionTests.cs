using Icy.Configuration;
using Icy.Rendering.Fonts;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Microsoft.Extensions.Logging;

namespace Icy.Tests.Rendering.Fonts
{
    /// <summary>
    /// Covers how text finds its font: the builder's font steps, lazy system-font indexing, cached misses, and the
    /// warnings logged when nothing resolves.
    /// </summary>
    public class FontSystemResolutionTests
    {
        private static IConfigurationBuilder NewBuilder(CapturingLoggerFactory? log = null)
        {
            IConfigurationBuilder builder = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureAssets()
                .AddBasicFontSupport();
            if (log != null)
                builder.ConfigureLogging(log);
            return builder;
        }

        private static int Count(CapturingLoggerFactory log, int eventId) => log.Entries.Count(e => e.EventId.Id == eventId);

        [Fact]
        public void ConfigureFonts_StepsApplyInBuild()
        {
            IcyConfiguration configuration = NewBuilder().ConfigureFonts().WithDefaultFontFamily("Airfool").Build();

            Assert.Equal("Airfool", configuration.Fonts.DefaultFontFamily);
        }

        [Fact]
        public void WithDefaultFontFamily_LastStepWins()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureFonts().WithDefaultFontFamily("Segoe UI")
                .ConfigureFonts().WithDefaultFontFamily("Airfool")
                .Build();

            Assert.Equal("Airfool", configuration.Fonts.DefaultFontFamily);
        }

        [Fact]
        public void ImportFont_Step_MakesTheFamilyResolvable()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureFonts().ImportFont("Resources/Airfool.otf").WithDefaultFontFamily("Airfool")
                .Build();

            Assert.NotNull(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));
        }

        [Fact]
        public void GetOrLoad_UnknownFamily_WarnsOnceAndReturnsFallback()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log).Build();

            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular)));
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular)));
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 20, FontStyle.Regular)));

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 1001);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Contains("No Such Font", entry.Message);
            Assert.Equal("Icy.Rendering.Fonts.FontSystem", entry.Category);
        }

        [Fact]
        public void GetOrLoad_UnknownFamily_IsNotRetried()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log).ConfigureFonts().UseSystemFonts().Build();

            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));
            int builds = configuration.Fonts.SystemFontIndexBuilds;
            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));
            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));

            Assert.Equal(1, builds);
            Assert.Equal(1, configuration.Fonts.SystemFontIndexBuilds);

            // Each retry would look the family up again and warn again: one warning means one lookup.
            Assert.Single(log.Entries, e => e.EventId.Id == 1001);
        }

        [Fact]
        public void ImportFont_AfterMiss_ResolvesTheFamily()
        {
            IcyConfiguration configuration = NewBuilder().Build();
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("Airfool", 16, FontStyle.Regular)));

            configuration.Fonts.ImportFont(configuration.Assets.DefaultAssetContext, "Resources/Airfool.otf");

            Assert.NotNull(configuration.Fonts.GetOrLoad(new FontInfo("Airfool", 16, FontStyle.Regular)));
        }

        [Fact]
        public void EnableSystemFonts_DoesNotIndexUntilFirstMiss()
        {
            IcyConfiguration configuration = NewBuilder().ConfigureFonts().UseSystemFonts().Build();

            Assert.True(configuration.Fonts.AreSystemFontsEnabled);
            Assert.Equal(0, configuration.Fonts.SystemFontIndexBuilds);

            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));
            configuration.Fonts.GetOrLoad(new FontInfo("Another Missing Font", 16, FontStyle.Regular));

            Assert.Equal(1, configuration.Fonts.SystemFontIndexBuilds);
        }

        [Fact]
        public void ResolveTextFont_NoDefaultFamilyAndNoFallback_WarnsOnce()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log).Build();

            Assert.Null(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));
            Assert.Null(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));

            Assert.Equal(1, Count(log, 1004));
        }

        [WindowsFact]
        public void EngineDefaultFontSteps_ResolveSegoeUi()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log)
                .ConfigureFonts().UseSystemFonts().WithDefaultFontFamily("Segoe UI")
                .Build();

            IFont? font = configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Bold);

            Assert.NotNull(font);
            Assert.Equal("Segoe UI", font.Info.Family);
            Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Warning);
        }
    }
}
