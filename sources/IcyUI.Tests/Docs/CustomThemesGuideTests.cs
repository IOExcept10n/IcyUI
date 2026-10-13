// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Configuration;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Behaviour of the "Custom themes" guide: its theme file, layered over the built-in theme with
    /// <see cref="ThemeBuildingExtensions.ExtendDefaultTheme"/>.
    /// </summary>
    public sealed class CustomThemesGuideTests : IDisposable
    {
        private readonly string root = Directory.CreateTempSubdirectory("icy-custom-themes-").FullName;

        public CustomThemesGuideTests()
        {
            Directory.CreateDirectory(Path.Combine(root, "Themes"));
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "custom-themes", "theme-extension.xml"),
                Path.Combine(root, "Themes", "MyGame.xml"));
        }

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void Button_ChangesItsColors_IncludingHoverAndPress_AndKeepsTheBuiltInTemplate()
        {
            Canvas canvas = ThemedCanvas();
            var button = new Button();
            canvas.Add(button);

            Assert.NotNull(button.Template);
            Assert.Equal(0xFF1F6F6B, Argb(button.Background));

            button.ControlState = ControlState.Hovered;
            canvas.Render();

            Assert.Equal(0xFF2A8A85, Argb(button.Background));

            button.ControlState = ControlState.Pressed;
            canvas.Render();

            Assert.Equal(0xFF174F4C, Argb(button.Background));
        }

        [Fact]
        public void ListRows_SelectInTheGamesColor()
        {
            Canvas canvas = ThemedCanvas();
            var item = new SelectorItem();
            canvas.Add(item);

            item.ControlState = ControlState.Selected;
            canvas.Render();

            Assert.Equal(0xFF1F6F6B, Argb(item.Background));
        }

        [Fact]
        public void CheckBox_ReplacesTheBuiltInStyle_WithItsOwnTemplateAndStates()
        {
            Canvas canvas = ThemedCanvas();
            var checkBox = new CheckBox();
            canvas.Add(checkBox);
            checkBox.ControlState = ControlState.Focused;
            canvas.Render();

            Assert.NotNull(checkBox.Template);
            Assert.Equal(0xFF5FD0C0, Argb(checkBox.BorderBrush));
            Assert.DoesNotContain(checkBox.EnumerateVisualSubtree(), e => e is Icon);

            checkBox.ControlState = ControlState.Checked;
            canvas.Render();

            Assert.Equal(0xFF5FD0C0, Argb(checkBox.Background));
        }

        [Fact]
        public void TypesTheFileDoesNotStyle_KeepTheBuiltInStyle()
        {
            Canvas canvas = ThemedCanvas();
            var slider = new Slider();
            canvas.Add(slider);

            Assert.NotNull(slider.Template);
        }

        private static long Argb(IBrush? brush) => (uint)Assert.IsType<SolidColorBrush>(brush).Color.ToArgb();

        private Canvas ThemedCanvas()
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
            builder.AddMarkupSupport();
            builder.UseDefaultTheme().ExtendDefaultTheme("Themes/MyGame.xml");
            return new Canvas(builder.Build()) { IsVisible = true };
        }
    }
}
