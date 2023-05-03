#if PANCAKE_ADVERTISING
using System.IO;
using Pancake.Monetization;
using Pancake;
#endif
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class MonetizeAdvertisingDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADVERTISING
            var adSetting = Resources.Load<AdSettings>(nameof(AdSettings));
            if (adSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Adverisement Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<AdSettings>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(AdSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(AdSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(AdSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Uniform.Red;
                if (GUILayout.Button("Uninstall Advertising", GUILayout.MaxHeight(25f)))
                {
                    bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Advertising", "Are you sure you want to uninstall advertising package ?", "Yes", "No");
                    if (confirmDelete)
                    {
                        Editor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADVERTISING");
                        AssetDatabase.Refresh();
                        RegistryManager.Resolve();
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(adSetting);
                editor.OnInspectorGUI();
            }

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Advertisement", GUILayout.MaxHeight(40f)))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADVERTISING");
                AssetDatabase.Refresh();
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}