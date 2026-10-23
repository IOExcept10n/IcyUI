// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using System.Numerics;

namespace Icy.UI
{
    /// <summary>
    /// Provides data for <see cref="UIElement.Tapped"/>: where a tap landed, which element it hit, and whether an element
    /// already handled it.
    /// </summary>
    /// <remarks>
    /// One instance travels from the hit element up through its ancestors (see <see cref="UIElement.OnTap(TapEventArgs)"/>),
    /// so <see cref="Handled"/> set on one element is seen by the rest, which then don't get the tap.
    /// </remarks>
    public class TapEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TapEventArgs"/> class.
        /// </summary>
        /// <param name="source">The element the tap hit.</param>
        /// <param name="screenPosition">Where the tap landed, in screen space.</param>
        public TapEventArgs(UIElement source, Point screenPosition)
        {
            ArgumentNullException.ThrowIfNull(source);
            Source = source;
            ScreenPosition = screenPosition;
        }

        /// <summary>
        /// Gets the element the tap hit: the innermost one under the pointer.
        /// </summary>
        public UIElement Source { get; }

        /// <summary>
        /// Gets where the tap landed, in screen (window) space.
        /// </summary>
        public Point ScreenPosition { get; }

        /// <summary>
        /// Gets or sets a value indicating whether an element has handled the tap; once <see langword="true"/>, the
        /// remaining ancestors don't get it.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets where the tap landed relative to <paramref name="relativeTo"/>'s top-left corner.
        /// </summary>
        /// <param name="relativeTo">The element to measure from.</param>
        /// <returns>The position in <paramref name="relativeTo"/>'s local space.</returns>
        public Vector2 GetPosition(UIElement relativeTo)
        {
            ArgumentNullException.ThrowIfNull(relativeTo);
            return relativeTo.PointToLocal(ScreenPosition);
        }
    }
}
