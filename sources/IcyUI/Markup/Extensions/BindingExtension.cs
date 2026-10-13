// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Reflection;
using Icy.Data.Bindings;
using Icy.UI;
using Microsoft.Extensions.Logging;

namespace Icy.Markup.Extensions
{
    /// <summary>
    /// <c>{Binding ...}</c>: binds a property to a path evaluated against an explicit source, a named element, or
    /// - the common case - the target's <see cref="UIElement.DataContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>{Binding Path=Name}</c> can be shortened to <c>{Binding Name}</c> - <c>Path</c> is this extension's
    /// <see cref="MarkupExtensionDefaultPropertyAttribute">default property</see>. <c>{Binding}</c> with no path at
    /// all binds the whole source object.
    /// </para>
    /// <para>
    /// Binding to a property the property-registry marks non-bindable
    /// (<see cref="Data.Bindings.Attributes.NonBindableAttribute.AsTarget"/>, or
    /// <see cref="Data.Markup.UIPropertyMetadata.IsBindable"/>) is an error, not a silent no-op - raised by
    /// <see cref="Binding.TargetProperty"/> itself, which this extension assigns through unchanged.
    /// </para>
    /// <para>
    /// Always resolves to <see cref="MarkupValue.Unset"/>: the binding is applied to the target directly (and, for
    /// the implicit-<see cref="UIElement.DataContext"/> case, re-applied whenever
    /// <see cref="UIElement.DataContextChanged"/> fires), so there is never a single value for the loader to
    /// assign afterward.
    /// </para>
    /// <para>
    /// A path is static (resolved once on a known type), compiled (see <see cref="Compiled"/>), or dynamic (resolved
    /// on each read) when the source type is unknown; see the Data binding concept page.
    /// </para>
    /// </remarks>
    [MarkupExtensionDefaultProperty(nameof(Path))]
    public sealed class BindingExtension : IMarkupExtension
    {
        /// <summary>
        /// Gets or sets the path, relative to the source, to bind to. Unset (or empty) binds the source itself.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the <c>x:Name</c> of the element to bind to, instead of the target's
        /// <see cref="UIElement.DataContext"/>.
        /// </summary>
        public string? ElementName { get; set; }

        /// <summary>
        /// Gets or sets the object to bind to directly, instead of the target's <see cref="UIElement.DataContext"/>.
        /// </summary>
        public object? Source { get; set; }

        /// <summary>
        /// Gets or sets the binding's direction. Unset keeps the target property's own default (see
        /// <see cref="Data.Markup.UIPropertyMetadata.DefaultTwoWayBinding"/>).
        /// </summary>
        public BindingMode? Mode { get; set; }

        /// <summary>
        /// Gets or sets when the binding refreshes the target from the source. See
        /// <see cref="Data.Bindings.UpdateTargetTrigger"/>.
        /// </summary>
        public UpdateTargetTrigger UpdateTargetTrigger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the path is compiled into delegates (<see cref="CompiledPropertyPath"/>)
        /// instead of read through reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Needs a known source type: an <c>x:DataType</c> on this element or an ancestor for a data-context binding, or
        /// <see cref="ElementName"/> or <see cref="Source"/>. Without one, the markup fails to load.
        /// </para>
        /// <para>
        /// A compiled data-context binding throws <see cref="BindingException"/> when the data context isn't the
        /// <c>x:DataType</c>; a static one warns and falls back to a dynamic path. Under NativeAOT, compiled paths are
        /// interpreted, at about the speed of static ones.
        /// </para>
        /// <para>
        /// A path that reads a member of a property typed <see cref="object"/> (such as
        /// <see cref="UI.Controls.SelectingItemsControl.SelectedItem"/><c>.Name</c>) can't be compiled: the member is
        /// only known at run time. Such a path is read dynamically even with <see cref="Compiled"/> set, without an
        /// error.
        /// </para>
        /// </remarks>
        public bool Compiled { get; set; }

        /// <inheritdoc/>
        /// <exception cref="MarkupException">
        /// <para>One of the following:</para>
        /// <list type="bullet">
        /// <item><description>the target doesn't support bindings;</description></item>
        /// <item><description>the property isn't a registered one;</description></item>
        /// <item><description><see cref="ElementName"/> names nothing in scope;</description></item>
        /// <item><description>the property is marked non-bindable;</description></item>
        /// <item><description>
        /// the source type is known (an <c>x:DataType</c>, <see cref="ElementName"/> or <see cref="Source"/>) and
        /// <see cref="Path"/> doesn't resolve on it;
        /// </description></item>
        /// <item><description><see cref="Compiled"/> is set without a known source type.</description></item>
        /// </list>
        /// </exception>
        public object? ProvideValue(MarkupExtensionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Target is not IBindingTarget bindingTarget)
                throw MarkupException.At($"'{context.Target.GetType().Name}' doesn't support bindings.", context.Node, context.SourcePath);

            if (context.Member.Reference is not { } targetProperty)
            {
                throw MarkupException.At(
                    $"'{context.Member.Name}' isn't a registered property, so it can't be a binding target.",
                    context.Node,
                    context.SourcePath);
            }

            UIElement? named = null;
            if (ElementName != null && !context.Names.TryFind(ElementName, out named))
                throw MarkupException.At($"No element named '{ElementName}' is in scope.", context.Node, context.SourcePath);

            var binding = new Binding(CreatePath(context, named))
            {
                TextConverter = context.Configuration.Types.TypeConverter,
                LoadingConfiguration = context.Configuration,
            };
            bindingTarget.Bind(binding);

            try
            {
                binding.TargetProperty = targetProperty;
            }
            catch (Exception ex)
            {
                bindingTarget.Unbind(binding);
                throw MarkupException.At(ex.Message, context.Node, context.SourcePath, ex);
            }

            if (Mode.HasValue)
                binding.Mode = Mode.Value;
            binding.UpdateTargetTrigger = UpdateTargetTrigger;

            if (Source != null)
            {
                binding.Source = Source;
            }
            else if (ElementName != null)
            {
                binding.Source = named;
            }
            else if (context.Target is UIElement owner)
            {
                binding.Source = owner.DataContext;

                // No explicit source - the binding tracks the target's inherited DataContext for its whole
                // lifetime, not just its value at bind time. See UIElement.DataContextChanged's remarks for why
                // this can't be resolved once and forgotten: the target is commonly still detached from its parent
                // when the extension runs, so there is often no effective DataContext yet to read. Assigning
                // Source alone only rewires the change-notification subscription - it doesn't itself re-pull a
                // value - so the refresh needs the same explicit UpdateTarget() the initial bind uses below.
                owner.DataContextChanged += (_, _) =>
                {
                    binding.Source = owner.DataContext;
                    if (binding.Mode != BindingMode.OneWayToSource)
                        binding.UpdateTarget();
                };
            }

            binding.IsEnabled = true;
            if (binding.Mode != BindingMode.OneWayToSource)
                binding.UpdateTarget();

            return MarkupValue.Unset;
        }

        // Walks the path's types the way the static resolution does, and names the first member that doesn't exist and
        // the type it was looked for on. Returns null when the walk reaches an object-typed value first: the rest of the
        // path can only be resolved on the run-time value.
        private static string? DescribeMissingMember(Type sourceType, string path)
        {
            Type current = sourceType;
            foreach (string part in path.Split('.'))
            {
                int bracket = part.IndexOf('[', StringComparison.Ordinal);
                string name = bracket == -1 ? part : part[..bracket];
                if (name.Length != 0 && name != "this")
                {
                    if (current == typeof(object))
                        return null;

                    PropertyInfo? property = PropertyPath.FindProperty(current, name);
                    if (property == null)
                    {
                        IEnumerable<string> members = PublicProperties(current).Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name);
                        return $"'{name}' isn't a public property of '{TypeName(current)}' (binding path '{path}').{NameSuggestion.Clause(name, members)}";
                    }

                    current = property.PropertyType;
                }

                // Each [...] after the name reads an element: the array's element type, or the matching indexer's type.
                while (bracket != -1)
                {
                    int close = part.IndexOf(']', bracket);
                    if (close == -1)
                        break;
                    if (current == typeof(object))
                        return null;

                    string arguments = part[(bracket + 1)..close];
                    if (current.IsArray)
                    {
                        current = current.GetElementType()!;
                    }
                    else
                    {
                        int count = arguments.Split(',').Length;
                        PropertyInfo? indexer = PublicProperties(current).FirstOrDefault(p => p.GetIndexParameters().Length == count);
                        if (indexer == null)
                            return $"'{TypeName(current)}' has no indexer for '[{arguments}]' (binding path '{path}').";
                        current = indexer.PropertyType;
                    }

                    bracket = part.IndexOf('[', close);
                }
            }

            return $"The binding path '{path}' doesn't resolve on '{TypeName(sourceType)}'.";
        }

        // A type's public instance properties; for an interface, those of the interfaces it extends too.
        private static IEnumerable<PropertyInfo> PublicProperties(Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return type.IsInterface
                ? properties.Concat(type.GetInterfaces().SelectMany(i => i.GetProperties(BindingFlags.Public | BindingFlags.Instance)))
                : properties;
        }

        // A type's name as it's written in C#: List<String> rather than List`1.
        private static string TypeName(Type type)
        {
            if (type.IsArray)
                return $"{TypeName(type.GetElementType()!)}[{new string(',', type.GetArrayRank() - 1)}]";
            if (!type.IsGenericType)
                return type.Name;

            string name = type.Name;
            int tick = name.IndexOf('`', StringComparison.Ordinal);
            if (tick != -1)
                name = name[..tick];
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>";
        }

        private IPropertyPath CreatePath(MarkupExtensionContext context, UIElement? named)
        {
            ILogger logger = context.Configuration.Logging.CreateLogger<Binding>();
            if (string.IsNullOrEmpty(Path) || Path == "this")
                return new DynamicPropertyPath("this", logger);

            bool dataContextBinding = Source == null && named == null;
            Type? sourceType = Source?.GetType() ?? named?.GetType() ?? (dataContextBinding ? context.DataType : null);
            if (sourceType == null)
            {
                if (Compiled)
                {
                    throw MarkupException.At(
                        $"'{{Binding {Path}, Compiled=True}}' needs a known source type: declare x:DataType on this element or an ancestor, or bind with ElementName or Source.",
                        context.Node,
                        context.SourcePath);
                }

                return new DynamicPropertyPath(Path, logger);
            }

            PropertyPath typed;
            try
            {
                typed = context.Configuration.Types.GetTypedPath(sourceType, Path, Compiled);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                // A member read on an object-typed value (a ListBox's SelectedItem, say) can only be resolved on
                // the run-time value, so such a path is dynamic - compiled or not. Everything before that member did
                // resolve, so a typo there still fails here.
                string? missing = DescribeMissingMember(sourceType, Path);
                if (missing == null)
                    return new DynamicPropertyPath(Path, logger);
                throw MarkupException.At(missing, context.Node, context.SourcePath, ex);
            }

            return dataContextBinding ? new CheckedPropertyPath(typed, sourceType, Compiled, Path, logger) : typed;
        }
    }
}
