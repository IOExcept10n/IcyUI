// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Icy.Data;
using Icy.Input;
using Icy.Input.Events;
using Icy.Rendering;
using Icy.Rendering.Fonts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Icy.Configuration
{
    /// <summary>
    /// Aggregates library services and provides fluent configuration for all of these.
    /// </summary>
    public class IcyConfiguration
    {
        private CultureInfo? culture;

        /// <summary>
        /// Initializes a new instance of the <see cref="IcyConfiguration"/> class.
        /// </summary>
        /// <param name="input">An input service that is used by library in this application.</param>
        /// <param name="assets">Configuration of the assets system used by library in this application.</param>
        /// <param name="renderContext">The rendering service instance used by library in this application.</param>
        /// <param name="types">An instance of the reflection-related services used in library.</param>
        /// <param name="theme">
        /// The default-theme configuration used by the library. Defaults to an empty <see cref="ThemeConfiguration"/>
        /// (no theme applied) when omitted.
        /// </param>
        /// <param name="scaling">
        /// The UI scaling defaults used by the library. Defaults to a new <see cref="ScalingConfiguration"/>
        /// (<see cref="UI.UIScaleMode.Dpi"/>) when omitted.
        /// </param>
        /// <param name="logging">
        /// Where IcyUI writes its log messages. Defaults to <see cref="NullLoggerFactory.Instance"/> (nothing is logged)
        /// when omitted.
        /// </param>
        public IcyConfiguration(IInputSystem input, AssetConfiguration assets, IRenderContext renderContext, ReflectionConfiguration types, ThemeConfiguration? theme = null, ScalingConfiguration? scaling = null, ILoggerFactory? logging = null)
        {
            Input = input;

            // Gesture slop is measured in DIPs: it follows the OS display scale, never the UI scale.
            input.Events.Gestures.Settings.DisplayScaleSource = () => renderContext.DisplayScale;

            // The numeric keypad's decimal key types the separator the bindings parse with.
            if (input.Events.Text is TextEvents text)
                text.CultureSource = () => Culture;
            Assets = assets;
            RenderContext = renderContext;
            Types = types;
            Theme = theme ?? new();
            Scaling = scaling ?? new();
            Logging = logging ?? NullLoggerFactory.Instance;
            if (types.AssemblyResolver is AssemblyResolver resolver)
                resolver.Logger = Logging.CreateLogger<AssemblyResolver>();
            Fonts = new(this);
        }

        /// <summary>
        /// Gets or sets the culture IcyUI formats and parses the values players read and type with.
        /// </summary>
        /// <value>
        /// The culture set for this configuration, or <see cref="CultureInfo.CurrentUICulture"/> (read at each use) while
        /// none is set. Setting <see langword="null"/> goes back to following <see cref="CultureInfo.CurrentUICulture"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// The default follows the UI language rather than <see cref="CultureInfo.CurrentCulture"/> (the system's
        /// regional format): a game shows numbers the way its own language writes them, so <c>3,5</c> in Russian and
        /// <c>3.5</c> in English, whatever the player's system settings are.
        /// </para>
        /// <para>Used by:</para>
        /// <list type="bullet">
        /// <item><description>
        /// bindings that convert between text and a value, for example a <see cref="UI.Controls.TextBox"/> two-way
        /// bound to a <see langword="float"/> (see <see cref="Data.Bindings.Binding.Culture"/>);
        /// </description></item>
        /// <item><description>
        /// the converter of a binding's <see cref="Data.Bindings.BindingConverterParameters"/> when it has no culture
        /// of its own.
        /// </description></item>
        /// </list>
        /// <para>
        /// Changing it applies to existing bindings the next time they update. Markup itself always uses the
        /// invariant culture: <c>Width="3.5"</c> means the same in every language.
        /// </para>
        /// </remarks>
        [AllowNull]
        public CultureInfo Culture
        {
            get => culture ?? CultureInfo.CurrentUICulture;
            set => culture = value;
        }

        /// <summary>
        /// Gets the factory IcyUI creates its loggers from. Categories are IcyUI's full type names, such as
        /// <c>Icy.Rendering.Fonts.FontSystem</c>, so a prefix filter like <c>"Icy.Markup"</c> selects one area.
        /// </summary>
        public ILoggerFactory Logging { get; }

        /// <summary>
        /// Gets a service for access to the library fonts.
        /// </summary>
        public FontSystem Fonts { get; }

        /// <summary>
        /// Gets the input service instance used by the library.
        /// </summary>
        public IInputSystem Input { get; }

        /// <summary>
        /// Gets the assets configuration section.
        /// </summary>
        public AssetConfiguration Assets { get; }

        /// <summary>
        /// Gets the rendering service instance used by the library.
        /// </summary>
        public IRenderContext RenderContext { get; }

        /// <summary>
        /// Gets the services for the reflection purposes.
        /// </summary>
        public ReflectionConfiguration Types { get; }

        /// <summary>
        /// Gets the default-theme configuration used by the library.
        /// </summary>
        public ThemeConfiguration Theme { get; }

        /// <summary>
        /// Gets the application-wide UI scaling defaults used by every <see cref="UI.Canvas"/> built against this configuration.
        /// </summary>
        public ScalingConfiguration Scaling { get; }
    }
}