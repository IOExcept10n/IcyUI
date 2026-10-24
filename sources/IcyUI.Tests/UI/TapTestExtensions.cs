using System.Drawing;
using Icy.UI;

namespace Icy.Tests.UI
{
    /// <summary>
    /// Calls <see cref="UIElement.OnTap(TapEventArgs)"/> directly, as the canvas does for the hit element, for tests that
    /// tap a control without going through hit testing.
    /// </summary>
    internal static class TapTestExtensions
    {
        public static TapEventArgs InvokeTap(this UIElement element)
        {
            var args = new TapEventArgs(element, Point.Empty);
            element.OnTap(args);
            return args;
        }
    }
}
