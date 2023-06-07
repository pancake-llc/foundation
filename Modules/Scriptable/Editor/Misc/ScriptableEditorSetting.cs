using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [EditorIcon("scriptable_editor_setting")]
    public class ScriptableEditorSetting : ScriptableSettings<ScriptableEditorSetting>
    {
        [SerializeField] private EVariableDrawMode drawMode = EVariableDrawMode.Default;

        public static EVariableDrawMode DrawMode => Instance.drawMode;
    }

    public enum EVariableDrawMode
    {
        Default,
        Minimal
    }
}