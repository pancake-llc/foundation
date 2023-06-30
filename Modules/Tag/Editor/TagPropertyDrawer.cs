using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomPropertyDrawer(typeof(Tag.Tag), true)]
    public class TagPropertyDrawer : TagPropertyDrawerBase<ScriptableTagGroup, Tag.Tag>
    {
        public TagPropertyDrawer()
        {
            label = "Tag";
            defaultLabel = "- Untagged -";
            showHelpIcon = false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}