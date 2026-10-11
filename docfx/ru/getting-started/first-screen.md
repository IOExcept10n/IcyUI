---
source-hash: 8f66668ac549
---

# Ваш первый экран

На этой странице мы соберём небольшое главное меню: заголовок и две кнопки, описанные разметкой и связанные с кодом
игры через привязку данных. Предполагается, что вы прошли
[Начало работы со Stride](stride.md) или [с MonoGame](monogame.md).

## 1. Опишите меню разметкой

Добавьте в проект игры файл `Menu.xml`:

[!code-xml[](../../snippets/markup/first-screen.xml)]

Установите для него свойство **Copy to Output Directory** в «Copy if newer» или добавьте в файл проекта:

```xml
<ItemGroup>
  <None Update="Menu.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Разметка — обычный XML: каждый элемент — это элемент управления IcyUI, а каждый атрибут задаёт свойство. Три значения
`{Binding Path=...}` — не готовый текст: они читают `Title`, `PlayCommand` и `QuitCommand` из *контекста данных*
меню — объекта, который вы передаёте из кода.

## 2. Загрузите её и передайте данные

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

Вызовите `MainMenu.Show(canvas, configuration)` из стартового скрипта вместо кнопки «Hello, IcyUI».

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

Вызовите `MainMenu.Show(canvas, configuration, viewModel)` в конце `Initialize()`. В качестве модели представления
подойдёт класс со строкой `Title` и командами `PlayCommand`/`QuitCommand` — как `MenuViewModel` на вкладке Stride.

---

<xref:Icy.Markup.MarkupLoader> превращает разметку в элементы. Свойство <xref:Icy.UI.UIElement.DataContext> корня
делает так, что каждая `{Binding}` внутри читает данные из вашей модели представления. Щелчок по кнопке или её
активация с клавиатуры или геймпада выполняет её команду.

## Что получилось

Меню, у которого компоновка живёт в разметке, а поведение — в C#. Меняйте разметку, чтобы переоформить или
перестроить меню, не трогая код, или измените `Title` модели представления, чтобы увидеть привязку в действии.

## Далее

- [Справочник по разметке](../markup/index.md) описывает всё, что умеет разметка.
- [Справочник по API](/api/index.html) (на английском) документирует каждый тип, начиная с <xref:Icy.UI.Canvas>.
