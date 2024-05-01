#if PANCAKE_ADVERTISING
using System.IO;
using Pancake.Monetization;
using Pancake;
#endif
using System;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class AdvertisingWindow
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADVERTISING
            var adSettings = ProjectDatabase.FindAll<AdSettings>();
            if (adSettings.IsNullOrEmpty())
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Adverisement Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
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
            }
            else
            {
                if (adSettings.Count > 1)
                {
                    EditorGUILayout.HelpBox("There is more than one AdSettings file in the project.\nPlease delete duplicate files keep only one file",
                        MessageType.Error);

                    EditorGUILayout.BeginVertical();
                    foreach (var t in adSettings.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(t, typeof(AdSettings), false);
                        GUI.enabled = true;
                        if (GUILayout.Button("delete", GUILayout.Width(30))) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(t));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    var editor = UnityEditor.Editor.CreateEditor(adSettings[0]);
                    editor.OnInspectorGUI();
                }
            }

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Advertisement", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
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