// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Diagnostics.Logging;
using Microsoft.Extensions.Logging;

namespace Icy.Data.Bindings
{
    /// <summary>
    /// A typed path for a binding whose source is a data context declared with <c>x:DataType</c>: the data context can
    /// turn out to be another type at run time, so each read and write checks it first.
    /// </summary>
    /// <param name="typed">The shared typed path, built for <paramref name="declaredType"/>.</param>
    /// <param name="declaredType">The <c>x:DataType</c>.</param>
    /// <param name="compiled">
    /// Whether the binding asked for a compiled path: a wrong type then throws instead of falling back.
    /// </param>
    /// <param name="path">The path text, for the fallback and the messages.</param>
    /// <param name="logger">Where the fallback is reported, or <see langword="null"/>.</param>
    /// <remarks>
    /// A source of the declared type, a subclass or an implementation goes through <paramref name="typed"/>. Any other
    /// source, for a static path, logs <see cref="IcyLog.BindingSourceTypeMismatch"/> once and reads through a
    /// <see cref="DynamicPropertyPath"/> from then on; for a compiled path, it throws <see cref="BindingException"/>.
    /// </remarks>
    internal sealed class CheckedPropertyPath(PropertyPath typed, Type declaredType, bool compiled, string path, ILogger? logger) : IPropertyPath
    {
        private DynamicPropertyPath? fallback;

        /// <inheritdoc/>
        public Type PropertyType => typed.PropertyType;

        /// <inheritdoc/>
        public object? GetValue(object source) => For(source).GetValue(source);

        /// <inheritdoc/>
        public void SetValue(object target, object? value) => For(target).SetValue(target, value);

        /// <inheritdoc/>
        public Type GetPropertyType(object source) => For(source).GetPropertyType(source);

        /// <inheritdoc/>
        public bool DependsOn(string propertyName) => typed.DependsOn(propertyName);

        private IPropertyPath For(object source)
        {
            if (fallback != null)
                return fallback;

            if (declaredType.IsInstanceOfType(source))
                return typed;

            if (compiled)
            {
                throw new BindingException(
                    $"The compiled binding path '{path}' expects a '{declaredType.Name}' (x:DataType) but its data context is a '{source.GetType().Name}'. Fix x:DataType or the data context.");
            }

            if (logger != null)
                IcyLog.BindingSourceTypeMismatch(logger, path, declaredType.Name, source.GetType().Name);
            return fallback = new DynamicPropertyPath(path, logger);
        }
    }
}
