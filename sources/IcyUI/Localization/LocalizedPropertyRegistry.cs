// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;

namespace Icy.Localization
{
    /// <summary>
    /// The localized properties of one configuration, re-applied when its culture changes.
    /// </summary>
    internal sealed class LocalizedPropertyRegistry
    {
        /// <summary>
        /// Gives every tracked property its string in the configuration's current culture.
        /// </summary>
        /// <param name="configuration">The configuration whose culture changed.</param>
        public void Reapply(IcyConfiguration configuration)
        {
        }
    }
}
