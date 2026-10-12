// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Input;
using Icy.MonoGame.Rendering;
using Icy.UI;
using Microsoft.Xna.Framework;

namespace Icy.MonoGame.Configuration
{
    /// <summary>
    /// IcyUI in a MonoGame game: a game component that pumps input, follows the display scale and draws
    /// <see cref="Canvases"/> every frame. Created by <see cref="MonoGameBuildingExtensions.UseIcyUI(Game)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Components draw during <c>base.Draw()</c> in your game's <c>Draw</c>, and <see cref="DrawableGameComponent.DrawOrder"/>
    /// defaults to <see cref="int.MaxValue"/>, so the UI covers what your game and its other components draw. Anything
    /// you draw after <c>base.Draw()</c> lands on top of the UI.
    /// </para>
    /// </remarks>
    public sealed class MonoGameIcyHost : DrawableGameComponent, IIcyHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonoGameIcyHost"/> class with a main canvas.
        /// </summary>
        /// <param name="game">The game the host belongs to.</param>
        /// <param name="configuration">The configuration IcyUI runs with.</param>
        internal MonoGameIcyHost(Game game, IcyConfiguration configuration)
            : base(game)
        {
            Configuration = configuration;
            MainCanvas = new Canvas(configuration) { IsInputEnabled = true };
            Canvases = [MainCanvas];
            DrawOrder = int.MaxValue;
        }

        /// <inheritdoc/>
        public IcyConfiguration Configuration { get; }

        /// <inheritdoc/>
        public Canvas MainCanvas { get; }

        /// <inheritdoc/>
        public IList<Canvas> Canvases { get; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Configuration.Input.Initialize();
            base.Initialize();
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (Configuration.Input is IUpdateableInput updateable)
                updateable.Update(gameTime.ElapsedGameTime);

            (Configuration.RenderContext as RenderContext)?.PollDisplayScale();
            base.Update(gameTime);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            // By index: a canvas added or removed by UI code during this frame mustn't break the loop.
            for (int i = 0; i < Canvases.Count; i++)
                Canvases[i].Render();
            base.Draw(gameTime);
        }
    }
}
