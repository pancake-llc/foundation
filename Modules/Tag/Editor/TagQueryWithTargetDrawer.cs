using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomPropertyDrawer(typeof(BTag.BTag.TagQueryWithTarget), true)]
    public class TagQueryWithTargetDrawer : BTagPropertyDrawerBase<BTagGroupBase, TagQuery>
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
            actionProp.enumValueIndex = (int) (BTag.BTag.Search) EditorGUI.EnumPopup(position, (BTag.BTag.Search) actionProp.enumValueIndex);

            EditorGUI.EndProperty();
        }
    }
}