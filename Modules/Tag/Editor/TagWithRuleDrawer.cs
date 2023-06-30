using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomPropertyDrawer(typeof(TagStatic.TagWithRule), true)]
    public class TagWithRuleDrawer : TagPropertyDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl)
        {
            EditorGUI.BeginProperty(position, lbl, property);

            var origPos = position;
            position.width = 80f;
            var ruleProp = property.FindPropertyRelative("rule");
            ruleProp.enumValueIndex = (int) (InclusionRule) EditorGUI.EnumPopup(position, (InclusionRule) ruleProp.enumValueIndex);

            position.x += position.width + 5;
            position.width = origPos.width - position.width;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("tag"), GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}