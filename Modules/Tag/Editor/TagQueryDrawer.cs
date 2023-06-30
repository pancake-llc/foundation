using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomPropertyDrawer(typeof(TagQuery), true)]
    public class TagQueryDrawer : TagPropertyDrawerBase<TagGroupBase, TagQuery>
    {
        public TagQueryDrawer() { defaultLabel = "- None -"; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}