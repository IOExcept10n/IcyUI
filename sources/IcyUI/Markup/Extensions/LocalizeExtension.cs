// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Localization;
using Icy.UI;

namespace Icy.Markup.Extensions
{
    /// <summary>
    /// <c>{Localize Key}</c>: the localized string for a key, in the configuration's
    /// <see cref="Configuration.IcyConfiguration.Culture"/>, updated when the culture changes.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// <c>{Localize Menu.Play}</c> is short for <c>{Localize Key=Menu.Play}</c>; <c>Context=</c> tells apart keys with the
    /// same text (gettext's <c>msgctxt</c>).
    /// </description></item>
    /// <item><description>
    /// On a property that doesn't take text, such as <c>Content</c>, the string goes through the configuration's text
    /// converter, as a plain string attribute does: <c>Content</c> gets a <see cref="UI.Controls.TextBlock"/>.
    /// </description></item>
    /// <item><description>
    /// A missing key shows the key itself (see <see cref="Configuration.IcyConfiguration.Localize"/>).
    /// </description></item>
    /// <item><description>
    /// In a style setter, the string is resolved once, when the style loads, and doesn't change with the culture:
    /// styles are shared by many elements. Templates are fine.
    /// </description></item>
    /// </list>
    /// </remarks>
    [MarkupExtensionDefaultProperty(nameof(Key))]
    public sealed class LocalizeExtension : IMarkupExtension
    {
        /// <summary>
        /// Gets or sets the key, such as <c>Menu.Play</c>.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional context for the key, or <see langword="null"/>.
        /// </summary>
        public string? Context { get; set; }

        /// <inheritdoc/>
        /// <exception cref="MarkupException">No key is given, or the string can't be converted to the property's type.</exception>
        public object? ProvideValue(MarkupExtensionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (string.IsNullOrEmpty(Key))
                throw MarkupException.At("{Localize} needs a key, such as {Localize Menu.Play}.", context.Node, context.SourcePath);

            object? value;
            try
            {
                value = LocalizedPropertyRegistry.ToPropertyValue(context.Configuration, context.Member, context.Configuration.Localize(Key, Context));
            }
            catch (Exception ex) when (ex is not MarkupException)
            {
                throw MarkupException.At($"{{Localize {Key}}} can't set '{context.Member.Name}': {ex.Message}", context.Node, context.SourcePath, ex);
            }

            if (context.Target is UIElement element)
                context.Configuration.LocalizedProperties.Register(element, context.Member, Key, Context, value);
            return value;
        }
    }
}
