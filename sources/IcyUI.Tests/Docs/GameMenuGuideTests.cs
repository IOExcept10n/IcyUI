// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using MyGame.Menus;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Behaviour of the "Building a game menu" guide: its three markup pages and the code in
    /// <c>IcyUI.Docs.Snippets.Stride/Guides/GameMenu.cs</c>, compiled into this project.
    /// </summary>
    public sealed class GameMenuGuideTests : IDisposable
    {
        private readonly string root = Directory.CreateTempSubdirectory("icy-game-menu-").FullName;

        public GameMenuGuideTests()
        {
            string menus = Path.Combine(root, "Menus");
            Directory.CreateDirectory(menus);
            string snippets = Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "game-menu");
            File.Copy(Path.Combine(snippets, "main-menu.xml"), Path.Combine(menus, "MainMenu.xml"));
            File.Copy(Path.Combine(snippets, "pause-menu.xml"), Path.Combine(menus, "PauseMenu.xml"));
            File.Copy(Path.Combine(snippets, "settings.xml"), Path.Combine(menus, "Settings.xml"));
        }

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void AppliedSize_RevertsWhenNotConfirmed()
        {
            var scaling = new ScalingConfiguration();
            var settings = new SettingsViewModel(scaling) { DraftScale = 1.5f };

            settings.ApplyCommand.Execute(null);
            Assert.Equal(1.5f, scaling.UserScale);
            Assert.True(settings.IsConfirming);

            settings.Tick(9f);
            Assert.Equal(1.5f, scaling.UserScale);

            settings.Tick(1.5f);
            Assert.Equal(1f, scaling.UserScale);
            Assert.Equal(1f, settings.DraftScale);
            Assert.False(settings.IsConfirming);
        }

        [Fact]
        public void AppliedSize_StaysWhenKept()
        {
            var scaling = new ScalingConfiguration();
            var settings = new SettingsViewModel(scaling) { DraftScale = 1.5f };
            settings.ApplyCommand.Execute(null);

            settings.KeepCommand.Execute(null);
            settings.Tick(30f);

            Assert.Equal(1.5f, scaling.UserScale);
            Assert.False(settings.IsConfirming);
        }

        [Fact]
        public void Cancel_DiscardsTheDraft_AndRevertCommandRestoresTheOldSize()
        {
            var scaling = new ScalingConfiguration();
            var settings = new SettingsViewModel(scaling) { DraftScale = 1.5f };

            settings.CancelCommand.Execute(null);
            Assert.Equal(1f, settings.DraftScale);
            Assert.Equal(1f, scaling.UserScale);

            settings.DraftScale = 2f;
            settings.ApplyCommand.Execute(null);
            settings.RevertCommand.Execute(null);
            Assert.Equal(1f, scaling.UserScale);
        }

        [Fact]
        public void MainMenu_ShowsItsPage_AndPlayHidesIt()
        {
            using var game = new Game(root);

            Assert.True(game.Menu.Menu.IsOpen);
            Page page = Assert.IsType<Page>(game.Frame.Navigation.CurrentPage);
            Assert.NotNull(page.FindControl<Button>("PlayButton"));

            page.FindRequiredControl<Button>("PlayButton").Command!.Execute(null);

            Assert.True(game.Menu.IsPlaying);
            Assert.False(game.Menu.Menu.IsOpen);
        }

        [Fact]
        public void Escape_PausesAndResumes()
        {
            using var game = new Game(root);
            game.Play();

            game.Menu.HandleEscape();
            Assert.True(game.Menu.IsPaused);
            Assert.True(game.Menu.Menu.IsOpen);
            Assert.NotNull(game.Frame.Navigation.CurrentPage!.FindControl<Button>("ResumeButton"));

            game.Menu.HandleEscape();
            Assert.False(game.Menu.IsPaused);
            Assert.False(game.Menu.Menu.IsOpen);
        }

        [Fact]
        public void ResumeButton_ResumesTheGame()
        {
            using var game = new Game(root);
            game.Play();
            game.Menu.HandleEscape();

            game.Frame.Navigation.CurrentPage!.FindRequiredControl<Button>("ResumeButton").Command!.Execute(null);

            Assert.False(game.Menu.IsPaused);
            Assert.False(game.Menu.Menu.IsOpen);
        }

        [Fact]
        public void Escape_OnSettings_GoesBackToThePauseMenu_NotToTheGame()
        {
            using var game = new Game(root);
            game.Play();
            game.Menu.HandleEscape();
            game.Frame.Navigation.CurrentPage!.FindRequiredControl<Button>("ResumeButton");
            game.Menu.Menu.SettingsCommand.Execute(null);
            Assert.NotNull(game.Frame.Navigation.CurrentPage!.FindControl<Slider>("UiSize"));

            game.Menu.HandleEscape();

            Assert.True(game.Menu.IsPaused);
            Assert.NotNull(game.Frame.Navigation.CurrentPage!.FindControl<Button>("ResumeButton"));
        }

        [Fact]
        public void Escape_OnTheMainMenu_DoesNothing()
        {
            using var game = new Game(root);

            game.Menu.HandleEscape();

            Assert.True(game.Menu.Menu.IsOpen);
            Assert.False(game.Menu.IsPaused);
        }

        [Fact]
        public void QuitToMenu_ReturnsToTheMainMenu()
        {
            using var game = new Game(root);
            game.Play();
            game.Menu.HandleEscape();

            game.Frame.Navigation.CurrentPage!.FindRequiredControl<Button>("QuitToMenuButton").Command!.Execute(null);

            Assert.False(game.Menu.IsPlaying);
            Assert.False(game.Menu.IsPaused);
            Assert.NotNull(game.Frame.Navigation.CurrentPage!.FindControl<Button>("PlayButton"));
        }

        [Fact]
        public void SettingsPage_BindsTheSliderApplyAndTheCountdown()
        {
            using var game = new Game(root);
            game.Menu.Menu.SettingsCommand.Execute(null);
            Page page = game.Frame.Navigation.CurrentPage!;
            var slider = page.FindRequiredControl<Slider>("UiSize");
            var confirm = page.FindRequiredControl<StackPanel>("ConfirmPanel");
            Assert.False(confirm.IsVisible);

            slider.Value = 1.5f;
            Assert.Equal(1.5f, game.Menu.Menu.Settings.DraftScale);

            page.FindRequiredControl<Button>("ApplyButton").Command!.Execute(null);
            Assert.True(confirm.IsVisible);
            Assert.Equal(1.5f, game.Configuration.Scaling.UserScale);

            game.Menu.Update(11f);
            Assert.False(confirm.IsVisible);
            Assert.Equal(1f, game.Configuration.Scaling.UserScale);
            Assert.Equal(1f, slider.Value);
        }

        [Fact]
        public void LeavingSettings_DropsAnUnconfirmedChange()
        {
            using var game = new Game(root);
            game.Menu.Menu.SettingsCommand.Execute(null);
            game.Menu.Menu.Settings.DraftScale = 1.5f;
            game.Menu.Menu.Settings.ApplyCommand.Execute(null);

            game.Frame.Navigation.CurrentPage!.FindRequiredControl<Button>("BackButton").Command!.Execute(null);

            Assert.Equal(1f, game.Configuration.Scaling.UserScale);
            Assert.NotNull(game.Frame.Navigation.CurrentPage!.FindControl<Button>("PlayButton"));
        }

        /// <summary>A themed canvas as the host's main canvas, with the guide's controller attached.</summary>
        private sealed class Game : IIcyHost, IDisposable
        {
            public Game(string root)
            {
                var builder = new IcyConfigurationBuilder();
                builder.ConfigureRendering(new FakeRenderContext())
                       .ConfigureInput(new FakeInputSystem())
                       .ConfigureTypes()
                       .ConfigureAssets()
                       .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
                builder.AddMarkupSupport();
                builder.UseDefaultTheme();
                Configuration = builder.Build();
                MainCanvas = new Canvas(Configuration) { IsVisible = true };
                Canvases = [MainCanvas];
                Menu = new MenuController(this);
            }

            public IcyConfiguration Configuration { get; }

            public Canvas MainCanvas { get; }

            public IList<Canvas> Canvases { get; }

            public MenuController Menu { get; }

            public Frame Frame => Menu.Frame;

            public void Play() => Menu.Menu.PlayCommand.Execute(null);

            public void Dispose()
            {
            }
        }
    }
}
