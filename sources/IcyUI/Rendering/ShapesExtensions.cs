// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Buffers;
using System.Drawing;
using System.Numerics;
using Icy.Rendering.Brushes;

namespace Icy.Rendering
{
    /// <summary>
    /// Provides extension methods for shapes drawing.
    /// </summary>
    public static class ShapesExtensions
    {
        private static readonly SimpleBrush White = new();

        /// <summary>
        /// Draws an arc.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="center">Center of an arc.</param>
        /// <param name="radius">Radius of the arc.</param>
        /// <param name="sides">Count of sides to make polygons.</param>
        /// <param name="color">Color of the lines.</param>
        /// <param name="startAngle">Angle of the arc beginning.</param>
        /// <param name="endAngle">Angle of the arc ending.</param>
        /// <param name="thickness">Thickness of arc stroke.</param>
        public static void DrawArc(this IRenderContext context, Vector2 center, float radius, int sides, Color color, float startAngle, float endAngle, float thickness = 1f)
        {
            using var arcMemory = MemoryPool<Vector2>.Shared.Rent(sides);
            var span = arcMemory.Memory.Span;
            CreateArc(span, radius, sides, startAngle, endAngle);
            DrawPolygon(context, center, span, color, thickness);

            static void CreateArc(Span<Vector2> points, float radius, int sides, float startAngle, float endAngle)
            {
                float max = Math.Max(endAngle - startAngle, 0);
                float step = max / sides;
                float theta = startAngle;
                FillArc(points, radius, sides, step, theta);
            }
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="center">Center of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="sides">Count of sides to make polygons.</param>
        /// <param name="color">Color of the circle.</param>
        /// <param name="thickness">Thickness of circle stroke.</param>
        public static void DrawCircle(this IRenderContext context, Vector2 center, float radius, int sides, Color color, float thickness = 1f)
        {
            using var circleMemory = MemoryPool<Vector2>.Shared.Rent(sides);
            var span = circleMemory.Memory.Span;
            CreateCircle(span, sides, radius);
            DrawPolygon(context, center, span, color, thickness);

            static void CreateCircle(Span<Vector2> points, int sides, float radius)
            {
                const float max = 2.0f * MathF.PI;
                float step = max / sides;
                FillArc(points, radius, sides, step);
            }
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="x">X coordinate of the circle center.</param>
        /// <param name="y">Y coordinate of the circle center.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="sides">Count of sides to make polygon.</param>
        /// <param name="color">Color of the circle stroke.</param>
        /// <param name="thickness">Thickness of the circle stroke.</param>
        public static void DrawCircle(this IRenderContext context, float x, float y, float radius, int sides, Color color, float thickness = 1f) =>
            DrawCircle(context, new(x, y), radius, sides, color, thickness);

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="point">Point of the line beginning.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="rotation">Angle to rotate line.</param>
        /// <param name="origin">Origin of the rotation to apply.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thickness of the line.</param>
        public static void DrawLine(this IRenderContext context, Vector2 point, float length, float rotation, Vector2 origin, Color color, float thickness = 1f)
        {
            Rectangle rect = new((int)point.X, (int)point.Y, (int)length, (int)thickness);
            context.Draw(context.WhiteTexture, new(rect, null, color, rotation, origin));
        }

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="point1">The first point of the line.</param>
        /// <param name="point2">The second point of the line.</param>
        /// <param name="color">Color to draw line with.</param>
        /// <param name="thickness">Thickness of the line.</param>
        public static void DrawLine(this IRenderContext context, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            // calculate the distance between the two vectors
            var distance = Vector2.Distance(point1, point2);

            // calculate the angle between the two vectors
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(context, point1, distance, angle, Vector2.Zero, color, thickness);
        }

        /// <summary>
        /// Draws a line with specified edge positions.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="x1">X component of the first line edge coordinate.</param>
        /// <param name="y1">Y component of the first line edge coordinate.</param>
        /// <param name="x2">X component of the second line edge coordinate.</param>
        /// <param name="y2">Y component of the second line edge coordinate.</param>
        /// <param name="color">Color to draw line with.</param>
        /// <param name="thickness">Thickness of the line stroke.</param>
        public static void DrawLine(this IRenderContext context, float x1, float y1, float x2, float y2, Color color, float thickness = 1f) =>
            DrawLine(context, new(x1, y1), new(x2, y2), color, thickness);

        /// <summary>
        /// Draws a square point at the specified position. The center of the point will be at the <paramref name="location"/>.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="location">Position to draw point at.</param>
        /// <param name="color">Color to draw point with.</param>
        /// <param name="size">Size of the point to draw.</param>
        public static void DrawPoint(this IRenderContext context, Vector2 location, Color color, float size = 1f)
        {
            var offset = new Vector2(0.5f) - new Vector2(size * 0.5f);
            location += offset;
            context.Draw(context.WhiteTexture, new(new Rectangle((int)location.X, (int)location.Y, (int)size, (int)size), null, color));
        }

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="offset">Value to offset all polygon points.</param>
        /// <param name="points">Set of points to draw a polygon.</param>
        /// <param name="color">Colors to draw lines with.</param>
        /// <param name="thickness">Thickness of lines to draw with.</param>
        public static void DrawPolygon(this IRenderContext context, Vector2 offset, Vector2[] points, Color color, float thickness = 1f) =>
            DrawPolygon(context, offset, points.AsSpan(), color, thickness);

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="offset">Value to offset all polygon points.</param>
        /// <param name="points">Pointer to the set of points to draw a polygon.</param>
        /// <param name="color">Colors to draw lines with.</param>
        /// <param name="thickness">Thickness of lines to draw with.</param>
        public static void DrawPolygon(this IRenderContext context, Vector2 offset, ReadOnlySpan<Vector2> points, Color color, float thickness = 1f)
        {
            if (points.Length == 0)
                return;

            if (points.Length == 1)
            {
                context.DrawPoint(points[0], color, (int)thickness);
                return;
            }

            for (var i = 0; i < points.Length - 1; i++)
                DrawLine(context, points[i] + offset, points[i + 1] + offset, color, thickness);

            // Close the polygon
            DrawLine(context, points[^1] + offset, points[0] + offset, color, thickness);
        }

        /// <summary>
        /// Draws an open polyline: a stroke through <paramref name="points"/> in order, without closing it.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="offset">Value to offset all points.</param>
        /// <param name="points">The points to connect, in order.</param>
        /// <param name="color">Color to draw the stroke with.</param>
        /// <param name="thickness">Thickness of the stroke.</param>
        /// <remarks>
        /// <para>Unlike <see cref="DrawLine(IRenderContext, Vector2, Vector2, Color, float)"/>, which grows its thickness to one side:</para>
        /// <list type="bullet">
        /// <item><description>each segment is centered on the line between its two points;</description></item>
        /// <item><description>the free ends stop at their points (flat ends);</description></item>
        /// <item><description>
        /// each joint gets one square, the thickness wide, centered on the shared point with a corner along the outer
        /// bisector: it fills the gap between the two segments' flat ends into a sharp (mitered) corner. Exact for
        /// right-angle joints such as <see cref="UI.Controls.Icon"/>'s glyphs; close for others.
        /// </description></item>
        /// </list>
        /// <para>
        /// Unrotated segments are drawn as plain integer rectangles, so the engines can snap them to whole physical pixels.
        /// Rotated segments keep their sub-pixel position: their <see cref="TextureRenderingOptions.Destination"/> is anchored
        /// at a whole pixel and <see cref="TextureRenderingOptions.Origin"/> (in texels) carries the fractional rest, so
        /// consecutive segments meet exactly at their shared point. A length rounded up to whole pixels is split evenly
        /// between both ends.
        /// </para>
        /// <para>With partial opacity, a joint blends over the ends of its segments.</para>
        /// </remarks>
        public static void DrawPolyline(this IRenderContext context, Vector2 offset, ReadOnlySpan<Vector2> points, Color color, float thickness = 1f)
        {
            if (points.Length == 0)
                return;

            if (points.Length == 1)
            {
                context.DrawPoint(points[0] + offset, color, (int)thickness);
                return;
            }

            for (var i = 0; i < points.Length - 1; i++)
                DrawCenteredSegment(context, points[i] + offset, points[i + 1] + offset, color, thickness);

            for (var i = 1; i < points.Length - 1; i++)
                DrawMiterJoint(context, points[i - 1] + offset, points[i] + offset, points[i + 1] + offset, color, thickness);
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="rectangle">Rectangle to draw.</param>
        /// <param name="color">Color to draw rectangle with.</param>
        /// <param name="thickness">Thickness of the rectangle stroke.</param>
        public static void DrawRectangle(this IRenderContext context, Rectangle rectangle, Color color, float thickness = 1f)
        {
            var t = (int)thickness;

            // Top
            context.Draw(context.WhiteTexture, new(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, t), null, color));

            // Bottom
            context.Draw(context.WhiteTexture, new(new Rectangle(rectangle.X, rectangle.Bottom - t, rectangle.Width, t), null, color));

            // Left
            context.Draw(context.WhiteTexture, new(new Rectangle(rectangle.X, rectangle.Y, t, rectangle.Height), null, color));

            // Right
            context.Draw(context.WhiteTexture, new(new Rectangle(rectangle.Right - t, rectangle.Y, t, rectangle.Height), null, color));
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="location">Coordinates of the top-left rectangle corner.</param>
        /// <param name="size">Size of the rectangle.</param>
        /// <param name="color">Color to draw rectangle with.</param>
        /// <param name="thickness">Thickness of the rectangle stroke.</param>
        public static void DrawRectangle(this IRenderContext context, Vector2 location, Vector2 size, Color color, float thickness = 1f) =>
            DrawRectangle(context, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="destination">Rectangle to draw.</param>
        /// <param name="color">Color of the rectangle to fill.</param>
        public static void FillRectangle(this IRenderContext context, Rectangle destination, Color color) =>
            context.Draw(context.WhiteTexture, new(destination, null, color));

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="location">Coordinates of the upper-left side of the rectangle.</param>
        /// <param name="size">Size of the rectangle.</param>
        /// <param name="color">Color to apply.</param>
        public static void FillRectangle(this IRenderContext context, Vector2 location, Vector2 size, Color color) =>
            FillRectangle(context, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color);

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="context">Instance of the <see cref="IRenderContext"/> to draw.</param>
        /// <param name="x">Position of the left rectangle side.</param>
        /// <param name="y">Position of the top rectangle side.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="color">Color to apply for a rectangle.</param>
        public static void FillRectangle(this IRenderContext context, float x, float y, float width, float height, Color color) =>
            FillRectangle(context, new Rectangle((int)x, (int)y, (int)width, (int)height), color);

        private static void DrawCenteredSegment(IRenderContext context, Vector2 from, Vector2 to, Color color, float thickness)
        {
            Vector2 delta = to - from;
            float length = delta.Length();
            if (length <= 0)
                return;

            Vector2 direction = delta / length;
            float half = thickness / 2;
            float rotation = MathF.Atan2(direction.Y, direction.X);
            if (rotation == 0)
            {
                // Unrotated: an integer rectangle the engines can snap to whole physical pixels.
                Vector2 corner = from - new Vector2(0, half);
                var snappable = new Rectangle(
                    (int)MathF.Round(corner.X),
                    (int)MathF.Round(corner.Y),
                    Math.Max(1, (int)MathF.Round(length)),
                    Math.Max(1, (int)MathF.Round(thickness)));
                context.Draw(context.WhiteTexture, new(snappable, null, color));
                return;
            }

            // Rotated quads are never snapped, so keep their sub-pixel position: rounding each segment's corner on its own
            // shifted neighbouring segments apart and left a seam at their shared point. The destination is anchored at a
            // whole pixel and the origin (in texels, rotated with the quad) carries the fractional rest, putting `from` at
            // the centerline, with the rounded-up width split evenly between the two ends.
            int width = (int)MathF.Ceiling(length);
            int height = Math.Max(1, (int)MathF.Round(thickness));
            DrawRotatedQuad(context, from, new Vector2((width - length) / 2, height / 2f), width, height, rotation, color);
        }

        // A square the stroke's thickness wide, centered on the joint, with one corner along the outer bisector: for a right
        // angle its outer edges run from one segment's outer corner to the miter tip and on to the other's, and its inner
        // corner lands where the segments' inner edges meet.
        private static void DrawMiterJoint(IRenderContext context, Vector2 previous, Vector2 joint, Vector2 next, Color color, float thickness)
        {
            Vector2 incoming = joint - previous;
            Vector2 outgoing = next - joint;
            if (incoming.LengthSquared() <= 0 || outgoing.LengthSquared() <= 0)
                return;

            Vector2 bisector = Vector2.Normalize(incoming) - Vector2.Normalize(outgoing);
            if (bisector.LengthSquared() <= 1e-6f)
                return;

            // The square's edges run at 45 degrees to the bisector, so a corner points along it.
            float rotation = MathF.Atan2(bisector.Y, bisector.X) + (MathF.PI / 4);
            int side = Math.Max(1, (int)MathF.Round(thickness));
            DrawRotatedQuad(context, joint, new Vector2(side / 2f, side / 2f), side, side, rotation, color);
        }

        // Draws a white quad of the given size rotated by `rotation`, with its local point `local` placed exactly at `point`.
        // The destination is anchored at a whole pixel and the origin (in texels) carries the fractional rest.
        private static void DrawRotatedQuad(IRenderContext context, Vector2 point, Vector2 local, int width, int height, float rotation, Color color)
        {
            var anchor = new Vector2(MathF.Round(point.X), MathF.Round(point.Y));
            Vector2 offset = point - anchor;
            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);
            Vector2 unrotated = new((offset.X * cos) + (offset.Y * sin), (offset.Y * cos) - (offset.X * sin));
            Size textureSize = context.WhiteTexture.Size;
            Vector2 origin = (local - unrotated) * new Vector2((float)textureSize.Width / width, (float)textureSize.Height / height);
            var rect = new Rectangle((int)anchor.X, (int)anchor.Y, width, height);
            context.Draw(context.WhiteTexture, new(rect, null, color, rotation, origin));
        }

        private static void FillArc(Span<Vector2> points, float radius, int sides, float step, float theta = 0f)
        {
            for (int i = 0; i < sides; i++)
            {
                points[i] = new(radius * MathF.Cos(theta), radius * MathF.Sin(theta));
                theta += step;
            }
        }

        // TODO: make FillSegment and FillCircle methods
        private class SimpleBrush : IBrush
        {
            public void Draw(IRenderContext context, in TextureRenderingOptions options) => context.Draw(context.WhiteTexture, options);
        }
    }
}
