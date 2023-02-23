using Pancake.AttributeDrawer;

[assembly: RegisterAttributeValidator(typeof(RequiredValidator), ApplyOnArrayElement = true)]

namespace Pancake.AttributeDrawer
{
    public class RequiredValidator : AttributeValidator<RequiredAttribute>
    {
        public override ValidationResult Validate(Property property)
        {
            if (property.FieldType == typeof(string))
            {
                var isNull = string.IsNullOrEmpty((string) property.Value);
                if (isNull)
                {
                    var message = Attribute.Message ?? $"{GetName(property)} is required";
                    return ValidationResult.Error(message);
                }
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(property.FieldType))
            {
                var isNull = null == (UnityEngine.Object) property.Value;
                if (isNull)
                {
                    var message = Attribute.Message ?? $"{GetName(property)} is required";
                    return ValidationResult.Error(message);
                }
            }
            else
            {
                return ValidationResult.Error("RequiredAttribute only valid on Object and String");
            }

            return ValidationResult.Valid;
        }

        private static string GetName(Property property)
        {
            var name = property.DisplayName;
            if (string.IsNullOrEmpty(name))
            {
                name = property.RawName;
            }

            return name;
        }
    }
}