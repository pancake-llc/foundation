using UnityEditor;
using UnityEngine;
using System;

namespace Pancake.Editor
{
    [ViewTarget(typeof(SliderAttribute))]
    sealed class SliderView : FieldView, ITypeValidationCallback
    {
        private SliderAttribute attribute;
        private SerializedProperty minProperty;
        private SerializedProperty maxProperty;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as SliderAttribute;
            minProperty = element.serializedObject.FindProperty(attribute.minProperty);
            maxProperty = element.serializedObject.FindProperty(attribute.maxProperty);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
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

            switch (element.serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    element.serializedProperty.intValue = EditorGUI.IntSlider(position,
                        label,
                        element.serializedProperty.intValue,
                        Convert.ToInt32(min),
                        Convert.ToInt32(max));
                    break;
                case SerializedPropertyType.Float:
                    element.serializedProperty.floatValue = EditorGUI.Slider(position,
                        label,
                        element.serializedProperty.floatValue,
                        min,
                        max);
                    break;
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return EditorGUIUtility.singleLineHeight; }

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