---
source-hash: 66198a12f32b
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

Замените стартовый скрипт этим (и выберите `MenuStartup` в **Add component**):

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

# [MonoGame](#tab/monogame)

Загрузите меню в конце `Initialize()`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

---

<xref:Icy.UI.Canvas.LoadMarkup(System.String,System.Object)> загружает файл разметки из папки сборки игры, делает
модель представления <xref:Icy.UI.UIElement.DataContext> меню, чтобы каждая `{Binding}` внутри читала данные из неё,
и добавляет меню на Canvas. Щелчок по кнопке или её активация с клавиатуры или геймпада выполняет её команду.

## Что получилось

Меню, у которого компоновка живёт в разметке, а поведение — в C#. Меняйте разметку, чтобы переоформить или
перестроить меню, не трогая код. `MenuViewModel` наследует `ObservableObject` (из CommunityToolkit.Mvvm, от которого
IcyUI уже зависит), поэтому изменение его `Title` во время игры обновляет заголовок на экране. Модель представления,
которая не вызывает `PropertyChanged`, читается только один раз — при создании привязки.

## Далее

- [Справочник по разметке](../markup/index.md) описывает всё, что умеет разметка.
- [Справочник по API](/api/index.html) (на английском) документирует каждый тип, начиная с <xref:Icy.UI.Canvas>.
