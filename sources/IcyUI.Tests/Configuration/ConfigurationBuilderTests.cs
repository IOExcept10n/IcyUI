using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Configuration
{
    public class ConfigurationBuilderTests
    {
        private static IConfigurationBuilder NewBuilder() =>
            new IcyConfigurationBuilder().ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem());

        [Fact]
        public void AfterBuild_StepsRunInDeclarationOrderOnTheBuiltConfiguration()
        {
            var calls = new List<string>();
            IcyConfiguration? seen = null;

            IcyConfiguration built = NewBuilder()
                .AfterBuild(c => { calls.Add("first"); seen = c; })
                .AfterBuild(_ => calls.Add("second"))
                .Build();

            Assert.Equal(["first", "second"], calls);
            Assert.Same(built, seen);
        }

        [Fact]
        public void UseDefaultTheme_OnTheBuilder_LoadsTheTheme()
        {
            IcyConfiguration configuration = NewBuilder().UseDefaultTheme().Build();

            Assert.NotNull(configuration.Theme.Theme);
        }

        [Fact]
        public void ConfigureTheme_Action_RunsAfterEarlierDeferredTheme()
        {
            var custom = new ResourceDictionary();

            IcyConfiguration configuration = NewBuilder()
                .UseDefaultTheme()
                .ConfigureTheme(t => t.Theme = custom)
                .Build();

            Assert.Same(custom, configuration.Theme.Theme);
        }

        [Fact]
        public void ThemeSetThroughTheSection_IsNotReplacedByAnEarlierDefaultTheme()
        {
            var custom = new ResourceDictionary();
            IConfigurationBuilder builder = NewBuilder();
            builder.UseDefaultTheme();
            builder.ConfigureTheme().Theme.Theme = custom;

            IcyConfiguration configuration = builder.Build();

            Assert.Same(custom, configuration.Theme.Theme);
        }

        [Fact]
        public void SectionLambdas_ReachTheirSections()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureGestures(g => g.Slop = 42f)
                .ConfigureDiagnostics(d => Assert.NotNull(d))
                .ConfigureMarkup(m => Assert.NotNull(m))
                .ConfigureAssets(a => Assert.NotNull(a))
                .ConfigureTypes(t => Assert.NotNull(t))
                .ConfigureRendering(r => Assert.NotNull(r))
                .ConfigureInput(i => Assert.NotNull(i))
                .ConfigureScaling(s => Assert.NotNull(s))
                .Build();

            Assert.Equal(42f, configuration.Input.Events.Gestures.Settings.Slop);
        }

        [Fact]
        public void AddMarkupSupport_OnTheBuilder_LetsAssetsLoadMarkup()
        {
            IcyConfiguration configuration = NewBuilder().AddMarkupSupport().Build();

            UIElement page = configuration.Assets.Load<UIElement>("Resources/TestPage.xml");

            Assert.NotNull(page);
        }
    }
}
