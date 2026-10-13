// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// Chains localizers: asks each in order, and the first string found wins.
    /// </summary>
    /// <param name="localizers">
    /// The localizers, the most important first: mod files before the game's own, a <see cref="DictionaryLocalizer"/>
    /// safety net last.
    /// </param>
    public sealed class CompositeLocalizer(params ILocalizer[] localizers) : ILocalizer
    {
        private readonly ILocalizer[] localizers = localizers ?? throw new ArgumentNullException(nameof(localizers));

        /// <summary>
        /// Gets the localizers, in the order they're asked.
        /// </summary>
        public IReadOnlyList<ILocalizer> Localizers => localizers;

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            foreach (ILocalizer localizer in localizers)
            {
                if (localizer.Localize(key, culture) is { } text)
                    return text;
            }

            return null;
        }
    }
}
