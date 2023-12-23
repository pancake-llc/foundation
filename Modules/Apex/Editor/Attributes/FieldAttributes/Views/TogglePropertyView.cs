using Pancake.Apex;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(TogglePropertyAttribute))]
    public sealed class TogglePropertyView : FieldView
    {
        private TogglePropertyAttribute attribute;
        private SerializedField boolValue;

        // Stored gui properties.
        private AnimFloat animWidth;

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
            animWidth = new AnimFloat(0, serializedField.Repaint) {speed = 5.5f};
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

            float totalWidth = position.width;
            position.width = 15;
            EditorGUI.PropertyField(position, boolValue.GetSerializedProperty(), GUIContent.none);
            totalWidth -= position.width + ApexGUIUtility.HorizontalSpacing;

            if (attribute.Hide)
            {
                animWidth.target = boolValue.GetSerializedProperty().boolValue ? totalWidth : 0;

                position.x = position.xMax + ApexGUIUtility.HorizontalSpacing;
                position.width = animWidth.value;
                if (position.width > 5)
                {
                    EditorGUI.PropertyField(position, serializedField.GetSerializedProperty(), GUIContent.none);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(boolValue.GetSerializedProperty().boolValue);
                position.x = position.xMax + ApexGUIUtility.HorizontalSpacing;
                position.width = totalWidth;
                EditorGUI.PropertyField(position, serializedField.GetSerializedProperty(), GUIContent.none);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}