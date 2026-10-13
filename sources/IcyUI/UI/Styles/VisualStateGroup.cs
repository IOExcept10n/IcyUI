// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Markup;

namespace Icy.UI.Styles
{
    /// <summary>
    /// Represents a mutually-exclusive set of <see cref="VisualState"/>s - at most one state within a group is active
    /// on a given element at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Separate groups are independent: an element has one active state per group, so states that must combine
    /// (a hover background and a focus border) belong in different groups. When two active states in different
    /// groups set the same property, the group registered later wins.
    /// </para>
    /// </remarks>
    [ContentProperty(nameof(States))]
    public class VisualStateGroup
    {
        /// <summary>
        /// Gets the optional name of this group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The name is what lets a style override a group: registering a group on an element replaces the group with
        /// the same name already registered there (see <see cref="UIElement.RegisterStateGroup(VisualStateGroup)"/>).
        /// Since a <see cref="Style.BasedOn"/> base and the theme's style register before the style itself, a
        /// style's <c>CommonStates</c> group replaces the theme's <c>CommonStates</c> instead of adding to it.
        /// </para>
        /// <para>
        /// A group with no name (<see langword="null"/> or empty) never replaces another group.
        /// </para>
        /// </remarks>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the states within this group.
        /// </summary>
        public List<VisualState> States { get; } = [];
    }
}
