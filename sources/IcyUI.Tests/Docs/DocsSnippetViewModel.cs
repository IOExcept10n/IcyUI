using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Icy.UI.Controls;

namespace Icy.Tests.Docs
{
    /// <summary>The data context markup snippets in docfx/snippets/markup may bind to; mirrors the docs' MenuViewModel.</summary>
    public sealed class DocsSnippetViewModel : ObservableObject
    {
        private string title = "My Game";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand PlayCommand { get; } = new RelayCommand(() => { });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { });

        public IReadOnlyList<string> Levels { get; } = ["Forest", "Caves", "Castle"];

        public IReadOnlyList<string> Difficulties { get; } = ["Easy", "Normal", "Hard"];

        public string PlayerName { get; set; } = "Player 1";

        public IReadOnlyList<TabItem> Tabs { get; } =
        [
            new TabItem { Header = "Video", Content = "Resolution: 1920 × 1080" },
            new TabItem { Header = "Audio", Content = "Master volume: 70%" },
            new TabItem { Header = "Controls", Content = "Invert Y axis: off" },
        ];

        public DocsSnippetSettings Settings { get; } = new();
    }
}
