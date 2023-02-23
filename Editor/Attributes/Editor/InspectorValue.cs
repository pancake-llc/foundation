using System;
using JetBrains.Annotations;

namespace Pancake.AttributeDrawer
{
    public struct InspectorValue<T>
    {
        internal InspectorValue(Property property) { Property = property; }

        public Property Property { get; }

        [Obsolete("Use SmartValue instead", true)] public T Value { get => (T) Property.Value; set => Property.SetValue(value); }

        [PublicAPI] public T SmartValue
        {
            get => (T) Property.Value;
            set
            {
                if (Property.Comparer.Equals(Property.Value, value))
                {
                    return;
                }

                Property.SetValue(value);
            }
        }
    }
}