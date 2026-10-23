// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.UI
{
    /// <summary>
    /// Specifies whether text breaks into lines to fit the width it has.
    /// </summary>
    public enum TextWrapping
    {
        /// <summary>
        /// The text breaks only at explicit line breaks (<c>\n</c>), however wide it gets.
        /// </summary>
        NoWrap,

        /// <summary>
        /// The text also breaks where a line would get wider than the width available: at a space, or inside a word
        /// that is wider than the whole line by itself.
        /// </summary>
        Wrap,
    }
}
