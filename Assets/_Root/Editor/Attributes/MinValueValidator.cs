﻿using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ValidatorTarget(typeof(MinValueAttribute))]
    class MinValueValidator : FieldValidator, ITypeValidationCallback
    {
        private MinValueAttribute attribute;
        private SerializedProperty value;

        /// <summary>
        /// Called once when initializing validator.
        /// </summary>
        /// <param name="serializedField">Serialized field with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            attribute = validatorAttribute as MinValueAttribute;
            value = serializedField.GetSerializedObject().FindProperty(attribute.property);
        }

        /// <summary>
        /// Implement this method to make some validation of serialized serialized field.
        /// </summary>
        /// <param name="serializedField">Serialized field with validator attribute.</param>
        public override void Validate(SerializedField serializedField)
        {
            switch (serializedField.GetSerializedProperty().propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (value != null)
                        serializedField.GetSerializedProperty().intValue = Mathf.Max(serializedField.GetSerializedProperty().intValue, value.intValue);
                    else
                        serializedField.GetSerializedProperty().intValue =
                            Mathf.Max(serializedField.GetSerializedProperty().intValue, System.Convert.ToInt32(attribute.value));
                    break;
                case SerializedPropertyType.Float:
                    if (value != null)
                        serializedField.GetSerializedProperty().floatValue = Mathf.Max(serializedField.GetSerializedProperty().floatValue, value.floatValue);
                    else
                        serializedField.GetSerializedProperty().floatValue = Mathf.Max(serializedField.GetSerializedProperty().floatValue, attribute.value);
                    break;
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;
        }
    }
}