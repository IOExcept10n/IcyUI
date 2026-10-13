---
_layout: landing
source-hash: 3ec199dbf142
---

# IcyUI

IcyUI — библиотека пользовательского интерфейса для игр на .NET. Экраны описываются XML-разметкой или кодом на C#,
оформляются темами и шаблонами элементов управления, привязываются к данным игры и управляются мышью, клавиатурой,
геймпадом или касаниями. Ядро не зависит от движка; тонкие пакеты интеграции подключают его к **Stride** и
**MonoGame**.

> [!NOTE]
> IcyUI находится на ранней стадии альфа-версии (`0.1.0-alpha`). Библиотекой можно пользоваться, но API может меняться
> между альфа-выпусками.

## Установка

Выберите пакет для своего движка — он подтянет основной пакет `IcyUI` автоматически.

```
dotnet add package IcyUI.Stride --prerelease
dotnet add package IcyUI.MonoGame --prerelease
```

## Что дальше

<div class="row row-cols-1 row-cols-md-3 g-3 mb-4">
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="getting-started/installation.md"><strong>Начало работы</strong><span>Установите IcyUI и выведите первый экран в игре на Stride или MonoGame.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="concepts/index.md"><strong>Концепции</strong><span>Компоновка, привязка данных, стили, ввод и остальная модель.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="controls/index.md"><strong>Элементы управления</strong><span>Все элементы управления со снимками экрана и разметкой.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="guides/index.md"><strong>Руководства</strong><span>Меню, свои элементы управления, темы, редактор.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="markup/index.md"><strong>Справочник по разметке</strong><span>XML-диалект для описания интерфейса.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="/api/index.html"><strong>Справочник по API</strong><span>Все открытые типы и члены (на английском).</span></a></div>
</div>

## Что внутри

- XML-разметка с привязкой данных, ресурсами, стилями, визуальными состояниями и шаблонами элементов управления.
- Встроенная тема по умолчанию.
- Панели компоновки: `Grid`, `StackPanel`, `WrapGrid`, `SplitPane` и виртуализирующий `ScrollViewer`.
- Элементы управления, например `Button`, `CheckBox`, `Slider`, `TextBox`, `ComboBox`, `ListBox`, `TreeView`,
  `TabControl`, `Expander`, `Dialog`, `ColorPicker` и `PropertyGrid`.
- Переход между элементами с клавиатуры и геймпада, жесты касания, перетаскивание и масштабирование с учётом DPI.
- Анимации.
- Внутриигровой редактор интерфейса с горячей перезагрузкой — в отдельном пакете `IcyUI.Design`.

IcyUI — проект с открытым исходным кодом под лицензией MIT:
[github.com/IOExcept10n/IcyUI](https://github.com/IOExcept10n/IcyUI).
