# Concepts

The ideas behind IcyUI: what you need to know to predict how your UI behaves. Each page explains one part of the
model with a small example, the rules that matter, and the usual pitfalls; the [API reference](../api/index.md)
has every member.

- **[Canvas, the host and the element tree](canvas-and-host.md):** Where your UI lives: the canvases the engine draws, the host that connects them to your game, and the tree of elements on each canvas.
- **[Configuration](configuration.md):** What `UseIcyUI()` sets up, the builder's sections, and how your settings override the engine's defaults.
- **[Layout](layout.md):** How elements get their size and position: measure and arrange, sizes and margins, alignment, panels and scale.
- **[Data binding](data-binding.md):** Connecting elements to your view model: the data context, binding modes, element bindings and conversion.
- **[Resources, styles and themes](resources-and-styles.md):** Reusing values and looks: resource dictionaries, implicit and keyed styles, the theme, and which value wins.
- **[Control templates and visual states](templates-and-visual-states.md):** Replacing how a control looks, and changing it with its state: hovered, pressed, focused, checked.
- **[Input, focus and navigation](input-and-focus.md):** Mouse, keyboard, gamepad and touch: focus, spatial navigation, gestures, and drag and drop.
- **[Animations](animations.md):** Animating any property over time, from code or from visual states, with easing.
- **[DPI and UI scaling](scaling.md):** Keeping the UI the right size on every display: DPI scaling, a reference resolution, and a player UI-size setting.
- **[Assets and fonts](assets-and-fonts.md):** Loading markup, images and fonts: where files come from, system and imported fonts, and the fallback font.
- **[Logging and diagnostics](logging-and-diagnostics.md):** IcyUI's warnings and where they go, and the debug tools that show layout boxes, focus and frame time.
