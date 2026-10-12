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
        public void ListBox_WithoutASize_SizesToItsItems()
        {
            // A width-to-content slot (Left alignment, a horizontal StackPanel, an Auto column) measured the list before it
            // had realized anything: zero wide, so it was arranged zero wide and stayed invisible.
            Canvas canvas = NewCanvas();
            var listBox = new ListBox { ItemsSource = new List<string> { "Forest", "Caves", "Castle" } };
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            panel.Children.Add(listBox);
            canvas.Add(panel);

            for (int frame = 0; frame < 3; frame++)
                canvas.Render();

            List<SelectorItem> items = Realized(listBox);
            Assert.Equal(3, items.Count);
            Assert.True(listBox.ActualBounds.Width > 0, $"The list is {listBox.ActualBounds.Width} wide.");
            Assert.All(items, item => Assert.True(item.ActualBounds.Width > 0, $"An item is {item.ActualBounds.Width} wide."));
            Assert.Equal(items.Sum(item => item.ActualBounds.Height), listBox.ActualBounds.Height);
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
