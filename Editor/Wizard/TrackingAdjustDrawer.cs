using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class TrackingAdjustDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADJUST
            Uniform.DrawInstalled("4.33.0");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(" Attach component Adjust into GameObject in Initial Scene" +
                                    "\n Adjust was mark as Dont DestroyOnload and all setting was already config" + "\n All you need to do is enter the token id",
                MessageType.Info);

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Adjust Package", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Adjust", "Are you sure you want to uninstall adjust package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.pancake.adjust");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
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