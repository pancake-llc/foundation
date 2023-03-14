﻿#if PANCAKE_ADVERTISING
using System.IO;
using Pancake.Monetization;
using Pancake;
#endif
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
                if (GUILayout.Button("Create IAP Setting", GUILayout.MaxHeight(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<AdSettings>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(AdSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(AdSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(AdSettings)}.asset");
                }

                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(adSetting);
                editor.OnInspectorGUI();
            }
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.sharp-zip-lib", "1.3.3-preview");
                Editor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADVERTISING");
                AssetDatabase.Refresh();
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}