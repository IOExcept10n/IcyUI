// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Input;
using Icy.UI;
using Stride.Core;
using Stride.Games;

namespace Icy.Stride.Configuration
{
    /// <summary>
    /// IcyUI in a Stride game: a game system that pumps input and follows the display scale, with
    /// <see cref="SceneRenderer"/> drawing <see cref="Canvases"/> on top of the scene. Created by
    /// <see cref="StrideBuildingExtensions.UseIcyUI(IGame)"/>.
    /// </summary>
    public sealed class StrideIcyHost : GameSystemBase, IIcyHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrideIcyHost"/> class with a main canvas.
        /// </summary>
        /// <param name="registry">The game's service registry.</param>
        /// <param name="configuration">The configuration IcyUI runs with.</param>
        /// <param name="sceneRenderer">The render stage that draws the canvases.</param>
        internal StrideIcyHost(IServiceRegistry registry, IcyConfiguration configuration, IcyUISceneRenderer sceneRenderer)
            : base(registry)
        {
            Configuration = configuration;
            SceneRenderer = sceneRenderer;
            MainCanvas = new Canvas(configuration) { IsInputEnabled = true };
            sceneRenderer.Canvases.Add(MainCanvas);
        }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IcyConfiguration Configuration { get; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public Canvas MainCanvas { get; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IList<Canvas> Canvases => SceneRenderer.Canvases;

        /// <summary>
        /// Gets the render stage that draws <see cref="Canvases"/>, wrapped around the graphics compositor's game
        /// renderer by <see cref="StrideBuildingExtensions.UseIcyUI(IGame)"/>.
        /// </summary>
        [DataMemberIgnore]
        public IcyUISceneRenderer SceneRenderer { get; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Configuration.Input.Initialize();
            base.Initialize();
            Enabled = true;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (Configuration.Input is IUpdateableInput updateable)
                updateable.Update(gameTime.Elapsed);

            (Configuration.RenderContext as Rendering.RenderContext)?.PollDisplayScale();
            base.Update(gameTime);
        }
    }
}
