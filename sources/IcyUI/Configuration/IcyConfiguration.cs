// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Icy.Assets;
using Icy.Data;
using Icy.Diagnostics.Logging;
using Icy.Input;
using Icy.Input.Events;
using Icy.Localization;
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
        private readonly HashSet<(string Key, string Culture)> reportedMissingStrings = [];
        private CultureInfo? culture;

        // The culture localized properties were last applied in: CurrentUICulture can change under an unset Culture.
        private CultureInfo appliedCulture = CultureInfo.CurrentUICulture;

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

            // Markup sets image properties from asset paths: <Image Source="Portraits/Warrior.png"/>.
            if (types.TypeConverter is TypeConversionManager converter)
                TexturePathConverter.Register(converter, assets);
            Fonts = new(this);
            LocalizedProperties = new LocalizedPropertyRegistry();
            CultureChanged += (_, _) => LocalizedProperties.Reapply(this);
        }

        /// <summary>
        /// Occurs when <see cref="Culture"/> changed to a different culture. IcyUI re-applies every localized property
        /// then (<c>{Localize}</c>, and <c>Localize</c> on elements built in code).
        /// </summary>
        /// <remarks>
        /// Raised when setting <see cref="Culture"/> leaves it different from the culture localized texts were last shown
        /// in. Not raised by changes of <see cref="CultureInfo.CurrentUICulture"/> alone, while <see cref="Culture"/>
        /// follows it: set <see cref="Culture"/> to switch the game's language.
        /// </remarks>
        public event EventHandler? CultureChanged;

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
        /// of its own;
        /// </description></item>
        /// <item><description>
        /// localized strings (<c>{Localize}</c> and <see cref="Localize"/>): changing the culture switches them
        /// (see <see cref="CultureChanged"/>).
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
            set
            {
                culture = value;
                if (Culture.Equals(appliedCulture))
                    return;

                appliedCulture = Culture;
                CultureChanged?.Invoke(this, EventArgs.Empty);
            }
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

        /// <summary>
        /// Gets the localized properties re-applied when <see cref="Culture"/> changes.
        /// </summary>
        internal LocalizedPropertyRegistry LocalizedProperties { get; }

        /// <summary>
        /// Gets the localized string for <paramref name="key"/> in <see cref="Culture"/>, from
        /// <see cref="AssetConfiguration.Localizer"/>.
        /// </summary>
        /// <param name="key">The key, such as <c>Menu.Play</c>.</param>
        /// <param name="context">
        /// An optional context that tells apart keys with the same text (gettext's <c>msgctxt</c>).
        /// </param>
        /// <returns>
        /// The string, or <paramref name="key"/> itself when the localizer has none: the text stays readable, and a
        /// warning (event 4001) is logged once per key and culture.
        /// </returns>
        public string Localize(string key, string? context = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (TryLocalize(key, context) is { } text)
                return text;

            CultureInfo current = Culture;
            string lookup = context == null ? key : $"{context}\u0004{key}";
            if (reportedMissingStrings.Add((lookup, current.Name)))
                IcyLog.LocalizedStringNotFound(Logging.CreateLogger("Icy.Localization"), key, context ?? string.Empty, current.Name);
            return key;
        }

        /// <summary>
        /// Gets the localized string for <paramref name="key"/> in <see cref="Culture"/>, or <see langword="null"/>
        /// without a warning when the localizer has none: for IcyUI's own strings, which have built-in English.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="context">An optional context for the key.</param>
        /// <returns>The string, or <see langword="null"/>.</returns>
        internal string? TryLocalize(string key, string? context = null) =>
            Assets.Localizer.Localize(context == null ? key : $"{context}\u0004{key}", Culture);
    }
}