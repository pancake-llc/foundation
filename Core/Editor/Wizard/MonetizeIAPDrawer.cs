#if PANCAKE_IAP
using System.IO;
using Pancake.IAP;
using Pancake;
#endif
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class MonetizeIAPDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_IAP
            var iapSetting = ProjectDatabase.FindAll<IAPSettings>();
            if (iapSetting.IsNullOrEmpty())
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create IAP Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<IAPSettings>();
                    const string path = "Assets/_Root/Storages/Settings";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(IAPSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(IAPSettings).TextColor("#f75369")} was created ad {path}/{nameof(IAPSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (iapSetting.Count > 1)
                {
                    EditorGUILayout.HelpBox("There is more than one IAPSettings file in the project.\nPlease delete duplicate files keep only one file",
                        MessageType.Error);

                    EditorGUILayout.BeginVertical();
                    foreach (var t in iapSetting.ToArray())
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(t, typeof(IAPSettings), false);
                        GUI.enabled = true;
                        if (GUILayout.Button("delete", GUILayout.Width(50))) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(t));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(iapSetting[0], typeof(IAPSettings), false);
                    GUI.enabled = true;
                    GUILayout.Space(10);
                    if (GUILayout.Button("Edit", GUILayout.MaxHeight(40f))) iapSetting[0].SelectAndPing();
                    GUI.backgroundColor = Uniform.Red;
                    if (GUILayout.Button("Delete Setting", GUILayout.MaxHeight(30f))) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(iapSetting[0]));
                    GUI.backgroundColor = Color.white;
                }
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall IAP Package", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall IAP", "Are you sure you want to uninstall in-app-purchase package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.purchasing");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Purchasing", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.purchasing", "4.7.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}