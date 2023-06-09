using System;
using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ValidatorTarget(typeof(MaxVector2Attribute))]
    public sealed class MaxVector2Validator : FieldValidator, ITypeValidationCallback
    {
        private MaxVector2Attribute attribute;
        private SerializedProperty property;
        private SerializedProperty xProperty;
        private SerializedProperty yProperty;

        /// <summary>
        /// Called once when initializing validator.
        /// </summary>
        /// <param name="serializedField">Serialized field with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            attribute = validatorAttribute as MaxVector2Attribute;

            property = serializedField.GetSerializedProperty();

            string relativePath = string.Empty;
            string[] paths = property.propertyPath.Split('.');
            if (paths.Length > 0)
            {
                Array.Resize<string>(ref paths, paths.Length - 1);
                relativePath = string.Join('.', paths);
            }

            if (!string.IsNullOrEmpty(attribute.xProperty))
            {
                string path = relativePath == string.Empty ? attribute.xProperty : $"{relativePath}.{attribute.xProperty}";
                xProperty = serializedField.GetSerializedObject().FindProperty(path);
            }

            if (!string.IsNullOrEmpty(attribute.yProperty))
            {
                string path = relativePath == string.Empty ? attribute.yProperty : $"{relativePath}.{attribute.yProperty}";
                yProperty = serializedField.GetSerializedObject().FindProperty(path);
            }
        }

        /// <summary>
        /// Implement this method to make some validation of serialized serialized field.
        /// </summary>
        /// <param name="serializedField">Serialized field with validator attribute.</param>
        public override void Validate(SerializedField serializedField)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2Int:
                {
                    int x = property.vector2IntValue.x;
                    if (xProperty != null)
                    {
                        x = Mathf.Min(x, xProperty.intValue);
                    }
                    else if (attribute.xProperty != null)
                    {
                        x = Mathf.Min(x, System.Convert.ToInt32(attribute.x));
                    }

                    int y = property.vector2IntValue.y;
                    if (yProperty != null)
                    {
                        y = Mathf.Min(y, yProperty.intValue);
                    }
                    else if (attribute.yProperty != null)
                    {
                        y = Mathf.Min(y, System.Convert.ToInt32(attribute.y));
                    }

                    property.vector2IntValue = new Vector2Int(x, y);
                }
                    break;
                case SerializedPropertyType.Vector2:
                {
                    float x = property.vector2Value.x;
                    if (xProperty != null)
                    {
                        x = Mathf.Min(x, xProperty.floatValue);
                    }
                    else if (attribute.xProperty != null)
                    {
                        x = Mathf.Min(x, attribute.x);
                    }

                    float y = property.vector2Value.y;
                    if (yProperty != null)
                    {
                        y = Mathf.Min(y, yProperty.floatValue);
                    }
                    else if (attribute.yProperty != null)
                    {
                        y = Mathf.Min(y, attribute.y);
                    }

                    property.vector2Value = new Vector2(x, y);
                }
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
            return property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int;
        }
    }
}