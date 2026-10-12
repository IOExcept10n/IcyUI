// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Icy.Configuration;
using Icy.Rendering.Display;
using Icy.Stride.Assets;
using Icy.Stride.Assets.Importers;
using Icy.Stride.Diagnostics;
using Icy.Stride.Rendering;
using Icy.UI;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering.Compositing;

namespace Icy.Stride.Configuration
{
    /// <summary>
    /// Provides extension methods for configuring IcyUI in Stride applications.
    /// </summary>
    public static class StrideBuildingExtensions
    {
        /// <summary>
        /// Sets IcyUI up in the game with the default Stride configuration: the default theme, markup support,
        /// system fonts with <c>"Segoe UI"</c>, and log messages in Stride's log.
        /// </summary>
        /// <param name="game">The game; call this from a startup script's <c>Start()</c>, or after <c>BeginRun()</c> in a code-only game.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game, or the scene has no graphics compositor yet.</exception>
        public static StrideIcyHost UseIcyUI(this IGame game) => game.UseIcyUI(static _ => { });

        /// <summary>
        /// Sets IcyUI up in the game: the default Stride configuration first, then <paramref name="configure"/>.
        /// </summary>
        /// <param name="game">The game; call this from a startup script's <c>Start()</c>, or after <c>BeginRun()</c> in a code-only game.</param>
        /// <param name="configure">Changes the defaults, for example <c>ui =&gt; ui.ConfigureFonts().WithDefaultFontFamily("Arial")</c>.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game, or the scene has no graphics compositor yet.</exception>
        public static StrideIcyHost UseIcyUI(this IGame game, Action<IConfigurationBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(configure);
            if (game.TryGetIcyHost(out _))
                return ThrowHelper.ThrowInvalidOperationException<StrideIcyHost>("IcyUI is already set up for this game; use GetIcyHost() to reach it.");

            GraphicsCompositor compositor = game.Services.GetService<SceneSystem>()?.GraphicsCompositor
                ?? ThrowHelper.ThrowInvalidOperationException<GraphicsCompositor>("UseIcyUI() needs the scene's graphics compositor: call it from a startup script's Start(), or after BeginRun() in a code-only game.");

            var builder = new IcyConfigurationBuilder();
            builder.WithDefaultStrideConfiguration(game);
            configure(builder);
            IcyConfiguration configuration = builder.Build();

            var overlay = new IcyUISceneRenderer { Configuration = configuration };
            var host = new StrideIcyHost(game.Services, configuration, overlay);
            game.GameSystems.Add(host);
            compositor.Game = new CompositeSceneRenderer(compositor.Game, overlay);
            return host;
        }

        /// <summary>
        /// Gets the IcyUI host of the game, if <see cref="UseIcyUI(IGame)"/> ran.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="host">The host, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when IcyUI is set up.</returns>
        public static bool TryGetIcyHost(this IGame game, [NotNullWhen(true)] out StrideIcyHost? host)
        {
            ArgumentNullException.ThrowIfNull(game);
            host = game.GameSystems.OfType<StrideIcyHost>().FirstOrDefault();
            return host != null;
        }

        /// <summary>
        /// Gets the IcyUI host of the game.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns>The host <see cref="UseIcyUI(IGame)"/> created.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(IGame)"/> first.</exception>
        public static StrideIcyHost GetIcyHost(this IGame game) =>
            game.TryGetIcyHost(out StrideIcyHost? host)
                ? host
                : ThrowHelper.ThrowInvalidOperationException<StrideIcyHost>("IcyUI isn't set up for this game yet. Call UseIcyUI() first.");

        /// <summary>Gets the configuration IcyUI runs with in the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.Configuration"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(IGame)"/> first.</exception>
        public static IcyConfiguration GetIcyConfiguration(this IGame game) => game.GetIcyHost().Configuration;

        /// <summary>Gets the main canvas of the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.MainCanvas"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(IGame)"/> first.</exception>
        public static Canvas GetIcyCanvas(this IGame game) => game.GetIcyHost().MainCanvas;

        /// <summary>
        /// Configures the default settings for Stride in the provided configuration builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Besides the Stride rendering, input and asset importers, the defaults are:
        /// </para>
        /// <list type="bullet">
        /// <item><description>markup support (<see cref="BuildingExtensions.AddMarkupSupport"/>);</description></item>
        /// <item><description>the built-in theme (<see cref="BuildingExtensions.UseDefaultTheme(IConfigurationBuilder)"/>);</description></item>
        /// <item><description>system fonts, with <c>"Segoe UI"</c> as the default family;</description></item>
        /// <item><description>log messages in Stride's log (<c>GlobalLogger</c>), shown in the game console and Game Studio's output.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="builder">The configuration builder instance to configure.</param>
        /// <param name="game">The game used for configuration.</param>
        /// <returns>The updated configuration builder instance for fluent configuration.</returns>
        public static IConfigurationBuilder WithDefaultStrideConfiguration(this IConfigurationBuilder builder, IGame game)
        {
            var renderContext = new Rendering.RenderContext(game.GraphicsDevice);
            renderContext.AttachDisplayScaleTracker(new DisplayScaleTracker(DisplayScales.GetProvider(), () => StrideNativeWindow.GetInfo(game)));

            // In fullscreen, Stride normalizes touches over the whole physical panel (see Input.Devices.TouchInput).
            IDesktopSizeProvider desktopSizes = DesktopSizes.GetProvider();
            System.Drawing.Size? TouchSurfaceSize() =>
                game.Window.IsFullscreen && desktopSizes.TryGetDesktopSize(StrideNativeWindow.GetInfo(game), out var panel) ? panel : null;
            builder.ConfigureRendering(renderContext)
                   .ConfigureInput(new Input.InputSystem(
                       game.Services.GetService<global::Stride.Input.InputManager>()
                       ?? ThrowHelper.ThrowInvalidOperationException<global::Stride.Input.InputManager>("The game has no InputManager service registered. Configure IcyUI after the game has been initialized."),
                       () => renderContext.ViewportSize,
                       TouchSurfaceSize))
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithAssetContextFactory(new StrideAssetContextFactory(game))
                   .AddBasicFontSupport()
                   .UseStrideImporters(game);
            builder.AddMarkupSupport();
            builder.UseDefaultTheme();
            builder.ConfigureFonts().UseSystemFonts().WithDefaultFontFamily("Segoe UI");
            builder.ConfigureLogging().AddLoggerProvider(new StrideLoggerProvider());
            return builder;
        }

        /// <summary>
        /// Adds Stride-specific importers to the asset configuration builder.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance to configure.</param>
        /// <param name="game">The game used for configuration.</param>
        /// <returns>The updated asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder UseStrideImporters(this IAssetConfigurationBuilder builder, IGame game) =>
            builder.AddImporter(new TextureImporter(game));
    }
}
