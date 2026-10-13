using System.ComponentModel;
using System.Globalization;
using Icy.Assets;
using Icy.Configuration;
using Icy.Data.Bindings;
using Icy.Data.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.Data.Bindings
{
    /// <summary>
    /// Typed numbers: '.' and ',' both work as the decimal separator whatever the culture (the keyboard layout may
    /// differ from the UI language), and there are no thousands separators, so nothing parses into a different number.
    /// </summary>
    public class BindingNumberTextTests
    {
        private sealed class Model : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public float Single { get; set; } = -1;

            public double Double { get; set; } = -1;

            public decimal Decimal { get; set; } = -1;

            public float? Nullable { get; set; } = -1;

            public int Integer { get; set; } = -1;

            public void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [Theory]
        [InlineData("en-US", "3.5")]
        [InlineData("en-US", "3,5")]
        [InlineData("ru-RU", "3,5")]
        [InlineData("ru-RU", "3.5")]
        [InlineData("de-DE", "3.5")]
        public void EitherSeparator_IsTheDecimalPoint(string culture, string text)
        {
            var (box, model) = Bind(culture, nameof(Model.Single));

            box.Text = text;

            Assert.Equal(3.5f, model.Single);
        }

        [Theory]
        [InlineData(nameof(Model.Double))]
        [InlineData(nameof(Model.Decimal))]
        [InlineData(nameof(Model.Nullable))]
        public void EveryNonIntegerType_AcceptsBothSeparators(string property)
        {
            var (box, model) = Bind("en-US", property);

            box.Text = "2,25";

            Assert.Equal(2.25m, Convert.ToDecimal(typeof(Model).GetProperty(property)!.GetValue(model), CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData("en-US", "1,000.5")]
        [InlineData("en-US", "1.000,5")]
        [InlineData("ru-RU", "1 000")]
        [InlineData("en-US", "1.2.3")]
        public void ThousandsSeparatorsAndRepeatedSeparators_AreInvalid(string culture, string text)
        {
            var (box, model) = Bind(culture, nameof(Model.Double));

            box.Text = text;

            Assert.Equal(-1, model.Double);
            Assert.True(box.ControlState.HasFlag(ControlState.Invalid));
        }

        [Fact]
        public void ALoneComma_IsADecimalPoint_NotAThousandsSeparator()
        {
            var (box, model) = Bind("en-US", nameof(Model.Double));

            box.Text = "1,000";

            Assert.Equal(1d, model.Double);
        }

        [Theory]
        [InlineData("3,5")]
        [InlineData("3.5")]
        [InlineData("1,000")]
        public void Integers_TakeNoSeparatorAtAll(string text)
        {
            var (box, model) = Bind("en-US", nameof(Model.Integer));

            box.Text = text;

            Assert.Equal(-1, model.Integer);
            Assert.True(box.ControlState.HasFlag(ControlState.Invalid));
        }

        [Fact]
        public void SignsAndExponents_StillParse()
        {
            var (box, model) = Bind("ru-RU", nameof(Model.Double));

            box.Text = "-1,5e3";

            Assert.Equal(-1500d, model.Double);
        }

        [Fact]
        public void Formatting_StillUsesTheCulturesSeparator()
        {
            var (box, model) = Bind("ru-RU", nameof(Model.Single));
            model.Single = 3.5f;

            model.Raise(nameof(Model.Single));

            Assert.Equal("3,5", box.Text);
        }

        private static (TextBox Box, Model Model) Bind(string culture, string property)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration())
            {
                Culture = CultureInfo.GetCultureInfo(culture),
            };
            var canvas = new Canvas(configuration);
            var box = new TextBox();
            canvas.Add(box);
            var model = new Model();
            IPropertyReference text = PropertyRegistry.Default.GetPropertyStore(typeof(TextBox)).GetProperty(nameof(TextBox.Text));
            var binding = new Binding(box, text, new PropertyPath(property, typeof(Model)))
            {
                Source = model,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                IsEnabled = true,
            };
            binding.UpdateTarget();
            return (box, model);
        }
    }
}
