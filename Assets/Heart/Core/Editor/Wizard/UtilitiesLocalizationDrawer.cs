using Pancake.ExLib;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesLocalizationDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_LOCALIZATION
        
#endif

#if !PANCAKE_LOCALIZATION
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Enable Localization", GUILayout.MaxHeight(40f)))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_LOCALIZATION");
                AssetDatabase.Refresh();
                RegistryManager.Resolve();
            }
            GUI.enabled = true;
#endif
        }
    }
}