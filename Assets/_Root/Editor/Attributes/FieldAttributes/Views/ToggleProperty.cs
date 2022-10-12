using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(TogglePropertyAttribute))]
    sealed class TogglePropertyView : FieldView
    {
        private TogglePropertyAttribute attribute;
        private SerializedField boolValue;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as TogglePropertyAttribute;
            boolValue = new SerializedField(element.serializedObject, attribute.boolValue);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            if (attribute.Hide)
            {
                if (boolValue.serializedProperty.boolValue)
                {
                    position.width -= 17;
                    EditorGUI.PropertyField(position, element.serializedProperty, label);
                    Rect boolValuePosition = new Rect(position.xMax + 2, position.y, 15, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(boolValuePosition, boolValue.serializedProperty, GUIContent.none);
                }
                else
                {
                    boolValue.OnGUI(position);
                }
            }
            else
            {
                position.width -= 17;
                EditorGUI.BeginDisabledGroup(boolValue.serializedProperty.boolValue);
                EditorGUI.PropertyField(position, element.serializedProperty, label);
                EditorGUI.EndDisabledGroup();
                Rect boolValuePosition = new Rect(position.xMax + 2, position.y, 15, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(boolValuePosition, boolValue.serializedProperty, GUIContent.none);
            }
        }
    }
}