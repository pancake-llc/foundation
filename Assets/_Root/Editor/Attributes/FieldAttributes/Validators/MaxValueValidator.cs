using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ValidatorTarget(typeof(MaxValueAttribute))]
    class MaxValueValidator : FieldValidator, ITypeValidationCallback
    {
        private MaxValueAttribute attribute;
        private SerializedProperty value;

        /// <summary>
        /// Called once when initializing element validator.
        /// </summary>
        /// <param name="property">Serialized element with ValidatorAttribute.</param>
        /// <param name="attribute">ValidatorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public override void Initialize(SerializedField element, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            attribute = validatorAttribute as MaxValueAttribute;
            value = element.serializedObject.FindProperty(attribute.property);
        }

        /// <summary>
        /// Implement this method to make some validation of serialized element.
        /// </summary>
        /// <param name="element">Serialized element with validator attribute.</param>
        public override void Validate(SerializedField element)
        {
            switch (element.serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (value != null)
                        element.serializedProperty.intValue = Mathf.Min(element.serializedProperty.intValue, value.intValue);
                    else
                        element.serializedProperty.intValue = Mathf.Min(element.serializedProperty.intValue, System.Convert.ToInt32(attribute.value));
                    break;
                case SerializedPropertyType.Float:
                    if (value != null)
                        element.serializedProperty.floatValue = Mathf.Min(element.serializedProperty.floatValue, value.floatValue);
                    else
                        element.serializedProperty.floatValue = Mathf.Min(element.serializedProperty.floatValue, attribute.value);
                    break;
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;
        }
    }
}