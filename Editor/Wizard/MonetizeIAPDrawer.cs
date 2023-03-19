#if PANCAKE_IAP
using System.IO;
using Pancake.IAP;
using Pancake;
#endif
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
                if (GUILayout.Button("Create IAP Setting", GUILayout.MaxHeight(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<IAPSettings>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(IAPSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(IAPSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(IAPSettings)}.asset");
                }

                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(iapSetting);
                editor.OnInspectorGUI();
            }
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Purchasing Package", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.purchasing", "4.7.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}