using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesNotificationDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_NOTIFICATION
            Uniform.DrawInstalled("1.0.1");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("See Wiki", GUILayout.MaxHeight(40f)))
            {
                Application.OpenURL("https://github.com/pancake-llc/notification/wiki");
            }

            EditorGUILayout.Space();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Notification", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Notification", "Are you sure you want to uninstall notification package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.pancake.notification");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Local Notification", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.pancake.notification", "https://github.com/pancake-llc/notification.git#1.0.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}