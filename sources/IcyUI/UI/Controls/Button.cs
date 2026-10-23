// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.ComponentModel;
using System.Windows.Input;
using Icy.Data.Markup.Attributes;
using Icy.Input;
using Icy.Input.Events;

namespace Icy.UI.Controls
{
    /// <summary>
    /// A <see cref="ContentControl"/> that fires <see cref="Click"/>/executes <see cref="Command"/> when tapped
    /// (see <see cref="ITouchEvents"/>) and is a tab-stop by default.
    /// </summary>
    /// <remarks>
    /// While <see cref="Command"/> can't execute with <see cref="CommandParameter"/>, the button is disabled:
    /// <see cref="UIElement.IsEnabled"/> reads <see langword="false"/>, and the button takes no input and shows the
    /// theme's Disabled look. It follows <see cref="ICommand.CanExecuteChanged"/> while it's on a canvas, and queries the
    /// command again when it's attached, so a detached button holds no subscription to a long-lived command.
    /// </remarks>
    public class Button : ContentControl, ICommandSource
    {
        private ICommand? command;
        private object? commandParameter;
        private bool canExecute = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button()
        {
            IsFocusable = true;
        }

        /// <summary>
        /// Occurs when the button is tapped/activated.
        /// </summary>
        public event EventHandler? Click;

        /// <summary>
        /// Gets or sets the command to execute when the button is tapped/activated, in addition to raising <see cref="Click"/>.
        /// </summary>
        /// <remarks>The button is disabled while the command can't execute; see the remarks on <see cref="Button"/>.</remarks>
        [Category("Behavior")]
        [DefaultValue(null)]
        [RegisterReference]
        public ICommand? Command
        {
            get => command;
            set
            {
                ICommand? old = command;
                if (!SetProperty(ref command, value))
                    return;

                if (Canvas != null)
                {
                    if (old != null)
                        old.CanExecuteChanged -= OnCommandCanExecuteChanged;
                    if (value != null)
                        value.CanExecuteChanged += OnCommandCanExecuteChanged;
                }

                RequeryCanExecute();
            }
        }

        /// <summary>
        /// Gets or sets the parameter passed to <see cref="Command"/>.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(null)]
        [RegisterReference]
        public object? CommandParameter
        {
            get => commandParameter;
            set
            {
                if (SetProperty(ref commandParameter, value))
                    RequeryCanExecute();
            }
        }

        /// <inheritdoc/>
        /// <remarks><see langword="false"/> while <see cref="Command"/> can't execute.</remarks>
        protected override bool IsEnabledCore => canExecute;

        /// <inheritdoc/>
        void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => OnCommandCanExecuteChanged(sender, e);

        /// <inheritdoc/>
        protected internal override void OnTap()
        {
            base.OnTap();
            OnClick();
        }

        /// <inheritdoc/>
        /// <remarks>Clicks the button, like a tap, unless it's disabled.</remarks>
        protected internal override bool OnActivate()
        {
            if (!IsEnabled)
                return false;

            OnClick();
            return true;
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            if (command != null)
                command.CanExecuteChanged += OnCommandCanExecuteChanged;
            RequeryCanExecute();
            base.OnAttached();
        }

        /// <inheritdoc/>
        protected override void OnDetached()
        {
            if (command != null)
                command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            base.OnDetached();
        }

        /// <summary>
        /// Raises <see cref="Click"/> and executes <see cref="Command"/> if it can execute.
        /// </summary>
        protected virtual void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);
        }

        private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            // Commands may raise this from any thread, such as an async command finishing on the thread pool.
            if (CheckAccess())
                RequeryCanExecute();
            else
                Dispatcher.Invoke(RequeryCanExecute);
        }

        private void RequeryCanExecute()
        {
            bool wasEnabled = IsEnabled;
            canExecute = command?.CanExecute(commandParameter) ?? true;
            InvalidateIsEnabled(wasEnabled);
        }
    }
}
