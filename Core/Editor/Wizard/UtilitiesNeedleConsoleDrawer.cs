using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesNeedleConsoleDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_NEEDLE_CONSOLE
            Uniform.DrawInstalled("2.3.17");
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(25f)))
            {
                SettingsService.OpenUserPreferences("Preferences/Needle/Console");
            }
           
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Needle Console", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Needle Console", "Are you sure you want to uninstall needle console package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.needle.console");
                    RegistryManager.Remove("com.pancake.demystifier");
                    RegistryManager.Remove("com.pancake.unsafe");
                    RegistryManager.Remove("com.pancake.immutable");
                    RegistryManager.Remove("com.pancake.reflection.metadata");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Needle Console", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.needle.console", "https://github.com/pancake-llc/needle-console.git?path=package#2.3.17");
                RegistryManager.Add("com.pancake.demystifier", "https://github.com/pancake-llc/ben-demystifier.git#0.4.1");
                RegistryManager.Add("com.pancake.unsafe", "https://github.com/pancake-llc/system-unsafe.git#5.0.0");
                RegistryManager.Add("com.pancake.immutable", "https://github.com/pancake-llc/system-immutable.git#5.0.0");
                RegistryManager.Add("com.pancake.reflection.metadata", "https://github.com/pancake-llc/reflection-metadata.git#5.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}