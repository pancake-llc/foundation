using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomPropertyDrawer(typeof(Tag), true)]
    public class BTagPropertyDrawer : BTagPropertyDrawerBase<TagGroup, Tag>
    {
        public BTagPropertyDrawer()
        {
            label = "Tag";
            defaultLabel = "- Untagged -";
            showHelpIcon = false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}