using System.IO;
using Pancake;
using Pancake.Tracking;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class TrackingIOS14AdvertisingSupportDrawer
    {
        public static void OnInspectorGUI()
        {
#if UNITY_IOS && PANCAKE_ATT
            Uniform.DrawInstalled("1.2.0");
            EditorGUILayout.Space();

            var ios14SupportSetting = Resources.Load<IOS14AdSupportSetting>(nameof(IOS14AdSupportSetting));
            if (ios14SupportSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create iOS 14 Ad Support Setting", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<IOS14AdSupportSetting>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(IOS14AdSupportSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(IOS14AdSupportSetting).TextColor("#f75369")} was created ad {path}/{nameof(IOS14AdSupportSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(ios14SupportSetting);
                editor.OnInspectorGUI();
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall iOS 14 Advertising Support Package", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall iOS14AdvertisingSupport", "Are you sure you want to uninstall iOS 14 Advertising Support package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.ads.ios-support");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
			GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install iOS 14 Advertising Support Package", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.unity.ads.ios-support", "1.2.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}