using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Data.Bindings;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Markup
{
    public sealed class MenuModel : ObservableObject
    {
        private string title = "Play";
        private float volume = 0.5f;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public float Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        public override string ToString() => "menu";
    }

    public sealed class OtherModel
    {
        public string Title { get; set; } = "Other";
    }

    public class TypedBindingTests
    {
        private static readonly string Ns =
            $"xmlns:m=\"clr-namespace:{typeof(MenuModel).Namespace};assembly={typeof(MenuModel).Assembly.GetName().Name}\"";

        private static (UIElement Root, CapturingLoggerFactory Log) Load(string body)
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            configuration.Culture = CultureInfo.InvariantCulture;
            return (new MarkupLoader(configuration).Load(body.Replace("NS", Ns, StringComparison.Ordinal), "x.xml"), log);
        }

        private static IPropertyPath PathOf(UIElement element) => Assert.IsType<Binding>(Assert.Single(((IBindingTarget)element).Bindings)).Path;

        [Fact]
        public void NoDataType_StaysDynamic()
        {
            var (root, _) = Load("<TextBlock NS Text=\"{Binding Title}\"/>");

            Assert.IsType<DynamicPropertyPath>(PathOf(root));
        }

        [Fact]
        public void DataType_MakesAStaticCheckedPath()
        {
            var (root, _) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title}\"/>");
            root.DataContext = new MenuModel();

            Assert.IsType<CheckedPropertyPath>(PathOf(root));
            Assert.Equal("Play", ((TextBlock)root).Text);
        }

        [Fact]
        public void Compiled_UnderDataType_ReadsAndWritesBack()
        {
            var (root, _) = Load("<TextBox NS x:DataType=\"m:MenuModel\" Text=\"{Binding Volume, Mode=TwoWay, Compiled=True}\"/>");
            var model = new MenuModel();
            root.DataContext = model;

            Assert.Equal("0.5", ((TextBox)root).Text);
            ((TextBox)root).Text = "0.75";
            Assert.Equal(0.75f, model.Volume);
        }

        [Fact]
        public void MisspelledPath_UnderDataType_FailsAtLoad_WithASuggestion()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Titel}\"/>"));

            Assert.Contains("Titel", ex.Message);
            Assert.Contains("Title", ex.Message);
            Assert.Contains("MenuModel", ex.Message);
        }

        [Fact]
        public void MisspelledPath_WithElementName_FailsAtLoad()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<StackPanel NS><Slider x:Name=\"volume\"/><TextBlock Text=\"{Binding Valeu, ElementName=volume}\"/></StackPanel>"));

            Assert.Contains("Valeu", ex.Message);
        }

        [Fact]
        public void ElementName_GetsAStaticPathOnTheNamedElementsType()
        {
            var (root, _) = Load("<StackPanel NS><Slider x:Name=\"volume\"/><TextBlock Text=\"{Binding Value, ElementName=volume}\"/></StackPanel>");

            IPropertyPath path = PathOf(((StackPanel)root).Children[1]);
            Assert.IsAssignableFrom<PropertyPath>(path);
        }

        [Fact]
        public void Compiled_WithoutAKnownSourceType_FailsAtLoad()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<TextBlock NS Text=\"{Binding Title, Compiled=True}\"/>"));

            Assert.Contains("x:DataType", ex.Message);
        }

        [Theory]
        [InlineData("{Binding}")]
        [InlineData("{Binding Compiled=True}")]
        public void NoPath_UnderDataType_StillBindsTheWholeSource(string binding)
        {
            // MenuModel.ToString() is "menu": the whole source, formatted into the text.
            var (root, _) = Load($"<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{binding}\"/>");

            root.DataContext = new MenuModel();

            Assert.Equal("menu", ((TextBlock)root).Text);
        }

        [Fact]
        public void Static_WrongDataContext_WarnsAndStillReads()
        {
            var (root, log) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title}\"/>");

            root.DataContext = new OtherModel();

            Assert.Equal("Other", ((TextBlock)root).Text);
            Assert.Single(log.Entries, e => e.EventId.Id == 2002);
        }

        [Fact]
        public void Compiled_WrongDataContext_ThrowsWhereItIsSet()
        {
            var (root, _) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title, Compiled=True}\"/>");

            var ex = Assert.Throws<BindingException>(() => root.DataContext = new OtherModel());
            Assert.Contains("OtherModel", ex.ToString());
        }
    }
}
