// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.Markup
{
    /// <summary>
    /// Covers <c>{StaticResource}</c> inside a document whose root is a <see cref="ResourceDictionary"/>: an entry can
    /// use the entries declared before it, as it can inside <c>&lt;Panel.Resources&gt;</c>.
    /// </summary>
    public sealed class StandaloneDictionaryResourceTests : IDisposable
    {
        private readonly string root = Directory.CreateTempSubdirectory("icy-dict-").FullName;

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void AnEntry_CanUseAnEarlierEntryOfTheSameDictionary()
        {
            var loader = new MarkupLoader(CreateConfiguration());

            var dictionary = (ResourceDictionary)loader.LoadObject(
                """
                <ResourceDictionary>
                  <ControlTemplate x:Key="Skin" TargetType="Button"><Border Background="Red"/></ControlTemplate>
                  <Style TargetType="Button" Template="{StaticResource Skin}"/>
                </ResourceDictionary>
                """);

            Assert.True(dictionary.TryGetValue("Skin", out object? skin));
            Assert.True(dictionary.TryGetValue(ResourceDictionary.GetImplicitStyleKey(typeof(Button)), out object? style));
            Assert.Same(skin, ((Style)style!).Setters["Template"]);
        }

        [Fact]
        public void AnEntry_CannotUseALaterEntry()
        {
            var loader = new MarkupLoader(CreateConfiguration());

            var ex = Assert.Throws<MarkupException>(() => loader.LoadObject(
                """
                <ResourceDictionary>
                  <Style TargetType="Button" Template="{StaticResource Skin}"/>
                  <ControlTemplate x:Key="Skin" TargetType="Button"><Border Background="Red"/></ControlTemplate>
                </ResourceDictionary>
                """,
                "later.xml"));

            Assert.Contains("No resource named 'Skin'", ex.Message);
        }

        [Fact]
        public void AnEntry_CanUseAnEntryOfAMergedDictionary()
        {
            var loader = new MarkupLoader(CreateConfiguration());

            var dictionary = (ResourceDictionary)loader.LoadObject(
                """
                <ResourceDictionary>
                  <ResourceDictionary.MergedDictionaries>
                    <ResourceDictionary>
                      <ControlTemplate x:Key="Skin" TargetType="Button"><Border Background="Red"/></ControlTemplate>
                    </ResourceDictionary>
                  </ResourceDictionary.MergedDictionaries>
                  <Style x:Key="Styled" TargetType="Button" Template="{StaticResource Skin}"/>
                </ResourceDictionary>
                """);

            Assert.True(dictionary.TryGetValue("Styled", out object? style));
            Assert.NotNull(((Style)style!).Setters["Template"]);
        }

        [Fact]
        public void TheBuiltInTheme_IsAStandaloneDictionary_SoACopyOfItLoadsAsAThemeFile()
        {
            using Stream stream = typeof(BuildingExtensions).Assembly.GetManifestResourceStream("Icy.Resources.Themes.DefaultTheme.xml")
                ?? throw new InvalidOperationException("The default theme isn't embedded under the expected name.");
            using var reader = new StreamReader(stream);
            File.WriteAllText(Path.Combine(root, "Copy.xml"), reader.ReadToEnd());

            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithDefaultAssetContext(new AssetContext(root + Path.DirectorySeparatorChar));
            builder.AddMarkupSupport();
            builder.UseDefaultTheme().UseThemeFile("Copy.xml");
            IcyConfiguration configuration = builder.Build();
            var canvas = new Canvas(configuration);
            var button = new Button();
            canvas.Add(button);

            Assert.NotNull(button.Template);
        }

        private static IcyConfiguration CreateConfiguration() =>
            new(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
    }
}
