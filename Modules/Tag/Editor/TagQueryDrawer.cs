using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomPropertyDrawer(typeof(TagQuery), true)]
    public class TagQueryDrawer : BTagPropertyDrawerBase<BTagGroupBase, TagQuery>
    {
        public TagQueryDrawer() { defaultLabel = "- None -"; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}