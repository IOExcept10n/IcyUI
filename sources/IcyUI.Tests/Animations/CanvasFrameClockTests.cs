using Icy.Animations;
using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Animations
{
    /// <summary>
    /// Animations advance by the real time between frames: one delta per frame, measured from one
    /// <see cref="Canvas.Render"/> to the next, whatever the frame did in between.
    /// </summary>
    public class CanvasFrameClockTests
    {
        [Fact]
        public void Animation_AdvancesByTheTimeBetweenFrames()
        {
            var (canvas, clock, element) = Setup();
            element.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(1), 0f, 1f));
            canvas.Render();

            clock.Advance(TimeSpan.FromSeconds(0.5));
            canvas.Render();

            Assert.Equal(0.5f, element.Opacity, 0.001f);
        }

        [Fact]
        public void TimeSpentInsideAFrame_CountsTowardTheNextFrame()
        {
            var (canvas, clock, element) = Setup();
            element.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(1), 0f, 1f));
            canvas.Render();

            // A slow frame: the time passes during the next Render, not between them.
            clock.Advance(TimeSpan.FromSeconds(0.25));
            canvas.Render();
            clock.Advance(TimeSpan.FromSeconds(0.25));
            canvas.Render();

            Assert.Equal(0.5f, element.Opacity, 0.001f);
        }

        [Fact]
        public void HiddenCanvas_StillAdvancesByRealTime()
        {
            var (canvas, clock, element) = Setup();
            canvas.IsVisible = false;
            element.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(1), 0f, 1f));
            canvas.Render();

            clock.Advance(TimeSpan.FromSeconds(0.1));
            canvas.Render();
            clock.Advance(TimeSpan.FromSeconds(0.4));
            canvas.Render();

            Assert.Equal(0.5f, element.Opacity, 0.001f);
        }

        [Fact]
        public void TwoAnimations_StartedTogether_StayInStep()
        {
            var (canvas, clock, element) = Setup();
            var other = new UIElement();
            canvas.Add(other);
            element.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(1), 0f, 1f));
            other.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(1), 0f, 1f));
            canvas.Render();

            clock.Advance(TimeSpan.FromSeconds(0.3));
            canvas.Render();

            Assert.Equal(element.Opacity, other.Opacity);
        }

        private static (Canvas Canvas, FakeClock Clock, UIElement Element) Setup()
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            var clock = new FakeClock();
            var canvas = new Canvas(configuration) { IsVisible = true, Clock = clock };
            var element = new UIElement();
            canvas.Add(element);
            return (canvas, clock, element);
        }

        private sealed class FakeClock : TimeProvider
        {
            private long ticks;

            public override long TimestampFrequency => TimeSpan.TicksPerSecond;

            public override long GetTimestamp() => ticks;

            public void Advance(TimeSpan by) => ticks += by.Ticks;
        }
    }
}
