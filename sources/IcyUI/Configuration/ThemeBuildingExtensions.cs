// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.UI;
using Icy.UI.Styles;

namespace Icy.Configuration
{
    /// <summary>
    /// Builder extensions for the theme: a theme from a file, or the built-in theme extended by one.
    /// </summary>
    public static class ThemeBuildingExtensions
    {
        /// <summary>
        /// Uses the resource dictionary in the file at <paramref name="path"/> as the theme, instead of the built-in one.
        /// </summary>
        /// <param name="builder">The theme section of a configuration builder.</param>
        /// <param name="path">The theme file, relative to the configuration's assets, such as <c>Themes/Dark.xml</c>.</param>
        /// <returns>The same section, for chaining.</returns>
        /// <remarks>
        /// <para>
        /// The file is a markup <see cref="ResourceDictionary"/> loaded when the configuration is built. It replaces the
        /// built-in theme entirely: controls get only the styles and templates the file gives them. To change a few
        /// things and keep the rest, use <see cref="ExtendDefaultTheme"/>.
        /// </para>
        /// <para>A theme is chosen at startup: canvases take it when they're created.</para>
        /// </remarks>
        public static IThemeConfigurationBuilder UseThemeFile(this IThemeConfigurationBuilder builder, string path)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(path);
            builder.AfterBuild(c => c.Theme.Theme = c.Assets.Load<ResourceDictionary>(path));
            return builder;
        }

        /// <summary>
        /// Layers the resource dictionary in the file at <paramref name="path"/> over the built-in theme.
        /// </summary>
        /// <param name="builder">The theme section of a configuration builder.</param>
        /// <param name="path">The file, relative to the configuration's assets, such as <c>Themes/MyGame.xml</c>.</param>
        /// <returns>The same section, for chaining.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>The file's resources win over the built-in theme's under the same key.</description></item>
        /// <item><description>
        /// An implicit style in the file builds on the built-in style for its type: it keeps the built-in template and
        /// visual states and changes only what it sets, as a style in a page's resources does. A style with its own
        /// <see cref="Style.BasedOn"/>, or <see cref="Style.ReplacesTheme"/> set, is left as written.
        /// </description></item>
        /// <item><description>Types the file doesn't style keep the built-in style.</description></item>
        /// </list>
        /// <para>A theme is chosen at startup: canvases take it when they're created.</para>
        /// </remarks>
        public static IThemeConfigurationBuilder ExtendDefaultTheme(this IThemeConfigurationBuilder builder, string path)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(path);
            builder.AfterBuild(c =>
            {
                ResourceDictionary builtIn = c.Theme.Theme ?? c.UseDefaultTheme().Theme.Theme!;
                ResourceDictionary extension = c.Assets.Load<ResourceDictionary>(path);
                foreach (string key in extension.Keys)
                {
                    if (ResourceDictionary.IsImplicitStyleKey(key)
                        && extension[key] is Style { BasedOn: null, ReplacesTheme: false } style
                        && builtIn.TryGetValue(key, out object? value)
                        && value is Style builtInStyle)
                    {
                        style.BasedOn = builtInStyle;
                    }
                }

                var theme = new ResourceDictionary();
                theme.MergedDictionaries.Add(builtIn);
                theme.MergedDictionaries.Add(extension);
                c.Theme.Theme = theme;
            });
            return builder;
        }
    }
}
