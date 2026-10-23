using System.Windows.Input;
using Icy.Assets;
using Icy.Configuration;
using Icy.Data.Markup;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// A <see cref="Button"/> is disabled while its <see cref="Button.Command"/> can't execute (ICY-007), and
    /// <see cref="UIElement.IsEnabled"/> reports that effective value.
    /// </summary>
    public class ButtonCommandStateTests
    {
        private static Canvas CreateCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()));

        private static Button Attached(Canvas canvas, Button button)
        {
            canvas.Add(button);
            return button;
        }

        [Fact]
        public void CommandThatCannotExecute_DisablesTheButton()
        {
            var canvas = CreateCanvas();
            var button = Attached(canvas, new Button { Command = new ToggleCommand(false) });

            Assert.False(button.IsEnabled);
            Assert.True(button.ControlState.HasFlag(ControlState.Disabled));
            Assert.False(button.IsHitTestVisible);
        }

        [Fact]
        public void CanExecuteChanged_ReEnablesTheButton_AndNotifiesIsEnabled()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(false);
            var button = Attached(canvas, new Button { Command = command });
            var changed = new List<string?>();
            button.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            command.Set(true);

            Assert.True(button.IsEnabled);
            Assert.False(button.ControlState.HasFlag(ControlState.Disabled));
            Assert.Contains(nameof(UIElement.IsEnabled), changed);
        }

        [Fact]
        public void ExplicitlyDisabledButton_StaysDisabled_WhenTheCommandCanExecute()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(false);
            var button = Attached(canvas, new Button { Command = command, IsEnabled = false });

            command.Set(true);

            Assert.False(button.IsEnabled);
        }

        [Fact]
        public void SwappingTheCommand_FollowsTheNewOne_AndIgnoresTheOld()
        {
            var canvas = CreateCanvas();
            var old = new ToggleCommand(true);
            var button = Attached(canvas, new Button { Command = old });

            var replacement = new ToggleCommand(false);
            button.Command = replacement;
            Assert.False(button.IsEnabled);

            old.Set(false);
            replacement.Set(true);
            old.Set(false);

            Assert.True(button.IsEnabled);
            Assert.Equal(0, old.SubscriberCount);
        }

        [Fact]
        public void ChangingTheCommandParameter_RequeriesCanExecute()
        {
            var canvas = CreateCanvas();
            var button = Attached(canvas, new Button { Command = new ParameterCommand(), CommandParameter = "no" });
            Assert.False(button.IsEnabled);

            button.CommandParameter = "yes";

            Assert.True(button.IsEnabled);
        }

        [Fact]
        public void DetachedButton_Unsubscribes_AndPicksUpTheCurrentStateOnReattach()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(true);
            var button = Attached(canvas, new Button { Command = command });

            canvas.Remove(button);
            Assert.Equal(0, command.SubscriberCount);
            command.Set(false);
            canvas.Add(button);

            Assert.False(button.IsEnabled);
            Assert.Equal(1, command.SubscriberCount);
        }

        [Fact]
        public void CanExecuteChangedOffTheUIThread_AppliesOnTheNextDispatcherUpdate()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(true);
            var button = Attached(canvas, new Button { Command = command });

            // A dedicated thread: Task.Wait could inline the work onto this one, and an await could resume elsewhere.
            var worker = new Thread(() => command.Set(false));
            worker.Start();
            worker.Join();
            Assert.True(button.IsEnabled);

            button.Dispatcher.Update();

            Assert.False(button.IsEnabled);
        }

        [Fact]
        public void StyleTierClearedWhileTheCommandCannotExecute_RestoresTheStoredValue_NotTheComputedOne()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(false);
            var button = Attached(canvas, new Button { Command = command });
            Assert.True(button.GetPropertyStore().TryGetProperty(nameof(UIElement.IsEnabled), out IPropertyReference? isEnabled));

            isEnabled.SetTierValue(button, PropertyValuePrecedence.Style, true);
            isEnabled.ClearTierValue(button, PropertyValuePrecedence.Style);
            command.Set(true);

            Assert.True(button.IsEnabled);
        }

        [Fact]
        public void ComputedIsEnabledChange_IsNotALocalValue_SoAStyleStillApplies()
        {
            var canvas = CreateCanvas();
            var command = new ToggleCommand(true);
            var button = Attached(canvas, new Button { Command = command });
            Assert.True(button.GetPropertyStore().TryGetProperty(nameof(UIElement.IsEnabled), out IPropertyReference? isEnabled));

            command.Set(false);
            command.Set(true);
            isEnabled.SetTierValue(button, PropertyValuePrecedence.Style, false);

            Assert.False(button.IsEnabled);
        }

        [Fact]
        public void PooledItemButton_ReboundToAnotherItem_FollowsThatItemsCommand()
        {
            var canvas = CreateCanvas();
            var first = new Item(new ToggleCommand(true));
            var second = new Item(new ToggleCommand(false));
            var template = (DataTemplate)new MarkupLoader(canvas.Configuration).LoadObject(
                """<DataTemplate><Button Command="{Binding Command}"/></DataTemplate>""");
            var control = new ItemsControl { ItemsSource = new List<object> { first, second }, ItemTemplate = template };
            canvas.Add(control);
            Invoke(control, "EnsureRealized", 0);
            ItemContainer container = RealizedContainers(control)[0];

            Invoke(control, "Derealize", 0);
            Invoke(control, "EnsureRealized", 1);

            Assert.Same(container, RealizedContainers(control)[1]);
            var button = container.EnumerateVisualSubtree().OfType<Button>().Single();
            Assert.False(button.IsEnabled);
            second.Command.Set(true);
            Assert.True(button.IsEnabled);
            Assert.Equal(0, first.Command.SubscriberCount);
        }

        private static void Invoke(ItemsControl control, string method, int index) =>
            typeof(ItemsControl).GetMethod(method, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(control, [index]);

        private static Dictionary<int, ItemContainer> RealizedContainers(ItemsControl control) =>
            (Dictionary<int, ItemContainer>)typeof(ItemsControl).GetProperty("RealizedContainers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(control)!;

        public sealed record Item(ToggleCommand Command);

        public sealed class ToggleCommand(bool canExecute) : ICommand
        {
            private EventHandler? canExecuteChanged;

            public event EventHandler? CanExecuteChanged
            {
                add => canExecuteChanged += value;
                remove => canExecuteChanged -= value;
            }

            public int SubscriberCount => canExecuteChanged?.GetInvocationList().Length ?? 0;

            public bool CanExecute(object? parameter) => canExecute;

            public void Execute(object? parameter)
            {
            }

            public void Set(bool value)
            {
                canExecute = value;
                canExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private sealed class ParameterCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => Equals(parameter, "yes");

            public void Execute(object? parameter)
            {
            }
        }
    }
}
