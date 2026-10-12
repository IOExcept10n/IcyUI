using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// An items control that no ScrollViewer hosts uses its own size as the viewport, so a bare list in a panel shows the
    /// items that fit instead of nothing.
    /// </summary>
    public class ItemsControlWithoutScrollHostTests
    {
        private static Canvas NewCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme())
            {
                IsVisible = true,
            };

        private static List<SelectorItem> Realized(UIElement control) =>
            control.EnumerateVisualSubtree().OfType<SelectorItem>().ToList();

        [Fact]
        public void ListBox_InAPanel_RealizesItsItems()
        {
            Canvas canvas = NewCanvas();
            var listBox = new ListBox { ItemsSource = new List<string> { "Forest", "Caves", "Castle" }, Height = 96 };
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(listBox);
            canvas.Add(panel);

            canvas.Render();

            Assert.Equal(3, Realized(listBox).Count);
        }

        [Fact]
        public void ListBox_InAPanel_RealizesOnlyWhatFits()
        {
            Canvas canvas = NewCanvas();
            var listBox = new ListBox { ItemsSource = Enumerable.Range(1, 500).Select(i => $"Item {i}").ToList(), Height = 60 };
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(listBox);
            canvas.Add(panel);

            canvas.Render();

            int realized = Realized(listBox).Count;
            Assert.InRange(realized, 1, 100);
        }

        [Fact]
        public void ClosedComboBox_RealizesNothing()
        {
            Canvas canvas = NewCanvas();
            var comboBox = new ComboBox { ItemsSource = new List<string> { "Easy", "Normal", "Hard" } };
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(comboBox);
            canvas.Add(panel);

            canvas.Render();

            Assert.Empty(Realized(comboBox));
        }
    }
}
