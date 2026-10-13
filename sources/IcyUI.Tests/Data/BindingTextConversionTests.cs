using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Data;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class BindingTextConversionTests
    {
        public sealed class TextSource : ObservableObject
        {
            private string text = string.Empty;

            public string Text
            {
                get => text;
                set => SetProperty(ref text, value);
            }
        }

        [Fact]
        public void StringBinding_UsesTheConfigurationsConverter_LikeAMarkupAttribute()
        {
            var types = new ReflectionConfiguration { TypeConverter = new ButtonMakingConverter(new TypeConversionManager()) };
            var button = (Button)NewLoader(types).Load("<Button xmlns=\"https://icyui.dev/markup\" Content=\"{Binding Text}\"/>", "x.xml");

            button.DataContext = new TextSource { Text = "Play" };

            Assert.IsType<Button>(button.Content);
        }

        [Fact]
        public void StringBinding_IntoANumber_ParsesWithTheLoadersConfigurationCulture()
        {
            // Not attached to a canvas yet: the loader's configuration still decides the culture.
            CultureInfo previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                var configuration = NewConfiguration(new ReflectionConfiguration());
                configuration.Culture = CultureInfo.GetCultureInfo("ru-RU");
                var button = (Button)new MarkupLoader(configuration).Load("<Button xmlns=\"https://icyui.dev/markup\" Width=\"{Binding Text}\"/>", "x.xml");

                button.DataContext = new TextSource { Text = "3,5" };

                Assert.Equal(3.5f, button.Width);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }

        private static IcyConfiguration NewConfiguration(ReflectionConfiguration types) =>
            new(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), types);

        private static MarkupLoader NewLoader(ReflectionConfiguration types) =>
            new(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), types));

        /// <summary>Turns any text bound into an element slot into a <see cref="Button"/>, so a test can tell which converter ran.</summary>
        private sealed class ButtonMakingConverter(ITypeConverter inner) : ITypeConverter
        {
            public object? Convert(object? value, Type targetType) =>
                value is string && targetType == typeof(Icy.UI.UIElement) ? new Button() : inner.Convert(value, targetType);

            public void RegisterConverter<TSource, TTarget>(IValueConverter<TSource, TTarget> converter) => inner.RegisterConverter(converter);

            public void RegisterConverter(Type from, Type to, IValueConverter converter) => inner.RegisterConverter(from, to, converter);
        }
    }
}
