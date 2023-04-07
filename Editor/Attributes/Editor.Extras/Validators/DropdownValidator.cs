using Pancake.Attribute;
using PancakeEditor.Attribute;

[assembly: RegisterAttributeValidator(typeof(DropdownValidator<>))]

namespace PancakeEditor.Attribute
{
    public class DropdownValidator<T> : AttributeValidator<DropdownAttribute>
    {
        private DropdownValuesResolver<T> _valuesResolver;

        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            _valuesResolver = DropdownValuesResolver<T>.Resolve(propertyDefinition, Attribute.Values);

            if (_valuesResolver.TryGetErrorString(out var error))
            {
                return error;
            }

            return ExtensionInitializationResult.Ok;
        }

        public override ValidationResult Validate(Property property)
        {
            foreach (var item in _valuesResolver.GetDropdownItems(property))
            {
                if (property.Comparer.Equals(item.Value, property.Value))
                {
                    return ValidationResult.Valid;
                }
            }

            var msg = $"Dropdown value '{property.Value}' not valid";

            switch (Attribute.ValidationMessageType)
            {
                case EMessageType.Info:
                    return ValidationResult.Info(msg);

                case EMessageType.Warning:
                    return ValidationResult.Warning(msg);

                case EMessageType.Error:
                    return ValidationResult.Error(msg);
            }

            return ValidationResult.Valid;
        }
    }
}