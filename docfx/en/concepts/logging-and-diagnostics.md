# Logging and diagnostics

When something doesn't work the way the markup says, IcyUI tells you in two ways: warnings in a log, for problems it
can name (a font that doesn't exist, a binding path with a typo), and debug tools drawn over the running game, for
problems you have to see (an element that's the wrong size, focus in the wrong place).

## The log

IcyUI logs through `Microsoft.Extensions.Logging`, the standard .NET logging interfaces. With the engine defaults its
messages go to the debugger's Output window on MonoGame, and to Stride's log (the game console and Game Studio's output)
on Stride. Only warnings and above are logged by default.

| Event | Level | Meaning |
|---|---|---|
| 1001 | Warning | No font is loaded for a family, so text using it falls back. |
| 1002 | Warning | A system font failed to import. |
| 1003 | Debug | A file in the system fonts folder isn't a readable font and was skipped. |
| 1004 | Warning | There's no default font to draw text without a `FontFamily`. |
| 2001 | Warning | A binding's path names a member its source doesn't have. |
| 2002 | Warning | A binding's data context isn't its `x:DataType`; it falls back to a dynamic path. |
| 3001 | Warning | An assembly couldn't be fully scanned for markup types. |
| 4001 | Warning | A localized string is missing for a key and language; the key is shown. |

Each is logged once per cause, not every frame.

## Changing where it goes

The logging section of the configuration takes a minimum level, extra providers, or a whole logger factory of your own:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#logging)]

- `ConfigureLogging(LogLevel.Information)` lowers the threshold (see [Configuration](configuration.md#setting-it-up));
- `AddLoggerProvider(provider)` adds any `ILoggerProvider`: Serilog, NLog, the console, or your own;
- `UseLoggerFactory(factory)` hands IcyUI your game's existing `ILoggerFactory`, so its messages join your own log with
  your filters.

## Debug tools

The canvas can draw debug tools over the UI. They're switched by name in <xref:Icy.UI.Canvas.ActiveDebugTools>:

| Tool | Shows |
|---|---|
| `"Bounds"` | Each element's margin, border, padding and content boxes. |
| `"Focus"` | An outline around the focused element. |
| `"DiagnosticsHud"` | A panel with frame time and FPS, memory, and the focused element. |

IcyUI binds no keys to them; bind your own, for example in a debug build:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#debug-keys)]

## Pitfalls

- **No log without a debugger on MonoGame.** The default provider writes only while a debugger is attached; add your
  own provider to keep a log when the game runs on its own.
- **Nothing logged for a binding that works.** Warning 2001 fires only when the member doesn't exist; a binding to a
  property that never raises `PropertyChanged` is silent. See [Data binding](data-binding.md).

## See also

- [Assets and fonts](assets-and-fonts.md) for font warnings.
- [Data binding](data-binding.md) for binding warnings.
