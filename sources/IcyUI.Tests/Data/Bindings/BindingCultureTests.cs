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

namespace Icy.Tests.Data.Bindings
{
    /// <summary>
    /// Text a player reads or types follows one culture: the configuration's, which follows the UI language
    /// (<see cref="CultureInfo.CurrentUICulture"/>) unless the game sets its own.
    /// </summary>
    public class BindingCultureTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

        private sealed class Model : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public float Ratio { get; set; }

            public void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [Fact]
        public void ConfigurationCulture_FollowsCurrentUICulture_UntilSet()
        {
            using var _ = new CultureScope(ui: Russian, formatting: English);
            var configuration = NewConfiguration();

            Assert.Same(Russian, configuration.Culture);

            configuration.Culture = English;
            Assert.Same(English, configuration.Culture);

            configuration.Culture = null;
            Assert.Same(Russian, configuration.Culture);
        }

        [Fact]
        public void WithCulture_SetsTheConfigurationsCulture()
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem()).ConfigureTypes().ConfigureAssets();
            builder.WithCulture(Russian);

            Assert.Same(Russian, builder.Build().Culture);
        }

        [Fact]
        public void AttachedTarget_ParsesWithItsCanvasConfigurationCulture()
        {
            using var _ = new CultureScope(ui: English, formatting: English);
            var configuration = NewConfiguration();
            configuration.Culture = Russian;
            var canvas = new Canvas(configuration);
            var box = new TextBox();
            canvas.Add(box);
            var model = new Model();
            Bind(box, model);

            box.Text = "1,5";

            Assert.Equal(1.5f, model.Ratio);
        }

        [Fact]
        public void AttachedTarget_FormatsWithItsCanvasConfigurationCulture()
        {
            using var _ = new CultureScope(ui: English, formatting: English);
            var configuration = NewConfiguration();
            configuration.Culture = Russian;
            var canvas = new Canvas(configuration);
            var box = new TextBox();
            canvas.Add(box);
            var model = new Model { Ratio = 2.5f };

            Bind(box, model);

            Assert.Equal("2,5", box.Text);
        }

        [Fact]
        public void ConfigurationCultureChange_AppliesToExistingBindings()
        {
            using var _ = new CultureScope(ui: English, formatting: English);
            var configuration = NewConfiguration();
            var canvas = new Canvas(configuration);
            var box = new TextBox();
            canvas.Add(box);
            var model = new Model { Ratio = 2.5f };
            Bind(box, model);
            Assert.Equal("2.5", box.Text);

            configuration.Culture = Russian;
            model.Raise(nameof(Model.Ratio));

            Assert.Equal("2,5", box.Text);
        }

        [Fact]
        public void ExplicitBindingCulture_WinsOverTheConfiguration()
        {
            using var _ = new CultureScope(ui: English, formatting: English);
            var configuration = NewConfiguration();
            configuration.Culture = English;
            var canvas = new Canvas(configuration);
            var box = new TextBox();
            canvas.Add(box);
            var model = new Model();
            Binding binding = Bind(box, model);

            binding.Culture = Russian;
            box.Text = "1,5";

            Assert.Equal(1.5f, model.Ratio);
        }

        [Fact]
        public void ConverterWithoutCulture_GetsTheBindingsCulture()
        {
            using var _ = new CultureScope(ui: Russian, formatting: English);
            var converter = new RecordingConverter();
            var model = new Model { Ratio = 1f };
            var box = new TextBox();
            Binding binding = Bind(box, model, new BindingConverterParameters(converter));

            Assert.Same(Russian, converter.LastCulture);

            box.Text = "2";
            Assert.Same(Russian, converter.LastCulture);
        }

        private static IcyConfiguration NewConfiguration() =>
            new(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());

        private static Binding Bind(TextBox box, Model model, BindingConverterParameters? converter = null)
        {
            IPropertyReference text = PropertyRegistry.Default.GetPropertyStore(typeof(TextBox)).GetProperty(nameof(TextBox.Text));
            var binding = new Binding(box, text, new PropertyPath(nameof(Model.Ratio), typeof(Model)))
            {
                Source = model,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ConverterParameters = converter,
                IsEnabled = true,
            };
            binding.UpdateTarget();
            return binding;
        }

        private sealed class RecordingConverter : TypeConverter
        {
            public CultureInfo? LastCulture { get; private set; }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                LastCulture = culture;
                return Convert.ToString(value, culture);
            }

            public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
            {
                LastCulture = culture;
                return Convert.ToSingle(value, culture);
            }
        }

        private sealed class CultureScope : IDisposable
        {
            private readonly CultureInfo previousUI = CultureInfo.CurrentUICulture;
            private readonly CultureInfo previousFormatting = CultureInfo.CurrentCulture;

            public CultureScope(CultureInfo ui, CultureInfo formatting)
            {
                CultureInfo.CurrentUICulture = ui;
                CultureInfo.CurrentCulture = formatting;
            }

            public void Dispose()
            {
                CultureInfo.CurrentUICulture = previousUI;
                CultureInfo.CurrentCulture = previousFormatting;
            }
        }
    }
}
