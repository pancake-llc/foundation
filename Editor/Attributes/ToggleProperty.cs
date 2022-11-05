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
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as TogglePropertyAttribute;
            boolValue = new SerializedField(serializedField.GetSerializedObject(), attribute.boolValue);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (attribute.Hide)
            {
                if (boolValue.GetSerializedProperty().boolValue)
                {
                    position.width -= 17;
                    EditorGUI.PropertyField(position, serializedField.GetSerializedProperty(), label);
                    Rect boolValuePosition = new Rect(position.xMax + 2, position.y, 15, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(boolValuePosition, boolValue.GetSerializedProperty(), GUIContent.none);
                }
                else
                {
                    boolValue.OnGUI(position);
                }
            }
            else
            {
                position.width -= 17;
                EditorGUI.BeginDisabledGroup(boolValue.GetSerializedProperty().boolValue);
                EditorGUI.PropertyField(position, serializedField.GetSerializedProperty(), label);
                EditorGUI.EndDisabledGroup();
                Rect boolValuePosition = new Rect(position.xMax + 2, position.y, 15, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(boolValuePosition, boolValue.GetSerializedProperty(), GUIContent.none);
            }
        }
    }
}