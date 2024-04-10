using Pancake;
using PancakeEditor.Common;

using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(HeartSettings), true)]
    public class HeartSettingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (HeartSettings.DebugView) EditorGUILayout.HelpBox("Swipe vertically on the side of the screen to display the debug menu", MessageType.Info);
            string[] propertiesToHide = HeartSettings.EnablePrivacyFirstOpen ? new[] {"m_Script"} : new[] {"m_Script", "privacyUrl", "privacyTitle", "privacyMessage"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);
        }
    }
}