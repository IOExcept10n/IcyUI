# Localizing your game UI

This guide takes a menu from hard-coded English to a game that speaks the player's language and lets them switch it:
keys in markup, one `.po` file per language, a language setting, text built in code, and a layer for community
translations.

You need a page loaded from markup ([Your first screen](../getting-started/first-screen.md)). The
[Localization](../concepts/localization.md) page describes the pieces; this guide wires them together. The settings page
is the one from [Building a game menu](game-menu.md).

## 1. Use keys in markup

Replace each visible text with `{Localize Key}`. Keys are names you choose; `Menu.Play` reads better than the English
sentence, and survives a rewording of it:

[!code-xml[](../../snippets/markup/guides/localization/localized-menu.xml)]

## 2. Write the translation files

Create a `Localization` folder next to the game with one gettext file per language, `en.po` and `ru.po`:

```po
# Localization/en.po
msgid "Menu.Title"
msgstr "My Game"

msgid "Menu.Play"
msgstr "Play"

msgid "Settings.Language"
msgstr "Language"
```

```po
# Localization/ru.po
msgid "Menu.Title"
msgstr "Моя игра"

msgid "Menu.Play"
msgstr "Играть"

msgid "Settings.Language"
msgstr "Язык"
```

Copy the files to the build output. Translators open them in Poedit (or a service such as Weblate or Crowdin), so give
them the files and the keys' meaning; they don't need your code. A player can add `de.po` to the folder and get a new
language without a new build.

## 3. Turn the localizer on

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#setup)]

`en` is the default language: a key missing from `ru.po` shows its English text rather than the key. The player's system
language is used until you set one.

## 4. Add a language setting

A combo box on the settings page sets the configuration's `Culture`; every `{Localize}` text changes at once, and numbers
bound in the UI switch to the new culture's format:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/Localization.cs#language-setting)]

Give the setting its own data context on the settings page, as the game menu guide does for its other settings. Show
each language by its own name, not translated, so a player who ended up in the wrong language can find theirs. Save the
choice with the rest of the player's settings and set `Culture` from it at startup.

## 5. Text built in code

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#from-code)]

This text follows the language like markup does. For a string you format yourself, call `Configuration.Localize("Hud.Score")`.

## 6. Community translations

To let players override or add strings from a `Mods/Localization` folder, chain two localizers. Give only the last one a
default language:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#chain)]

## 7. Fonts

A language needs a font with its letters. If the default font misses Cyrillic or CJK glyphs, text shows as boxes. See
[Assets and fonts](../concepts/assets-and-fonts.md) to choose one that covers your languages.

## What you built

A menu with no text in code or markup, a translation file per language, a setting that switches language while the game
runs, and a folder where players can add their own.

> [!NOTE]
> Text with arguments (`"Score: {0}"`) isn't filled in for you: localize the format string with
> `Configuration.Localize` and format it in code.

## Next

- [Localization](../concepts/localization.md): the localizers, fallbacks and pitfalls.
- [Data binding](../concepts/data-binding.md#conversion): numbers and dates in the current culture.
