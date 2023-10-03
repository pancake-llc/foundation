using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Tracking;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class TrackingAdjustDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADJUST
            Uniform.DrawInstalled("4.34.1");
            EditorGUILayout.Space();

            var adjustSetting = Resources.Load<AdjustConfig>(nameof(AdjustConfig));
            if (adjustSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Adjust Setting", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<AdjustConfig>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(AdjustConfig)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(AdjustConfig).TextColor("#f75369")} was created ad {path}/{nameof(AdjustConfig)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(adjustSetting);
                editor.OnInspectorGUI();
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Adjust Package", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Adjust", "Are you sure you want to uninstall adjust package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.pancake.adjust");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Adjust Package", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.pancake.adjust", "https://github.com/pancake-llc/adjust.git#4.34.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}