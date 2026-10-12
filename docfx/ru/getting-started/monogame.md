---
source-hash: 381563956bdb
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

4. **Настройте IcyUI в классе `Game`.** Вызовите `UseIcyUI()` в `Initialize()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` добавляет IcyUI как компонент игры и возвращает хост: `ui.MainCanvas` — это Canvas, на который
   добавляются элементы, а `ui.Configuration` хранит все настройки. Настройки по умолчанию загружают встроенную тему и
   используют системный шрифт Segoe UI. IcyUI рисует во время `base.Draw()`, после вашей отрисовки; всё, что
   нарисовано после `base.Draw()`, перекроет интерфейс. Строка, переданная как содержимое, становится
   <xref:Icy.UI.Controls.TextBlock>.

5. **Запустите игру.** Появится кнопка «Hello, IcyUI», реагирующая на мышь и клавиатуру.

> [!NOTE]
> IcyUI не поставляется со шрифтом. На Linux и macOS или чтобы использовать свой шрифт, импортируйте файл шрифта,
> поставляемый с игрой:
> `this.UseIcyUI(ui => ui.ConfigureFonts().ImportFont("Fonts/MyFont.ttf").WithDefaultFontFamily("My Font"))`. См.
> [Шрифты на других платформах](stride.md#шрифты-на-других-платформах). Предупреждения IcyUI появляются в окне
> Output отладчика.

> [!NOTE]
> На бэкенде DesktopGL ввод касаниями недоступен, а на Windows для ARM64 игра может зависнуть при выходе. WindowsDX
> не использует слой SDL, из-за которого возникают обе проблемы, но ввод касаниями и корректный выход на ARM64 там
> пока не проверены.

## Далее

На странице [Ваш первый экран](first-screen.md) добавляется меню, описанное разметкой.
