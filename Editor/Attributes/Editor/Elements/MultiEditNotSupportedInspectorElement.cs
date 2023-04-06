using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Attribute
{
    public class MultiEditNotSupportedInspectorElement : InspectorElement
    {
        private readonly Property _property;
        private readonly GUIContent _message;

        public MultiEditNotSupportedInspectorElement(Property property)
        {
            _property = property;
            _message = new GUIContent("Multi edit not supported");
        }

        public override float GetHeight(float width) { return EditorGUIUtility.singleLineHeight; }

        public override void OnGUI(Rect position) { EditorGUI.LabelField(position, _property.DisplayNameContent, _message); }
    }
}