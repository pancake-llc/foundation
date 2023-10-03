using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal class ShowIfPropertyDrawer : PropertyDrawer
    {
        private bool _showField = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (ShowIfAttribute) this.attribute;
            var conditionField = property.serializedObject.FindProperty(att.conditionFieldName);

            if (conditionField == null)
            {
                ShowError(position, label, "Field " + att.conditionFieldName + " does not exist.");
                return;
            }

            switch (conditionField.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    try
                    {
                        var comparisionValue = att.comparisonValue == null || (bool) att.comparisonValue;
                        _showField = conditionField.boolValue == comparisionValue;
                    }
                    catch
                    {
                        ShowError(position, label, "Invalid comparison Value Type");
                        return;
                    }

                    break;
                default:
                    ShowError(position, label, att.conditionFieldName + " is not a bool. Invalid Type.");
                    return;
            }

            if (_showField)
            {
                EditorGUI.indentLevel++;
                var color = GUI.backgroundColor;
                GUI.backgroundColor = Uniform.FieryRose;
                EditorGUI.PropertyField(position, property, label, true);
                EditorGUI.indentLevel--;
                GUI.backgroundColor = color;
            }
        }

        private void ShowError(Rect position, GUIContent label, string errorText)
        {
            EditorGUI.LabelField(position, label, new GUIContent(errorText));
            _showField = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return _showField ? EditorGUI.GetPropertyHeight(property, label) : 0f; }
    }
}