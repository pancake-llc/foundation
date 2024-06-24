#if PANCAKE_IAP
using System.IO;
using Pancake.IAP;
#endif
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class IAPWindow
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_IAP
            var iapSettings = ProjectDatabase.FindAll<IAPSettings>();
            if (iapSettings.IsNullOrEmpty())
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create IAP Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<IAPSettings>();
                    const string path = "Assets/_Root/Storages/Settings";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(IAPSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(IAPSettings).SetColor("f75369")} was created ad {path}/{nameof(IAPSettings)}.asset");
                }

                GUI.enabled = true;
            }
            else
            {
                if (iapSettings.Count > 1)
                {
                    EditorGUILayout.HelpBox("There is more than one IAPSettings file in the project.\nPlease delete duplicate files keep only one file",
                        MessageType.Error);

                    EditorGUILayout.BeginVertical();
                    foreach (var t in iapSettings.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(t, typeof(IAPSettings), false);
                        GUI.enabled = true;
                        if (GUILayout.Button(Uniform.IconContent("Toolbar Minus", "Remove"), GUILayout.Width(30f)))
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(t));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    var editor = UnityEditor.Editor.CreateEditor(iapSettings[0]);
                    editor.OnInspectorGUI();
                }
            }

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Purchasing", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.purchasing", "4.11.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}