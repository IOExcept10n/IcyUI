// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.Configuration
{
    /// <summary>
    /// Covers <see cref="ThemeBuildingExtensions.UseThemeFile"/> and <see cref="ThemeBuildingExtensions.ExtendDefaultTheme"/>.
    /// </summary>
    public sealed class ThemeFileTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "IcyUI.Tests", Guid.NewGuid().ToString("N"));

        public ThemeFileTests() => Directory.CreateDirectory(root);

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void UseThemeFile_ReplacesTheTheme()
        {
            Write("Dark.xml", """<ResourceDictionary><Style TargetType="Button" Padding="99"/></ResourceDictionary>""");

            IcyConfiguration config = Build(t => t.UseThemeFile("Dark.xml"));
            var canvas = new Canvas(config);
            var button = new Button();
            canvas.Add(button);

            Assert.Equal(99, button.Padding.Left);
            Assert.Null(button.Template);
        }

        [Fact]
        public void ExtendDefaultTheme_ImplicitStyle_KeepsTheBuiltInTemplateAndStates()
        {
            Write("Mine.xml", """<ResourceDictionary><Style TargetType="Button" Background="#FF008080"/></ResourceDictionary>""");

            IcyConfiguration config = Build(t => t.ExtendDefaultTheme("Mine.xml"));
            var canvas = new Canvas(config);
            var button = new Button();
            canvas.Add(button);

            Assert.NotNull(button.Template);
            Assert.Equal(Color.FromArgb(0xFF, 0x00, 0x80, 0x80).ToArgb(), ((SolidColorBrush)button.Background).Color.ToArgb());

            button.ControlState = ControlState.Hovered | ControlState.Pressed;
            canvas.Render();

            Assert.Equal(Color.FromArgb(0xFF, 0x2E, 0x2E, 0x38).ToArgb(), ((SolidColorBrush)button.Background).Color.ToArgb());
        }

        [Fact]
        public void ExtendDefaultTheme_UnstyledTypes_KeepTheBuiltInStyle()
        {
            Write("Mine.xml", """<ResourceDictionary><Style TargetType="Button" Background="#FF008080"/></ResourceDictionary>""");

            IcyConfiguration config = Build(t => t.ExtendDefaultTheme("Mine.xml"));
            var canvas = new Canvas(config);
            var checkBox = new CheckBox();
            canvas.Add(checkBox);

            Assert.NotNull(checkBox.Template);
        }

        [Fact]
        public void ExtendDefaultTheme_ExplicitBasedOnAndReplacesTheme_AreLeftAlone()
        {
            Write("Mine.xml", """<ResourceDictionary><Style TargetType="Button" ReplacesTheme="True" Padding="7"/></ResourceDictionary>""");

            IcyConfiguration config = Build(t => t.ExtendDefaultTheme("Mine.xml"));

            Assert.True(config.Theme.Theme!.TryGetValue(ResourceDictionary.GetImplicitStyleKey(typeof(Button)), out object? value));
            var style = Assert.IsType<Style>(value);
            Assert.True(style.ReplacesTheme);
            Assert.Null(style.BasedOn);
        }

        [Fact]
        public void ExtendDefaultTheme_KeyedResources_FromBothAreFound()
        {
            Write("Mine.xml", """<ResourceDictionary><Style x:Key="Accent" TargetType="Button" Padding="3"/></ResourceDictionary>""");

            IcyConfiguration config = Build(t => t.ExtendDefaultTheme("Mine.xml"));

            Assert.True(config.Theme.Theme!.TryGetValue("IcyDefaultButtonTemplate", out _));
            Assert.True(config.Theme.Theme.TryGetValue("Accent", out _));
        }

        [Fact]
        public void MissingThemeFile_FailsTheBuild()
        {
            Exception ex = Assert.ThrowsAny<Exception>(() => Build(t => t.UseThemeFile("Nope.xml")));

            Assert.Contains("Nope.xml", ex.ToString());
        }

        private void Write(string name, string body) => File.WriteAllText(Path.Combine(root, name), body);

        private IcyConfiguration Build(Action<IThemeConfigurationBuilder> theme)
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
            builder.AddMarkupSupport();
            theme(builder.UseDefaultTheme());
            return builder.Build();
        }
    }
}
