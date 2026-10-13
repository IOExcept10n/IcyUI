using System.Globalization;
using Icy.Configuration;
using Icy.Localization;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Localization
{
    public class LocalizeTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

        private static (IcyConfiguration Configuration, Canvas Canvas) Create()
        {
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .Build();
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { English, "Menu.Play", "Play" },
                { Russian, "Menu.Play", "Играть" },
                { English, "Menu.Title", "Main menu" },
                { Russian, "Menu.Title", "Главное меню" },
                { Russian, "Verb\u0004Open", "Открыть" },
            };
            configuration.Culture = English;
            return (configuration, new Canvas(configuration));
        }

        private static T Load<T>(IcyConfiguration configuration, string body)
            where T : UIElement
        {
            var root = (StackPanel)new MarkupLoader(configuration).Load($"<StackPanel xmlns=\"https://icyui.dev/markup\">{body}</StackPanel>", "x.xml");
            return (T)root.Children[0];
        }

        [Fact]
        public void Markup_StringProperty_FollowsTheCulture()
        {
            var (configuration, _) = Create();
            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;

            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Markup_Content_BecomesATextBlock_AndFollowsTheCulture()
        {
            var (configuration, _) = Create();
            var button = Load<Button>(configuration, "<Button Content=\"{Localize Menu.Play}\"/>");
            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);

            configuration.Culture = Russian;

            Assert.Equal("Играть", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void Markup_Context()
        {
            var (configuration, _) = Create();
            configuration.Culture = Russian;

            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Open, Context=Verb}\"/>");

            Assert.Equal("Открыть", text.Text);
        }

        [Fact]
        public void Markup_NoKey_FailsAtLoad()
        {
            var (configuration, _) = Create();

            Assert.Throws<MarkupException>(() => Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize}\"/>"));
        }

        [Fact]
        public void TextTheGameSetItself_IsLeftAlone()
        {
            var (configuration, _) = Create();
            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");

            text.Text = "Score: 10";
            configuration.Culture = Russian;

            Assert.Equal("Score: 10", text.Text);
        }

        [Fact]
        public void Code_AttachedElement_FollowsTheCulture()
        {
            var (configuration, canvas) = Create();
            var text = new TextBlock();
            canvas.Add(text);

            text.Localize(nameof(TextBlock.Text), "Menu.Title");
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;
            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Code_ElementLocalizedBeforeAttaching_GetsItsTextOnAttach()
        {
            var (configuration, canvas) = Create();
            var text = new TextBlock();

            text.Localize(nameof(TextBlock.Text), "Menu.Title");
            canvas.Add(text);
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;
            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Code_UnknownProperty_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TextBlock().Localize("Txet", "Menu.Title"));
        }

        [Fact]
        public void StyleSetter_ResolvesOnce()
        {
            var (configuration, canvas) = Create();
            var root = (StackPanel)new MarkupLoader(configuration).Load(
                "<StackPanel xmlns=\"https://icyui.dev/markup\"><StackPanel.Resources><Style TargetType=\"TextBlock\" Text=\"{Localize Menu.Title}\"/></StackPanel.Resources><TextBlock/></StackPanel>",
                "x.xml");
            canvas.Add(root);
            var text = (TextBlock)root.Children[0];
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;

            Assert.Equal("Main menu", text.Text);
        }

        [Fact]
        public void CollectedElements_AreDropped()
        {
            var (configuration, _) = Create();
            LoadAndForget(configuration);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            configuration.Culture = Russian;

            Assert.Equal(0, configuration.LocalizedProperties.Count);
        }

        private static void LoadAndForget(IcyConfiguration configuration) =>
            Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");
    }
}
