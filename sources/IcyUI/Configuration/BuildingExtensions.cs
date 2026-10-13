using System.Globalization;
using Icy.Assets;
using Icy.Assets.Importers;
using Icy.Assets.Importers.BitmapFonts;
using Icy.Assets.Importers.DynamicFonts;
using Icy.Assets.Parsers;
using Icy.Data;
using Icy.Data.Markup;
using Icy.Diagnostics;
using Icy.Input;
using Icy.Input.Gestures;
using Icy.Localization;
using Icy.Markup;
using Icy.Rendering;
using Icy.Rendering.Fonts;
using Microsoft.Extensions.Logging;

namespace Icy.Configuration
{
    /// <summary>
    /// Provides extension methods for configuring asset settings in the library.
    /// </summary>
    public static class BuildingExtensions
    {
        private const string DefaultThemeResourceName = "Icy.Resources.Themes.DefaultTheme.xml";

        /// <summary>
        /// Configures the logging options with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configure">Receives the <see cref="LoggingConfiguration"/>.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, Action<LoggingConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            ILoggingConfigurationBuilder section = builder.ConfigureLogging();
            configure(section.Logging);
            return section;
        }

        /// <summary>
        /// Sends IcyUI's log messages to <paramref name="factory"/> instead of IcyUI's own providers.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="factory">The factory to create IcyUI's loggers from. IcyUI doesn't dispose it.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, ILoggerFactory factory)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.ConfigureLogging().UseLoggerFactory(factory);
        }

        /// <summary>
        /// Sets the lowest level IcyUI's own providers receive. <see cref="LogLevel.Debug"/> shows why something
        /// doesn't render.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="minimumLevel">The lowest level to log.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, LogLevel minimumLevel)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.ConfigureLogging().WithMinimumLevel(minimumLevel);
        }

        /// <summary>
        /// Sends IcyUI's log messages to <paramref name="factory"/> instead of IcyUI's own providers.
        /// </summary>
        /// <param name="builder">The logging section.</param>
        /// <param name="factory">The factory to create IcyUI's loggers from. IcyUI doesn't dispose it.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder UseLoggerFactory(this ILoggingConfigurationBuilder builder, ILoggerFactory factory)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(factory);
            builder.Logging.Factory = factory;
            return builder;
        }

        /// <summary>
        /// Adds a provider to IcyUI's own logger factory.
        /// </summary>
        /// <param name="builder">The logging section.</param>
        /// <param name="provider">The provider to write to.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder AddLoggerProvider(this ILoggingConfigurationBuilder builder, ILoggerProvider provider)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(provider);
            builder.Logging.Providers.Add(provider);
            return builder;
        }

        /// <summary>
        /// Sets the lowest level IcyUI's own providers receive.
        /// </summary>
        /// <param name="builder">The logging section.</param>
        /// <param name="minimumLevel">The lowest level to log.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder WithMinimumLevel(this ILoggingConfigurationBuilder builder, LogLevel minimumLevel)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Logging.MinimumLevel = minimumLevel;
            return builder;
        }

        /// <summary>
        /// Sets the localizer for the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="localizer">The localizer instance to be used.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        /// <remarks>
        /// Built in: <see cref="DictionaryLocalizer"/>, <see cref="ResourceManagerLocalizer"/>, and
        /// <see cref="CompositeLocalizer"/> to chain several.
        /// </remarks>
        public static IAssetConfigurationBuilder WithLocalizer(this IAssetConfigurationBuilder builder, ILocalizer localizer)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(localizer);
            builder.Assets.Localizer = localizer;
            return builder;
        }

        /// <summary>
        /// Sets the localizer for the asset configuration using a default instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the localizer to create.</typeparam>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder WithLocalizer<T>(this IAssetConfigurationBuilder builder)
            where T : ILocalizer, new()
        {
            builder.Assets.Localizer = new T();
            return builder;
        }

        /// <summary>
        /// Sets the default asset context for the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="context">The asset context to be set as default.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder WithDefaultAssetContext(this IAssetConfigurationBuilder builder, IAssetContext context)
        {
            builder.Assets.DefaultAssetContext = context;
            return builder;
        }

        /// <summary>
        /// Sets the asset context factory for the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="factory">The asset context factory to be set to the configuration.</param>
        /// <returns>THe current asset configuration builder to fluent configuration.</returns>
        public static IAssetConfigurationBuilder WithAssetContextFactory(this IAssetConfigurationBuilder builder, IAssetContextFactory factory)
        {
            builder.Assets.AssetContextFactory = factory;
            return builder;
        }

        /// <summary>
        /// Adds an asset importer to the asset configuration.
        /// </summary>
        /// <typeparam name="T">The type of the asset to be imported.</typeparam>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="importer">The asset importer instance to be added.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddImporter<T>(this IAssetConfigurationBuilder builder, IAssetImporter<T> importer)
        {
            builder.Assets.AssetResolver.RegisterImporter(importer);
            return builder;
        }

        /// <summary>
        /// Adds an asset importer of the specified type to the asset configuration.
        /// </summary>
        /// <typeparam name="TAsset">The type of the asset to be imported.</typeparam>
        /// <typeparam name="TImporter">The type of the asset importer to be created.</typeparam>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddImporter<TAsset, TImporter>(this IAssetConfigurationBuilder builder)
            where TImporter : IAssetImporter<TAsset>, new()
        {
            builder.Assets.AssetResolver.RegisterImporter(new TImporter());
            return builder;
        }

        /// <summary>
        /// Adds an asset parser to the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="parser">The asset parser instance to be added.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddParser(this IAssetConfigurationBuilder builder, IAssetParser parser)
        {
            builder.Assets.AssetResolver.RegisterParser(parser);
            return builder;
        }

        /// <summary>
        /// Adds an asset parser of the specified type to the asset configuration.
        /// </summary>
        /// <typeparam name="T">The type of the asset parser to be created.</typeparam>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddParser<T>(this IAssetConfigurationBuilder builder)
            where T : IAssetParser, new()
        {
            builder.Assets.AssetResolver.RegisterParser(new T());
            return builder;
        }

        /// <summary>
        /// Adds support for bitmap fonts to the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddBitmapFontSupport(this IAssetConfigurationBuilder builder) =>
            builder.AddImporter<IFont, BMFontImporter>();

        /// <summary>
        /// Adds support for dynamic TrueType fonts to the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder AddDynamicFontSupport(this IAssetConfigurationBuilder builder) =>
            builder.AddImporter<IFont, DynamicFontImporter>()
                   .AddImporter<IGlyphRasterizer, StbRasterizerImporter>();

        /// <summary>
        /// Adds support for basic font types to the asset configuration.
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        /// <remarks>
        /// This method adds support for both bitmap fonts and TrueType fonts.
        /// </remarks>
        public static IAssetConfigurationBuilder AddBasicFontSupport(this IAssetConfigurationBuilder builder) =>
            builder.AddBitmapFontSupport()
                  .AddDynamicFontSupport();

        /// <summary>
        /// Sets the assembly resolver for the reflection configuration.
        /// </summary>
        /// <param name="builder">The reflection configuration builder instance.</param>
        /// <param name="resolver">The assembly resolver instance to be used.</param>
        /// <returns>The current reflection configuration builder instance for fluent configuration.</returns>
        public static IReflectionConfigurationBuilder WithAssemblyResolver(this IReflectionConfigurationBuilder builder, IAssemblyResolver resolver)
        {
            builder.Types.AssemblyResolver = resolver;
            return builder;
        }

        /// <summary>
        /// Sets the type converter for the reflection configuration.
        /// </summary>
        /// <param name="builder">The reflection configuration builder instance.</param>
        /// <param name="converter">The type converter instance to be used.</param>
        /// <returns>The current reflection configuration builder instance for fluent configuration.</returns>
        public static IReflectionConfigurationBuilder WithTypeConverter(this IReflectionConfigurationBuilder builder, ITypeConverter converter)
        {
            builder.Types.TypeConverter = converter;
            builder.Types.PropertyRegistry.TypeConverter = converter;
            return builder;
        }

        /// <summary>
        /// Sets the property registry for the reflection configuration.
        /// </summary>
        /// <param name="builder">The reflection configuration builder instance.</param>
        /// <param name="registry">The property registry instance to be used.</param>
        /// <returns>The current reflection configuration builder instance for fluent configuration.</returns>
        /// <remarks>
        /// Only useful for isolating a configuration from the process-wide
        /// <see cref="Data.Markup.PropertyRegistry.Default"/> - tests being the usual reason. Elements must be
        /// constructed against the same registry, so build them inside a
        /// <see cref="Data.Markup.PropertyRegistry.UseScope"/> for <paramref name="registry"/>; see the remarks on
        /// <see cref="Data.Markup.PropertyRegistry"/> for why mixing registries breaks value precedence.
        /// </remarks>
        public static IReflectionConfigurationBuilder WithPropertyRegistry(this IReflectionConfigurationBuilder builder, PropertyRegistry registry)
        {
            builder.Types.PropertyRegistry = registry;
            return builder;
        }

        /// <summary>
        /// Configures the markup facet - the types markup may name, how they're constructed, and how bare text
        /// becomes objects.
        /// </summary>
        /// <param name="builder">The configuration builder instance; <see cref="IConfigurationBuilder.ConfigureTypes()"/>
        /// isn't needed first.</param>
        /// <param name="configure">The configuration action applied to <see cref="ReflectionConfiguration.Markup"/>.</param>
        /// <returns>The reflection section of the builder.</returns>
        /// <example>
        /// <code language="csharp">
        /// builder.ConfigureMarkup(m => m.RegisterShortName&lt;HealthBar&gt;());
        /// </code>
        /// </example>
        public static IReflectionConfigurationBuilder ConfigureMarkup(this IConfigurationBuilder builder, Action<MarkupConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IReflectionConfigurationBuilder section = builder.ConfigureTypes();
            configure(section.Types.Markup);
            return section;
        }

        /// <summary>
        /// Configures the catalog of registered debug overlays and HUD panels.
        /// </summary>
        /// <param name="builder">The configuration builder instance; <see cref="IConfigurationBuilder.ConfigureTypes()"/>
        /// isn't needed first.</param>
        /// <param name="configure">The configuration action applied to <see cref="ReflectionConfiguration.Diagnostics"/>.</param>
        /// <returns>The reflection section of the builder.</returns>
        /// <example>
        /// <code language="csharp">
        /// builder.ConfigureDiagnostics(d => d.RegisterOverlay&lt;MyCustomOverlay&gt;());
        /// </code>
        /// </example>
        public static IReflectionConfigurationBuilder ConfigureDiagnostics(this IConfigurationBuilder builder, Action<DebugToolRegistry> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IReflectionConfigurationBuilder section = builder.ConfigureTypes();
            configure(section.Types.Diagnostics);
            return section;
        }

        /// <summary>
        /// Adds support for loading markup documents through the asset pipeline - both a <see cref="UI.UIElement"/>
        /// document (a <c>Page</c>, say) and a standalone <see cref="UI.ResourceDictionary"/> document (a theme
        /// file referenced via <c>Source=</c>), so either can be requested by path like any other asset.
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <returns>The asset section of the builder.</returns>
        /// <remarks>
        /// The importers need the whole built configuration (assets, types and fonts), so they're registered by an
        /// <see cref="IConfigurationBuilder.AfterBuild"/> step. Both engine integrations call this by default, which
        /// is what lets <see cref="AssetConfiguration.Load{T}(string)"/> and <see cref="UI.Canvas.LoadMarkup"/> load
        /// markup files.
        /// </remarks>
        public static IAssetConfigurationBuilder AddMarkupSupport(this IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c =>
            {
                c.Assets.AssetResolver.RegisterImporter(new MarkupImporter(c));
                c.Assets.AssetResolver.RegisterImporter(new ResourceDictionaryImporter(c));
            });
            return builder.ConfigureAssets();
        }

        /// <summary>
        /// Loads IcyUI's built-in theme once the configuration is built. Both engine integrations call it by default.
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <returns>The theme section of the builder.</returns>
        /// <remarks>
        /// Runs as an <see cref="IConfigurationBuilder.AfterBuild"/> step, so a
        /// <see cref="ConfigureTheme(IConfigurationBuilder, Action{ThemeConfiguration})"/> callback declared later
        /// replaces the default theme. A theme already assigned through the builder's theme section
        /// (<c>builder.ConfigureTheme().Theme.Theme = myTheme</c>) is kept, and the built-in one isn't loaded.
        /// </remarks>
        public static IThemeConfigurationBuilder UseDefaultTheme(this IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c =>
            {
                if (c.Theme.Theme == null)
                    c.UseDefaultTheme();
            });
            return builder.ConfigureTheme();
        }

        /// <summary>
        /// Configures gesture recognition: hold delay, multi-tap interval and movement tolerance.
        /// </summary>
        /// <param name="builder">The configuration builder instance; its input system must already be set.</param>
        /// <param name="configure">Receives the input system's <see cref="GestureSettings"/>.</param>
        /// <returns>The input section of the builder.</returns>
        public static IInputConfigurationBuilder ConfigureGestures(this IConfigurationBuilder builder, Action<GestureSettings> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IInputConfigurationBuilder section = builder.ConfigureInput();
            configure(section.InputSystem.Events.Gestures.Settings);
            return section;
        }

        /// <summary>
        /// Configures the asset options with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <param name="configure">Receives the <see cref="AssetConfiguration"/>.</param>
        /// <returns>The asset section of the builder.</returns>
        public static IAssetConfigurationBuilder ConfigureAssets(this IConfigurationBuilder builder, Action<AssetConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IAssetConfigurationBuilder section = builder.ConfigureAssets();
            configure(section.Assets);
            return section;
        }

        /// <summary>
        /// Configures the reflection options (type resolution, conversion, markup, debug tools) with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <param name="configure">Receives the <see cref="ReflectionConfiguration"/>.</param>
        /// <returns>The reflection section of the builder.</returns>
        public static IReflectionConfigurationBuilder ConfigureTypes(this IConfigurationBuilder builder, Action<ReflectionConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IReflectionConfigurationBuilder section = builder.ConfigureTypes();
            configure(section.Types);
            return section;
        }

        /// <summary>
        /// Configures the input system with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder instance; its input system must already be set.</param>
        /// <param name="configure">Receives the <see cref="IInputSystem"/>.</param>
        /// <returns>The input section of the builder.</returns>
        public static IInputConfigurationBuilder ConfigureInput(this IConfigurationBuilder builder, Action<IInputSystem> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IInputConfigurationBuilder section = builder.ConfigureInput();
            configure(section.InputSystem);
            return section;
        }

        /// <summary>
        /// Configures the render context with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder instance; its render context must already be set.</param>
        /// <param name="configure">Receives the <see cref="IRenderContext"/>.</param>
        /// <returns>The rendering section of the builder.</returns>
        public static IRenderingConfigurationBuilder ConfigureRendering(this IConfigurationBuilder builder, Action<IRenderContext> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IRenderingConfigurationBuilder section = builder.ConfigureRendering();
            configure(section.RenderContext);
            return section;
        }

        /// <summary>
        /// Configures the default-theme facet - the <see cref="UI.ResourceDictionary"/> a <see cref="UI.Canvas"/>
        /// exposes to every element attached to it (see <see cref="UI.Canvas.Resources"/>).
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <param name="configure">The configuration action applied to the built configuration's <see cref="ThemeConfiguration"/>.</param>
        /// <returns>The theme section of the builder.</returns>
        /// <remarks>
        /// The callback runs when the configuration is built, in order with other deferred steps such as
        /// <see cref="UseDefaultTheme(IConfigurationBuilder)"/>, so a theme set here replaces a default theme an engine
        /// integration requested earlier.
        /// </remarks>
        /// <example>
        /// <code language="csharp">
        /// builder.ConfigureTheme(t => t.Theme = myCustomThemeDictionary);
        /// </code>
        /// </example>
        public static IThemeConfigurationBuilder ConfigureTheme(this IConfigurationBuilder builder, Action<ThemeConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            builder.AfterBuild(c => configure(c.Theme));
            return builder.ConfigureTheme();
        }

        /// <summary>
        /// Sets the culture IcyUI formats and parses the values players read and type with
        /// (<see cref="IcyConfiguration.Culture"/>).
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <param name="culture">
        /// The culture to use, or <see langword="null"/> to follow <see cref="CultureInfo.CurrentUICulture"/> (the default).
        /// </param>
        /// <returns>The configuration builder instance, for fluent chaining.</returns>
        /// <example>
        /// <code language="csharp">
        /// builder.WithCulture(CultureInfo.GetCultureInfo("ru-RU"));
        /// </code>
        /// </example>
        public static IConfigurationBuilder WithCulture(this IConfigurationBuilder builder, CultureInfo? culture)
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AfterBuild(c => c.Culture = culture);
        }

        /// <summary>
        /// Configures the UI scaling facet - the defaults every <see cref="UI.Canvas"/> uses to compute
        /// <see cref="UI.Canvas.EffectiveScale"/>.
        /// </summary>
        /// <param name="builder">The configuration builder instance.</param>
        /// <param name="configure">The configuration action applied to <see cref="ScalingConfiguration"/>.</param>
        /// <returns>The scaling section of the builder.</returns>
        /// <example>
        /// <code language="csharp">
        /// builder.ConfigureScaling().ConfigureScaling(s => s.Mode = UIScaleMode.ReferenceResolution);
        /// </code>
        /// </example>
        public static IScalingConfigurationBuilder ConfigureScaling(this IConfigurationBuilder builder, Action<ScalingConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IScalingConfigurationBuilder section = builder.ConfigureScaling();
            configure(section.Scaling);
            return section;
        }

        /// <summary>
        /// Loads IcyUI's own bundled default theme and assigns it to <see cref="IcyConfiguration.Theme"/>'s
        /// <see cref="ThemeConfiguration.Theme"/> property, so a <see cref="UI.Canvas"/> built against
        /// <paramref name="configuration"/> gives every Tier-1 control (<see cref="UI.Controls.Button"/>,
        /// <see cref="UI.Controls.CheckBox"/>, etc.) a real default look with no further setup.
        /// </summary>
        /// <param name="configuration">The built configuration to load the theme through and assign it to.</param>
        /// <returns><paramref name="configuration"/>, for fluent chaining.</returns>
        /// <exception cref="InvalidOperationException">The bundled default-theme resource is missing.</exception>
        /// <remarks>
        /// Loading the theme markup needs a fully-built <see cref="IcyConfiguration"/> (a real
        /// <see cref="Markup.MarkupLoader"/>, type resolution, and property registration). On a builder, use
        /// <see cref="UseDefaultTheme(IConfigurationBuilder)"/>, which runs this after <see cref="IConfigurationBuilder.Build"/>;
        /// call this one to switch back to the default theme at runtime.
        /// </remarks>
        public static IcyConfiguration UseDefaultTheme(this IcyConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            using Stream stream = typeof(BuildingExtensions).Assembly.GetManifestResourceStream(DefaultThemeResourceName)
                ?? throw new InvalidOperationException($"The bundled default theme resource '{DefaultThemeResourceName}' is missing.");
            var loader = new MarkupLoader(configuration);

            // Loaded as a throwaway UIElement (see DefaultTheme.xml's own remarks on why it isn't a bare
            // <ResourceDictionary> document) purely to reach its .Resources - the element itself is discarded.
            UI.UIElement host = loader.Load(stream, "DefaultTheme.xml");
            configuration.Theme.Theme = host.Resources;
            return configuration;
        }

        /// <summary>
        /// Registers a type converter for converting values from one type to another.
        /// </summary>
        /// <param name="builder">The reflection configuration builder instance.</param>
        /// <param name="from">The source type for the conversion.</param>
        /// <param name="to">The target type for the conversion.</param>
        /// <param name="converter">The value converter instance to be registered.</param>
        /// <returns>The current reflection configuration builder instance for fluent configuration.</returns>
        public static IReflectionConfigurationBuilder AddTypeConverter(this IReflectionConfigurationBuilder builder, Type from, Type to, IValueConverter converter)
        {
            builder.Types.TypeConverter.RegisterConverter(from, to, converter);
            return builder;
        }

        /// <summary>
        /// Registers a type converter for converting values from a specified source type to a target type.
        /// </summary>
        /// <typeparam name="TSource">The source type for the conversion.</typeparam>
        /// <typeparam name="TTarget">The target type for the conversion.</typeparam>
        /// <param name="builder">The reflection configuration builder instance.</param>
        /// <param name="converter">The value converter instance to be registered.</param>
        /// <returns>The current reflection configuration builder instance for fluent configuration.</returns>
        public static IReflectionConfigurationBuilder AddTypeConverter<TSource, TTarget>(this IReflectionConfigurationBuilder builder, IValueConverter<TSource, TTarget> converter)
        {
            builder.Types.TypeConverter.RegisterConverter(converter);
            return builder;
        }
    }
}
