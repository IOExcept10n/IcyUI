// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections;
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings added from code, per culture. Also a safety net at the end of a <see cref="CompositeLocalizer"/>.
    /// </summary>
    /// <remarks>
    /// A lookup tries the culture, its parents (<c>ru-RU</c>, then <c>ru</c>), then the strings added without a culture.
    /// Supports collection initializers: <c>new DictionaryLocalizer { { ru, "Menu.Play", "Играть" } }</c>.
    /// </remarks>
    public sealed class DictionaryLocalizer : ILocalizer, IEnumerable
    {
        private readonly Dictionary<(string Culture, string Key), string> strings = [];

        /// <summary>
        /// Adds or replaces the string for <paramref name="key"/> in <paramref name="culture"/>.
        /// </summary>
        /// <param name="culture">The language of <paramref name="value"/>.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The localized string.</param>
        public void Add(CultureInfo culture, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(culture);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            strings[(culture.Name, key)] = value;
        }

        /// <summary>
        /// Adds or replaces the string for <paramref name="key"/> used when no culture-specific one exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The string.</param>
        public void Add(string key, string value) => Add(CultureInfo.InvariantCulture, key, value);

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(culture);
            for (CultureInfo current = culture; ; current = current.Parent)
            {
                if (strings.TryGetValue((current.Name, key), out string? value))
                    return value;
                if (current.Equals(CultureInfo.InvariantCulture))
                    return null;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => strings.GetEnumerator();
    }
}
