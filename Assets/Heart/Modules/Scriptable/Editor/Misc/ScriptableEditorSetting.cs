using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [EditorIcon("scriptable_editor_setting")]
    public class ScriptableEditorSetting : ScriptableSettings<ScriptableEditorSetting>
    {
        internal const float BUTTON_HEIGHT = 30f;

        [SerializeField] private EVariableDrawMode drawMode = EVariableDrawMode.Default;
        [SerializeField] private List<string> categories = new List<string> {"Default"};

        public static EVariableDrawMode DrawMode => Instance.drawMode;
        public static List<string> Categories => Instance.categories;
    }

    public enum EVariableDrawMode
    {
        Default,
        Minimal
    }
}