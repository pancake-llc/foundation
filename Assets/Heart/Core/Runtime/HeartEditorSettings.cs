#if UNITY_EDITOR
using UnityEngine;

namespace Pancake
{
    [EditorIcon("so_dark_setting")]
    public class HeartEditorSettings : ScriptableSettings<HeartEditorSettings>
    {
        [Header("Editor")] [Tooltip("Indicates whether you can immediately edit the name asset upon creation?")] [SerializeField]
        private ECreationMode nameCreationMode;

        [SerializeField] private ETargetFrameRate targetFrameRate = ETargetFrameRate.ByDevice;


        public static ECreationMode EditorNameCreationMode => Instance.nameCreationMode;
        public static ETargetFrameRate TargetFrameRate => Instance.targetFrameRate;
    }
}
#endif