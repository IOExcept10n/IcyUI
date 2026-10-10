using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Icy.Tests.Docs
{
    /// <summary>The data context markup snippets in docfx/snippets/markup may bind to; mirrors the snippet projects' view model.</summary>
    public sealed class DocsSnippetViewModel
    {
        public string Title { get; set; } = "My Game";

        public ICommand PlayCommand { get; } = new RelayCommand(() => { });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { });
    }
}
