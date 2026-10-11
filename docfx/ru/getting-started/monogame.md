---
source-hash: 11f43bb0c863
---

# Начало работы с MonoGame

На этой странице мы подключим IcyUI к игре на MonoGame.

1. **Создайте проект** из шаблона MonoGame:

   ```
   dotnet new mgdesktopgl -o MyGame
   ```

   или `dotnet new mgwindowsdx` для бэкенда Direct3D. С IcyUI работают оба.

2. **Переведите его на .NET 10.** В `MyGame/MyGame.csproj` укажите `<TargetFramework>net10.0</TargetFramework>`
   (или `net10.0-windows`, если шаблон использовал суффикс `-windows`) и убедитесь, что пакеты MonoGame версии 3.8.5.1
   или новее.

3. **Добавьте пакет:**

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.MonoGame --prerelease
   ```

4. **Настройте IcyUI в классе `Game`.** Вызовите `UseIcyUI()` в `Initialize()` и отрисуйте `Canvas` в `Draw()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` регистрирует IcyUI как компонент игры, `GetIcyConfiguration()` возвращает конфигурацию, с которой
   создаётся каждый элемент IcyUI, а `UseDefaultTheme()` загружает встроенную тему. <xref:Icy.UI.Canvas> — корень
   всего, что рисует IcyUI: элементы добавляются в него методом <xref:Icy.UI.Canvas.Add(Icy.UI.UIElement)>.

5. **Запустите игру.** Пока `Canvas` пуст — его заполнит страница [Ваш первый экран](first-screen.md).

> [!NOTE]
> На бэкенде DesktopGL ввод касаниями недоступен, а на Windows для ARM64 игра может зависнуть при выходе. У бэкенда
> WindowsDX нет ни той, ни другой проблемы.

## Далее

На странице [Ваш первый экран](first-screen.md) добавляется меню, описанное разметкой.
