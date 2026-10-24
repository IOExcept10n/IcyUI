using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// Brushes a control shows on a template part (<see cref="Slider.ThumbBrush"/>, <see cref="SplitPane.DividerBrush"/>) are
    /// stored on the control, so one set before the theme's template applies still reaches the part (the ICY-013 pattern).
    /// </summary>
    public class TemplatePartBrushTests
    {
        private static Canvas CreateCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme());

        private static int ColorOfPart(UIElement control, string part) =>
            Assert.IsType<SolidColorBrush>(control.EnumerateVisualSubtree().OfType<Border>().First(b => b.Name == part).Background).Color.ToArgb();

        [Fact]
        public void SliderThumbBrush_SetBeforeTheThemesTemplate_IsShownOnItsThumb()
        {
            Canvas canvas = CreateCanvas();
            var slider = new Slider { ThumbBrush = new SolidColorBrush(Color.Orange) };

            canvas.Add(slider);

            Assert.NotNull(slider.Template);
            Assert.Equal(Color.Orange.ToArgb(), ColorOfPart(slider, "PART_Thumb"));
        }

        [Fact]
        public void SplitPaneDividerBrush_SetBeforeTheThemesTemplate_IsShownOnItsDivider()
        {
            Canvas canvas = CreateCanvas();
            var pane = new SplitPane { DividerBrush = new SolidColorBrush(Color.Orange), First = new Border(), Second = new Border() };

            canvas.Add(pane);

            Assert.NotNull(pane.Template);
            Assert.Equal(Color.Orange.ToArgb(), ColorOfPart(pane, "PART_Divider"));
        }
    }
}
