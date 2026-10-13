// Engine-neutral code for docfx/*/guides/custom-controls.md. Each #region is one example on the page. This file is also
// compiled into IcyUI.Tests (and into the MonoGame snippets), so the guide's behaviour is tested.
namespace MyGame.Controls
{
    using System.ComponentModel;
    using System.Drawing;
    using Icy.Data.Markup.Attributes;
    using Icy.Rendering.Brushes;
    using Icy.UI;
    using Icy.UI.Controls;
    using Icy.UI.Styles;

    #region health-bar
    /// <summary>
    /// A bar that shows how much health is left. Below a quarter it switches to a "critical" look, which the theme styles.
    /// </summary>
    public class HealthBar : Control
    {
        private const float CriticalRatio = 0.25f;

        private readonly Border defaultFill;
        private Border fill;
        private float maximum = 100;
        private float health = 100;

        public HealthBar()
        {
            defaultFill = fill = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Color.MediumSeaGreen),
            };
            ((Border)Chrome).Child = fill;
        }

        /// <summary>The brush of the filled part. A template binds its fill to this property.</summary>
        [Category("Appearance")]
        [RegisterReference]
        public IBrush FillBrush
        {
            get => fill.Background;
            set => fill.Background = value;
        }

        /// <summary>The health that fills the bar.</summary>
        [Category("Behavior")]
        [DefaultValue(100f)]
        [RegisterReference]
        [AffectsArrange]
        public float Maximum
        {
            get => maximum;
            set
            {
                if (SetProperty(ref maximum, value))
                {
                    Value = health;
                    UpdateCriticalState();
                    InvalidateArrange();
                }
            }
        }

        /// <summary>The current health, kept between 0 and <see cref="Maximum"/>.</summary>
        [Category("Behavior")]
        [DefaultValue(100f)]
        [RegisterReference]
        [AffectsArrange]
        public float Value
        {
            get => health;
            set
            {
                if (SetProperty(ref health, float.Clamp(value, 0, Maximum)))
                {
                    UpdateCriticalState();
                    InvalidateArrange();
                }
            }
        }

        protected override Size MeasureContent() => new(160, 18);

        protected override void ArrangeContent()
        {
            float ratio = Maximum > 0 ? Value / Maximum : 0;
            fill.Width = ContentBounds.Width * ratio;
            base.ArrangeContent();
        }

        // A template provides the fill as a part named PART_Fill; without one, the built-in fill stays.
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null && GetTemplateChild<Border>("PART_Fill") is { } part)
            {
                fill = part;
                return;
            }

            fill = defaultFill;
            if (Template == null)
                ((Border)Chrome).Child = fill;
        }

        // Highlighted is the state a theme styles: a custom control can only use the states IcyUI defines.
        private void UpdateCriticalState()
        {
            bool critical = Maximum > 0 && Value / Maximum < CriticalRatio;
            ControlState = critical ? ControlState | ControlState.Highlighted : ControlState & ~ControlState.Highlighted;
        }
    }
    #endregion
}
