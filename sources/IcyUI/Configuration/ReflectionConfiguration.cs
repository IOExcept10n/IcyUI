// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.Concurrent;
using Icy.Data;
using Icy.Data.Bindings;
using Icy.Data.Markup;
using Icy.Diagnostics;
using Icy.Markup;

namespace Icy.Configuration
{
    /// <summary>
    /// Represents configuration for the reflection-related service used in library.
    /// </summary>
    public class ReflectionConfiguration
    {
        // Typed paths are immutable once built, so every binding on the same (registry, type, path, kind) shares one.
        // Bounded by the distinct paths in the game's markup; never evicted.
        private readonly ConcurrentDictionary<(PropertyRegistry Registry, Type Type, string Path, bool Compiled), PropertyPath> typedPaths = new();

        /// <summary>
        /// Gets or sets an instance of the <see cref="IAssemblyResolver"/> used in library.
        /// </summary>
        public IAssemblyResolver AssemblyResolver { get; set; } = new AssemblyResolver();

        /// <summary>
        /// Gets or sets an instance of the <see cref="ITypeConverter"/> used in library.
        /// </summary>
        public ITypeConverter TypeConverter { get; set; } = new TypeConversionManager();

        /// <summary>
        /// Gets or sets the <see cref="Data.Markup.PropertyRegistry"/> this configuration resolves registered
        /// properties through.
        /// </summary>
        /// <remarks>
        /// Defaults to whichever registry was <see cref="Data.Markup.PropertyRegistry.Current"/> when this
        /// configuration was created - normally <see cref="Data.Markup.PropertyRegistry.Default"/>. Assigning a
        /// different registry only affects objects also built against it: a
        /// <see cref="Data.Markup.DependencyObject"/> captures its registry at construction, so create elements
        /// inside a <see cref="Data.Markup.PropertyRegistry.UseScope"/> for the same registry rather than swapping
        /// this afterwards. See the remarks on <see cref="Data.Markup.PropertyRegistry"/>.
        /// </remarks>
        public PropertyRegistry PropertyRegistry { get; set; } = PropertyRegistry.Current;

        /// <summary>
        /// Gets or sets the markup facet: which types markup may name, how they're constructed, and how bare text
        /// becomes objects.
        /// </summary>
        /// <remarks>
        /// Markup lives here rather than in a section of its own because it is built entirely out of the other
        /// three services on this configuration - <see cref="AssemblyResolver"/> resolves its type names,
        /// <see cref="TypeConverter"/> converts its attribute values, and <see cref="PropertyRegistry"/> resolves
        /// its properties. Configure it fluently through
        /// <see cref="BuildingExtensions.ConfigureMarkup(IConfigurationBuilder, Action{Markup.MarkupConfiguration})"/>.
        /// </remarks>
        public MarkupConfiguration Markup { get; set; } = new();

        /// <summary>
        /// Gets or sets the catalog of registered debug overlays and HUD panels.
        /// </summary>
        /// <remarks>
        /// Configure it fluently through
        /// <see cref="BuildingExtensions.ConfigureDiagnostics(IConfigurationBuilder, Action{DebugToolRegistry})"/>.
        /// </remarks>
        public DebugToolRegistry Diagnostics { get; set; } = new();

        /// <summary>
        /// Gets the typed path for <paramref name="path"/> on <paramref name="sourceType"/>, building it on first use.
        /// </summary>
        /// <param name="sourceType">The type the path starts at.</param>
        /// <param name="path">The member path, such as <c>Player.Name</c>.</param>
        /// <param name="compiled">
        /// <see langword="true"/> for a <see cref="CompiledPropertyPath"/>; <see langword="false"/> for a reflection-based
        /// <see cref="PropertyPath"/>.
        /// </param>
        /// <returns>The shared path.</returns>
        /// <exception cref="ArgumentException">A member of <paramref name="path"/> doesn't exist.</exception>
        internal PropertyPath GetTypedPath(Type sourceType, string path, bool compiled) =>
            typedPaths.GetOrAdd(
                (PropertyRegistry, sourceType, path, compiled),
                static key => key.Compiled
                    ? new CompiledPropertyPath(key.Path, key.Type, key.Registry)
                    : new PropertyPath(key.Path, key.Type, key.Registry));
    }
}