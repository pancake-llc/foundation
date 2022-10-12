using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(MinMaxSliderAttribute))]
    sealed class MinMaxSliderView : FieldView, ITypeValidationCallback
    {
        private MinMaxSliderAttribute attribute;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label) { attribute = viewAttribute as MinMaxSliderAttribute; }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            Rect[] splitRect = HorizontalContainer.SplitIndentRectangle(position, 3);

            int padding = (int) splitRect[0].width - 41 - (EditorGUI.indentLevel * 17);
            int space = 3;

            splitRect[0].width -= padding + space;
            splitRect[2].width -= padding + space;
            splitRect[1].x -= padding;
            splitRect[1].width += padding * 2;
            splitRect[2].x += padding + space;

            switch (element.serializedProperty.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    Vector2 vector = element.serializedProperty.vector2Value;
                    vector.x = EditorGUI.FloatField(splitRect[0], vector.x);
                    if (vector.x < attribute.min)
                        vector.x = attribute.min;
                    else if (vector.x > vector.y)
                        vector.x = vector.y;

                    vector.y = EditorGUI.FloatField(splitRect[2], vector.y);
                    if (vector.y > attribute.max)
                        vector.y = attribute.max;
                    else if (vector.y < vector.x)
                        vector.y = vector.x;

                    EditorGUI.MinMaxSlider(splitRect[1],
                        ref vector.x,
                        ref vector.y,
                        attribute.min,
                        attribute.max);

                    element.serializedProperty.vector2Value = vector;
                    break;

                case SerializedPropertyType.Vector2Int:
                    int min = Convert.ToInt32(attribute.min);
                    int max = Convert.ToInt32(attribute.max);

                    Vector2Int vectorInt = element.serializedProperty.vector2IntValue;
                    vectorInt.x = EditorGUI.IntField(splitRect[0], vectorInt.x);
                    if (vectorInt.x < min)
                        vectorInt.x = min;
                    else if (vectorInt.x > vectorInt.y)
                        vectorInt.x = vectorInt.y;

                    vectorInt.y = EditorGUI.IntField(splitRect[2], vectorInt.y);
                    if (vectorInt.y > max)
                        vectorInt.y = max;
                    else if (vectorInt.y < vectorInt.x)
                        vectorInt.y = vectorInt.x;

                    float xInt = vectorInt.x;
                    float yInt = vectorInt.y;
                    EditorGUI.MinMaxSlider(splitRect[1],
                        ref xInt,
                        ref yInt,
                        min,
                        max);
                    vectorInt.x = Convert.ToInt32(xInt);
                    vectorInt.y = Convert.ToInt32(yInt);

                    element.serializedProperty.vector2IntValue = vectorInt;
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
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int;
        }
    }
}