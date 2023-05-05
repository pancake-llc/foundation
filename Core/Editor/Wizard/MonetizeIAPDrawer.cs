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
            var iapSetting = Resources.Load<IAPSettings>(nameof(IAPSettings));
            if (iapSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create IAP Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<IAPSettings>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(IAPSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(IAPSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(IAPSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("Edit", GUILayout.MaxHeight(40f))) iapSetting.SelectAndPing();
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