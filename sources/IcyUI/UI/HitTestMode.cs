// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.UI
{
    /// <summary>
    /// Specifies which elements a hit test can return.
    /// </summary>
    public enum HitTestMode
    {
        /// <summary>
        /// An element is hit where it paints (see <c>UIElement.HitTestSelf</c>): panels and presenters never are, and a
        /// border or control only with a background. This is how pointer input is routed.
        /// </summary>
        Painted,

        /// <summary>
        /// Every element is hit inside its bounds, painted or not: for design tools, which pick and drop into empty
        /// panels too.
        /// </summary>
        Bounds,
    }
}
