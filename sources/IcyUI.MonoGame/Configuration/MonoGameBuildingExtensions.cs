// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Icy.Configuration;
using Icy.Diagnostics.Logging;
using Icy.MonoGame.Assets;
using Icy.MonoGame.Assets.Importers;
using Icy.MonoGame.Input;
using Icy.MonoGame.Rendering;
using Icy.Rendering.Display;
using Icy.UI;
using Microsoft.Xna.Framework;

namespace Icy.MonoGame.Configuration
{
    /// <summary>
    /// Provides extension methods for configuring the IcyUI in MonoGame applications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IcyUI.MonoGame doesn't depend on a MonoGame backend. The host references one of:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>MonoGame.Framework.DesktopGL</c> (OpenGL/SDL), 3.8.5.1 or later;</description></item>
    /// <item><description><c>MonoGame.Framework.WindowsDX</c> (Direct3D 11), 3.8.5.1 or later.</description></item>
    /// </list>
    /// <para>
    /// On DesktopGL, touch input isn't reported, and on Windows-on-ARM64 <see cref="Game.Dispose()"/> can hang on shutdown.
    /// </para>
    /// </remarks>
    public static class MonoGameBuildingExtensions
    {
        /// <summary>
        /// Sets IcyUI up in the game with the default MonoGame configuration: the default theme, markup support,
        /// system fonts with <c>"Segoe UI"</c>, and log messages in the debugger's output window.
        /// </summary>
        /// <param name="game">The game; call this in <c>Initialize()</c>, before <c>base.Initialize()</c>.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game.</exception>
        public static MonoGameIcyHost UseIcyUI(this Game game) => game.UseIcyUI(static _ => { });

        /// <summary>
        /// Sets IcyUI up in the game: the default MonoGame configuration first, then <paramref name="configure"/>.
        /// </summary>
        /// <param name="game">The game; call this in <c>Initialize()</c>, before <c>base.Initialize()</c>.</param>
        /// <param name="configure">Changes the defaults, for example <c>ui =&gt; ui.ConfigureFonts().WithDefaultFontFamily("Arial")</c>.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game.</exception>
        public static MonoGameIcyHost UseIcyUI(this Game game, Action<IConfigurationBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(configure);
            if (game.TryGetIcyHost(out _))
                return ThrowHelper.ThrowInvalidOperationException<MonoGameIcyHost>("IcyUI is already set up for this game; use GetIcyHost() to reach it.");

            var builder = new IcyConfigurationBuilder();
            builder.WithDefaultMonoGameConfiguration(game);
            configure(builder);
            var host = new MonoGameIcyHost(game, builder.Build());
            game.Components.Add(host);
            return host;
        }

        /// <summary>
        /// Gets the IcyUI host of the game, if <see cref="UseIcyUI(Game)"/> ran.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="host">The host, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when IcyUI is set up.</returns>
        public static bool TryGetIcyHost(this Game game, [NotNullWhen(true)] out MonoGameIcyHost? host)
        {
            ArgumentNullException.ThrowIfNull(game);
            host = game.Components.OfType<MonoGameIcyHost>().FirstOrDefault();
            return host != null;
        }

        /// <summary>
        /// Gets the IcyUI host of the game.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns>The host <see cref="UseIcyUI(Game)"/> created.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static MonoGameIcyHost GetIcyHost(this Game game) =>
            game.TryGetIcyHost(out MonoGameIcyHost? host)
                ? host
                : ThrowHelper.ThrowInvalidOperationException<MonoGameIcyHost>("IcyUI isn't set up for this game yet. Call UseIcyUI() first.");

        /// <summary>Gets the configuration IcyUI runs with in the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.Configuration"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static IcyConfiguration GetIcyConfiguration(this Game game) => game.GetIcyHost().Configuration;

        /// <summary>Gets the main canvas of the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.MainCanvas"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static Canvas GetIcyCanvas(this Game game) => game.GetIcyHost().MainCanvas;

        /// <summary>
        /// Configures the default settings for MonoGame in the provided configuration builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Besides the MonoGame rendering, input and asset importers, the defaults are:
        /// </para>
        /// <list type="bullet">
        /// <item><description>markup support (<see cref="BuildingExtensions.AddMarkupSupport"/>);</description></item>
        /// <item><description>the built-in theme (<see cref="BuildingExtensions.UseDefaultTheme(IConfigurationBuilder)"/>);</description></item>
        /// <item><description>system fonts, with <c>"Segoe UI"</c> as the default family;</description></item>
        /// <item><description>log messages in the debugger's output window (<see cref="DebuggerLoggerProvider"/>).</description></item>
        /// </list>
        /// </remarks>
        /// <param name="builder">The configuration builder instance to configure.</param>
        /// <param name="game">The <see cref="Game"/> instance used for configuration.</param>
        /// <returns>The updated configuration builder instance for fluent configuration.</returns>
        public static IConfigurationBuilder WithDefaultMonoGameConfiguration(this IConfigurationBuilder builder, Game game)
        {
            var renderContext = new RenderContext(game.GraphicsDevice);
            renderContext.AttachDisplayScaleTracker(new DisplayScaleTracker(DisplayScales.GetProvider(), () => MonoGameNativeWindow.GetInfo(game)));
            builder.ConfigureRendering(renderContext)
                   .ConfigureInput(new InputSystem(game))
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithAssetContextFactory(new MonoGameAssetContextFactory(game))
                   .AddBasicFontSupport()
                   .UseMonoGameImporters(game);
            builder.AddMarkupSupport();
            builder.UseDefaultTheme();
            builder.ConfigureFonts().UseSystemFonts().WithDefaultFontFamily("Segoe UI");
            builder.ConfigureLogging().AddLoggerProvider(new DebuggerLoggerProvider());
            return builder;
        }

        /// <summary>
        /// Adds MonoGame-specific importers to the asset configuration builder.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance to configure.</param>
        /// <param name="game">The <see cref="Game"/> instance used for configuration.</param>
        /// <returns>The updated asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder UseMonoGameImporters(this IAssetConfigurationBuilder builder, Game game) =>
            builder.AddImporter(new TextureImporter(game));
    }
}