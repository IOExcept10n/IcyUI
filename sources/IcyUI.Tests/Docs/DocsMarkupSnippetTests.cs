using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
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
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            configuration.UseDefaultTheme();
            var loader = new MarkupLoader(configuration);

            UIElement root = loader.Load(File.ReadAllText(Path.Combine(SnippetRoot, file)), file);
            root.DataContext = new DocsSnippetViewModel();

            Assert.NotNull(root);
        }
    }
}
