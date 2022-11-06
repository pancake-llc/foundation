using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class NoDrawerInspectorElement : InspectorElement
    {
        private readonly GUIContent _message;
        private readonly Property _property;

        public NoDrawerInspectorElement(Property property)
        {
            _property = property;
            _message = new GUIContent($"No drawer for {property.FieldType}");
        }

        public override float GetHeight(float width) { return EditorGUIUtility.singleLineHeight; }

        public override void OnGUI(Rect position) { EditorGUI.LabelField(position, _property.DisplayNameContent, _message); }
    }
}