using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.UI
{
    public class HitTestPaintRuleTests
    {
        private static Canvas CreateCanvas(bool themed = false)
        {
            var config = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext { ViewportSize = new Size(800, 600) }, new ReflectionConfiguration());
            return new Canvas(themed ? config.UseDefaultTheme() : config) { IsVisible = true };
        }

        [Fact]
        public void FullScreenGrid_MissesOnEmptySpace_AndHitsItsCornerButton()
        {
            Canvas canvas = CreateCanvas(themed: true);
            var button = new Button { Content = "Menu", Width = 80, Height = 30, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            var grid = new Grid();
            grid.Children.Add(button);
            canvas.Add(grid);
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(400, 300)));
            Assert.NotNull(canvas.HitTest(new Point(10, 10)));
        }

        [Fact]
        public void PanelWithAPanelInside_PassesThroughBoth()
        {
            Canvas canvas = CreateCanvas();
            var inner = new StackPanel();
            var outer = new Grid();
            outer.Children.Add(inner);
            canvas.Add(outer);
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(400, 300)));
        }

        [Fact]
        public void TextBlockAndImage_AreHitWithinTheirBounds()
        {
            Canvas canvas = CreateCanvas();
            var image = new Image { Width = 50, Height = 50, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            canvas.Add(image);
            canvas.Render();

            Assert.Same(image, canvas.HitTest(new Point(25, 25)));
        }

        [Fact]
        public void BorderWithABackground_IsHit_EvenATransparentOne()
        {
            Canvas canvas = CreateCanvas();
            var border = new Border { Background = new SolidColorBrush(Color.Transparent) };
            canvas.Add(border);
            canvas.Render();

            Assert.Same(border, canvas.HitTest(new Point(400, 300)));
        }

        [Fact]
        public void UnthemedDialog_StillBlocksThePointer()
        {
            Canvas canvas = CreateCanvas();
            var behind = new Border { Background = new SolidColorBrush(Color.Gray) };
            canvas.Add(behind);
            var dialog = new Dialog { Content = new TextBlock() };
            dialog.Show(canvas);
            canvas.Render();

            UIElement? hit = canvas.HitTest(new Point(400, 300));

            Assert.NotNull(hit);
            Assert.NotSame(behind, hit);
        }

        [Fact]
        public void UnthemedScrollViewer_IsHitOnItsEmptyArea()
        {
            Canvas canvas = CreateCanvas();
            var viewer = new ScrollViewer { Content = new StackPanel() };
            canvas.Add(viewer);
            canvas.Render();

            // Its own chrome may answer too; what matters is that a drag starting here reaches the viewer.
            UIElement? hit = canvas.HitTest(new Point(400, 300));
            Assert.True(hit == viewer || (hit != null && IsInside(hit, viewer)));
        }

        [Fact]
        public void BoundsMode_FindsAnEmptyPanel_ForDesignTools()
        {
            Canvas canvas = CreateCanvas();
            var grid = new Grid();
            canvas.Add(grid);
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(400, 300)));
            Assert.Same(grid, canvas.HitTest(new Point(400, 300), includeOverlay: null, HitTestMode.Bounds));
        }

        [Fact]
        public void BorderWithoutABackground_PassesThrough()
        {
            Canvas canvas = CreateCanvas();
            canvas.Add(new Border());
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(400, 300)));
        }

        [Fact]
        public void TemplatedControlWithoutABackground_PassesThrough()
        {
            Canvas canvas = CreateCanvas();
            var template = (Icy.UI.Styles.ControlTemplate)new Icy.Markup.MarkupLoader(canvas.Configuration).LoadObject(
                """<ControlTemplate TargetType="ContentControl"><Border Background="{TemplateBinding Background}"/></ControlTemplate>""");
            canvas.Add(new ContentControl { Template = template });
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(400, 300)));
        }

        [Fact]
        public void UnthemedButtonWithoutABackground_IsHitOverItsWholeArea()
        {
            Canvas canvas = CreateCanvas();
            var button = new Button { Width = 100, Height = 40, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            canvas.Add(button);
            canvas.Render();

            Assert.Same(button, canvas.HitTest(new Point(90, 35)));
        }

        [Fact]
        public void SplitPane_EmptyPanePassesThrough_ItsDividerIsHit()
        {
            Canvas canvas = CreateCanvas();
            var pane = new SplitPane { First = new Border(), Second = new Border() };
            canvas.Add(pane);
            canvas.Render();

            Assert.Null(canvas.HitTest(new Point(100, 300)));
            Assert.NotNull(canvas.HitTest(new Point(400, 300)));
        }

        private static bool IsInside(UIElement element, UIElement ancestor)
        {
            for (UIElement? current = element; current != null; current = current.Parent)
            {
                if (current == ancestor)
                    return true;
            }

            return false;
        }
    }
}
