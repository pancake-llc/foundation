using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesNotificationDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_NOTIFICATION
            Uniform.DrawInstalled("2.2.2");
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Open Mobile Unity Notification Setting", GUILayout.MaxHeight(40f)))
            {
                UnityEditor.SettingsService.OpenProjectSettings("Project/Mobile Notifications");
            }
            
            if (GUILayout.Button("See Wiki", GUILayout.MaxHeight(40f)))
            {
                Application.OpenURL("https://github.com/pancake-llc/heart/wiki/notification");
            }

            EditorGUILayout.Space();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Notification", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Notification", "Are you sure you want to uninstall notification package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.mobile.notifications");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Local Notification", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.mobile.notifications", "2.2.2");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}