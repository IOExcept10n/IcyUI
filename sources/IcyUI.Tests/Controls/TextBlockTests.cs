using System.Drawing;
using Icy.Configuration;
using Icy.Tests.Rendering;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// <see cref="TextBlock"/> resolves its font via <see cref="Icy.Configuration.IcyConfiguration.Fonts"/>, which
    /// requires a full <see cref="Icy.UI.Canvas"/> to exist at all - out of scope for these lightweight,
    /// no-graphics-device control tests (actual glyph measurement/rendering is covered by
    /// <c>Rendering/Fonts/SpriteFontTests.cs</c>). These tests instead verify TextBlock's own contract: it never
    /// throws and safely renders/measures as empty whenever a font can't be resolved (no Canvas attached, or no
    /// FontFamily set), rather than crashing or measuring garbage.
    /// </summary>
    public class TextBlockTests
    {
        [Theory]
        [InlineData("Player")]
        [InlineData("Plaer")]
        [InlineData("ace")]
        public void MeasureContent_OneLine_IsTheFontsLineHeight(string text)
        {
            // The baseline sits at the font's ascent below the top, so a descender (the y in "Player") reaches
            // ascent - descent below it. Measuring only the ink's own extent cut that off, and differed per word.
            var builder = new Icy.Configuration.IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new Icy.Tests.Input.FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .AddBasicFontSupport();
            var configuration = builder.Build();
            configuration.Fonts.ImportFont(configuration.Assets.DefaultAssetContext, "Resources/Airfool.otf");
            var canvas = new Icy.UI.Canvas(configuration);
            var textBlock = new TextBlock { FontFamily = "Airfool", FontSize = 20, Text = text };
            canvas.Add(textBlock);
            Icy.Rendering.Fonts.IFont font = configuration.Fonts.GetOrLoad(new Icy.Rendering.Fonts.FontInfo("Airfool", 20, Icy.Rendering.Fonts.FontStyle.Regular))!;

            Size measured = textBlock.Measure();

            Assert.Equal((int)MathF.Ceiling(font.Metrics.Ascent - font.Metrics.Descent), measured.Height);
        }

        [Fact]
        public void MeasureContent_ReturnsEmpty_WhenNotAttachedToCanvas()
        {
            var textBlock = new TextBlock { FontFamily = "Arial", FontSize = 16, Text = "Hello" };

            Size measured = textBlock.Measure();

            Assert.Equal(Size.Empty, measured);
        }

        [Fact]
        public void MeasureContent_ReturnsEmpty_WhenFontFamilyNotSet()
        {
            var textBlock = new TextBlock { Text = "Hello" };

            Size measured = textBlock.Measure();

            Assert.Equal(Size.Empty, measured);
        }

        [Fact]
        public void MeasureContent_ReturnsEmpty_WhenTextIsEmpty()
        {
            var textBlock = new TextBlock { FontFamily = "Arial" };

            Size measured = textBlock.Measure();

            Assert.Equal(Size.Empty, measured);
        }

        [Fact]
        public void Draw_DoesNotThrow_WhenFontUnresolvable()
        {
            var textBlock = new TextBlock { FontFamily = "Arial", Text = "Hello" };
            var context = new FakeRenderContext();

            textBlock.Arrange(new Rectangle(0, 0, 100, 20));
            var exception = Record.Exception(() => textBlock.Draw(context));

            Assert.Null(exception);
            Assert.Empty(context.DrawCalls);
        }

        [Fact]
        public void Text_SettingNull_CoercesToEmptyString()
        {
            var textBlock = new TextBlock { Text = null! };

            Assert.Equal(string.Empty, textBlock.Text);
        }
    }
}
