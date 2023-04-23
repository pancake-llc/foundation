using Pancake;
using Pancake.Attribute;
using UnityEngine;

namespace PancakeEditor
{
    [HideMono]
    [EditorIcon("scriptable_setting")]
    public class ScriptableEditorSetting : ScriptableSettings<ScriptableEditorSetting>
    {
        [Tooltip("Default: displays all the parameters of variables. Minimal : only displays the value.")] [SerializeField]
        private EVariableDrawMode drawMode = EVariableDrawMode.Default;

        public static EVariableDrawMode DrawMode => Instance.drawMode;
    }

    public enum EVariableDrawMode
    {
        Default,
        Minimal
    }
}