using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Markup
{
    // Top-level, not nested in the test class: a clr-namespace prefix can't name a nested type (Outer+Inner).
    public sealed class ScopeOuterModel
    {
    }

    public sealed class ScopeItemModel
    {
    }

    /// <summary>Records the DataType of every context it's resolved in.</summary>
    public sealed class CaptureDataTypeExtension : IMarkupExtension
    {
        public static List<Type?> Seen { get; } = [];

        public object? ProvideValue(MarkupExtensionContext context)
        {
            Seen.Add(context.DataType);
            return MarkupValue.Unset;
        }
    }

    /// <summary>
    /// Which x:DataType a markup extension sees: the nearest one on the element or its ancestors, never one from
    /// outside the template the extension is in.
    /// </summary>
    public class DataTypeScopeTests
    {
        private static readonly string Ns =
            $"xmlns:m=\"clr-namespace:{typeof(ScopeOuterModel).Namespace};assembly={typeof(ScopeOuterModel).Assembly.GetName().Name}\"";

        private static UIElement Load(string body)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            configuration.Types.Markup.RegisterExtension<CaptureDataTypeExtension>("Capture");
            CaptureDataTypeExtension.Seen.Clear();
            return new MarkupLoader(configuration).Load(body.Replace("NS", Ns, StringComparison.Ordinal), "x.xml");
        }

        [Fact]
        public void NoDataType_IsNull()
        {
            Load("<StackPanel NS><TextBlock Text=\"{Capture}\"/></StackPanel>");

            Assert.Equal([null], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void AncestorDataType_Applies()
        {
            Load("<StackPanel NS x:DataType=\"m:ScopeOuterModel\"><Border><TextBlock Text=\"{Capture}\"/></Border></StackPanel>");

            Assert.Equal([typeof(ScopeOuterModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTypeOnTheSameElement_Applies_WhateverTheAttributeOrder()
        {
            Load("<StackPanel NS><TextBlock Text=\"{Capture}\" x:DataType=\"m:ScopeOuterModel\"/></StackPanel>");

            Assert.Equal([typeof(ScopeOuterModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void NestedDataType_WinsOverTheOuterOne()
        {
            Load("<StackPanel NS x:DataType=\"m:ScopeOuterModel\"><Border x:DataType=\"m:ScopeItemModel\"><TextBlock Text=\"{Capture}\"/></Border></StackPanel>");

            Assert.Equal([typeof(ScopeItemModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTemplate_DoesNotSeeTheOuterDataType()
        {
            var root = (ItemsControl)Load(
                """
                <ItemsControl NS x:DataType="m:ScopeOuterModel">
                  <ItemsControl.ItemTemplate>
                    <DataTemplate><TextBlock Text="{Capture}"/></DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
                """);
            CaptureDataTypeExtension.Seen.Clear();

            root.ItemTemplate!.Build(new ScopeItemModel());

            Assert.Equal([null], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTemplate_DeclaresItsOwnDataType()
        {
            var root = (ItemsControl)Load(
                """
                <ItemsControl NS x:DataType="m:ScopeOuterModel">
                  <ItemsControl.ItemTemplate>
                    <DataTemplate x:DataType="m:ScopeItemModel"><TextBlock Text="{Capture}"/></DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
                """);
            CaptureDataTypeExtension.Seen.Clear();

            root.ItemTemplate!.Build(new ScopeItemModel());
            root.ItemTemplate!.Build(new ScopeItemModel());

            Assert.Equal([typeof(ScopeItemModel), typeof(ScopeItemModel)], CaptureDataTypeExtension.Seen);
        }
    }
}
