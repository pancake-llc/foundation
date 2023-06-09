using Pancake.Apex;
using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ValidatorTarget(typeof(MaxValueAttribute))]
    public sealed class MaxValueValidator : FieldValidator, ITypeValidationCallback
    {
        private MaxValueAttribute attribute;
        private SerializedProperty property;
        private SerializedProperty minProperty;

        /// <summary>
        /// Called once when initializing validator.
        /// </summary>
        /// <param name="serializedField">Serialized field with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            attribute = validatorAttribute as MaxValueAttribute;

            property = serializedField.GetSerializedProperty();

            string relativePath = string.Empty;
            string[] paths = property.propertyPath.Split('.');
            if (paths.Length > 0)
            {
                Array.Resize<string>(ref paths, paths.Length - 1);
                relativePath = string.Join('.', paths);
            }

            if (!string.IsNullOrEmpty(attribute.property))
            {
                string path = relativePath == string.Empty ? attribute.property : $"{relativePath}.{attribute.property}";
                minProperty = serializedField.GetSerializedObject().FindProperty(path);
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
                case SerializedPropertyType.Integer:
                    if (minProperty != null)
                        property.intValue = Mathf.Min(property.intValue, minProperty.intValue);
                    else
                        property.intValue = Mathf.Min(property.intValue, System.Convert.ToInt32(attribute.value));
                    break;
                case SerializedPropertyType.Float:
                    if (minProperty != null)
                        property.floatValue = Mathf.Min(property.floatValue, minProperty.floatValue);
                    else
                        property.floatValue = Mathf.Min(property.floatValue, attribute.value);
                    break;
                case SerializedPropertyType.Vector2:
                    if (minProperty != null)
                    {
                        Vector2 vector = property.vector2Value;

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.floatValue);
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.floatValue);
                        }

                        property.vector2Value = vector;
                    }
                    else
                    {
                        Vector2 vector = property.vector2Value;
                        vector.x = Mathf.Min(vector.x, attribute.value);
                        vector.y = Mathf.Min(vector.y, attribute.value);
                        property.vector2Value = vector;
                    }

                    break;
                case SerializedPropertyType.Vector2Int:
                    if (minProperty != null)
                    {
                        Vector2Int vector = property.vector2IntValue;
                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.x = Mathf.Min(vector.x, System.Convert.ToInt32(minProperty.floatValue));
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.y = Mathf.Min(vector.y, System.Convert.ToInt32(minProperty.floatValue));
                        }

                        property.vector2IntValue = vector;
                    }
                    else
                    {
                        Vector2Int vector = property.vector2IntValue;
                        vector.x = Mathf.Min(vector.x, System.Convert.ToInt32(attribute.value));
                        vector.y = Mathf.Min(vector.y, System.Convert.ToInt32(attribute.value));
                        property.vector2IntValue = vector;
                    }

                    break;

                case SerializedPropertyType.Vector3:
                    if (minProperty != null)
                    {
                        Vector3 vector = property.vector3Value;

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.floatValue);
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.floatValue);
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.z = Mathf.Min(vector.z, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.z = Mathf.Min(vector.z, minProperty.floatValue);
                        }

                        property.vector3Value = vector;
                    }
                    else
                    {
                        Vector3 vector = property.vector3Value;
                        vector.x = Mathf.Min(vector.x, attribute.value);
                        vector.y = Mathf.Min(vector.y, attribute.value);
                        vector.z = Mathf.Min(vector.z, attribute.value);
                        property.vector3Value = vector;
                    }

                    break;
                case SerializedPropertyType.Vector3Int:
                    if (minProperty != null)
                    {
                        Vector3Int vector = property.vector3IntValue;
                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.x = Mathf.Min(vector.x, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.x = Mathf.Min(vector.x, System.Convert.ToInt32(minProperty.floatValue));
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.y = Mathf.Min(vector.y, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.y = Mathf.Min(vector.y, System.Convert.ToInt32(minProperty.floatValue));
                        }

                        if (property.propertyType == SerializedPropertyType.Integer)
                        {
                            vector.z = Mathf.Min(vector.z, minProperty.intValue);
                        }
                        else if (property.propertyType == SerializedPropertyType.Float)
                        {
                            vector.z = Mathf.Min(vector.z, System.Convert.ToInt32(minProperty.floatValue));
                        }

                        property.vector3IntValue = vector;
                    }
                    else
                    {
                        Vector3Int vector = property.vector3IntValue;
                        vector.x = Mathf.Min(vector.x, System.Convert.ToInt32(attribute.value));
                        vector.y = Mathf.Min(vector.y, System.Convert.ToInt32(attribute.value));
                        vector.z = Mathf.Min(vector.z, System.Convert.ToInt32(attribute.value));
                        property.vector3IntValue = vector;
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
            return property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float ||
                   property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int ||
                   property.propertyType == SerializedPropertyType.Vector3 || property.propertyType == SerializedPropertyType.Vector3Int;
        }
    }
}