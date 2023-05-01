using Pancake;
using UnityEngine;

namespace PancakeEditor
{
    //[HideMonoScript]
    [EditorIcon("scriptable_setting")]
    public class ScriptableEditorSetting : ScriptableSettings<ScriptableEditorSetting>
    {
        //[InfoBox("Default: displays all the parameters of variables. Minimal : only displays the value.")]
        [SerializeField]
        private EVariableDrawMode drawMode = EVariableDrawMode.Default;

        public static EVariableDrawMode DrawMode => Instance.drawMode;
    }

    public enum EVariableDrawMode
    {
        Default,
        Minimal
    }
}