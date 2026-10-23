using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Controls
{
    /// <summary>
    /// <see cref="Slider"/> and <see cref="ProgressBar"/> accept their limits and value in any order (ICY-012): crossed
    /// limits never throw, and a value set before the limits that allow it is kept.
    /// </summary>
    public class RangeCoercionTests
    {
        [Fact]
        public void Slider_MinimumAboveTheCurrentMaximum_DoesNotThrow_AndCoercesToTheMinimum()
        {
            var slider = new Slider { Maximum = 0 };

            slider.Minimum = 0.75f;

            Assert.Equal(0.75f, slider.Value);
        }

        [Fact]
        public void Slider_LimitsAppliedMinimumFirst_KeepTheValueSetEarlier()
        {
            var slider = new Slider { Maximum = 0, Value = 1.5f };

            slider.Minimum = 0.75f;
            slider.Maximum = 2f;

            Assert.Equal(1.5f, slider.Value);
        }

        [Fact]
        public void Slider_ValueAboveTheDefaultMaximum_IsKeptOnceTheMaximumAllowsIt()
        {
            var slider = new Slider { Value = 150 };
            Assert.Equal(100, slider.Value);

            slider.Maximum = 200;

            Assert.Equal(150, slider.Value);
        }

        [Fact]
        public void Slider_ValueChanged_FollowsTheEffectiveValue()
        {
            var slider = new Slider { Value = 150 };
            int raised = 0;
            slider.ValueChanged += (_, _) => raised++;

            slider.Maximum = 200;
            slider.Maximum = 200;

            Assert.Equal(1, raised);
        }

        [Fact]
        public void ProgressBar_MinimumAboveTheCurrentMaximum_DoesNotThrow_AndCoercesToTheMinimum()
        {
            var bar = new ProgressBar { Maximum = 0 };

            bar.Minimum = 0.75f;

            Assert.Equal(0.75f, bar.Value);
        }

        [Fact]
        public void ProgressBar_ValueAboveTheDefaultMaximum_IsKeptOnceTheMaximumAllowsIt()
        {
            var bar = new ProgressBar { Value = 150 };

            bar.Maximum = 200;

            Assert.Equal(150, bar.Value);
        }
    }
}
