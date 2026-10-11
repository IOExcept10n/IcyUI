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
    }
}
