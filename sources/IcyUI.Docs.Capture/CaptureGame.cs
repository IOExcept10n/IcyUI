// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using Icy.Configuration;
using Icy.Markup;
using Icy.Stride.Configuration;
using Icy.Tests.Docs;
using Icy.UI;
using Icy.UI.Controls;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Image = Stride.Graphics.Image;
using Stride.Rendering.Compositing;
using Color4 = Stride.Core.Mathematics.Color4;

namespace Icy.Docs.Capture
{
    /// <summary>
    /// Renders every controls snippet (docfx/images/controls) and README snippet (docs/images) once and saves each as a
    /// PNG. One snippet is shown at a time, for enough frames that layout and templates settle, then captured from the
    /// back buffer and removed.
    /// </summary>
    internal sealed class CaptureGame : Game
    {
        // Long enough for the default theme's transitions (expansion, visual states) to finish before the capture.
        private const int SettleFrames = 30;

        private readonly Queue<CaptureJob> pending = new();
        private readonly Dictionary<string, SortedDictionary<string, string>> manifests = new(StringComparer.Ordinal);
        private StrideIcyHost? host;
        private (CaptureJob Job, UIElement Root)? current;
        private int framesLeft;

        public CaptureGame()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
            GraphicsDeviceManager.PreferredBackBufferHeight = 800;
        }

        protected override void BeginRun()
        {
            base.BeginRun();

            SceneSystem.GraphicsCompositor = new GraphicsCompositor
            {
                Game = new SceneRendererCollection { new ClearRenderer { Color = new Color4(0.1f, 0.1f, 0.12f, 1f) } },
            };
            SceneSystem.SceneInstance = new SceneInstance(Services, new Scene());
            host = this.UseIcyUI();
            host.MainCanvas.ScaleMode = UIScaleMode.None;

            string root = SnippetCapture.FindRepositoryRoot(AppContext.BaseDirectory);
            string markup = Path.Combine(root, "docfx", "snippets", "markup");

            // Control screenshots are padded crops for the docs; the README cover is the scene itself, edge to edge.
            Enqueue(Path.Combine(markup, "controls"), Path.Combine(root, "docfx", "images", "controls"), SnippetCapture.Padding);
            Enqueue(Path.Combine(markup, "readme"), Path.Combine(root, "docs", "images"), 0);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (current != null || host == null || Environment.ExitCode != 0)
                return;

            if (!pending.TryDequeue(out CaptureJob? job))
            {
                foreach ((string folder, SortedDictionary<string, string> manifest) in manifests)
                {
                    File.WriteAllLines(Path.Combine(folder, "captured.txt"), manifest.Select(x => $"{x.Key} {x.Value}"));
                    Console.WriteLine($"Captured {manifest.Count} snippet(s) into {folder}");
                }

                Exit();
                return;
            }

            try
            {
                UIElement element = new MarkupLoader(host.Configuration).Load(File.ReadAllText(job.File), Path.GetFileName(job.File));
                element.DataContext = new DocsSnippetViewModel();
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Top;
                element.Margin = new Thickness(job.Padding);
                host.MainCanvas.Add(element);

                // A window draws only while open; a snippet showing one opens it.
                if (element is Window window)
                    window.Show();
                current = (job, element);
                framesLeft = SettleFrames;
            }
            catch (Exception ex)
            {
                Fail(job.Name, ex);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (current is not { } shown || --framesLeft > 0)
                return;

            try
            {
                Texture backBuffer = GraphicsDevice.Presenter.BackBuffer;
                Rectangle area = SnippetCapture.CaptureArea(shown.Root.ActualBounds, new Size(backBuffer.Width, backBuffer.Height), shown.Job.Padding);
                using Image frame = backBuffer.GetDataAsImage(GraphicsContext.CommandList);
                using Image cropped = SnippetCapture.Crop(frame, area);
                using (FileStream stream = File.Create(Path.Combine(shown.Job.ImageFolder, shown.Job.Name + ".png")))
                    cropped.Save(stream, ImageFileType.Png);
                manifests[shown.Job.ImageFolder][shown.Job.Name] = SnippetCapture.Hash(shown.Job.File);
            }
            catch (Exception ex)
            {
                Fail(shown.Job.Name, ex);
                return;
            }

            host!.MainCanvas.Remove(shown.Root);
            current = null;
        }

        private void Enqueue(string snippetFolder, string imageFolder, int padding)
        {
            Directory.CreateDirectory(imageFolder);
            manifests[imageFolder] = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (string file in Directory.EnumerateFiles(snippetFolder, "*.xml").Order(StringComparer.Ordinal))
                pending.Enqueue(new CaptureJob(file, imageFolder, padding));
        }

        private void Fail(string name, Exception ex)
        {
            Console.Error.WriteLine($"FAILED {name}: {ex.Message}");
            Environment.ExitCode = 1;
            pending.Clear();
            current = null;
            Exit();
        }

        /// <summary>One snippet to render: its markup file, the folder its PNG goes to and the margin kept around it.</summary>
        private sealed record CaptureJob(string File, string ImageFolder, int Padding)
        {
            public string Name => Path.GetFileNameWithoutExtension(File);
        }
    }
}
