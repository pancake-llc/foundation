using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [EditorIcon("scriptable_editor_setting")]
    public class ScriptableEditorSetting : ScriptableSettings<ScriptableEditorSetting>
    {
        internal const float BUTTON_HEIGHT = 30f;

        [SerializeField] private List<string> categories = new() {"Default"};

        public static List<string> Categories => Instance.categories;
    }

    [UnityEditor.CustomEditor(typeof(ScriptableEditorSetting), true)]
    public class ScriptableEditorSettingDrawer : UnityEditor.Editor
    {
    }
}