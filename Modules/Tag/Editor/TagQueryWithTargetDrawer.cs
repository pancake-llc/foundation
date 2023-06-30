using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomPropertyDrawer(typeof(TagStatic.TagQueryWithTarget), true)]
    public class TagQueryWithTargetDrawer : TagPropertyDrawerBase<TagGroupBase, TagQuery>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl)
        {
            EditorGUI.BeginProperty(position, lbl, property);
            bool wideDisplay = position.width > 250f;
            float actionWidth = wideDisplay ? 100f : 60f;
            position.width -= actionWidth;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("query"), new GUIContent("Query"));
            position.x += position.width + 5;
            position.width = actionWidth - 5f;

            var actionProp = property.FindPropertyRelative("target");
            actionProp.enumValueIndex = (int) (TagStatic.Search) EditorGUI.EnumPopup(position, (TagStatic.Search) actionProp.enumValueIndex);

            EditorGUI.EndProperty();
        }
    }
}