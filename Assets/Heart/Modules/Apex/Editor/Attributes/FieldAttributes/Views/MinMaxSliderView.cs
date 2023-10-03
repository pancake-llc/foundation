using Pancake.Apex;
using System;
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
                OnVector2GUI(ref position, serializedProperty);
            }
            else if (serializedProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                OnVector2IntGUI(ref position, serializedProperty);
            }
        }

        private void OnVector2GUI(ref Rect position, in SerializedProperty property)
        {
            float totalWidth = position.width;
            Vector2 vector = property.vector2Value;

            position.width = EditorGUIUtility.fieldWidth;
            vector.x = EditorGUI.FloatField(position, vector.x);
            if (vector.x < attribute.min)
                vector.x = attribute.min;
            else if (vector.x > vector.y)
                vector.x = vector.y;
            totalWidth -= position.width;

            position.x = position.xMax + WIDTH;
            position.width = totalWidth - position.width - (WIDTH * 2);
            EditorGUI.MinMaxSlider(position,
                ref vector.x,
                ref vector.y,
                attribute.min,
                attribute.max);

            position.x = position.xMax + WIDTH;
            position.width = EditorGUIUtility.fieldWidth;
            vector.y = EditorGUI.FloatField(position, vector.y);
            if (vector.y > attribute.max)
                vector.y = attribute.max;
            else if (vector.y < vector.x)
                vector.y = vector.x;

            property.vector2Value = vector;
        }

        private void OnVector2IntGUI(ref Rect position, in SerializedProperty property)
        {
            float totalWidth = position.width;
            int min = Convert.ToInt32(attribute.min);
            int max = Convert.ToInt32(attribute.max);
            Vector2Int vectorInt = property.vector2IntValue;

            position.width = EditorGUIUtility.fieldWidth;
            vectorInt.x = EditorGUI.IntField(position, vectorInt.x);
            if (vectorInt.x < min)
                vectorInt.x = min;
            else if (vectorInt.x > vectorInt.y)
                vectorInt.x = vectorInt.y;
            totalWidth -= position.width;

            position.x = position.xMax + WIDTH;
            position.width = totalWidth - position.width - (WIDTH * 2);
            float xInt = vectorInt.x;
            float yInt = vectorInt.y;
            EditorGUI.MinMaxSlider(position,
                ref xInt,
                ref yInt,
                min,
                max);
            vectorInt.x = Convert.ToInt32(xInt);
            vectorInt.y = Convert.ToInt32(yInt);

            position.x = position.xMax + WIDTH;
            position.width = EditorGUIUtility.fieldWidth;
            vectorInt.y = EditorGUI.IntField(position, vectorInt.y);
            if (vectorInt.y > max)
                vectorInt.y = max;
            else if (vectorInt.y < vectorInt.x)
                vectorInt.y = vectorInt.x;

            property.vector2IntValue = vectorInt;
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