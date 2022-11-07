using Pancake.Editor;
using UnityEditor;

[assembly: RegisterValueValidator(typeof(MissingReferenceValidator))]

namespace Pancake.Editor
{
    public class MissingReferenceValidator : ValueValidator<UnityEngine.Object>
    {
        public override ValidationResult Validate(InspectorValue<UnityEngine.Object> propertyValue)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out var serializedProperty) &&
                serializedProperty.propertyType == SerializedPropertyType.ObjectReference && serializedProperty.objectReferenceValue == null &&
                serializedProperty.objectReferenceInstanceIDValue != 0)
            {
                return ValidationResult.Warning($"{GetName(propertyValue.Property)} is missing");
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