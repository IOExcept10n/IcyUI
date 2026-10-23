using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// A <see cref="ProgressBar.FillBrush"/> set in markup reaches the theme template's fill (ICY-013).
    /// </summary>
    public class ProgressBarFillBrushTests
    {
        private static Canvas CreateCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme());

        private static int FillColor(ProgressBar bar) =>
            Assert.IsType<SolidColorBrush>(bar.EnumerateVisualSubtree().OfType<Border>().Single(b => b.Name == "PART_Fill").Background).Color.ToArgb();

        [Fact]
        public void FillBrushFromMarkup_IsShownByTheThemeTemplate()
        {
            var canvas = CreateCanvas();
            var bar = (ProgressBar)new MarkupLoader(canvas.Configuration).LoadObject("""<ProgressBar FillBrush="#FFFF0000"/>""");

            canvas.Add(bar);

            Assert.Equal(Color.FromArgb(255, 255, 0, 0).ToArgb(), FillColor(bar));
        }

        [Fact]
        public void FillBrushChangedAfterTheTemplateApplies_UpdatesTheFill()
        {
            var canvas = CreateCanvas();
            var bar = new ProgressBar();
            canvas.Add(bar);

            bar.FillBrush = new SolidColorBrush(Color.Lime);

            Assert.Equal(Color.Lime.ToArgb(), FillColor(bar));
            Assert.Equal(Color.Lime, Assert.IsType<SolidColorBrush>(bar.FillBrush).Color);
        }

        [Fact]
        public void TwoBarsWithDifferentBrushes_KeepTheirOwn()
        {
            var canvas = CreateCanvas();
            var loader = new MarkupLoader(canvas.Configuration);
            var health = (ProgressBar)loader.LoadObject("""<ProgressBar FillBrush="#FFFF0000"/>""");
            var mana = (ProgressBar)loader.LoadObject("""<ProgressBar FillBrush="#FF0000FF"/>""");

            canvas.Add(health);
            canvas.Add(mana);

            Assert.Equal(Color.FromArgb(255, 255, 0, 0).ToArgb(), FillColor(health));
            Assert.Equal(Color.FromArgb(255, 0, 0, 255).ToArgb(), FillColor(mana));
        }
    }
}
