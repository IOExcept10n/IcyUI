# Docs Plan 2b: Concepts — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Concepts section: an overview and 11 pages that give a game developer working knowledge of IcyUI's
model, in English and Russian, with every example tested or compiled.

**Architecture:** Markup-first examples in `docfx/snippets/markup/concepts/*.xml` (loaded and behaviour-tested by
`DocsMarkupSnippetTests`); C# only where markup can't express it, in `#region`s of
`sources/IcyUI.Docs.Snippets.Stride/Concepts.cs` (compiled by the solution build). Pages are written English-first with
Russian stubs; after Ivan's review, one task translates them.

**Tech Stack:** DocFX, IcyUI markup, C# (Stride snippet project), xUnit.

**Spec:** `docs/superpowers/specs/2026-10-12-docs-plans-2-3-design.md` (§2 Concepts list, §5 content standards, §6
workflow), plus the non-superseded parts of `docs/superpowers/specs/2026-10-10-docs-site-design.md`.

## Global Constraints

- Examples: markup first; C# only for what markup can't do (host setup, configuration, animations started from code,
  logging, assets loaded from code, drag and drop, debug tools). Decided with Ivan on 2026-10-12.
- Depth: working knowledge, about 600–1200 words per page: the model, the rules needed to predict behaviour, common
  pitfalls; member lists stay in the API reference (`<xref:…>`). No internals. Decided with Ivan on 2026-10-12.
- Page shape (spec §5): the idea; a minimal example; how it interacts with neighbouring concepts; related links.
- Every `<xref:…>` uid is verified to exist in `docfx/api/*.yml` before use (`grep -l "^- uid: <uid>$" docfx/api/*.yml`);
  an inherited member is linked under its declaring type.
- Every behavioural claim is checked against the code while writing (the controls pages caught three wrong claims
  this way). Don't promise what isn't shipped; gaps go in `> [!NOTE]` boxes.
- Russian: `docfx/ru/_glossary.md` terms; code, types and markup stay English; `source-hash:` of the English page.
- Strict docs build (`bash .github/scripts/build-docs.sh`) passes with zero warnings.
- **A library bug found at any step stops the plan; ask Ivan.** A snippet that fails because this plan wrote a wrong
  attribute is a plan error: fix it from the XML docs and ledger a ruling.

## Review Focus

1. **A keyed `Style` on a themed `Button` must keep the theme's look** (template, background), or the styles page
   must say why it doesn't. Pinned in Task 1 (`Styles_KeyedStyle_KeepsTheThemeTemplate`); if it fails, that's a stop.
2. **`ElementName` bindings must follow their source** (the binding page shows a slider driving a label). Pinned in
   Task 1 (`Binding_ElementName_FollowsTheSlider`).
3. **Bindings are one-way unless `Mode=TwoWay`**: the binding page states it; pinned in Task 1
   (`Binding_WithoutMode_IsOneWay`).
4. **A custom template must still show the control's content** through `TemplateBinding`. Pinned in Task 1
   (`Template_ShowsTheButtonsContent`).
5. **Russian anchors**: links from Russian concept pages to Russian sections/anchors resolve (strict build +
   `check-navigation.sh` in Task 8).

---

## Verified facts (gathered 2026-10-12, for the page tasks)

- **Canvas/host:** `IIcyHost.Configuration`, `MainCanvas`, `Canvases` (drawn in order, later on top); `Canvas.Add`,
  `Remove`, `LoadMarkup(path, dataContext)`, `AddOverlay`/`RemoveOverlay`/`BringOverlayToFront` (overlays above all
  roots: popups, dialogs), `HitTest`, `IsMouseOverGUI` (true while the pointer is over UI: skip game input then),
  `Focus`, `FocusedElement`, `MoveFocus`, `ActiveDebugTools`, `Resources`, `ScaleMode`, `EffectiveScale`,
  `IsKeyboardNavigationEnabled`. `UIElement.Parent`, `LogicalParent` (parent or canvas), `ZIndex`, `IsVisible`,
  `IsHitTestVisible`, `Opacity`, `ClipToBounds`, `Name`. MonoGame draws canvases during `base.Draw()`
  (`DrawOrder = int.MaxValue`); Stride draws them in the graphics compositor after the scene.
- **Configuration:** `UseIcyUI(ui => …)` applies engine defaults first, then the lambda. Defaults: markup support,
  default theme, system fonts with "Segoe UI", a log provider (MonoGame: debugger output; Stride: GlobalLogger).
  Sections: `ConfigureTheme`, `ConfigureFonts` (+`UseSystemFonts`, `WithDefaultFontFamily`, `ImportFont`,
  `WithFallbackFont`), `ConfigureLogging` (`LogLevel`, `AddLoggerProvider`, `UseLoggerFactory`, `WithMinimumLevel`),
  `ConfigureScaling`, `ConfigureGestures` (`GestureSettings.HoldDelay` 0.5 s, `MultiTapDelay` 0.3 s, `Slop` 10),
  `ConfigureAssets`, `ConfigureTypes` (`AddTypeConverter`), `ConfigureMarkup`, `ConfigureDiagnostics`. Lambda
  sections run after the build in declaration order, so a later one overrides an earlier one; a theme set on the
  theme section isn't replaced by the default theme.
- **Layout:** two passes: measure (desired size) then arrange (final rectangle). `Width`/`Height`/`Min*`/`Max*` use
  NaN for "unset"; `Margin` outside, `Padding` inside; `HorizontalAlignment`/`VerticalAlignment` default `Stretch`
  (the theme centers `Button`; a local value wins, even `Stretch`). Panels: StackPanel, Grid (`Auto`, px, `*`, `2*`,
  spans), WrapGrid, ScrollViewer, SplitPane. An items control outside a ScrollViewer sizes to its items.
- **Binding:** `{Binding Path, ElementName=, Source=, Mode=, UpdateTargetTrigger=}`. Modes `OneWay` (default for
  every property), `TwoWay`, `OneTime`, `OneWayToSource`. `UpdateTargetTrigger`: `Reactive` (on `PropertyChanged`) or
  `EveryFrame`. Source must raise `INotifyPropertyChanged` (CommunityToolkit `ObservableObject`) to update. Text
  converts to the target type (numbers parse in the current culture; strings become `TextBlock` content); a two-way
  numeric TextBox shows `Invalid` on bad input. A path that doesn't exist logs warning 2001 once per binding.
  Converters (`BindingConverterParameters`) are code-only; markup has no `Converter=`.
- **Resources/styles/themes:** `Resources` on any element and on `Canvas`; `{StaticResource key}` searches the element
  and its ancestors, then the canvas, then the theme. Implicit style = no `x:Key`, applies to exact type.
  `Style.BasedOn`, `Style.Setters`, `Style.StateGroups`. Precedence: local value > animation > visual state > style >
  default. Theme: `ThemeConfiguration.Theme` (a `ResourceDictionary`), default theme `DefaultTheme.xml`.
- **Templates/visual states:** `ControlTemplate TargetType=…`, `{TemplateBinding Prop}`, `ContentPresenter`, named
  parts `PART_*` (Slider `PART_Thumb`, ProgressBar `PART_Fill`, SplitPane `PART_Divider`, Expander `PART_Header`,
  TreeViewItem `PART_Indent`/`PART_Expander`, Dropdown `PART_ToggleButton`). `VisualStateGroup` of `VisualState Name
  State Duration Easing` + property values; `ControlState` flags: Normal, Hovered, Pressed, Disabled, Focused,
  Touching, Checked, Expanded, Selected, Highlighted, Invalid; combinations like `State="Selected,Hovered"`.
- **Input/focus/navigation:** keyboard: arrows move focus to the nearest control in that direction (spatial), Tab /
  Shift+Tab next/previous, Enter activates, Escape closes a modal; gamepad: D-pad = arrows, A activates, B closes,
  X/Y navigate back/forward (Frame), thumbstick clicks previous/next. `IsFocusable`, `IsFocusScope` (Window/Dialog
  trap focus, restore it on close), `Canvas.Focus`, `IsKeyboardNavigationEnabled`, focused elements are brought into
  view. Gestures (`IGestureEvents`): Tapped, Held, DragStarted/Moved/Completed/Canceled, PinchStarted/Changed/Completed,
  PointerPressed/Released. Drag and drop: `IDragSource.TryBeginDrag(Point, out payload, out preview)`; `IDropTarget`
  `CanDrop`/`OnDragEnter`/`OnDragOver`/`OnDragLeave`/`OnDrop`(`DragDropSession` with `Source`, `Payload`,
  `ScreenPoint`, `Preview`, `CurrentTarget`). No touch on MonoGame DesktopGL.
- **Animations:** `element.Animate(Timeline)` starts and returns an `Animation` (`IsRunning`, `IsPaused`, `Pause`,
  `Resume`, `Stop`, `Completed`). `Timeline.FromTo(property, duration, from, to, easing, repeatCount, autoReverse)`,
  `Timeline.Forever`, keyframes. Easing: Linear, EaseIn/Out/InOut Quad and Cubic. Visual states animate with
  `Duration`/`Easing`. An animation outranks style and visual state, a local value outranks it.
- **DPI and scaling:** `UIScaleMode` None / Dpi (default) / ReferenceResolution (`ReferenceSize`, `ReferenceFit` Fit
  or Fill); `UserScale` multiplies; per-canvas `Canvas.ScaleMode` overrides; `EffectiveScale`. A host without a
  PerMonitorV2 manifest reports scale 1 (Windows stretches it). Display settings: apply with Apply/Cancel and a
  timed revert (Ivan's rule) — mention in the page as advice for settings screens.
- **Assets and fonts:** `Configuration.Assets.Load<T>(path)` / `LoadAsync<T>`, relative to `DefaultAssetContext`
  (`AssetContext.ApplicationContext` = the app folder); importers per type (markup, resource dictionaries, textures
  via engines, fonts). Fonts: `FontSystem.DefaultFontFamily`, `FallbackFont`, `EnableSystemFonts`, `ImportFont`,
  `GetOrLoad`; lookups by family, size and style; a missing family warns once (1001) and uses the fallback; system
  fonts are indexed lazily on first miss; licensing (don't ship Windows fonts; SIL OFL fonts are fine).
- **Logging and diagnostics:** `IcyConfiguration.Logging`; minimum level `Warning` by default; events 1001 font not
  found, 1002 system font import failed, 1003 skipped font file (Debug), 1004 no default font, 2001 binding member not
  found, 3001 assembly scan incomplete; once per cause. Debug tools: `Canvas.ActiveDebugTools` names `"Bounds"`
  (box model), `"Focus"` (focus outline), `"DiagnosticsHud"` (frame time, memory, focused element); the samples bind
  F1/F2 with `Input.Events.RegisterCommand(command, new KeyGesture(Keys.F1))`; the library binds no keys itself.

---

### Task 1: Concept markup snippets and their tests

**Files:**
- Create: `docfx/snippets/markup/concepts/{layout,binding,styles,template}.xml`
- Modify: `sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`

**Interfaces:**
- Produces: four snippets the pages include as `../../snippets/markup/concepts/<name>.xml`.

- [ ] **Step 1: Failing tests.** Add to `DocsMarkupSnippetTests` (helper `Load(string file)` exists; use
  `Path.Combine("concepts", …)`; render via a themed `Canvas` where layout or templates matter):

```csharp
        [Fact]
        public void Binding_WithoutMode_IsOneWay()
        {
            var root = (StackPanel)Load(Path.Combine("concepts", "binding.xml"));
            var viewModel = new DocsSnippetViewModel();
            root.DataContext = viewModel;
            var title = Assert.IsType<TextBlock>(root.Children[0]);

            title.Text = "Changed on screen";

            Assert.Equal("My Game", viewModel.Title);
        }

        [Fact]
        public void Binding_ElementName_FollowsTheSlider()
        {
            var root = (StackPanel)Load(Path.Combine("concepts", "binding.xml"));
            root.DataContext = new DocsSnippetViewModel();
            var slider = Assert.IsType<Slider>(root.Children[2]);
            var label = Assert.IsType<TextBlock>(root.Children[3]);

            slider.Value = 25;

            Assert.Equal("25", label.Text);
        }

        [Fact]
        public void Styles_KeyedStyle_KeepsTheThemeTemplate()
        {
            var canvas = ThemedCanvas();
            var root = (StackPanel)Load(Path.Combine("concepts", "styles.xml"));
            canvas.Add(root);
            canvas.Render();

            var quit = Assert.IsType<Button>(root.Children[1]);
            Assert.NotNull(quit.Template);
        }

        [Fact]
        public void Template_ShowsTheButtonsContent()
        {
            var canvas = ThemedCanvas();
            var root = (StackPanel)Load(Path.Combine("concepts", "template.xml"));
            canvas.Add(root);
            canvas.Render();

            var button = Assert.IsType<Button>(root.Children[0]);
            Assert.Contains(button.EnumerateVisualSubtree(), e => e is TextBlock { Text: "Play" });
        }

        private static Canvas ThemedCanvas() =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme())
            {
                IsVisible = true,
            };
```

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~DocsMarkupSnippetTests"` →
FAIL (files missing).

- [ ] **Step 2: Snippets.**

`layout.xml`
```xml
<!-- Each child gets the space its parent offers; its size, margin and alignment decide where it sits in it. -->
<StackPanel Orientation="Vertical" Width="320" Padding="8">
  <Button HorizontalAlignment="Left" Margin="0,0,0,8">Left</Button>
  <Button Margin="0,0,0,8">Centered (the theme's default for buttons)</Button>
  <Button HorizontalAlignment="Stretch" Margin="0,0,0,8">Stretch</Button>
  <Button HorizontalAlignment="Right" MinWidth="160">Right, at least 160 wide</Button>
</StackPanel>
```

`binding.xml`
```xml
<!-- One-way (the default), two-way, and element-to-element bindings. The data context is the view model. -->
<StackPanel Orientation="Vertical" Width="260">
  <TextBlock Text="{Binding Title}" FontSize="20" Margin="0,0,0,8" />
  <TextBox Text="{Binding PlayerName, Mode=TwoWay}" Margin="0,0,0,8" />
  <Slider x:Name="Volume" Minimum="0" Maximum="100" Value="70" />
  <TextBlock Text="{Binding Value, ElementName=Volume}" />
</StackPanel>
```

`styles.xml`
```xml
<!-- A keyed style based on another: BasedOn reuses its setters, and the theme still supplies the button's template. -->
<StackPanel Orientation="Vertical" Width="220">
  <StackPanel.Resources>
    <Style x:Key="MenuButton" TargetType="Button" Padding="24,8" HorizontalAlignment="Stretch" />
    <Style x:Key="DangerButton" TargetType="Button" BasedOn="{StaticResource MenuButton}" Background="#FFB03A3A" />
  </StackPanel.Resources>
  <Button Style="{StaticResource MenuButton}" Margin="0,0,0,8">Continue</Button>
  <Button Style="{StaticResource DangerButton}">Quit</Button>
</StackPanel>
```

`template.xml`
```xml
<!-- A control template replaces how a control looks; TemplateBinding passes the control's own properties into it. -->
<StackPanel Orientation="Vertical">
  <StackPanel.Resources>
    <ControlTemplate x:Key="FlatButton" TargetType="Button">
      <Border Background="{TemplateBinding Background}" Padding="{TemplateBinding Padding}">
        <ContentPresenter Content="{TemplateBinding Content}" HorizontalAlignment="Center" VerticalAlignment="Center" />
      </Border>
    </ControlTemplate>
    <Style TargetType="Button" Template="{StaticResource FlatButton}" Background="#FF3C78D8" Padding="20,8">
      <Style.StateGroups>
        <VisualStateGroup Name="CommonStates">
          <VisualState Name="Hovered" State="Hovered" Background="#FF5A87E6" Duration="0:0:0.15" />
          <VisualState Name="Pressed" State="Pressed" Background="#FF2C5AA8" />
        </VisualStateGroup>
      </Style.StateGroups>
    </Style>
  </StackPanel.Resources>
  <Button>Play</Button>
</StackPanel>
```

- [ ] **Step 3: Run.** Same command → all pass. If `Styles_KeyedStyle_KeepsTheThemeTemplate` fails (a keyed style drops
  the theme's template), **stop and ask Ivan** (library behaviour). Other failures from wrong names: fix the snippet,
  ledger a ruling.
- [ ] **Step 4: Mutation check.** Remove `ElementName=Volume` from `binding.xml`; confirm
  `Binding_ElementName_FollowsTheSlider` fails; restore.
- [ ] **Step 5: Full suite** (`Failed: 0`) and commit:
  `git add docfx/snippets/markup/concepts sources/IcyUI.Tests/Docs && git commit -m "Docs: concept markup snippets with behaviour tests"`.

---

### Task 2: Concept C# snippets

**Files:**
- Create: `sources/IcyUI.Docs.Snippets.Stride/Concepts.cs` (namespace `MyGame.Concepts`, file header comment like
  `Controls.cs`)

**Interfaces:**
- Produces regions: `canvases`, `game-input`, `configure`, `animate`, `scaling`, `load-asset`, `drag-drop`,
  `debug-keys`, `logging`.

- [ ] **Step 1: Write the regions** (each a static method or small class inside a static class `ConceptsExamples`,
  except `drag-drop` which is a class):

```csharp
        #region canvases
        // A second canvas for a HUD, drawn above the main one; a background canvas at index 0 would draw below.
        public static Canvas AddHudCanvas(IIcyHost ui)
        {
            var hud = new Canvas(ui.Configuration);
            ui.Canvases.Add(hud);
            return hud;
        }
        #endregion

        #region game-input
        // In your game's update: don't fire when the player clicks a button.
        public static bool ShouldFire(IIcyHost ui, bool mousePressed) => mousePressed && !ui.MainCanvas.IsMouseOverGUI;
        #endregion

        #region configure
        public static StrideIcyHost SetUp(IGame game) => game.UseIcyUI(ui =>
        {
            ui.ConfigureFonts().WithDefaultFontFamily("Noto Sans");                 // instead of Segoe UI
            ui.ConfigureLogging(LogLevel.Information);                             // more than warnings
            ui.ConfigureGestures(gestures => gestures.HoldDelay = TimeSpan.FromSeconds(0.8));
        });
        #endregion

        #region animate
        // Fades a panel in over 0.3 seconds.
        public static Animation FadeIn(UIElement panel) =>
            panel.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(0.3), 0f, 1f, Easing.EaseOutCubic));
        #endregion

        #region scaling
        // The UI is designed for 1920 x 1080 and scales to fit the window; the player's UI size setting multiplies it.
        public static void DesignFor1080p(IConfigurationBuilder ui, float playerUiScale) =>
            ui.ConfigureScaling(scaling =>
            {
                scaling.Mode = UIScaleMode.ReferenceResolution;
                scaling.ReferenceSize = new Size(1920, 1080);
                scaling.ReferenceFit = ReferenceFit.Fit;
                scaling.UserScale = playerUiScale;
            });
        #endregion

        #region load-asset
        // Loads a markup file from the game's folder without adding it anywhere yet.
        public static UIElement LoadPauseMenu(IIcyHost ui) => ui.Configuration.Assets.Load<UIElement>("Menus/Pause.xml");
        #endregion

        #region debug-keys
        // F1 toggles the layout boxes, F2 the diagnostics panel (frame time, memory, focused element).
        public static void BindDebugKeys(IIcyHost ui)
        {
            void Toggle(string tool)
            {
                if (!ui.MainCanvas.ActiveDebugTools.Remove(tool))
                    ui.MainCanvas.ActiveDebugTools.Add(tool);
            }

            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(() => Toggle("Bounds")), new KeyGesture(Keys.F1));
            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(() => Toggle("DiagnosticsHud")), new KeyGesture(Keys.F2));
        }
        #endregion

        #region logging
        // Sends IcyUI's messages to your own logging, for example Serilog or the console, through any ILoggerProvider.
        public static StrideIcyHost SetUpWithLogging(IGame game, ILoggerProvider provider) =>
            game.UseIcyUI(ui => ui.ConfigureLogging().AddLoggerProvider(provider));
        #endregion
```

`drag-drop` (top-level class in the same namespace):
```csharp
    #region drag-drop
    /// <summary>An inventory slot: drag its item to another slot.</summary>
    public sealed class InventorySlot : Border, IDragSource, IDropTarget
    {
        public string? Item { get; set; }

        public bool TryBeginDrag(Point screenPoint, out object? payload, out UIElement? preview)
        {
            payload = Item;
            preview = Item is null ? null : new TextBlock { Text = Item };
            return Item is not null;
        }

        public bool CanDrop(DragDropSession session) => Item is null && session.Payload is string;

        public void OnDragEnter(DragDropSession session) => Opacity = 0.7f;

        public void OnDragOver(DragDropSession session)
        {
        }

        public void OnDragLeave(DragDropSession session) => Opacity = 1f;

        public void OnDrop(DragDropSession session)
        {
            Item = (string)session.Payload;
            ((InventorySlot)session.Source).Item = null;
            Opacity = 1f;
        }
    }
    #endregion
```
Adapt names to the real signatures if the build says otherwise (`Border`'s namespace, `ConfigureLogging()` overload
with no arguments, `RegisterCommand`'s parameter types) and ledger each change. Usings: `System.Drawing`,
`CommunityToolkit.Mvvm.Input`, `Icy.Animations`, `Icy.Configuration`, `Icy.Input.Devices`, `Icy.Input.DragDrop`,
`Icy.Stride.Configuration`, `Icy.UI`, `Icy.UI.Controls`, `Microsoft.Extensions.Logging`, `Stride.Games`.

- [ ] **Step 2: Build.** `dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release`
  → `0 Error(s)`. Commit: `git add sources/IcyUI.Docs.Snippets.Stride/Concepts.cs && git commit -m "Docs: compiled C# examples for the concept pages"`.

---

### Tasks 3–5: Concept pages (English) with Russian stubs

**Files:**
- `docfx/en/concepts/{index,canvas-and-host,configuration,layout,data-binding,resources-and-styles,templates-and-visual-states,input-and-focus,animations,scaling,assets-and-fonts,logging-and-diagnostics}.md`
- `docfx/en/concepts/toc.yml` (Overview + the 11 in this order), `docfx/ru/concepts/toc.yml` (Обзор, Canvas, хост и
  дерево элементов, Конфигурация, Компоновка, Привязка данных, Ресурсы, стили и темы, Шаблоны и визуальные состояния,
  Ввод, фокус и навигация, Анимации, DPI и масштабирование, Ресурсы и шрифты, Журнал и диагностика)
- Russian stubs for each new page (`translation: stub`), same format as Plan 2a.

Page skeleton (headings in English; adapt the middle sections to the topic):
```markdown
# <Title>

<The idea in one or two paragraphs.>

## <Example heading>

[!code-xml[](../../snippets/markup/concepts/<snippet>.xml)]   or   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#<region>)]

## <The rules: 2–4 sections, each a rule a developer needs to predict behaviour>

## Pitfalls

## See also
```

- [ ] **Task 3:** `index.md` (replace the stub: one paragraph per page, in order), `canvas-and-host.md` (regions
  `canvases`, `game-input`), `configuration.md` (region `configure`), `layout.md` (`layout.xml`), `data-binding.md`
  (`binding.xml`). Build docs, check navigation; commit "Docs: concepts — canvas and host, configuration, layout,
  data binding".
- [ ] **Task 4:** `resources-and-styles.md` (`styles.xml`, link the markup reference's resources section),
  `templates-and-visual-states.md` (`template.xml`), `input-and-focus.md` (region `drag-drop`), `animations.md` (region
  `animate`). Commit "Docs: concepts — styles, templates, input, animations".
- [ ] **Task 5:** `scaling.md` (region `scaling`), `assets-and-fonts.md` (region `load-asset`; reuse `Fonts.cs#own-font`),
  `logging-and-diagnostics.md` (regions `logging`, `debug-keys`). Commit "Docs: concepts — scaling, assets and fonts,
  logging and diagnostics".

Each page uses the Verified facts above for its topic; link controls pages and the markup reference where they cover
details. Each task ends with `bash .github/scripts/build-docs.sh` (exit 0), `check-navigation.sh` and
`check-translations.sh` (no stale or missing).

---

### Task 6: Ivan reviews the English pages (stop)

Serve the site (`dotnet docfx serve docfx/_site`), give Ivan `http://localhost:8080/concepts/index.html` and the page
list, wait for approval or edits; apply edits, rebuild, commit.

### Task 7: Russian pages

Translate the 12 approved pages (overview + 11), add any new glossary terms, set `source-hash`. Checks: build exit 0,
translations fresh for all concept pages (only Guides and Project overviews remain stubs), navigation passes. Commit
"Docs: concepts in Russian".

### Task 8: Verification and whole-branch review

Release build (shipped libraries warning-free), tests `Failed: 0`, docs build, navigation, translations; then the
final whole-branch review by a fresh reviewer, the fix pass, and the report to Ivan.
