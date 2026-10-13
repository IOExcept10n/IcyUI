using System.Globalization;
using Icy.Configuration;
using Icy.Localization;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Localization
{
    public class ConfigurationLocalizeTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

        private static (IcyConfiguration Configuration, CapturingLoggerFactory Log) Create()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { English, "Menu.Play", "Play" },
                { Russian, "Menu.Play", "Играть" },
                { Russian, "Verb\u0004Open", "Открыть" },
            };
            configuration.Culture = English;
            return (configuration, log);
        }

        [Fact]
        public void Localize_UsesTheConfigurationsCulture_AndContext()
        {
            var (configuration, _) = Create();
            Assert.Equal("Play", configuration.Localize("Menu.Play"));

            configuration.Culture = Russian;

            Assert.Equal("Играть", configuration.Localize("Menu.Play"));
            Assert.Equal("Открыть", configuration.Localize("Open", "Verb"));
        }

        [Fact]
        public void MissingKey_ShowsTheKey_AndWarnsOncePerCulture()
        {
            var (configuration, log) = Create();

            Assert.Equal("Menu.Quit", configuration.Localize("Menu.Quit"));
            configuration.Localize("Menu.Quit");
            configuration.Culture = Russian;
            configuration.Localize("Menu.Quit");

            Assert.Equal(2, log.Entries.Count(e => e.EventId.Id == 4001));
            Assert.Contains("Menu.Quit", log.Entries.First(e => e.EventId.Id == 4001).Message);
        }

        [Fact]
        public void CultureChanged_IsRaisedOnlyForADifferentCulture()
        {
            var (configuration, _) = Create();
            int raised = 0;
            configuration.CultureChanged += (_, _) => raised++;

            configuration.Culture = English;
            configuration.Culture = Russian;
            configuration.Culture = Russian;

            Assert.Equal(1, raised);
        }

        [Fact]
        public void SettingCulture_AfterTheGameChangedCurrentUICulture_StillSwitchesTheTexts()
        {
            CultureInfo previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = English;
            try
            {
                IcyConfiguration configuration = new IcyConfigurationBuilder()
                    .ConfigureRendering(new FakeRenderContext())
                    .ConfigureInput(new FakeInputSystem())
                    .Build();
                configuration.Assets.Localizer = new DictionaryLocalizer
                {
                    { English, "Menu.Play", "Play" },
                    { Russian, "Menu.Play", "Играть" },
                };
                var canvas = new Canvas(configuration);
                var text = new TextBlock();
                canvas.Add(text);
                text.Localize(nameof(TextBlock.Text), "Menu.Play");
                Assert.Equal("Play", text.Text);

                // The game switches .NET's UI culture first (for its own resources), then IcyUI's.
                CultureInfo.CurrentUICulture = Russian;
                configuration.Culture = Russian;

                Assert.Equal("Играть", text.Text);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }

        [Fact]
        public void ResettingToFollowTheUICulture_RaisesOnlyWhenTheCultureDiffers()
        {
            var (configuration, _) = Create();
            CultureInfo previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = English;
            try
            {
                int raised = 0;
                configuration.CultureChanged += (_, _) => raised++;

                configuration.Culture = null;

                Assert.Equal(0, raised);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }
    }
}
