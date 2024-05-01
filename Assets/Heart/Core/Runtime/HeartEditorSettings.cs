using UnityEngine;

namespace Pancake
{
#if UNITY_EDITOR
    [EditorIcon("so_gray_setting")]
    public class HeartEditorSettings : ScriptableSettings<HeartEditorSettings>
    {
        [Header("Editor")] [Tooltip("Indicates whether you can immediately edit the name asset upon creation?")] [SerializeField]
        private ECreationMode nameCreationMode;


        public static ECreationMode EditorNameCreationMode => Instance.nameCreationMode;
    }
#endif
}