using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesSelectiveProfilingDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_SELECTIVE_PROFILING
            Uniform.DrawInstalled("1.0.1-pre.1");
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(25f)))
            {
                SettingsService.OpenProjectSettings("Project/Needle/Selective Profiler");
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Selective Profiling", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Selective Profiling",
                    "Are you sure you want to uninstall selective profiling package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.needle.selective-profiling");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Selective Profiling", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.needle.selective-profiling", "https://github.com/pancake-llc/selective-profiling.git?path=package#1.0.1-pre.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}