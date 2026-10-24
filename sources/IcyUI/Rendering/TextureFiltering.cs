// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Rendering
{
    /// <summary>
    /// Specifies how a texture's pixels are sampled when it's drawn larger or smaller than its size.
    /// </summary>
    public enum TextureFiltering
    {
        /// <summary>
        /// Neighbouring pixels are blended, so a scaled image looks smooth. Right for photos, painted art and smooth fonts.
        /// </summary>
        Linear,

        /// <summary>
        /// The nearest pixel is taken, so a scaled image keeps hard pixel edges. Right for pixel art and pixel fonts.
        /// </summary>
        Point,
    }
}
