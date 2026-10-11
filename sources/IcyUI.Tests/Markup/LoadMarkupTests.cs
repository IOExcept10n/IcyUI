using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Markup
{
    public class LoadMarkupTests
    {
        private static Canvas NewCanvas() =>
            new(new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .AddMarkupSupport()
                .Build());

        [Fact]
        public void LoadMarkup_AddsTheElementAndSetsItsDataContext()
        {
            Canvas canvas = NewCanvas();
            var data = new object();

            UIElement element = canvas.LoadMarkup("Resources/TestPage.xml", data);

            Assert.Same(canvas, element.Canvas);
            Assert.Same(data, element.DataContext);
        }

        [Fact]
        public void LoadMarkup_Twice_AddsTwoDistinctElements()
        {
            Canvas canvas = NewCanvas();

            UIElement first = canvas.LoadMarkup("Resources/TestPage.xml");
            UIElement second = canvas.LoadMarkup("Resources/TestPage.xml");

            Assert.NotSame(first, second);
        }
    }
}
