// Skeleton for docfx/*/guides/engine-integration.md. Each #region is one example on the page. It is written against the XNA
// API so it compiles here (MonoGame) and shows the shape an FNA integration takes. The outlines are abstract: the guide
// explains what each member does, and IcyUI.MonoGame holds the complete implementations.
namespace MyGame.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Icy.Configuration;
    using Icy.Data;
    using Icy.Input;
    using Icy.Input.Clipboard;
    using Icy.Input.Devices;
    using Icy.Rendering;
    using Icy.UI;
    using Microsoft.Xna.Framework;

    #region render-context-outline
    /// <summary>What an engine gives IcyUI to draw with. Everything IcyUI draws is textured quads in this order: Begin, many Draw, End.</summary>
    public abstract class EngineRenderContext : IRenderContext
    {
        public abstract event EventHandler? ViewportResize;

        public abstract IRenderOptions Options { get; }      // opacity, scissor rectangle: the engine applies them to what follows

        public abstract ITexture WhiteTexture { get; }       // solid fills are tinted draws of this

        public abstract Transform2D Transform { get; set; }  // applied to every Draw

        public abstract Size ViewportSize { get; }           // the back buffer, in pixels

        public abstract void Begin();                        // open a sprite batch (straight alpha, scissor test on)

        public abstract void Draw(ITexture texture, in TextureRenderingOptions options);   // one quad

        public abstract void End();                          // submit the batch

        public abstract void Flush();                        // End then Begin, when a state changes mid-frame (an effect, the scissor)

        public abstract ITexture CreateTexture<TColor>(int width, int height, TColor[] data)
            where TColor : unmanaged;                        // font atlases and generated images

        public abstract void ApplyEffect(IEffect effect);

        public abstract void ClearEffects();

        public abstract IEffect GetBuiltInEffect(EffectCode code);

        public abstract void Dispose();
    }
    #endregion

    #region input-outline
    /// <summary>What an engine gives IcyUI to read input from. Each device turns the engine's state into IcyUI's events.</summary>
    public abstract class EngineInputSystem : IInputSystem, IUpdateableInput
    {
        public abstract IInputEventSystem Events { get; }    // the events canvases subscribe to: pointer, keys, navigation, text

        public abstract IKeyboardInput? Keyboard { get; }

        public abstract IMouseInput Mouse { get; }

        public abstract IGamepadInput? Gamepad { get; }      // null when the engine has no gamepads

        public abstract ITouchInput? Touch { get; }          // null when the engine reports no touch

        public abstract IClipboard Clipboard { get; }

        public abstract bool IsInitialized { get; }

        public abstract void Initialize();                   // hook the engine: window text input, device state

        public abstract void Update(TimeSpan deltaTime);     // poll the devices; called once per game update

        public abstract bool IsDeviceAvailable<T>()
            where T : IInputDeviceListener;

        public abstract T GetInputDevice<T>()
            where T : IInputDeviceListener;

        public abstract bool TryGetInputDevice<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? device)
            where T : IInputDeviceListener;

        public abstract IEnumerator<IInputDeviceListener> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    #endregion

    #region host
    /// <summary>The game component that runs IcyUI: input and display scale in Update, every canvas in Draw, after the game's own drawing.</summary>
    public sealed class EngineIcyHost : DrawableGameComponent, IIcyHost
    {
        public EngineIcyHost(Game game, IcyConfiguration configuration)
            : base(game)
        {
            Configuration = configuration;
            MainCanvas = new Canvas(configuration) { IsInputEnabled = true };
            Canvases = [MainCanvas];
            DrawOrder = int.MaxValue;                        // drawn last, so the UI is on top of the game
        }

        public IcyConfiguration Configuration { get; }

        public Canvas MainCanvas { get; }

        public IList<Canvas> Canvases { get; }

        public override void Initialize()
        {
            Configuration.Input.Initialize();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Configuration.Input is IUpdateableInput updateable)
                updateable.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < Canvases.Count; i++)     // by index: UI code may add a canvas during the frame
                Canvases[i].Render();
            base.Draw(gameTime);
        }
    }
    #endregion

    #region use-icyui
    /// <summary>The entry point a game calls: build the configuration, let the game adjust it, add the host.</summary>
    public static class EngineBuildingExtensions
    {
        public static EngineIcyHost UseIcyUI(this Game game, Action<IConfigurationBuilder> configure)
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(CreateRenderContext(game))     // your IRenderContext
                   .ConfigureInput(CreateInputSystem(game))           // your IInputSystem
                   .ConfigureAssets();
            builder.AddMarkupSupport();
            builder.UseDefaultTheme();
            configure(builder);                                       // the game's lambda runs after the defaults, so it can replace them

            var host = new EngineIcyHost(game, builder.Build());
            game.Components.Add(host);
            return host;
        }

        private static IRenderContext CreateRenderContext(Game game) => throw new NotImplementedException();

        private static IInputSystem CreateInputSystem(Game game) => throw new NotImplementedException();
    }
    #endregion
}
