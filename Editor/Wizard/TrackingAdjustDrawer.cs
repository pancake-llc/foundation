using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class TrackingAdjustDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADJUST
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Installed");
            var lastRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(lastRect.x + 55, lastRect.y, 10, lastRect.height);
            GUI.Label(iconRect, Uniform.IconContent("CollabNew"), Uniform.InstalledIcon);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(" Attach component Adjust into GameObject in Initial Scene" +
                                    "\n Adjust was mark as Dont DestroyOnload and all setting was already config" +
                                    "\n All you need to do is enter the token id", MessageType.Info);
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Adjust Package", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.pancake.adjust", "https://github.com/pancake-llc/adjust.git#4.33.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}