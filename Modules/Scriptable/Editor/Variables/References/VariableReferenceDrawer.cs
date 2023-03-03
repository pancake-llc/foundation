using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomPropertyDrawer(typeof(VariableReference<,>), true)]
    public class VariableReferenceDrawer : UnityEditor.PropertyDrawer
    {
        private readonly string[] _popupOptions = {"Use Local", "Use Variable"};

        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_popupStyle == null)
            {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) {imagePosition = ImagePosition.ImageOnly};
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            var useLocal = property.FindPropertyRelative("useLocal");
            var localValue = property.FindPropertyRelative("localValue");
            var variable = property.FindPropertyRelative("variable");

            var buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax + 15;

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useLocal.boolValue ? 0 : 1, _popupOptions, _popupStyle);

            useLocal.boolValue = result == 0;

            EditorGUI.PropertyField(position, useLocal.boolValue ? localValue : variable, GUIContent.none);

            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}