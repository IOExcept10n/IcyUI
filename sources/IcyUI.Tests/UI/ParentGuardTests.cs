using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.UI
{
    /// <summary>
    /// An element belongs to one parent: adding it somewhere else without removing it first throws, instead of
    /// leaving it listed by two parents (drawn twice, and detached from both when either lets it go).
    /// </summary>
    public class ParentGuardTests
    {
        [Fact]
        public void PanelAdd_ElementInAnotherPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => new StackPanel().Children.Add(element));
        }

        [Fact]
        public void PanelAdd_AfterRemovingFromOldPanel_Works()
        {
            var element = new UIElement();
            var oldPanel = new StackPanel();
            oldPanel.Children.Add(element);
            oldPanel.Children.Remove(element);

            var newPanel = new StackPanel();
            newPanel.Children.Add(element);

            Assert.Same(newPanel, element.Parent);
        }

        [Fact]
        public void PanelAdd_SameElementTwice_Throws()
        {
            var element = new UIElement();
            var panel = new StackPanel();
            panel.Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => panel.Children.Add(element));
            Assert.Single(panel.Children);
        }

        [Fact]
        public void PanelIndexerSet_ReparentsLikeRemoveAndAdd()
        {
            var panel = new StackPanel();
            var oldChild = new UIElement();
            var newChild = new UIElement();
            panel.Children.Add(oldChild);

            panel.Children[0] = newChild;

            Assert.Null(oldChild.Parent);
            Assert.Same(panel, newChild.Parent);
        }

        [Fact]
        public void PanelIndexerSet_ElementInAnotherPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);
            var panel = new StackPanel();
            panel.Children.Add(new UIElement());

            Assert.Throws<InvalidOperationException>(() => panel.Children[0] = element);
        }

        [Fact]
        public void PanelAdd_CanvasRoot_Throws()
        {
            var canvas = CreateCanvas();
            var element = new UIElement();
            canvas.Add(element);

            Assert.Throws<InvalidOperationException>(() => new StackPanel().Children.Add(element));
        }

        [Fact]
        public void CanvasAdd_ElementInPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => CreateCanvas().Add(element));
        }

        [Fact]
        public void CanvasAdd_RootOfAnotherCanvas_Throws()
        {
            var element = new UIElement();
            CreateCanvas().Add(element);

            Assert.Throws<InvalidOperationException>(() => CreateCanvas().Add(element));
        }

        [Fact]
        public void CanvasAdd_AfterRemove_Works()
        {
            var element = new UIElement();
            var first = CreateCanvas();
            first.Add(element);
            first.Remove(element);

            var second = CreateCanvas();
            second.Add(element);

            Assert.Same(second, element.Canvas);
        }

        [Fact]
        public void BorderChild_ElementInPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => new Border { Child = element });
        }

        [Fact]
        public void ContentControlContent_ElementOfAnotherButton_Throws()
        {
            var text = new TextBlock();
            _ = new Button { Content = text };

            Assert.Throws<InvalidOperationException>(() => new Button { Content = text });
        }

        [Fact]
        public void ContentControlContent_ThemedButtonsContentIntoPanel_Throws()
        {
            // Templated: a ContentPresenter inside the template hosts the content, not the button itself.
            var canvas = CreateCanvas(themed: true);
            var text = new TextBlock();
            canvas.Add(new Button { Content = text });
            canvas.Render();

            Assert.Throws<InvalidOperationException>(() => new StackPanel().Children.Add(text));
        }

        [Fact]
        public void ContentControlContent_Replaced_OldContentCanMoveElsewhere()
        {
            var text = new TextBlock();
            var button = new Button { Content = text };
            button.Content = new TextBlock();

            var panel = new StackPanel();
            panel.Children.Add(text);

            Assert.Same(panel, text.Parent);
        }

        [Fact]
        public void SplitPaneFirst_ElementInPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => new SplitPane { First = element });
        }

        [Fact]
        public void ExpanderContent_ElementInPanel_Throws()
        {
            var element = new UIElement();
            new StackPanel().Children.Add(element);

            Assert.Throws<InvalidOperationException>(() => new Expander { Content = element });
        }

        private static Canvas CreateCanvas(bool themed = false)
        {
            var config = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            return new Canvas(themed ? config.UseDefaultTheme() : config) { IsVisible = true };
        }
    }
}
