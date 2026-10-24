using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Rendering;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.UI
{
    /// <summary>
    /// <see cref="Canvas.TextureFiltering"/> and the inherited <see cref="UIElement.TextureFiltering"/> choose how textures
    /// are sampled (ICY-010): <see cref="TextureFiltering.Point"/> keeps upscaled pixel art crisp.
    /// </summary>
    public class TextureFilteringTests
    {
        private readonly FakeRenderContext context = new() { ViewportSize = new Size(800, 600) };
        private readonly Canvas canvas;

        public TextureFilteringTests()
        {
            canvas = new Canvas(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), context, new ReflectionConfiguration())) { IsVisible = true };
        }

        private static Border Box(string name) =>
            new() { Name = name, Width = 10, Height = 10, Background = new SolidColorBrush(Color.Red), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };

        private TextureFiltering FilteringOf(Border box) =>
            context.DrawCalls.Single(c => c.Options.Destination.Size == box.ActualBounds.Size && c.TransformAtDrawTime.Apply(new Rectangle(Point.Empty, box.ActualBounds.Size)).Location == box.PointToScreen(System.Numerics.Vector2.Zero)).FilteringAtDrawTime;

        [Fact]
        public void ByDefault_EverythingIsLinear()
        {
            canvas.Add(Box("a"));
            canvas.Render();

            Assert.All(context.DrawCalls, c => Assert.Equal(TextureFiltering.Linear, c.FilteringAtDrawTime));
        }

        [Fact]
        public void CanvasDefault_AppliesToRootsAndOverlays_AndIsRestoredAfterTheFrame()
        {
            canvas.TextureFiltering = TextureFiltering.Point;
            canvas.Add(Box("root"));
            canvas.AddOverlay(Box("overlay"));

            canvas.Render();

            Assert.NotEmpty(context.DrawCalls);
            Assert.All(context.DrawCalls, c => Assert.Equal(TextureFiltering.Point, c.FilteringAtDrawTime));
            Assert.Equal(TextureFiltering.Linear, context.Options.Filtering);
        }

        [Fact]
        public void ElementFiltering_CoversItsSubtree_AndNotItsSiblings()
        {
            Border inner = Box("inner");
            var portrait = new Border { Width = 20, Height = 20, TextureFiltering = TextureFiltering.Point, Child = inner, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            Border sibling = Box("sibling");
            sibling.Margin = new Thickness(100, 0, 0, 0);
            var panel = new Grid();
            panel.Children.Add(portrait);
            panel.Children.Add(sibling);
            canvas.Add(panel);

            canvas.Render();

            Assert.Equal(TextureFiltering.Point, FilteringOf(inner));
            Assert.Equal(TextureFiltering.Linear, FilteringOf(sibling));
        }

        [Fact]
        public void ElementFiltering_OverridesThePointCanvas()
        {
            canvas.TextureFiltering = TextureFiltering.Point;
            Border smooth = Box("smooth");
            smooth.TextureFiltering = TextureFiltering.Linear;
            canvas.Add(smooth);

            canvas.Render();

            Assert.Equal(TextureFiltering.Linear, FilteringOf(smooth));
        }

        [Fact]
        public void InheritedFiltering_SetsNothingPerElement()
        {
            canvas.TextureFiltering = TextureFiltering.Point;
            var panel = new StackPanel();
            for (int i = 0; i < 10; i++)
                panel.Children.Add(Box("b" + i));
            canvas.Add(panel);
            var options = (FakeRenderContext.FakeRenderOptions)context.Options;

            canvas.Render();

            // Once for the canvas's frame and once to restore it: never per element.
            Assert.Equal(2, options.FilteringSets);
        }

        [Fact]
        public void Markup_SetsTheFiltering()
        {
            var border = (Border)new Icy.Markup.MarkupLoader(canvas.Configuration).LoadObject("""<Border TextureFiltering="Point"/>""");

            Assert.Equal(TextureFiltering.Point, border.TextureFiltering);
        }
    }
}
