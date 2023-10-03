#if PANCAKE_ADVERTISING
using System.IO;
using Pancake.Monetization;
using Pancake;
#endif
using Pancake.ExLibEditor;
using Pancake.ExLib;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class MonetizeAdvertisingDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADVERTISING
            var adSetting = ProjectDatabase.FindAll<AdSettings>();
            if (adSetting.IsNullOrEmpty())
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Adverisement Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<AdSettings>();
                    const string path = "Assets/_Root/Storages/Settings";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(AdSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(AdSettings).TextColor("#f75369")} was created ad {path}/{nameof(AdSettings)}.asset");
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
                        ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADVERTISING");
                        AssetDatabase.Refresh();
                        RegistryManager.Resolve();
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                if (adSetting.Count > 1)
                {
                    EditorGUILayout.HelpBox("There is more than one AdSettings file in the project.\nPlease delete duplicate files keep only one file",
                        MessageType.Error);

                    EditorGUILayout.BeginVertical();
                    foreach (var t in adSetting.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(t, typeof(AdSettings), false);
                        GUI.enabled = true;
                        if (GUILayout.Button("delete", GUILayout.Width(50))) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(t));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    var editor = UnityEditor.Editor.CreateEditor(adSetting[0]);
                    editor.OnInspectorGUI();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(20);

                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = Uniform.Green;
                    if (GUILayout.Button("Ping", GUILayout.Height(24))) adSetting[0].SelectAndPing();

                    GUI.backgroundColor = Uniform.Red;
                    if (GUILayout.Button("Delete AdSettings", GUILayout.Height(24)))
                    {
                        bool confirmDelete = EditorUtility.DisplayDialog("Delete AdSettings", "Are you sure you want to delete ad settings?", "Yes", "No");
                        if (confirmDelete) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(adSetting[0]));
                    }

                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }
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