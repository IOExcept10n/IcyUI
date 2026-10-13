// Engine-neutral code for docfx/*/guides/game-menu.md. Each #region is one example on the page. This file is also compiled
// into IcyUI.Tests (and into the MonoGame snippets), so the guide's behaviour is tested. It must not reference an engine.
namespace MyGame.Menus
{
    using System;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Configuration;
    using Icy.Input.Devices;
    using Icy.UI;
    using Icy.UI.Controls;

    #region apply-revert
    /// <summary>
    /// The settings page's data. The slider edits a draft; Apply commits it, and unless the player confirms within ten
    /// seconds, the previous size comes back, so a bad size can never leave the UI unusable.
    /// </summary>
    public sealed class SettingsViewModel : ObservableObject
    {
        private const float RevertSeconds = 10;

        private readonly ScalingConfiguration scaling;
        private float draftScale;
        private float previousScale;
        private float remaining;
        private bool isConfirming;

        public SettingsViewModel(ScalingConfiguration scaling)
        {
            this.scaling = scaling;
            draftScale = scaling.UserScale;
            ApplyCommand = new RelayCommand(Apply);
            CancelCommand = new RelayCommand(Cancel);
            KeepCommand = new RelayCommand(Keep);
            RevertCommand = new RelayCommand(Revert);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke(this, EventArgs.Empty));
        }

        /// <summary>Raised when the player presses Back; the menu goes to the previous page.</summary>
        public event EventHandler? BackRequested;

        /// <summary>The slider's value. Nothing changes on screen until Apply.</summary>
        public float DraftScale
        {
            get => draftScale;
            set => SetProperty(ref draftScale, value);
        }

        /// <summary>True while an applied change waits for the player to keep or revert it.</summary>
        public bool IsConfirming
        {
            get => isConfirming;
            private set => SetProperty(ref isConfirming, value);
        }

        /// <summary>The time left, from 1 down to 0, for the countdown bar.</summary>
        public float RevertProgress => remaining / RevertSeconds;

        public string CountdownText => $"Keep this size? Reverting in {MathF.Ceiling(remaining)} s";

        public ICommand ApplyCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand KeepCommand { get; }

        public ICommand RevertCommand { get; }

        public ICommand BackCommand { get; }

        /// <summary>Advances the countdown. Call it every frame with the frame time, even while the game is paused.</summary>
        public void Tick(float elapsedSeconds)
        {
            if (!IsConfirming)
                return;

            remaining -= elapsedSeconds;
            if (remaining <= 0)
                Revert();
            else
                OnCountdownChanged();
        }

        /// <summary>Puts the previous size back, if a change is waiting for confirmation.</summary>
        public void Revert()
        {
            if (!IsConfirming)
                return;

            scaling.UserScale = previousScale;
            DraftScale = previousScale;
            Finish();
        }

        private void Apply()
        {
            if (IsConfirming)
                return;

            previousScale = scaling.UserScale;
            scaling.UserScale = DraftScale;
            remaining = RevertSeconds;
            IsConfirming = true;
            OnCountdownChanged();
        }

        private void Keep() => Finish();

        private void Cancel() => DraftScale = scaling.UserScale;

        private void Finish()
        {
            remaining = 0;
            IsConfirming = false;
            OnCountdownChanged();
        }

        private void OnCountdownChanged()
        {
            OnPropertyChanged(nameof(RevertProgress));
            OnPropertyChanged(nameof(CountdownText));
        }
    }
    #endregion

    #region menu-viewmodel
    /// <summary>
    /// The data behind every menu page: the commands the pages bind to. Starting, resuming and quitting are the game's
    /// business, so they're events; opening Settings and going back are the menu's own.
    /// </summary>
    public sealed class MenuViewModel : ObservableObject
    {
        private readonly Frame frame;

        public MenuViewModel(Frame frame, SettingsViewModel settings)
        {
            this.frame = frame;
            Settings = settings;
            settings.BackRequested += (_, _) => GoBack();
            PlayCommand = new RelayCommand(() => PlayRequested?.Invoke(this, EventArgs.Empty));
            ResumeCommand = new RelayCommand(() => ResumeRequested?.Invoke(this, EventArgs.Empty));
            QuitToMenuCommand = new RelayCommand(() => QuitToMenuRequested?.Invoke(this, EventArgs.Empty));
            QuitCommand = new RelayCommand(() => QuitRequested?.Invoke(this, EventArgs.Empty));
            SettingsCommand = new RelayCommand(OpenSettings);
        }

        public event EventHandler? PlayRequested;

        public event EventHandler? ResumeRequested;

        public event EventHandler? QuitToMenuRequested;

        public event EventHandler? QuitRequested;

        public string Title => "My Game";

        public SettingsViewModel Settings { get; }

        public ICommand PlayCommand { get; }

        public ICommand ResumeCommand { get; }

        public ICommand QuitToMenuCommand { get; }

        public ICommand QuitCommand { get; }

        public ICommand SettingsCommand { get; }

        public bool IsOpen => frame.IsVisible;

        public void ShowMainMenu() => Show("Menus/MainMenu.xml");

        public void ShowPauseMenu() => Show("Menus/PauseMenu.xml");

        /// <summary>Hides the menu, dropping any change that was never confirmed.</summary>
        public void Close()
        {
            Settings.Revert();
            frame.Navigation.Clear();
            frame.IsVisible = false;
        }

        /// <summary>Goes back one page, dropping an unconfirmed change. Returns false on the first page.</summary>
        public bool GoBack()
        {
            Settings.Revert();
            return frame.Navigation.TryNavigateBack() != null;
        }

        private void OpenSettings()
        {
            // Bindings follow their data context, not what is nested inside it: the page binds to the settings itself.
            if (frame.Navigation.Navigate("Menus/Settings.xml") is { } page)
                page.DataContext = Settings;
        }

        private void Show(string page)
        {
            Settings.Revert();
            frame.Navigation.Clear();
            frame.IsVisible = true;
            frame.Navigation.Navigate(page);
        }
    }
    #endregion

    #region menu-setup
    /// <summary>
    /// Owns the menu and the game's play/pause state. Engine-neutral: the game's update loop calls <see cref="Update"/>
    /// and checks <see cref="IsPaused"/>.
    /// </summary>
    public sealed class MenuController
    {
        public MenuController(IIcyHost ui)
        {
            Frame = new Frame
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            ui.MainCanvas.Add(Frame);

            Menu = new MenuViewModel(Frame, new SettingsViewModel(ui.Configuration.Scaling));
            Frame.DataContext = Menu;

            Menu.PlayRequested += (_, _) => StartPlaying();
            Menu.ResumeRequested += (_, _) => Resume();
            Menu.QuitToMenuRequested += (_, _) => QuitToMenu();
            Menu.QuitRequested += (_, _) => QuitRequested?.Invoke(this, EventArgs.Empty);
            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(HandleEscape), new KeyGesture(Keys.Escape));

            Menu.ShowMainMenu();
        }

        /// <summary>Raised when the player chooses Quit; the game exits.</summary>
        public event EventHandler? QuitRequested;

        public Frame Frame { get; }

        public MenuViewModel Menu { get; }

        public bool IsPlaying { get; private set; }

        /// <summary>True while the pause menu is open: gameplay should skip its update.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Call every frame with the frame time. The Settings countdown runs even while paused.</summary>
        public void Update(float elapsedSeconds) => Menu.Settings.Tick(elapsedSeconds);

        /// <summary>Escape: pause during play, go back a page in a menu, or resume from the pause menu's first page.</summary>
        public void HandleEscape()
        {
            if (!Menu.IsOpen)
            {
                if (IsPlaying)
                    Pause();
            }
            else if (!Menu.GoBack() && IsPaused)
            {
                Resume();
            }
        }

        private void StartPlaying()
        {
            IsPlaying = true;
            IsPaused = false;
            Menu.Close();
        }

        private void Pause()
        {
            IsPaused = true;
            Menu.ShowPauseMenu();
        }

        private void Resume()
        {
            IsPaused = false;
            Menu.Close();
        }

        private void QuitToMenu()
        {
            IsPlaying = false;
            IsPaused = false;
            Menu.ShowMainMenu();
        }
    }
    #endregion
}
