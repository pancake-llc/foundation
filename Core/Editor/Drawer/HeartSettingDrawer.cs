using Pancake;
using Pancake.ExLibEditor;
using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(HeartSettings), true)]
    public class HeartSettingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            string[] propertiesToHide = HeartSettings.EnablePrivacyFirstOpen ? new[] {"m_Script"} : new[] {"m_Script", "privacyUrl", "privacyTitle", "privacyMessage"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);
        }
    }
}