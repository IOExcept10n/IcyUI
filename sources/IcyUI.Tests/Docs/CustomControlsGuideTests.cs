// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;
using MyGame.Controls;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Behaviour of the "Custom controls" guide: the <c>HealthBar</c> in
    /// <c>IcyUI.Docs.Snippets.Stride/Guides/HealthBar.cs</c> (compiled into this project) and the two markup snippets that use it.
    /// </summary>
    public sealed class CustomControlsGuideTests : IDisposable
    {
        private static readonly string AssemblyName = typeof(HealthBar).Assembly.GetName().Name!;

        private readonly string root = Directory.CreateTempSubdirectory("icy-custom-controls-").FullName;

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void Value_IsKeptBetweenZeroAndMaximum()
        {
            var bar = new HealthBar { Maximum = 50 };

            bar.Value = 80;
            Assert.Equal(50, bar.Value);

            bar.Value = -5;
            Assert.Equal(0, bar.Value);

            bar.Value = 40;
            bar.Maximum = 20;
            Assert.Equal(20, bar.Value);
        }

        [Fact]
        public void AValueSetBeforeAHigherMaximum_IsKept()
        {
            // Markup and bindings set properties in any order: Value="150" may arrive before Maximum="200".
            var bar = new HealthBar { Value = 150, Maximum = 200 };

            Assert.Equal(150, bar.Value);
        }

        [Fact]
        public void AFillBrushSetOnThePage_IsShownByTheThemesTemplate()
        {
            Canvas canvas = Canvas(theme: true, out _);
            var bar = new HealthBar { Width = 102, Height = 20, FillBrush = new SolidColorBrush(System.Drawing.Color.Blue) };
            canvas.Add(bar);
            canvas.Render();

            Assert.NotNull(bar.Template);
            Assert.Equal(System.Drawing.Color.Blue.ToArgb(), Assert.IsType<SolidColorBrush>(FillOf(bar).Background).Color.ToArgb());
        }

        [Fact]
        public void WithoutATheme_TheBuiltInFillGrowsWithTheValue()
        {
            Canvas canvas = Canvas(theme: false, out _);
            var bar = new HealthBar { Width = 102, Height = 20, Value = 50, HorizontalAlignment = HorizontalAlignment.Left };
            canvas.Add(bar);
            canvas.Render();

            Assert.InRange(FillOf(bar).ActualBounds.Width, 50 - 1, 50 + 1);
        }

        [Fact]
        public void WithTheGamesTheme_ThePartFillGrowsWithTheValue()
        {
            Canvas canvas = Canvas(theme: true, out _);
            var bar = new HealthBar { Width = 102, Height = 20, HorizontalAlignment = HorizontalAlignment.Left };
            canvas.Add(bar);
            bar.Value = 25;
            canvas.Render();

            Assert.NotNull(bar.Template);
            Assert.InRange(FillOf(bar).ActualBounds.Width, 25 - 1, 25 + 1);

            bar.Value = 100;
            canvas.Render();

            Assert.InRange(FillOf(bar).ActualBounds.Width, 100 - 1, 100 + 1);
        }

        [Fact]
        public void BelowAQuarter_TheThemeShowsTheCriticalLook()
        {
            Canvas canvas = Canvas(theme: true, out _);
            var bar = new HealthBar { Width = 102, Height = 20 };
            canvas.Add(bar);
            canvas.Render();
            Assert.Equal(unchecked((int)0xFF3CB371), Assert.IsType<SolidColorBrush>(bar.FillBrush).Color.ToArgb());

            bar.Value = 24;
            canvas.Render();
            Assert.True(bar.ControlState.HasFlag(ControlState.Highlighted));
            Assert.Equal(unchecked((int)0xFFD83C3C), Assert.IsType<SolidColorBrush>(bar.FillBrush).Color.ToArgb());

            bar.Value = 25;
            canvas.Render();
            Assert.False(bar.ControlState.HasFlag(ControlState.Highlighted));
            Assert.Equal(unchecked((int)0xFF3CB371), Assert.IsType<SolidColorBrush>(bar.FillBrush).Color.ToArgb());
        }

        [Fact]
        public void TheCriticalState_KeepsTheOtherStateFlags()
        {
            var bar = new HealthBar { ControlState = ControlState.Hovered };

            bar.Value = 10;
            Assert.Equal(ControlState.Hovered | ControlState.Highlighted, bar.ControlState);

            bar.Value = 90;
            Assert.Equal(ControlState.Hovered, bar.ControlState);
        }

        [Fact]
        public void HudSnippet_ValueFollowsTheViewModel()
        {
            Canvas canvas = Canvas(theme: true, out IcyConfiguration configuration);
            UIElement hud = new MarkupLoader(configuration).Load(Snippet("health-hud.xml"), "health-hud.xml");
            var viewModel = new HudViewModel();
            hud.DataContext = viewModel;
            canvas.Add(hud);
            var bar = Assert.IsType<HealthBar>(hud.FindRequiredControl<HealthBar>("PlayerHealth"));
            Assert.Equal(80, bar.Value);

            viewModel.Health = 10;

            Assert.Equal(10, bar.Value);
            Assert.True(bar.ControlState.HasFlag(ControlState.Highlighted));
        }

        [Fact]
        public void ShortName_MakesThePrefixUnnecessary()
        {
            IcyConfiguration configuration = Configuration(theme: false);
            configuration.Types.Markup.RegisterShortName<HealthBar>();

            UIElement bar = new MarkupLoader(configuration).Load("""<HealthBar Maximum="50" Value="30"/>""", "short.xml");

            var health = Assert.IsType<HealthBar>(bar);
            Assert.Equal(30, health.Value);
        }

        // The chrome's border holds the fill, so the fill is the innermost border.
        private static Border FillOf(HealthBar bar) => bar.EnumerateVisualSubtree().OfType<Border>().Last();

        private static string Snippet(string file) =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "custom-controls", file))
                .Replace("assembly=MyGame", "assembly=" + AssemblyName);

        private Canvas Canvas(bool theme, out IcyConfiguration configuration)
        {
            configuration = Configuration(theme);
            return new Canvas(configuration) { IsVisible = true };
        }

        private IcyConfiguration Configuration(bool theme)
        {
            Directory.CreateDirectory(Path.Combine(root, "Themes"));
            File.WriteAllText(Path.Combine(root, "Themes", "MyGame.xml"), Snippet("health-theme.xml"));
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
            builder.AddMarkupSupport();
            IThemeConfigurationBuilder themeBuilder = builder.UseDefaultTheme();
            if (theme)
                themeBuilder.ExtendDefaultTheme("Themes/MyGame.xml");
            return builder.Build();
        }

        private sealed class HudViewModel : ObservableObject
        {
            private float health = 80;

            public float Health
            {
                get => health;
                set => SetProperty(ref health, value);
            }
        }
    }
}
