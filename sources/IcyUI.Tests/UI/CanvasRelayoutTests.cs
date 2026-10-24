using System.Drawing;
using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Xunit;

namespace Icy.Tests.UI
{
    /// <summary>
    /// <see cref="Canvas.RequestRelayout(UIElement)"/>: requests left at the per-frame pass limit carry over to the next
    /// frame instead of being dropped.
    /// </summary>
    public class CanvasRelayoutTests
    {
        [Fact]
        public void RequestsBeyondTheFramesPassLimit_AreHandledNextFrame()
        {
            var canvas = new Canvas(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext { ViewportSize = new Size(800, 600) }, new ReflectionConfiguration())) { IsVisible = true };
            var element = new RelayoutRequester(requests: 5);
            canvas.Add(element);

            canvas.Render();
            canvas.Render();

            Assert.Equal(6, element.Arranges);
        }

        private sealed class RelayoutRequester(int requests) : UIElement
        {
            private int remaining = requests;

            public int Arranges { get; private set; }

            protected override void ArrangeContent()
            {
                Arranges++;
                if (remaining-- > 0)
                    Canvas?.RequestRelayout(this);
            }
        }
    }
}
