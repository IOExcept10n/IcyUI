// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Data.Markup;

namespace Icy.Design
{
    /// <summary>
    /// Reloads the tracked documents of a <see cref="DesignSession"/> when their source files change on disk.
    /// </summary>
    /// <remarks>
    /// File system events arrive on thread pool threads. They're debounced per file and then posted to the dispatcher
    /// of the thread that created the watcher, which applies them; a document is never touched from a watcher thread.
    /// </remarks>
    internal sealed class SourceFileWatcher : IDisposable
    {
        private const int DebounceMilliseconds = 150;

        private static readonly StringComparer PathComparer =
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        private readonly DesignSession session;
        private readonly Dispatcher dispatcher;
        private readonly object sync = new();
        private readonly Dictionary<string, FileSystemWatcher> watchers = new(PathComparer);
        private readonly Dictionary<string, Timer> pending = new(PathComparer);
        private bool disposed;

        public SourceFileWatcher(DesignSession session, Dispatcher dispatcher)
        {
            this.session = session;
            this.dispatcher = dispatcher;
            session.DocumentTracked += OnDocumentTracked;
            foreach (DesignDocument document in session.Documents)
                Watch(document);
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                    return;

                disposed = true;
                session.DocumentTracked -= OnDocumentTracked;
                foreach (FileSystemWatcher watcher in watchers.Values)
                    watcher.Dispose();
                foreach (Timer timer in pending.Values)
                    timer.Dispose();
                watchers.Clear();
                pending.Clear();
            }
        }

        private void OnDocumentTracked(object? sender, DesignDocument document) => Watch(document);

        private void Watch(DesignDocument document)
        {
            string? path = ResolvePath(document);
            if (path == null || Path.GetDirectoryName(path) is not { Length: > 0 } folder)
                return;

            lock (sync)
            {
                if (disposed || watchers.ContainsKey(folder))
                    return;

                var watcher = new FileSystemWatcher(folder, "*.xml")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                };
                watcher.Changed += (_, e) => Schedule(e.FullPath, 0);
                watcher.Renamed += (_, e) => Schedule(e.FullPath, 0);
                watcher.EnableRaisingEvents = true;
                watchers.Add(folder, watcher);
            }
        }

        private string? ResolvePath(DesignDocument document) =>
            document.SourcePath != null ? session.SourcePathResolver(document.SourcePath) : null;

        private void Schedule(string path, int attempt)
        {
            lock (sync)
            {
                if (disposed)
                    return;

                if (pending.TryGetValue(path, out Timer? existing))
                {
                    existing.Change(DebounceMilliseconds, Timeout.Infinite);
                    return;
                }

                Timer? timer = null;
                timer = new Timer(
                    _ =>
                    {
                        lock (sync)
                        {
                            if (disposed)
                                return;

                            pending.Remove(path);
                            timer?.Dispose();
                        }

                        dispatcher.Invoke(() => Reload(path, attempt));
                    },
                    null,
                    DebounceMilliseconds,
                    Timeout.Infinite);
                pending.Add(path, timer);
            }
        }

        private void Reload(string path, int attempt)
        {
            if (disposed || session.IsDisposed)
                return;

            foreach (DesignDocument document in session.Documents)
            {
                if (ResolvePath(document) is not { } resolved || !PathComparer.Equals(resolved, path))
                    continue;

                try
                {
                    if (File.ReadAllText(path) != document.Text)
                        document.ReloadFromSource();
                }
                catch (IOException)
                {
                    // Still locked by the editor: try once more, then wait for the next change.
                    if (attempt == 0)
                        Schedule(path, 1);
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }
    }
}
