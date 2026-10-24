// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Icy.Data.Bindings;

namespace Icy.Data.Markup
{
    /// <summary>
    /// Represents an object that can store attached properties, and whose registered properties
    /// (see <see cref="Attributes.RegisterReferenceAttribute"/>) participate in the value-precedence
    /// system used by styles, visual states, and animations.
    /// </summary>
    public abstract partial class DependencyObject : BindableObject
    {
        private bool raisingComputedChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyObject"/> class, binding it to the
        /// <see cref="Markup.PropertyRegistry.Current"/> registry for its lifetime.
        /// </summary>
        protected DependencyObject()
        {
            PropertyRegistry = Markup.PropertyRegistry.Current;
            PropertyChanged += OnDependencyObjectPropertyChanged;
        }

        /// <summary>
        /// Gets the <see cref="Markup.PropertyRegistry"/> this instance resolves its properties through, captured
        /// when it was constructed.
        /// </summary>
        /// <remarks>
        /// Captured once rather than read per access so the object keeps using one registry even if a different one
        /// later becomes <see cref="Markup.PropertyRegistry.Current"/> - an object whose property references came
        /// from two registries would have its value-precedence bookkeeping split between them. Use
        /// <see cref="Markup.PropertyRegistry.For(object)"/> to reach this from code holding an arbitrary target.
        /// </remarks>
        public PropertyRegistry PropertyRegistry { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyStore"/> registered for this instance's runtime type, resolving and
        /// registering it on first use.
        /// </summary>
        /// <returns>The property store for <see cref="object.GetType()"/>.</returns>
        public IPropertyStore GetPropertyStore() => PropertyRegistry.GetPropertyStore(GetType());

        /// <summary>
        /// Gets the value a registered property stores, for a property whose getter computes more than that, such as
        /// <see cref="UI.UIElement.IsEnabled"/>.
        /// </summary>
        /// <param name="propertyName">The property's name.</param>
        /// <param name="value">The stored value.</param>
        /// <returns>
        /// <see langword="true"/> when the property has a stored value apart from its getter; <see langword="false"/>
        /// to read the getter.
        /// </returns>
        /// <remarks>
        /// The value-precedence system records and restores the stored value, so a style cleared later never writes a
        /// computed value back as if it had been assigned.
        /// </remarks>
        internal virtual bool TryGetStoredValue(string propertyName, out object? value)
        {
            value = null;
            return false;
        }

        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> for a property whose computed value changed with
        /// no assignment, which the value-precedence system doesn't record as a local value.
        /// </summary>
        /// <param name="propertyName">The property's name.</param>
        internal void OnComputedPropertyChanged(string propertyName)
        {
            raisingComputedChange = true;
            try
            {
                OnPropertyChanged(propertyName);
            }
            finally
            {
                raisingComputedChange = false;
            }
        }

        /// <summary>
        /// Sets a property's backing field and raises <see cref="INotifyPropertyChanged.PropertyChanged"/> when the value
        /// changes. An assignment of the value the property already holds still counts as a local value, so a style
        /// applied later doesn't replace it.
        /// </summary>
        /// <typeparam name="T">The property's type.</typeparam>
        /// <param name="field">The property's backing field.</param>
        /// <param name="newValue">The value to assign.</param>
        /// <param name="propertyName">The property's name; filled in by the compiler.</param>
        /// <returns><see langword="true"/> when the value changed.</returns>
        /// <remarks>
        /// Without this, <c>new Button { HorizontalAlignment = HorizontalAlignment.Stretch }</c> would change nothing
        /// (<see cref="UI.HorizontalAlignment.Stretch"/> is the default), wouldn't be recorded, and the theme's style
        /// would override it once the button joins a canvas.
        /// </remarks>
        protected new bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (base.SetProperty(ref field, newValue, propertyName))
                return true;

            if (propertyName is not null && GetPropertyStore().TryGetProperty(propertyName, out IPropertyReference? property))
                property.NotifyLocalValueChanged(this);
            return false;
        }

        private void OnDependencyObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // A direct assignment to a property (as opposed to a style/visual-state/animation tier
            // contribution) always becomes the new fallback value once every tier is cleared. Properties
            // that no precedence tier has ever touched are skipped at effectively no cost here.
            if (e.PropertyName is null || raisingComputedChange)
                return;
            if (GetPropertyStore().TryGetProperty(e.PropertyName, out IPropertyReference? property))
                property.NotifyLocalValueChanged(this);
        }
    }
}
