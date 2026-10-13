# Localization

IcyUI shows text in the player's language: markup asks for a string by key with `{Localize}`, a localizer finds it in
the current language, and changing the language updates every localized text at once.

## Setting it up

Choose where your translations live and give the configuration a localizer. With gettext `.po` files, one per
language in a `Localization` folder next to the game:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#setup)]

| Localizer | Strings come from | Good for |
|---|---|---|
| <xref:Icy.Localization.PoLocalizer> | gettext `.po` files, one per language (`en.po`, `ru.po`, `pt_BR.po`). | Most games: translators use tools like Poedit, Weblate or Crowdin, and players can add a language without a new build. |
| <xref:Icy.Localization.ResourceManagerLocalizer> | `.resx` resources compiled into the game. | Projects that already use .NET resources. |
| <xref:Icy.Localization.DictionaryLocalizer> | Strings added from code. | Prototypes, tests, a few built-in strings. |
| <xref:Icy.Localization.CompositeLocalizer> | Several localizers, asked in order. | Mod translations over the game's own, or a safety net. |

Any other source works too: implement <xref:Icy.Localization.ILocalizer>, one method that returns the string for a key
and a culture, or `null`.

A culture falls back to its parent and then to the default language: for `ru-RU`, `ru-RU.po`, then `ru.po`, then
`en.po`. A key that isn't found anywhere shows as the key itself, and IcyUI logs a warning once (event 4001).

## In markup

[!code-xml[](../../snippets/markup/concepts/localize.xml)]

- `{Localize Menu.Play}` works on any property that takes text. On `Content`, the string becomes a `TextBlock`, as plain
  text does.
- `Context=` tells apart two keys with the same text (gettext's `msgctxt`): `{Localize Open, Context=Verb}`.

## Switching languages

The language is the configuration's `Culture`, the same setting numbers are shown in:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#switch)]

Every `{Localize}` text changes right away. A binding shows numbers in the new format the next time it updates. By
default, `Culture` follows the system's UI language; set it when your game has its own language setting.

## UI built in code

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#from-code)]

It updates with the language like `{Localize}`, and an element that isn't on a canvas yet gets its text when it's added.
`Configuration.Localize("Hud.Score")` returns a string for code that formats text itself.

## Mods and layering

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#chain)]

## Performance

A localized text costs one lookup when it loads and one small entry so it can change language. Each `.po` file is read
once, the first time its language is needed. Switching languages updates each localized text once; nothing runs between
switches.

## Pitfalls

- **Your own text replaces the localized one for good.** Setting a localized property yourself (a score, a player
  name) stops it from changing with the language.
- **Styles don't switch.** `{Localize}` in a style setter is read once, when the style loads. Put it on the elements, or
  in a template.
- **Missing glyphs.** A font without Cyrillic or CJK letters can't show those languages; pick a font that covers your
  languages.
- **Arguments.** `"Score: {0}"` isn't filled in for you yet: format it in code with `Configuration.Localize`.

## See also

- [Data binding](data-binding.md#conversion) for numbers in the current culture.
- [Configuration](configuration.md).
