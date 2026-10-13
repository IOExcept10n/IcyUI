// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Configuration;
using Icy.Data.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using MyGame.HotReload;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Behaviour of the "Hot reload and the in-game editor" guide: <c>DevTools</c> and <c>ReloadablePage</c> in
    /// <c>IcyUI.Docs.Snippets.Stride/Guides/HotReload.cs</c> (compiled into this project).
    /// </summary>
    public sealed class HotReloadGuideTests : IDisposable
    {
        private const string Original = """<StackPanel x:Name="Root"><Button x:Name="Play" Width="100">Play</Button></StackPanel>""";

        private readonly string output = Directory.CreateTempSubdirectory("icy-output-").FullName;
        private readonly string source = Directory.CreateTempSubdirectory("icy-source-").FullName;
        private readonly TestHost host;
        private readonly DevTools tools;
        private readonly ReloadablePage page;

        public HotReloadGuideTests()
        {
            File.WriteAllText(Path.Combine(output, "Menu.xml"), Original);
            File.WriteAllText(Path.Combine(source, "Menu.xml"), Original);
            host = new TestHost(output);
            tools = new DevTools(host, source);
            page = new ReloadablePage(host, tools.Design, "Menu.xml", new object());
        }

        public void Dispose()
        {
            tools.Dispose();
            Directory.Delete(output, recursive: true);
            Directory.Delete(source, recursive: true);
        }

        [Fact]
        public void ASavedFile_ChangesTheLivePage()
        {
            Assert.Equal(100, Play(page.Page).Width);

            File.WriteAllText(Path.Combine(source, "Menu.xml"), Original.Replace("100", "200"));

            Assert.True(PumpUntil(() => Play(page.Page).Width == 200));
        }

        [Fact]
        public void AnEditInsideAStyle_ShowsAfterReload()
        {
            const string withStyle = """<StackPanel x:Name="Root"><StackPanel.Resources><Style TargetType="Button" Padding="30"/></StackPanel.Resources><Button x:Name="Play" Width="100">Play</Button></StackPanel>""";
            UIElement before = page.Page;
            File.WriteAllText(Path.Combine(source, "Menu.xml"), withStyle);
            DesignDocumentProbe document = new(tools);
            Assert.True(PumpUntil(() => document.Text == withStyle));
            Assert.True(document.NeedsReload);
            Assert.NotEqual(30, Play(before).Padding.Left);

            page.Reload();

            Assert.NotSame(before, page.Page);
            Assert.Equal(30, Play(page.Page).Padding.Left);
            Assert.False(before.IsAttached);
        }

        [Fact]
        public void Reload_UsesTheLatestText_NotTheBuildOutputsCopy()
        {
            File.WriteAllText(Path.Combine(source, "Menu.xml"), Original.Replace("100", "250"));
            DesignDocumentProbe document = new(tools);
            Assert.True(PumpUntil(() => document.Text.Contains("250", StringComparison.Ordinal)));

            page.Reload();

            Assert.Equal(250, Play(page.Page).Width);
            Assert.Contains("100", File.ReadAllText(Path.Combine(output, "Menu.xml")));
        }

        private static Button Play(UIElement root) => root.FindRequiredControl<Button>("Play");

        private static bool PumpUntil(Func<bool> condition)
        {
            Dispatcher dispatcher = Dispatcher.GetCurrentThreadDispatcher();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < 3000)
            {
                dispatcher.Update();
                if (condition())
                    return true;
                Thread.Sleep(10);
            }

            return condition();
        }

        private readonly struct DesignDocumentProbe(DevTools tools)
        {
            private Icy.Design.DesignDocument Document => tools.Design.Documents.Single();

            public string Text => Document.Text;

            public bool NeedsReload => Document.NeedsReload;
        }

        private sealed class TestHost : IIcyHost
        {
            public TestHost(string root)
            {
                var builder = new IcyConfigurationBuilder();
                builder.ConfigureRendering(new FakeRenderContext())
                       .ConfigureInput(new FakeInputSystem())
                       .ConfigureTypes()
                       .ConfigureAssets()
                       .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
                builder.AddMarkupSupport();
                builder.UseDefaultTheme();
                Configuration = builder.Build();
                MainCanvas = new Canvas(Configuration) { IsVisible = true };
                Canvases = [MainCanvas];
            }

            public IcyConfiguration Configuration { get; }

            public Canvas MainCanvas { get; }

            public IList<Canvas> Canvases { get; }
        }
    }
}
