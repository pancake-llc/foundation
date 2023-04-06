using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEditor;
using UnityEngine;

[assembly: RegisterAttributeValidator(typeof(AssetsOnlyValidator))]

namespace PancakeEditor.Attribute
{
    public class AssetsOnlyValidator : AttributeValidator<AssetsOnlyAttribute>
    {
        public override ExtensionInitializationResult Initialize(PropertyDefinition propertyDefinition)
        {
            if (!typeof(Object).IsAssignableFrom(propertyDefinition.FieldType))
            {
                return "AssetsOnly attribute can be used only on Object fields";
            }

            return ExtensionInitializationResult.Ok;
        }

        public override ValidationResult Validate(Property property)
        {
            var obj = property.TryGetSerializedProperty(out var serializedProperty) ? serializedProperty.objectReferenceValue : (Object) property.Value;

            if (obj == null || AssetDatabase.Contains(obj))
            {
                return ValidationResult.Valid;
            }

            return ValidationResult.Error($"{obj} is not as asset.");
        }
    }
}