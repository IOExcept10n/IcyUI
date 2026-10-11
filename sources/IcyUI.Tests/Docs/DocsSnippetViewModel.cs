using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    }
}
