using System;
using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ValidatorTarget(typeof(MinVector3Attribute))]
    public sealed class MinVector3Validator : FieldValidator, ITypeValidationCallback
    {
        private SerializedProperty property;
        private MinVector3Attribute attribute;
        private SerializedProperty xProperty;
        private SerializedProperty yProperty;
        private SerializedProperty zProperty;

        /// <summary>
        /// Called once when initializing validator.
        /// </summary>
        /// <param name="serializedField">Serialized field with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            attribute = validatorAttribute as MinVector3Attribute;

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

            if (!string.IsNullOrEmpty(attribute.zProperty))
            {
                string path = relativePath == string.Empty ? attribute.zProperty : $"{relativePath}.{attribute.zProperty}";
                zProperty = serializedField.GetSerializedObject().FindProperty(path);
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
                case SerializedPropertyType.Vector3Int:
                {
                    int x = property.vector3IntValue.x;
                    if (xProperty != null)
                    {
                        x = Mathf.Max(x, xProperty.intValue);
                    }
                    else if (attribute.xProperty != null)
                    {
                        x = Mathf.Max(x, System.Convert.ToInt32(attribute));
                    }

                    int y = property.vector3IntValue.y;
                    if (yProperty != null)
                    {
                        y = Mathf.Max(y, yProperty.intValue);
                    }
                    else if (attribute.yProperty != null)
                    {
                        y = Mathf.Max(y, System.Convert.ToInt32(attribute));
                    }

                    int z = property.vector3IntValue.z;
                    if (zProperty != null)
                    {
                        z = Mathf.Max(z, zProperty.intValue);
                    }
                    else if (attribute.zProperty != null)
                    {
                        z = Mathf.Max(z, System.Convert.ToInt32(attribute.z));
                    }

                    property.vector3IntValue = new Vector3Int(x, y, z);
                }
                    break;
                case SerializedPropertyType.Vector3:
                {
                    float x = property.vector3Value.x;
                    if (xProperty != null)
                    {
                        x = Mathf.Max(x, xProperty.floatValue);
                    }
                    else if (attribute.xProperty != null)
                    {
                        x = Mathf.Max(x, attribute.x);
                    }

                    float y = property.vector3Value.y;
                    if (yProperty != null)
                    {
                        y = Mathf.Max(y, yProperty.floatValue);
                    }
                    else if (attribute.yProperty != null)
                    {
                        y = Mathf.Max(y, attribute.y);
                    }

                    float z = property.vector3Value.z;
                    if (zProperty != null)
                    {
                        z = Mathf.Max(z, zProperty.floatValue);
                    }
                    else if (attribute.zProperty != null)
                    {
                        z = Mathf.Max(z, attribute.z);
                    }

                    property.vector3Value = new Vector3(x, y, z);
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
            return property.propertyType == SerializedPropertyType.Vector3 || property.propertyType == SerializedPropertyType.Vector3Int;
        }
    }
}