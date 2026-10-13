// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Reflection;
using Icy.Configuration;
using Icy.Data.Markup;
using Icy.Diagnostics.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Icy.Data
{
    /// <summary>
    /// Represents a default implementation of the assembly resolving service.
    /// </summary>
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, Type?> typesCache = [];
        private readonly HashSet<Assembly> reportedAssemblies = [];

        /// <inheritdoc/>
        public Assembly DefaultAssembly { get; set; } = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Gets or sets where the resolver warns about assemblies whose types can't all be loaded.
        /// <see cref="IcyConfiguration"/> sets it from <see cref="IcyConfiguration.Logging"/>.
        /// </summary>
        public ILogger Logger { get; set; } = NullLogger.Instance;

        /// <summary>
        /// Gets or sets the assemblies <see cref="FindType"/> searches (tests replace it).
        /// </summary>
        internal Func<IEnumerable<Assembly>> AssembliesProvider { get; set; } = AppDomain.CurrentDomain.GetAssemblies;

        /// <summary>
        /// Gets or sets how an assembly's types are listed (tests replace it).
        /// </summary>
        internal Func<Assembly, Type[]> TypesProvider { get; set; } = a => a.GetTypes();

        /// <inheritdoc/>
        public Assembly? FindAssembly(string name) => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);

        /// <inheritdoc/>
        public Type? FindType(string name)
        {
            if (typesCache.TryGetValue(name, out Type? value))
                return value;

            Type? result = TryGetType(name) ?? DefaultAssembly.GetType(name);
            if (result == null)
            {
                // Each assembly is scanned on its own: one whose types can't all load must not end the search.
                foreach (Assembly assembly in AssembliesProvider())
                {
                    result = assembly.GetType(name) ?? FindByShortName(assembly, name);
                    if (result != null)
                        break;
                }
            }

            return typesCache[name] = result;
        }

        /// <inheritdoc/>
        public IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();

        private static Type? TryGetType(string name)
        {
            try
            {
                return Type.GetType(name);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or BadImageFormatException)
            {
                return null;
            }
        }

        // Only types with a parameterless constructor: markup creates the element itself.
        private Type? FindByShortName(Assembly assembly, string name)
        {
            Type?[] types;
            try
            {
                types = TypesProvider(assembly);
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
                if (reportedAssemblies.Add(assembly))
                    IcyLog.AssemblyScanIncomplete(Logger, assembly.GetName().Name ?? assembly.FullName ?? "?", ex);
            }

            return types.OfType<Type>().Where(x => x.GetConstructor(Type.EmptyTypes) != null).FirstOrDefault(x => x.Name == name || x.FullName == name);
        }
    }
}