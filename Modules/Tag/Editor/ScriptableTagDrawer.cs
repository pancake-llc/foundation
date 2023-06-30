using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomPropertyDrawer(typeof(ScriptableTag), true)]
    public class ScriptableTagDrawer : TagPropertyDrawerBase<TagGroup<ScriptableTag>, ScriptableTag>
    {
        public ScriptableTagDrawer()
        {
            label = "Asset";
            defaultLabel = "- None -";
            showHelpIcon = false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}