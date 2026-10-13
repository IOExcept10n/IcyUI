// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// A source of localized strings: localization files, RESX resources, a dictionary filled from code, or any
    /// other place a game keeps its translations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IcyUI asks the configuration's localizer (<see cref="Configuration.AssetConfiguration.Localizer"/>) for every
    /// localized string, in the configuration's <see cref="Configuration.IcyConfiguration.Culture"/>. Built in:
    /// <see cref="DictionaryLocalizer"/>, <see cref="ResourceManagerLocalizer"/>, <see cref="PoLocalizer"/>, and
    /// <see cref="CompositeLocalizer"/> to chain them.
    /// </para>
    /// </remarks>
    public interface ILocalizer
    {
        /// <summary>
        /// Gets the string for <paramref name="key"/> in <paramref name="culture"/>.
        /// </summary>
        /// <param name="key">
        /// The key. A key with a context is <c>context + '\u0004' + key</c>, as gettext stores it.
        /// </param>
        /// <param name="culture">The language to localize into.</param>
        /// <returns>
        /// The string, or <see langword="null"/> when this localizer has none: the caller decides what to show.
        /// Implementations try <paramref name="culture"/>'s parents (<c>ru-RU</c>, then <c>ru</c>) when their source
        /// supports it.
        /// </returns>
        string? Localize(string key, CultureInfo culture);
    }
}
