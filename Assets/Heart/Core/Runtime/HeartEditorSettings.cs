using UnityEngine;

namespace Pancake
{
#if UNITY_EDITOR
    [EditorIcon("so_dark_setting")]
    public class HeartEditorSettings : ScriptableSettings<HeartEditorSettings>
    {
        [Header("Editor")] [Tooltip("Indicates whether you can immediately edit the name asset upon creation?")] [SerializeField]
        private ECreationMode nameCreationMode;

        [SerializeField] private TargetFrameRate targetFrameRate = TargetFrameRate.ByDevice;


        public static ECreationMode EditorNameCreationMode => Instance.nameCreationMode;
        public static TargetFrameRate TargetFrameRate => Instance.targetFrameRate;
    }
#endif
}