using Pancake.Attribute;
using JetBrains.Annotations;

namespace PancakeEditor.Attribute
{
    public abstract class Validator : PropertyExtension
    {
        [PublicAPI]
        public abstract ValidationResult Validate(Property property);
    }

    public abstract class AttributeValidator : Validator
    {
        internal System.Attribute RawAttribute { get; set; }
    }

    public abstract class AttributeValidator<TAttribute> : AttributeValidator where TAttribute : System.Attribute
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