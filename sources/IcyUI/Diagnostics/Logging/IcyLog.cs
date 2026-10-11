// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// Every message IcyUI logs, as source-generated methods: a call costs one <see cref="ILogger.IsEnabled"/> check
    /// and no formatting when nothing listens. Event ids: 1xxx fonts, 2xxx bindings, 3xxx types.
    /// </summary>
    internal static partial class IcyLog
    {
        [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "No font is loaded for family '{Family}' ({Style}): text using it falls back to FontSystem.FallbackFont, or doesn't render when there's none. Import the font file with FontSystem.ImportFont, or call EnableSystemFonts() if it's installed on this system.")]
        public static partial void FontNotFound(ILogger logger, string family, Rendering.Fonts.FontStyle style);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "The system font '{Family}' ({Style}) failed to import; text using it falls back to FontSystem.FallbackFont.")]
        public static partial void SystemFontImportFailed(ILogger logger, string family, Rendering.Fonts.FontStyle style, Exception exception);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Skipped '{FileName}' in the system fonts folder: it isn't a readable font.")]
        public static partial void SystemFontFileSkipped(ILogger logger, string fileName, Exception exception);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Text has no font: FontSystem.DefaultFontFamily is empty and FallbackFont is null, so text without its own FontFamily doesn't render. Set a default family (the engine integrations use \"Segoe UI\").")]
        public static partial void NoDefaultFont(ILogger logger);
    }
}
