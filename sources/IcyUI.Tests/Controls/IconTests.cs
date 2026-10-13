using System.Drawing;
using System.Numerics;
using Icy.Rendering.Brushes;
using Icy.Tests.Rendering;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    public class IconTests
    {
        [Fact]
        public void MeasureContent_ReturnsFixedDefaultSize()
        {
            var icon = new Icon();

            Size measured = icon.Measure();

            Assert.Equal(new Size(16, 16), measured);
        }

        [Fact]
        public void OnRender_DrawsWithoutThrowing()
        {
            var icon = new Icon { Kind = IconKind.Checkmark, Stroke = new SolidColorBrush(Color.White) };
            icon.Arrange(new Rectangle(0, 0, 16, 16));
            var context = new FakeRenderContext();

            icon.Draw(context);

            Assert.NotEmpty(context.DrawCalls);
        }

        [Theory]
        [InlineData(IconKind.Checkmark)]
        [InlineData(IconKind.ChevronUp)]
        [InlineData(IconKind.ChevronDown)]
        [InlineData(IconKind.ChevronLeft)]
        [InlineData(IconKind.ChevronRight)]
        public void OnRender_EveryKind_DrawsWithoutThrowing(IconKind kind)
        {
            var icon = new Icon { Kind = kind };
            icon.Arrange(new Rectangle(0, 0, 16, 16));
            var context = new FakeRenderContext();

            icon.Draw(context);

            Assert.NotEmpty(context.DrawCalls);
        }

        [Fact]
        public void OnRender_AwayFromTheOrigin_DrawsInsideItsOwnBounds()
        {
            // Draw() already moves the context to the icon's position, so the glyph must be drawn in local space.
            var icon = new Icon { Kind = IconKind.ChevronRight };
            icon.Arrange(new Rectangle(100, 50, 16, 16));
            var context = new FakeRenderContext { Transform = Icy.Rendering.Transform2D.Identity };

            icon.Draw(context);

            Assert.NotEmpty(context.DrawCalls);
            foreach (var call in context.DrawCalls)
            {
                Rectangle screen = call.TransformAtDrawTime.Apply(call.Options.Destination);
                Assert.InRange(screen.X, 100, 116);
                Assert.InRange(screen.Y, 50, 66);
            }
        }
    
        [Theory]
        [InlineData(IconKind.Checkmark)]
        [InlineData(IconKind.ChevronRight)]
        public void OnRender_DrawsAnOpenStroke_NotAClosedTriangle(IconKind kind)
        {
            var icon = new Icon { Kind = kind };
            icon.Arrange(new Rectangle(0, 0, 16, 16));
            var context = new FakeRenderContext { Transform = Icy.Rendering.Transform2D.Identity };

            icon.Draw(context);

            // Three points, two segments (and one joint, a square): closing the shape would add a third segment.
            Assert.Equal(2, context.DrawCalls.Count(c => c.Options.Destination.Width != c.Options.Destination.Height));
        }

        [Fact]
        public void DrawPolyline_CentersEachSegmentAndEndsItFlat()
        {
            var context = new FakeRenderContext { Transform = Icy.Rendering.Transform2D.Identity };

            Icy.Rendering.ShapesExtensions.DrawPolyline(context, new System.Numerics.Vector2(5, 0), [new(0, 10), new(10, 10)], Color.White, 2);

            // A free end stops at its point: a square cap on a thin diagonal stroke showed as a stub at the tip.
            var call = Assert.Single(context.DrawCalls);
            Assert.Equal(new Rectangle(5, 9, 10, 2), call.Options.Destination);
            Assert.Equal(0f, call.Options.Rotation);
        }

        [Theory]
        [InlineData(5.6f, 3.2f, 10.4f, 8f, 5.6f, 12.8f)]   // ChevronRight at 16x16
        [InlineData(3.2f, 10.4f, 8f, 5.6f, 12.8f, 10.4f)]  // ChevronUp at 16x16
        [InlineData(3.2f, 8f, 6.72f, 11.52f, 13.6f, 4f)]   // Checkmark at 16x16
        [InlineData(7.3f, 2.1f, 12.45f, 9.9f, 4.05f, 14.7f)]
        public void DrawPolyline_EverySegmentRunsFromPointToPoint(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            // Integer destinations used to round each segment's corner on its own, so two diagonal segments missed their
            // shared joint by a fraction of a pixel and left a visible seam there.
            Vector2[] points = [new(x1, y1), new(x2, y2), new(x3, y3)];
            var context = new FakeRenderContext { Transform = Icy.Rendering.Transform2D.Identity };

            Icy.Rendering.ShapesExtensions.DrawPolyline(context, Vector2.Zero, points, Color.White, 2);

            var segments = context.DrawCalls.Where(c => c.Options.Destination.Width != c.Options.Destination.Height).ToList();
            Assert.Equal(2, segments.Count);
            for (int i = 0; i < 2; i++)
            {
                var (start, end) = Centerline(segments[i].Texture, segments[i].Options);
                Vector2 direction = Vector2.Normalize(end - start);
                float length = Vector2.Distance(start, end);
                Assert.True(DistanceToLine(points[i], start, end) < 0.01f, $"Segment {i} misses {points[i]}.");
                Assert.True(DistanceToLine(points[i + 1], start, end) < 0.01f, $"Segment {i} misses {points[i + 1]}.");

                // No cap: the stroke ends at its points, give or take the half pixel its whole-pixel length rounds up.
                Assert.InRange(Vector2.Dot(points[i] - start, direction), -0.01f, 0.51f);
                Assert.InRange(Vector2.Dot(points[i + 1] - start, direction), length - 0.51f, length + 0.01f);
            }
        }

        [Theory]
        [InlineData(5.6f, 3.2f, 10.4f, 8f, 5.6f, 12.8f)]   // ChevronRight at 16x16
        [InlineData(3.2f, 10.4f, 8f, 5.6f, 12.8f, 10.4f)]  // ChevronUp at 16x16
        [InlineData(3.2f, 8f, 6.72f, 11.52f, 13.6f, 4f)]   // Checkmark at 16x16
        public void DrawPolyline_JointIsAMiterSquareOnTheCorner(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            // Each segment's square cap overlapped the other's at the joint and stuck out past the corner as a lump. One
            // square, centered on the corner with a vertex along the outer bisector, fills the gap between the flat ends
            // into a sharp corner instead.
            Vector2[] points = [new(x1, y1), new(x2, y2), new(x3, y3)];
            var context = new FakeRenderContext { Transform = Icy.Rendering.Transform2D.Identity };

            Icy.Rendering.ShapesExtensions.DrawPolyline(context, Vector2.Zero, points, Color.White, 2);

            var joint = Assert.Single(context.DrawCalls, c => c.Options.Destination.Width == c.Options.Destination.Height);
            Assert.Equal(2, joint.Options.Destination.Width);
            Vector2 center = QuadPoint(joint.Texture, joint.Options, 0.5f, 0.5f);
            Assert.True(Vector2.Distance(center, points[1]) < 0.01f, $"The joint is centered at {center}, not on the corner {points[1]}.");

            Vector2 incoming = Vector2.Normalize(points[1] - points[0]);
            Vector2 outgoing = Vector2.Normalize(points[2] - points[1]);
            Vector2 outerBisector = Vector2.Normalize(incoming - outgoing);
            Vector2 corner = QuadPoint(joint.Texture, joint.Options, 0f, 0f) - center;
            float cosine = MathF.Abs(Vector2.Dot(Vector2.Normalize(corner), outerBisector));
            float sine = MathF.Abs((corner.X * outerBisector.Y) - (corner.Y * outerBisector.X)) / corner.Length();
            Assert.True(cosine > 0.999f || sine > 0.999f, $"No vertex of the joint square points along the outer bisector {outerBisector}.");
        }

        // A point of a drawn quad at fractions (u, v) of its size, in SpriteBatch semantics.
        private static Vector2 QuadPoint(Icy.Rendering.ITexture texture, Icy.Rendering.TextureRenderingOptions options, float u, float v)
        {
            Rectangle destination = options.Destination;
            Vector2 scale = new((float)destination.Width / texture.Size.Width, (float)destination.Height / texture.Size.Height);
            Matrix3x2 rotation = Matrix3x2.CreateRotation(options.Rotation);
            Vector2 local = new(destination.Width * u, destination.Height * v);
            return new Vector2(destination.X, destination.Y) + Vector2.Transform(local - (options.Origin * scale), rotation);
        }

        // The centerline of a drawn quad, in SpriteBatch semantics: the origin is in texels and the quad rotates around it.
        private static (Vector2 Start, Vector2 End) Centerline(Icy.Rendering.ITexture texture, Icy.Rendering.TextureRenderingOptions options)
        {
            Rectangle destination = options.Destination;
            Vector2 scale = new((float)destination.Width / texture.Size.Width, (float)destination.Height / texture.Size.Height);
            Matrix3x2 rotation = Matrix3x2.CreateRotation(options.Rotation);
            Vector2 position = new(destination.X, destination.Y);
            Vector2 ToWorld(Vector2 local) => position + Vector2.Transform(local - (options.Origin * scale), rotation);
            float middle = destination.Height / 2f;
            return (ToWorld(new(0, middle)), ToWorld(new(destination.Width, middle)));
        }

        private static float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 direction = Vector2.Normalize(lineEnd - lineStart);
            Vector2 offset = point - lineStart;
            return MathF.Abs((offset.X * direction.Y) - (offset.Y * direction.X));
        }
    }
}
