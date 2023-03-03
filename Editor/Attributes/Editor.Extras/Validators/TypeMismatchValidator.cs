using PancakeEditor.Attribute;
using Pancake.Attribute;
using UnityEditor;

[assembly: RegisterValueValidator(typeof(TypeMismatchValidator<>))]

namespace PancakeEditor.Attribute
{
    public class TypeMismatchValidator<T> : ValueValidator<T> where T : UnityEngine.Object
    {
        public override ValidationResult Validate(InspectorValue<T> propertyValue)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out var serializedProperty) &&
                serializedProperty.propertyType == SerializedPropertyType.ObjectReference && serializedProperty.objectReferenceValue != null &&
                (serializedProperty.objectReferenceValue is T) == false)
            {
                var displayName = propertyValue.Property.DisplayName;
                var actual = serializedProperty.objectReferenceValue.GetType().Name;
                var expected = propertyValue.Property.FieldType.Name;
                var msg = $"{displayName} does not match the type: actual = {actual}, expected = {expected}";
                return ValidationResult.Warning(msg);
            }

            return ValidationResult.Valid;
        }
    }
}