// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information

namespace Icy.UI.Controls
{
    /// <summary>
    /// Clamps a range control's value to its limits without ever throwing.
    /// </summary>
    /// <remarks>
    /// Bindings and markup attributes set <c>Minimum</c>, <c>Maximum</c> and <c>Value</c> in any order, so the limits can
    /// cross for a moment (a minimum of 0.75 applied while the maximum is still 0). <see cref="float.Clamp(float, float, float)"/>
    /// throws then; here a maximum below the minimum counts as the minimum.
    /// </remarks>
    internal static class RangeCoercion
    {
        /// <summary>
        /// Clamps <paramref name="value"/> to <paramref name="minimum"/>..<paramref name="maximum"/>.
        /// </summary>
        /// <param name="value">The requested value.</param>
        /// <param name="minimum">The lower limit.</param>
        /// <param name="maximum">The upper limit; a value below <paramref name="minimum"/> counts as <paramref name="minimum"/>.</param>
        /// <returns>The value within the limits.</returns>
        public static float Coerce(float value, float minimum, float maximum)
        {
            if (value <= minimum)
                return minimum;

            float upper = maximum < minimum ? minimum : maximum;
            return value > upper ? upper : value;
        }
    }
}
