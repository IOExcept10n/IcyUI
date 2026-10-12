// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;

namespace Icy.UI
{
    /// <summary>
    /// IcyUI running inside a game: its configuration and the canvases the engine draws every frame. Returned by each
    /// engine integration's <c>UseIcyUI()</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Engine-independent game code can take an <see cref="IIcyHost"/>; the engine's own host type adds
    /// engine-specific settings.
    /// </para>
    /// <para>
    /// A canvas you render yourself with <see cref="Canvas.Render"/> (for example into a render target) must not be in
    /// <see cref="Canvases"/>, or it's drawn twice.
    /// </para>
    /// </remarks>
    public interface IIcyHost
    {
        /// <summary>Gets the configuration every element in this game is created with.</summary>
        IcyConfiguration Configuration { get; }

        /// <summary>Gets the canvas <c>UseIcyUI()</c> created, with input enabled; <see cref="Canvases"/>[0].</summary>
        Canvas MainCanvas { get; }

        /// <summary>Gets the canvases drawn every frame, in order: a later canvas draws on top of an earlier one.</summary>
        IList<Canvas> Canvases { get; }
    }
}
