// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Rendering.Fonts
{
    /// <summary>
    /// A fallback resolver that wants to know about characters neither a font nor any fallback could draw.
    /// </summary>
    internal interface IMissingGlyphReporter
    {
        /// <summary>
        /// Reports that <paramref name="codepoint"/> isn't drawn in <paramref name="font"/>.
        /// </summary>
        /// <param name="font">The font the text asked for.</param>
        /// <param name="codepoint">The missing character.</param>
        void ReportMissingGlyph(FontInfo font, int codepoint);
    }
}
