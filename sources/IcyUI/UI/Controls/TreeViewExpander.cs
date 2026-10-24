// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.UI.Controls
{
    /// <summary>
    /// The chevron of a <see cref="TreeViewItem"/>: an <see cref="Icon"/> that reports its own taps, so a tap on it opens
    /// or closes the row without selecting it.
    /// </summary>
    /// <remarks>
    /// It raises <see cref="UIElement.Tapped"/> and marks the tap handled, so the tap never reaches its
    /// <see cref="TreeViewItem"/> (see <see cref="UIElement.OnTap(TapEventArgs)"/>). Name it
    /// <see cref="TreeViewItem.ExpanderPartName"/> in a <see cref="TreeViewItem"/> template.
    /// </remarks>
    public class TreeViewExpander : Icon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewExpander"/> class, showing <see cref="IconKind.ChevronRight"/>.
        /// </summary>
        public TreeViewExpander()
        {
            Kind = IconKind.ChevronRight;
        }

        /// <inheritdoc/>
        /// <remarks>Raises <see cref="UIElement.Tapped"/> while enabled and marks the tap handled, so the row isn't tapped too.</remarks>
        protected internal override void OnTap(TapEventArgs e)
        {
            if (!IsEnabled)
                return;

            base.OnTap(e);
            e.Handled = true;
        }
    }
}
