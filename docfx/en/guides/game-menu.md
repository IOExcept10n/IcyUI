# Building a game menu

This guide builds the menus of a small game: a main menu (Play, Settings, Quit), a pause menu (Resume, Settings, Quit to
menu), and a settings page where the player changes the UI size. The settings follow the rule for anything that changes
how the screen looks: nothing applies while the player drags, **Apply** commits, and a change that isn't confirmed
within ten seconds reverts, so a bad size can never leave the UI unusable.

You need a game with IcyUI set up and one screen on it: finish [Getting started](../getting-started/stride.md) and
[Your first screen](../getting-started/first-screen.md) first. The menus are pages of one
<xref:Icy.UI.Controls.Frame>, which navigates between them and keeps a back history; see
[Tabs and navigation](../controls/navigation.md).

## 1. Describe the pages in markup

Each page is a file under `Menus/` in your game project, with **Copy to Output Directory** set to "Copy if newer":

```xml
<ItemGroup>
  <None Include="Menus\*.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

The main menu is a <xref:Icy.UI.Controls.Page> whose buttons run commands on the view model:

[!code-xml[](../../snippets/markup/guides/game-menu/main-menu.xml)]

`KeepAlive` makes the frame keep the page when the player opens Settings, so Back returns to the same page. The pause
menu is the same shape:

[!code-xml[](../../snippets/markup/guides/game-menu/pause-menu.xml)]

## 2. Write the view models

Two view models drive the pages. `MenuViewModel` holds the commands. Starting, resuming and quitting are the game's
business, so they're events; opening Settings and going back are the menu's own, and use the frame's
<xref:Icy.UI.Controls.Frame.Navigation>:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/GameMenu.cs#menu-viewmodel)]

`ShowMainMenu` and `ShowPauseMenu` clear the frame's history and navigate to a page by its path;
<xref:Icy.Navigation.INavigationService.Navigate(System.String)> loads the markup through the asset pipeline.

## 3. Add the settings page with Apply and Cancel

The settings page edits a *draft*: the slider is bound two-way to `DraftScale`, and nothing else changes until Apply.

[!code-xml[](../../snippets/markup/guides/game-menu/settings.xml)]

This page binds to `SettingsViewModel` directly, not through `MenuViewModel.Settings`. A binding follows its data
context and nothing further along the path, so `OpenSettings` in the view model above sets the page's `DataContext` to
the settings object when it navigates. See [Data binding](../concepts/data-binding.md).

The settings view model applies the draft to <xref:Icy.Configuration.ScalingConfiguration.UserScale>, and puts the
previous value back unless it's kept in time:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/GameMenu.cs#apply-revert)]

The countdown is plain code: `Tick` takes the frame time. Call it every frame, including while the game is paused,
because the player is in the pause menu when the countdown matters. Leaving the page, or closing the menu, also reverts
an unconfirmed change.

## 4. Show the menu and pause the game

`MenuController` ties the pieces together: it adds the frame to the main canvas, wires the events, and handles Escape
with <xref:Icy.Input.Devices.KeyGesture>. Escape pauses during play; in a menu it goes back one page; on the first page
of the pause menu it resumes.

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/GameMenu.cs#menu-setup)]

Nothing in it refers to an engine, so the same code runs on both. What differs is the game loop, which calls `Update` and
skips gameplay while `IsPaused` is set:

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/GameMenuStride.cs#pause-toggle)]

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/GameMenuMonoGame.cs#pause-toggle)]

---

"Paused" is your flag, not the engine's: IcyUI keeps drawing and animating the menu, since it's a separate layer from
your gameplay. Skip your own updates (movement, timers, AI) while it's set.

## What you built

A main menu, a pause menu and a settings page in markup, behind one frame with back history, and a controller that starts,
pauses and resumes the game. The UI size follows Apply and Cancel, and an unconfirmed change reverts after ten seconds,
whatever the player does next.

> [!NOTE]
> The gamepad's X and Y raise back and forward events, but a `Frame` doesn't act on them yet, so every page has a Back
> button. Escape (and **B**, which closes dialogs) doesn't go back a page by itself either: this guide's `HandleEscape`
> does that, for the keyboard.

## Next

- [Input, focus and navigation](../concepts/input-and-focus.md): how a menu is operated with a keyboard or a gamepad.
- [Scaling](../concepts/scaling.md): what `UserScale` multiplies, and the other scaling settings.
- [Data binding](../concepts/data-binding.md): how pages follow their view model.
- [Custom themes](custom-themes.md): give the menu buttons and lists your game's colors.
