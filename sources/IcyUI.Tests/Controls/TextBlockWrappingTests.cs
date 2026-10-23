using System.Drawing;
using Icy.Configuration;
using Icy.Rendering.Fonts;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// <see cref="TextBlock.TextWrapping"/> breaks text into lines by the width it has (ICY-008), and
    /// <see cref="TextBlock.TextAlignment"/> places each line within that width.
    /// </summary>
    public class TextBlockWrappingTests
    {
        private const float Size = 20;

        private readonly FakeRenderContext renderContext = new();
        private readonly Canvas canvas;
        private readonly IFont font;

        public TextBlockWrappingTests()
        {
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(renderContext)
                .ConfigureInput(new FakeInputSystem())
                .ConfigureTypes()
                .ConfigureAssets()
                .AddBasicFontSupport()
                .Build();
            configuration.Fonts.ImportFont(configuration.Assets.DefaultAssetContext, "Resources/Airfool.otf");
            canvas = new Canvas(configuration) { IsVisible = true };
            font = configuration.Fonts.GetOrLoad(new FontInfo("Airfool", Size, FontStyle.Regular))!;
        }

        private int LinesHeight(int lines)
        {
            FontMetrics m = font.Metrics;
            return (int)MathF.Ceiling((m.Ascent - m.Descent) + ((lines - 1) * (m.Ascent - m.Descent + m.LineGap)));
        }

        private float Advance(string text) => font.MeasureAdvance(text, default).X;

        // Fits the widest of the words, so each one gets a line of its own and none breaks inside.
        private float WidestWord(params string[] words) => MathF.Ceiling(words.Max(Advance)) + 1;

        private TextBlock NewText(string text, TextWrapping wrapping = TextWrapping.Wrap) =>
            new() { FontFamily = "Airfool", FontSize = Size, Text = text, TextWrapping = wrapping };

        [Fact]
        public void ExplicitWidth_WrapsAtTheSpaceThatNoLongerFits()
        {
            TextBlock text = NewText("alpha beta gamma");
            text.Width = MathF.Ceiling(Advance("alpha beta")) + 1;
            canvas.Add(text);

            Assert.Equal(LinesHeight(2), text.Measure().Height);
        }

        [Fact]
        public void NoWrap_KeepsOneLine()
        {
            TextBlock text = NewText("alpha beta gamma", TextWrapping.NoWrap);
            text.Width = MathF.Ceiling(Advance("alpha beta")) + 1;
            canvas.Add(text);

            Assert.Equal(LinesHeight(1), text.Measure().Height);
        }

        [Fact]
        public void MaxWidth_LimitsTheLineToo()
        {
            TextBlock text = NewText("alpha beta gamma");
            text.MaxWidth = WidestWord("alpha", "beta", "gamma");
            canvas.Add(text);

            Size measured = text.Measure();

            Assert.Equal(LinesHeight(3), measured.Height);
            Assert.True(measured.Width <= text.MaxWidth);
        }

        [Fact]
        public void AWordWiderThanTheLine_BreaksInsideTheWord()
        {
            TextBlock text = NewText("abcdefghij");
            text.Width = MathF.Ceiling(Advance("abcde")) + 1;
            canvas.Add(text);

            Assert.Equal(LinesHeight(2), text.Measure().Height);
        }

        [Fact]
        public void ExplicitLineBreaks_StillBreak_AndWrapWithinEachLine()
        {
            TextBlock text = NewText("alpha beta\ngamma");
            text.Width = WidestWord("alpha", "beta", "gamma");
            canvas.Add(text);

            Assert.Equal(LinesHeight(3), text.Measure().Height);
        }

        [Fact]
        public void StretchedInAPanel_WrapsToTheSlot_AndThePanelGrowsInTheSameFrame()
        {
            TextBlock text = NewText("alpha beta gamma");
            var panel = new StackPanel { Width = MathF.Ceiling(Advance("alpha beta")) + 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(text);
            canvas.Add(panel);

            canvas.Render();

            Assert.Equal(LinesHeight(2), text.ActualBounds.Height);
            Assert.Equal(LinesHeight(2), panel.ActualBounds.Height);
        }

        [Fact]
        public void WideningTheSlot_UnwrapsTheText()
        {
            TextBlock text = NewText("alpha beta gamma");
            text.HorizontalAlignment = HorizontalAlignment.Left;
            var panel = new StackPanel { Width = MathF.Ceiling(Advance("alpha beta")) + 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(text);
            canvas.Add(panel);
            canvas.Render();

            panel.Width = MathF.Ceiling(Advance("alpha beta gamma")) + 10;
            canvas.Render();

            Assert.Equal(LinesHeight(1), text.ActualBounds.Height);
        }

        [Theory]
        [InlineData(TextAlignment.Left, 0f)]
        [InlineData(TextAlignment.Center, 0.5f)]
        [InlineData(TextAlignment.Right, 1f)]
        public void TextAlignment_PlacesTheLineWithinTheWidth(TextAlignment alignment, float factor)
        {
            const int width = 300;
            TextBlock text = NewText("ab", TextWrapping.NoWrap);
            text.Width = width;
            text.TextAlignment = alignment;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.VerticalAlignment = VerticalAlignment.Top;
            canvas.Add(text);
            canvas.Render();

            int left = renderContext.DrawCalls.Min(c => c.TransformAtDrawTime.Apply(c.Options.Destination).Left);
            float expected = factor * (width - Advance("ab"));

            Assert.InRange(left - text.ActualBounds.Left, expected - 2, expected + 2);
        }
    }
}
