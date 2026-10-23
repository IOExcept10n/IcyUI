using System.Drawing;
using System.Numerics;
using Icy.Configuration;
using Icy.Input.Events;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.UI
{
    public class TapRoutingTests
    {
        private static (Canvas Canvas, FakeInputSystem Input) CreateCanvas()
        {
            var input = new FakeInputSystem();
            IcyConfiguration config = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext { ViewportSize = new Size(800, 600) })
                .ConfigureInput(input)
                .ConfigureTypes()
                .ConfigureAssets()
                .Build()
                .UseDefaultTheme();
            return (new Canvas(config) { IsInputEnabled = true, IsVisible = true }, input);
        }

        private static void TapAt(FakeInputSystem input, Point point) =>
            input.Events.Touch.RaiseTap(new TouchInfo(point, 1));

        private static Point Center(UIElement element)
        {
            Rectangle b = element.ActualBounds;
            return new Point(b.X + (b.Width / 2), b.Y + (b.Height / 2));
        }

        [Fact]
        public void ButtonInsideATappableBorder_HandlesTheTap_AndTheBorderNeverSeesIt()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var button = new Button { Content = "Go", Width = 80, Height = 30 };
            var border = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Gray), Width = 200, Height = 100, Child = button };
            int clicks = 0, borderTaps = 0;
            button.Click += (_, _) => clicks++;
            border.Tapped += (_, _) => borderTaps++;
            canvas.Add(border);
            canvas.Render();

            TapAt(input, Center(button));

            Assert.Equal(1, clicks);
            Assert.Equal(0, borderTaps);
        }

        [Fact]
        public void UnhandledTap_ReachesEveryAncestorInOrder_WithTheSameArgs()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var inner = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Red), Width = 40, Height = 40 };
            var outer = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Gray), Width = 200, Height = 100, Child = inner };
            var seen = new List<(UIElement Sender, TapEventArgs Args)>();
            inner.Tapped += (s, e) => seen.Add(((UIElement)s!, e));
            outer.Tapped += (s, e) => seen.Add(((UIElement)s!, e));
            canvas.Add(outer);
            canvas.Render();

            TapAt(input, Center(inner));

            Assert.Equal([inner, outer], seen.Select(x => x.Sender));
            Assert.Same(seen[0].Args, seen[1].Args);
            Assert.Same(inner, seen[0].Args.Source);
            Assert.Equal(Center(inner), seen[0].Args.ScreenPosition);
        }

        [Fact]
        public void GetPosition_IsLocalToTheElementAsked()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var target = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Gray), Width = 100, Height = 100, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(50, 20, 0, 0) };
            Vector2? local = null;
            target.Tapped += (_, e) => local = e.GetPosition(target);
            canvas.Add(target);
            canvas.Render();

            TapAt(input, new Point(60, 25));

            Assert.Equal(new Vector2(10, 5), local);
        }

        [Fact]
        public void HandlerSettingHandled_StopsTheBubble()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var inner = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Red), Width = 40, Height = 40 };
            var outer = new Border { Background = new Icy.Rendering.Brushes.SolidColorBrush(Color.Gray), Width = 200, Height = 100, Child = inner };
            int outerTaps = 0;
            inner.Tapped += (_, e) => e.Handled = true;
            outer.Tapped += (_, _) => outerTaps++;
            canvas.Add(outer);
            canvas.Render();

            TapAt(input, Center(inner));

            Assert.Equal(0, outerTaps);
        }

        [Fact]
        public void TapOnADisabledListItem_DoesNotSelectIt()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var list = new ListBox { ItemsSource = new List<object> { "a", "b" }, Width = 200, Height = 200, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            canvas.Add(list);
            canvas.Render();
            SelectorItem second = list.EnumerateVisualSubtree().OfType<SelectorItem>().ElementAt(1);
            second.IsEnabled = false;
            canvas.Render();

            TapAt(input, Center(second));

            Assert.Equal(-1, list.SelectedIndex);
        }
    }
}
