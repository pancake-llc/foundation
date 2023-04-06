using UnityEngine;

namespace PancakeEditor.Attribute
{
    public class LabelInspectorElement : InspectorElement
    {
        private readonly GUIContent _label;

        public LabelInspectorElement(string label) { _label = new GUIContent(label); }

        public override float GetHeight(float width) { return GUI.skin.label.CalcHeight(_label, width); }

        public override void OnGUI(Rect position) { GUI.Label(position, _label); }
    }
}