using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    [CustomPropertyDrawer(typeof(VariableReference<,>),true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        private readonly string[] _popupOptions =
            { "Use Local", "Use Variable" };

        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_popupStyle == null)
            {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            var useLocal = property.FindPropertyRelative("UseLocal");
            var localValue = property.FindPropertyRelative("LocalValue");
            var variable = property.FindPropertyRelative("Variable");

            Rect buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax + 15;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var result = EditorGUI.Popup(buttonRect, useLocal.boolValue ? 0 : 1, _popupOptions, _popupStyle);

            useLocal.boolValue = result == 0;

            EditorGUI.PropertyField(position,
                useLocal.boolValue ? localValue : variable,
                GUIContent.none);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}