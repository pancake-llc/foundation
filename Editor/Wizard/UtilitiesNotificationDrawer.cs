using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesNotificationDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_NOTIFICATION
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Installed");
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + 55, buttonRect.y, 10, buttonRect.height);
            GUI.Label(iconRect, Uniform.IconContent("CollabNew"), Uniform.InstalledIcon);
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("See Wiki", GUILayout.MaxHeight(40f)))
            {
                Application.OpenURL("https://github.com/pancake-llc/notification/wiki");
            }
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Local Notification", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.pancake.notification", "https://github.com/pancake-llc/notification.git#1.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}