using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEditor;
using UnityEngine;

[assembly: RegisterAttributeDrawer(typeof(LabelWidthDrawer), DrawerOrder.Decorator)]

namespace PancakeEditor.Attribute
{
    public class LabelWidthDrawer : AttributeDrawer<LabelWidthAttribute>
    {
        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = Attribute.Width;
            next.OnGUI(position);
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}