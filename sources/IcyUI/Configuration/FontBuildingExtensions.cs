// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Rendering.Fonts;

namespace Icy.Configuration
{
    /// <summary>
    /// Font steps for <see cref="IFontConfigurationBuilder"/>. Each runs on the built configuration's
    /// <see cref="IcyConfiguration.Fonts"/>, in declaration order.
    /// </summary>
    /// <example>
    /// <code language="csharp">
    /// builder.ConfigureFonts()
    ///        .ImportFont("Fonts/MyFont.ttf")
    ///        .WithDefaultFontFamily("My Font");
    /// </code>
    /// </example>
    public static class FontBuildingExtensions
    {
        /// <summary>
        /// Configures the font system with a callback once the configuration is built.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configure">Receives the built <see cref="FontSystem"/>.</param>
        /// <returns>The fonts section of the builder.</returns>
        public static IFontConfigurationBuilder ConfigureFonts(this IConfigurationBuilder builder, Action<FontSystem> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            builder.AfterBuild(c => configure(c.Fonts));
            return builder.ConfigureFonts();
        }

        /// <summary>
        /// Lets fonts installed on this system be found by family name (<see cref="FontSystem.EnableSystemFonts"/>).
        /// </summary>
        /// <param name="builder">The fonts section of a configuration builder.</param>
        /// <returns>The same fonts section, for chaining.</returns>
        /// <remarks>
        /// On Windows this is the Windows fonts folder. On Linux and macOS, .NET maps it to the user's font folder
        /// (<c>~/.fonts</c>, <c>~/Library/Fonts</c>), so ship the font with the game and use
        /// <see cref="ImportFont(IFontConfigurationBuilder, string)"/> there instead.
        /// </remarks>
        public static IFontConfigurationBuilder UseSystemFonts(this IFontConfigurationBuilder builder) =>
            builder.Defer(f => f.EnableSystemFonts());

        /// <summary>
        /// Sets the family text uses when it doesn't name one (<see cref="FontSystem.DefaultFontFamily"/>).
        /// </summary>
        /// <param name="builder">The fonts section of a configuration builder.</param>
        /// <param name="family">The family name stored in the font file, such as <c>"Segoe UI"</c>.</param>
        /// <returns>The same fonts section, for chaining.</returns>
        public static IFontConfigurationBuilder WithDefaultFontFamily(this IFontConfigurationBuilder builder, string family)
        {
            ArgumentNullException.ThrowIfNull(family);
            return builder.Defer(f => f.DefaultFontFamily = family);
        }

        /// <summary>
        /// Imports a font file shipped with the game, relative to the default asset context (the application folder
        /// unless changed).
        /// </summary>
        /// <param name="builder">The fonts section of a configuration builder.</param>
        /// <param name="path">The font file's path, for example <c>"Fonts/MyFont.ttf"</c>. Copy it to the output folder.</param>
        /// <returns>The same fonts section, for chaining.</returns>
        /// <remarks>The file is loaded when the configuration is built; a missing file throws there.</remarks>
        public static IFontConfigurationBuilder ImportFont(this IFontConfigurationBuilder builder, string path)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(path);
            builder.AfterBuild(c => c.Fonts.ImportFont(c.Assets.DefaultAssetContext, path));
            return builder;
        }

        /// <summary>
        /// Sets the font used when nothing else resolves (<see cref="FontSystem.FallbackFont"/>).
        /// </summary>
        /// <param name="builder">The fonts section of a configuration builder.</param>
        /// <param name="createFont">Creates the font from the built configuration.</param>
        /// <returns>The same fonts section, for chaining.</returns>
        public static IFontConfigurationBuilder WithFallbackFont(this IFontConfigurationBuilder builder, Func<IcyConfiguration, IFont> createFont)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(createFont);
            builder.AfterBuild(c => c.Fonts.FallbackFont = createFont(c));
            return builder;
        }

        private static IFontConfigurationBuilder Defer(this IFontConfigurationBuilder builder, Action<FontSystem> step)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c => step(c.Fonts));
            return builder;
        }
    }
}
