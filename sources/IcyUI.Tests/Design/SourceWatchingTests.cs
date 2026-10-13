// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Data.Markup;
using Icy.Design;
using Icy.UI;

namespace Icy.Tests.Design
{
    public sealed class SourceWatchingTests : IDisposable
    {
        private const string Original = """<StackPanel><Button x:Name="b" Width="10"/></StackPanel>""";
        private const string Edited = """<StackPanel><Button x:Name="b" Width="20"/></StackPanel>""";
        private readonly string root = Directory.CreateTempSubdirectory("icy-watch-").FullName;
        private readonly DesignTestHost host = new();

        public SourceWatchingTests() => host.Session.UseSourceRoot(root);

        public void Dispose()
        {
            host.Dispose();
            Directory.Delete(root, recursive: true);
        }

        [Fact]
        public void SavedFile_ReloadsItsDocument_OnTheUiThread()
        {
            string path = WritePage("page.xml", Original);
            (UIElement page, DesignDocument document) = host.Load(Original, "page.xml");
            using IDisposable watch = host.Session.WatchSourceFiles();

            File.WriteAllText(path, Edited);
            Thread.Sleep(400);

            Assert.Equal(Original, document.Text);
            Assert.True(PumpUntil(() => document.Text == Edited));
            Assert.Equal(20, DesignTestHost.Named<UIElement>(page, "b").Width);
        }

        [Fact]
        public void SeveralQuickWrites_ReloadOnce()
        {
            string path = WritePage("page.xml", Original);
            (_, DesignDocument document) = host.Load(Original, "page.xml");
            using IDisposable watch = host.Session.WatchSourceFiles();
            int changes = 0;
            document.Changed += (_, _) => changes++;

            for (int i = 0; i < 4; i++)
            {
                File.WriteAllText(path, i == 3 ? Edited : Original + new string(' ', i + 1));
                Thread.Sleep(20);
            }

            Assert.True(PumpUntil(() => document.Text == Edited));
            PumpFor(500);
            Assert.Equal(1, changes);
        }

        [Fact]
        public void RenameSave_IsSeen()
        {
            string path = WritePage("page.xml", Original);
            (_, DesignDocument document) = host.Load(Original, "page.xml");
            using IDisposable watch = host.Session.WatchSourceFiles();

            string temp = Path.Combine(root, "page.xml.tmp");
            File.WriteAllText(temp, Edited);
            File.Move(temp, path, overwrite: true);

            Assert.True(PumpUntil(() => document.Text == Edited));
        }

        [Fact]
        public void TextEqualToTheDocument_DoesNotReload()
        {
            string path = WritePage("page.xml", Original);
            (UIElement page, DesignDocument document) = host.Load(Original, "page.xml");
            using IDisposable watch = host.Session.WatchSourceFiles();
            document.Editor.SetAttribute(host.IdOf(DesignTestHost.Named<UIElement>(page, "b")), "Width", "20");
            Assert.True(document.IsModified);

            File.WriteAllText(path, document.Text);
            PumpFor(700);

            Assert.True(document.IsModified);
        }

        [Fact]
        public void Dispose_StopsWatching()
        {
            string path = WritePage("page.xml", Original);
            (_, DesignDocument document) = host.Load(Original, "page.xml");
            IDisposable watch = host.Session.WatchSourceFiles();
            watch.Dispose();

            File.WriteAllText(path, Edited);

            Assert.False(PumpUntil(() => document.Text == Edited, 700));
        }

        [Fact]
        public void DocumentTrackedLater_IsWatched()
        {
            string path = WritePage("late.xml", Original);
            using IDisposable watch = host.Session.WatchSourceFiles();
            (_, DesignDocument document) = host.Load(Original, "late.xml");

            File.WriteAllText(path, Edited);

            Assert.True(PumpUntil(() => document.Text == Edited));
        }

        private static bool PumpUntil(Func<bool> condition, int timeoutMs = 2000)
        {
            Dispatcher dispatcher = Dispatcher.GetCurrentThreadDispatcher();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < timeoutMs)
            {
                dispatcher.Update();
                if (condition())
                    return true;
                Thread.Sleep(10);
            }

            return condition();
        }

        private static void PumpFor(int milliseconds) => PumpUntil(static () => false, milliseconds);

        private string WritePage(string name, string text)
        {
            string path = Path.Combine(root, name);
            File.WriteAllText(path, text);
            return path;
        }
    }
}
