using Pancake.Apex;
using System;
using PancakeEditor.Common;

using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(MinMaxSliderAttribute))]
    public sealed class MinMaxSliderView : FieldView, ITypeValidationCallback
    {
        private const float WIDTH = 5;

        private MinMaxSliderAttribute attribute;
        private SerializedProperty serializedProperty;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as MinMaxSliderAttribute;
            serializedProperty = serializedField.GetSerializedProperty();
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            if (serializedProperty.propertyType == SerializedPropertyType.Vector2)
            {
                Uniform.DrawSlideVector2(ref position, serializedProperty, attribute.min, attribute.max);
            }
            else if (serializedProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                Uniform.DrawSilderVector2Int(ref position, serializedProperty, attribute.min, attribute.max);
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
            return property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int;
        }
    }
}