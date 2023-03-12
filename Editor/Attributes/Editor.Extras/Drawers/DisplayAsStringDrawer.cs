using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEditor;
using UnityEngine;

[assembly: RegisterAttributeDrawer(typeof(DisplayAsStringDrawer), DrawerOrder.Decorator, ApplyOnArrayElement = true)]

namespace PancakeEditor.Attribute
{
    public class DisplayAsStringDrawer : AttributeDrawer<DisplayAsStringAttribute>
    {
        public override float GetHeight(float width, Property property, InspectorElement next) { return EditorGUIUtility.singleLineHeight; }

        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var value = property.Value;
            var text = value != null ? value.ToString() : "Null";

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            position = EditorGUI.PrefixLabel(position, controlId, property.DisplayNameContent);
            GUI.Label(position, text);
        }
    }
}