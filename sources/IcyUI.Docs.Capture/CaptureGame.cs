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
    /// Renders every controls snippet once and saves it as a PNG. One snippet is shown at a time, for enough frames that
    /// layout and templates settle, then captured from the back buffer and removed.
    /// </summary>
    internal sealed class CaptureGame : Game
    {
        // Long enough for the default theme's transitions (expansion, visual states) to finish before the capture.
        private const int SettleFrames = 30;

        private readonly Queue<string> pending = new();
        private readonly SortedDictionary<string, string> manifest = new(StringComparer.Ordinal);
        private string snippetFolder = string.Empty;
        private string imageFolder = string.Empty;
        private StrideIcyHost? host;
        private (string Name, UIElement Root)? current;
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
            snippetFolder = Path.Combine(root, "docfx", "snippets", "markup", "controls");
            imageFolder = Path.Combine(root, "docfx", "images", "controls");
            Directory.CreateDirectory(imageFolder);
            foreach (string file in Directory.EnumerateFiles(snippetFolder, "*.xml").Order(StringComparer.Ordinal))
                pending.Enqueue(file);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (current != null || host == null || Environment.ExitCode != 0)
                return;

            if (!pending.TryDequeue(out string? file))
            {
                File.WriteAllLines(Path.Combine(imageFolder, "captured.txt"), manifest.Select(x => $"{x.Key} {x.Value}"));
                Console.WriteLine($"Captured {manifest.Count} snippet(s) into {imageFolder}");
                Exit();
                return;
            }

            string name = Path.GetFileNameWithoutExtension(file);
            try
            {
                UIElement element = new MarkupLoader(host.Configuration).Load(File.ReadAllText(file), Path.GetFileName(file));
                element.DataContext = new DocsSnippetViewModel();
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Top;
                element.Margin = new Thickness(SnippetCapture.Padding);
                host.MainCanvas.Add(element);

                // A window draws only while open; a snippet showing one opens it.
                if (element is Window window)
                    window.Show();
                current = (name, element);
                framesLeft = SettleFrames;
            }
            catch (Exception ex)
            {
                Fail(name, ex);
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
                Rectangle area = SnippetCapture.CaptureArea(shown.Root.ActualBounds, new Size(backBuffer.Width, backBuffer.Height));
                using Image frame = backBuffer.GetDataAsImage(GraphicsContext.CommandList);
                using Image cropped = SnippetCapture.Crop(frame, area);
                using (FileStream stream = File.Create(Path.Combine(imageFolder, shown.Name + ".png")))
                    cropped.Save(stream, ImageFileType.Png);
                manifest[shown.Name] = SnippetCapture.Hash(Path.Combine(snippetFolder, shown.Name + ".xml"));
            }
            catch (Exception ex)
            {
                Fail(shown.Name, ex);
                return;
            }

            host!.MainCanvas.Remove(shown.Root);
            current = null;
        }

        private void Fail(string name, Exception ex)
        {
            Console.Error.WriteLine($"FAILED {name}: {ex.Message}");
            Environment.ExitCode = 1;
            pending.Clear();
            current = null;
            Exit();
        }
    }
}
