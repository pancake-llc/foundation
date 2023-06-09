using Pancake.Apex;
using UnityEditor;
using UnityEngine;
using System;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(SliderAttribute))]
    public sealed class SliderView : FieldView, ITypeValidationCallback
    {
        private SliderAttribute attribute;
        private SerializedProperty minProperty;
        private SerializedProperty maxProperty;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as SliderAttribute;
            minProperty = serializedField.GetSerializedObject().FindProperty(attribute.minProperty);
            maxProperty = serializedField.GetSerializedObject().FindProperty(attribute.maxProperty);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            float min = attribute.minValue;
            if (minProperty != null)
            {
                switch (minProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        min = minProperty.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        min = minProperty.floatValue;
                        break;
                }
            }

            float max = attribute.maxValue;
            if (maxProperty != null)
            {
                switch (maxProperty.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        max = maxProperty.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        max = maxProperty.floatValue;
                        break;
                }
            }

            switch (serializedField.GetSerializedProperty().propertyType)
            {
                case SerializedPropertyType.Integer:
                    serializedField.GetSerializedProperty().intValue = EditorGUI.IntSlider(position,
                        label,
                        serializedField.GetSerializedProperty().intValue,
                        Convert.ToInt32(min),
                        Convert.ToInt32(max));
                    break;
                case SerializedPropertyType.Float:
                    serializedField.GetSerializedProperty().floatValue = EditorGUI.Slider(position,
                        label,
                        serializedField.GetSerializedProperty().floatValue,
                        min,
                        max);
                    break;
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label) { return EditorGUIUtility.singleLineHeight; }

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