// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Markup;
using Icy.UI;

namespace Icy.Localization
{
    /// <summary>
    /// The localized properties of one configuration, re-applied when its culture changes. Holds its elements weakly
    /// and does nothing between changes.
    /// </summary>
    internal sealed class LocalizedPropertyRegistry
    {
        private readonly List<Entry> entries = [];

        /// <summary>
        /// Gets the number of properties tracked, for tests.
        /// </summary>
        public int Count => entries.Count;

        /// <summary>
        /// Converts a localized string to what <paramref name="member"/> holds: the string itself, or what the
        /// configuration's text converter makes of it (a <see cref="UI.Controls.TextBlock"/> for <c>Content</c>).
        /// </summary>
        /// <param name="configuration">The configuration whose text converter is used.</param>
        /// <param name="member">The property the string is for.</param>
        /// <param name="text">The localized string.</param>
        /// <returns>The value to assign.</returns>
        public static object? ToPropertyValue(IcyConfiguration configuration, MarkupMember member, string text) =>
            member.PropertyType.IsAssignableFrom(typeof(string))
                ? text
                : configuration.Types.TypeConverter.Convert(text, member.PropertyType);

        /// <summary>
        /// Tracks <paramref name="member"/> of <paramref name="target"/>, which now holds <paramref name="value"/>.
        /// </summary>
        /// <param name="target">The element.</param>
        /// <param name="member">The localized property.</param>
        /// <param name="key">The key.</param>
        /// <param name="context">The key's context, or <see langword="null"/>.</param>
        /// <param name="value">The value the property holds now.</param>
        public void Register(UIElement target, MarkupMember member, string key, string? context, object? value) =>
            entries.Add(new Entry(new WeakReference<UIElement>(target), member, key, context, value));

        /// <summary>
        /// Gives every tracked property its string in the configuration's current culture. Drops collected elements,
        /// and properties the game has set to something else since.
        /// </summary>
        /// <param name="configuration">The configuration whose culture changed.</param>
        public void Reapply(IcyConfiguration configuration) => entries.RemoveAll(entry => !entry.Reapply(configuration));

        private sealed class Entry(WeakReference<UIElement> target, MarkupMember member, string key, string? context, object? value)
        {
            private object? lastValue = value;

            public bool Reapply(IcyConfiguration configuration)
            {
                if (!target.TryGetTarget(out UIElement? element) || !Equals(member.GetValue(element), lastValue))
                    return false;

                object? next = ToPropertyValue(configuration, member, configuration.Localize(key, context));
                member.SetValue(element, next);
                lastValue = next;
                return true;
            }
        }
    }
}
