using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomPropertyDrawer(typeof(ScriptableBTag), true)]
    public class ScriptableBTagDrawer : BTagPropertyDrawerBase<BTagGroup<ScriptableBTag>, ScriptableBTag>
    {
        public ScriptableBTagDrawer()
        {
            label = "Asset";
            defaultLabel = "- None -";
            showHelpIcon = false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent lbl) { base.OnGUI(position, property, lbl); }
    }
}