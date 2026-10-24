// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.ComponentModel;
using Icy.Data.Markup.Attributes;
using Icy.UI.Styles;

namespace Icy.UI.Controls
{
    /// <summary>
    /// The container a <see cref="Selector"/> realizes each item as - an <see cref="ItemContainer"/> with
    /// selection/highlight state layered on top.
    /// </summary>
    /// <remarks>
    /// <see cref="IsSelected"/>/<see cref="IsHighlighted"/> mirror <see cref="ToggleButton.IsChecked"/>'s own
    /// shape (set/clear a <see cref="ControlState"/> flag, no other side effects here) - <see cref="Selector"/>
    /// itself is what keeps them in sync with <see cref="SelectingItemsControl.SelectedIndex"/>/its internal highlighted
    /// index, including across a pool-and-reuse cycle (see <see cref="Selector.AttachContainer(ItemContainer, int)"/>).
    /// </remarks>
    public class SelectorItem : ItemContainer
    {
        private bool isHighlighted;
        private bool isSelected;

        /// <summary>
        /// Gets or sets a value indicating whether this item is the <see cref="Selector"/>'s current
        /// <see cref="SelectingItemsControl.SelectedItem"/>. Setting this sets/clears <see cref="ControlState.Selected"/>.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [RegisterReference]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (!SetProperty(ref isSelected, value))
                    return;
                ControlState = value ? ControlState | ControlState.Selected : ControlState & ~ControlState.Selected;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item is the <see cref="Selector"/> popup's current
        /// keyboard/gamepad-highlighted item. Setting this sets/clears <see cref="ControlState.Highlighted"/>.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        [RegisterReference]
        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (!SetProperty(ref isHighlighted, value))
                    return;
                ControlState = value ? ControlState | ControlState.Highlighted : ControlState & ~ControlState.Highlighted;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Raises <see cref="UIElement.Tapped"/>, which <see cref="WrapGrid"/>/<see cref="ListBox"/> select the item from,
        /// and marks the tap handled. Skipped while <see cref="UIElement.IsEnabled"/> is <see langword="false"/>: the
        /// canvas never hits a disabled item anyway (see <see cref="UIElement.IsHitTestVisible"/>), and this guard keeps a
        /// caller invoking it directly from selecting one.
        /// </remarks>
        protected internal override void OnTap(TapEventArgs e)
        {
            if (!IsEnabled)
                return;

            base.OnTap(e);
            e.Handled = true;
        }
    }
}
