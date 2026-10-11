---
source-hash: 00ed2f600c97
---

# Начало работы со Stride

На этой странице мы выведем кнопку IcyUI в игре на Stride. Большинство игр на Stride — это проекты Game Studio,
поэтому этот путь идёт первым; если игра написана только кодом, переходите к разделу
[Без Game Studio](#без-game-studio).

## В проекте Game Studio

1. **Создайте проект** в лаунчере Stride (подойдёт любой шаблон, например «New Game»).

2. **Переведите его на .NET 10.** Закройте Game Studio и в каждом файле проекта решения
   (`MyGame/MyGame.csproj` и проект платформы, например `MyGame.Windows/MyGame.Windows.csproj`) укажите:

   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

   Проект платформы Windows может использовать `net10.0-windows` — сохраните суффикс, который у него был.

3. **Добавьте пакет** в проект игры `MyGame/MyGame.csproj` (общий, а не проект платформы):

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.Stride --prerelease
   ```

4. **Добавьте стартовый скрипт.** Создайте в проекте игры файл `IcyUIStartup.cs`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]

   `UseIcyUI()` подключает IcyUI к игре и оборачивает графический компоновщик сцены (graphics compositor), так что
   интерфейс рисуется поверх всего, что рисует сцена. `GetIcyConfiguration()` возвращает конфигурацию, с которой
   создаётся каждый элемент IcyUI, а `UseDefaultTheme()` загружает встроенную тему.

5. **Прикрепите скрипт.** Снова откройте проект в Game Studio и дождитесь перезагрузки скриптов. В стартовой сцене
   выберите любую сущность (подойдёт камера), затем в панели свойств нажмите **Add component** и выберите
   `IcyUIStartup`.

6. **Запустите игру.** Поверх сцены появится кнопка «Hello, IcyUI», реагирующая на мышь и клавиатуру.

## Без Game Studio

Если вы наследуете собственный класс `Game` и настраиваете сцену в коде, настройте IcyUI в `BeginRun()`, когда
графический компоновщик уже создан:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#code-only)]

`UseIcyUI()` оборачивает `SceneSystem.GraphicsCompositor`, поэтому к этому моменту он не должен быть `null`.

> [!NOTE]
> IcyUI рисует поверх вывода компоновщика сцены. Элементы видны, только когда они добавлены в <xref:Icy.UI.Canvas>,
> который находится в коллекции `Canvases` оверлея.

## Далее

На странице [Ваш первый экран](first-screen.md) кнопка заменяется меню, описанным разметкой.
