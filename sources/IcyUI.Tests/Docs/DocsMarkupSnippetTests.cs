using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;
using Xunit;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Loads every markup snippet the documentation includes (docfx/snippets/markup/**/*.xml, copied next to the test
    /// assembly) through the real markup loader, so a renamed control or property breaks a test instead of the docs.
    /// </summary>
    public class DocsMarkupSnippetTests
    {
        private static readonly string SnippetRoot = Path.Combine(AppContext.BaseDirectory, "DocsSnippets");

        public static TheoryData<string> Snippets()
        {
            var data = new TheoryData<string>();
            if (!Directory.Exists(SnippetRoot))
                return data;
            foreach (string file in Directory.EnumerateFiles(SnippetRoot, "*.xml", SearchOption.AllDirectories).Order())
                data.Add(Path.GetRelativePath(SnippetRoot, file));
            return data;
        }

        [Fact]
        public void SnippetFolder_IsNotEmpty() => Assert.NotEmpty(Snippets());

        [Theory]
        [MemberData(nameof(Snippets))]
        public void EveryMarkupSnippet_Loads(string file)
        {
            UIElement root = Load(file);
            root.DataContext = new DocsSnippetViewModel();

            Assert.NotNull(root);
        }

        [Fact]
        public void ControlsSnippets_OneForEveryFamily()
        {
            string[] families = ["buttons", "text", "images", "range", "lists", "tree-view", "navigation", "expander-split", "scrolling", "panels", "dialogs", "editors"];

            foreach (string family in families)
                Assert.True(File.Exists(Path.Combine(SnippetRoot, "controls", family + ".xml")), $"No controls/{family}.xml snippet.");
        }

        [Fact]
        public void ListsSnippet_ShowsTheViewModelsLevels()
        {
            var root = (StackPanel)Load(Path.Combine("controls", "lists.xml"));
            root.DataContext = new DocsSnippetViewModel();

            var listBox = Assert.IsType<ListBox>(root.Children[1]);
            Assert.Equal(new[] { "Forest", "Caves", "Castle" }, listBox.ItemsSource!.Cast<string>());
        }

        [Fact]
        public void Binding_WithoutMode_IsOneWay()
        {
            var root = (StackPanel)Load(Path.Combine("concepts", "binding.xml"));
            var viewModel = new DocsSnippetViewModel();
            root.DataContext = viewModel;
            var title = Assert.IsType<TextBlock>(root.Children[0]);

            title.Text = "Changed on screen";

            Assert.Equal("My Game", viewModel.Title);
        }

        [Fact]
        public void Binding_ElementName_FollowsTheSlider()
        {
            var root = (StackPanel)Load(Path.Combine("concepts", "binding.xml"));
            root.DataContext = new DocsSnippetViewModel();
            var slider = Assert.IsType<Slider>(root.Children[2]);
            var label = Assert.IsType<TextBlock>(root.Children[3]);

            slider.Value = 25;

            Assert.Equal("25", label.Text);
        }

        [Fact]
        public void Styles_KeyedStyle_KeepsTheThemeTemplate()
        {
            var canvas = ThemedCanvas();
            var root = (StackPanel)Load(Path.Combine("concepts", "styles.xml"));
            canvas.Add(root);
            canvas.Render();

            var quit = Assert.IsType<Button>(root.Children[1]);
            Assert.NotNull(quit.Template);
        }

        [Fact]
        public void Template_ShowsTheButtonsContent()
        {
            var canvas = ThemedCanvas();
            var root = (StackPanel)Load(Path.Combine("concepts", "template.xml"));
            canvas.Add(root);
            canvas.Render();

            var button = Assert.IsType<Button>(root.Children[0]);
            Assert.Contains(button.EnumerateVisualSubtree(), e => e is TextBlock { Text: "Play" });
        }

        [Fact]
        public void StateGroups_FocusedButtonShowsHoverAndFocusTogether()
        {
            var canvas = ThemedCanvas();
            var root = (StackPanel)Load(Path.Combine("concepts", "state-groups.xml"));
            canvas.Add(root);
            canvas.Render();
            var button = Assert.IsType<Button>(root.Children[0]);

            button.ControlState = ControlState.Focused | ControlState.Hovered | ControlState.Pressed;

            Assert.Equal(Color.FromArgb(unchecked((int)0xFF2C5AA8)), Assert.IsType<SolidColorBrush>(button.Background).Color);
            Assert.Equal(Color.FromArgb(unchecked((int)0xFFFFD24A)), Assert.IsType<SolidColorBrush>(button.BorderBrush).Color);
        }

        private static Canvas ThemedCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme())
            {
                IsVisible = true,
            };

        [Fact]
        public void FirstScreen_TitleFollowsTheViewModel()
        {
            // "Your first screen" tells readers to change the view model's Title to see binding at work.
            var root = (StackPanel)Load("first-screen.xml");
            var viewModel = new DocsSnippetViewModel();
            root.DataContext = viewModel;
            var title = Assert.IsType<TextBlock>(root.Children[0]);
            Assert.Equal("My Game", title.Text);

            viewModel.Title = "Paused";

            Assert.Equal("Paused", title.Text);
        }

        private static UIElement Load(string file)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            configuration.UseDefaultTheme();
            return new MarkupLoader(configuration).Load(File.ReadAllText(Path.Combine(SnippetRoot, file)), file);
        }
    }
}
