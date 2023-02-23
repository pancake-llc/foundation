using System;
using JetBrains.Annotations;

namespace Pancake.AttributeDrawer
{
    public abstract class Validator : PropertyExtension
    {
        [PublicAPI]
        public abstract ValidationResult Validate(Property property);
    }

    public abstract class AttributeValidator : Validator
    {
        internal Attribute RawAttribute { get; set; }
    }

    public abstract class AttributeValidator<TAttribute> : AttributeValidator where TAttribute : Attribute
    {
        [PublicAPI] public TAttribute Attribute => (TAttribute) RawAttribute;
    }

    public abstract class ValueValidator : Validator
    {
    }

    public abstract class ValueValidator<T> : ValueValidator
    {
        public sealed override ValidationResult Validate(Property property) { return Validate(new InspectorValue<T>(property)); }

        [PublicAPI]
        public abstract ValidationResult Validate(InspectorValue<T> propertyValue);
    }
}