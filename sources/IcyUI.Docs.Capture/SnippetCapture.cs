// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Stride.Graphics;
using Image = Stride.Graphics.Image;

namespace Icy.Docs.Capture
{
    /// <summary>File locations, hashing and cropping for the capture run.</summary>
    internal static class SnippetCapture
    {
        public const int Padding = 16;

        /// <summary>Walks up from <paramref name="start"/> to the folder that contains <c>.git</c>.</summary>
        public static string FindRepositoryRoot(string start)
        {
            for (DirectoryInfo? dir = new(start); dir != null; dir = dir.Parent)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || File.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
            }

            throw new DirectoryNotFoundException($"No .git folder above {start}.");
        }

        /// <summary>The first 12 hex digits of the SHA-256 of the file with CRs removed, as build-docs.sh computes it.</summary>
        public static string Hash(string file)
        {
            string text = File.ReadAllText(file).Replace("\r", string.Empty, StringComparison.Ordinal);
            return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..12];
        }

        /// <summary>The element's bounds plus <see cref="Padding"/>, clipped to the back buffer.</summary>
        public static Rectangle CaptureArea(Rectangle bounds, Size backBuffer)
        {
            Rectangle area = Rectangle.Inflate(bounds, Padding, Padding);
            return Rectangle.Intersect(area, new Rectangle(Point.Empty, backBuffer));
        }

        /// <summary>Copies <paramref name="area"/> of <paramref name="source"/> into a new image of the same format.</summary>
        public static Image Crop(Image source, Rectangle area)
        {
            PixelFormat format = source.Description.Format;
            PixelBuffer from = source.GetPixelBuffer(0, 0);
            if (from.PixelSize != 4)
                throw new NotSupportedException($"Can't crop a back buffer in {format}: only 4-byte pixel formats are supported.");

            var cropped = Image.New2D(area.Width, area.Height, 1, format);
            PixelBuffer to = cropped.GetPixelBuffer(0, 0);
            for (int y = 0; y < area.Height; y++)
            {
                for (int x = 0; x < area.Width; x++)
                    to.SetPixel(x, y, from.GetPixel<uint>(area.X + x, area.Y + y));
            }

            return cropped;
        }
    }
}
