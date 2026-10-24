// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;

namespace Icy.Rendering
{
    /// <summary>
    /// Represents an interface for storing the global renderer options.
    /// </summary>
    public interface IRenderOptions
    {
        /// <summary>
        /// Gets or sets the opacity of current drawing pass.
        /// </summary>
        float Opacity { get; set; }

        /// <summary>
        /// Gets or sets the area of scissor rectangle.
        /// </summary>
        Rectangle Scissor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current render pass supports visual effects such as shaders.
        /// </summary>
        /// <remarks>
        /// Note that enabling effect may affect the performance because effects applying can interrupt texture batching.
        /// </remarks>
        bool EnableEffects { get; set; }

        /// <summary>
        /// Gets or sets how textures drawn from now on are sampled when scaled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Engine integrations switch the sampler between draw batches, so a change ends the current batch; the canvas and
        /// <see cref="UI.UIElement.TextureFiltering"/> only set it where it actually changes.
        /// </para>
        /// <para>
        /// The default implementation always samples linearly and ignores the setter, for render contexts that don't
        /// support point sampling.
        /// </para>
        /// </remarks>
        TextureFiltering Filtering
        {
            get => TextureFiltering.Linear;
            set { }
        }
    }
}
