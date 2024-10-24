using System.IO;
using Pancake.Common;
using Pancake.Tracking;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Editor = PancakeEditor.Common.Editor;

namespace PancakeEditor
{
    internal static class AdjustWindow
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_ADJUST
            Uniform.DrawInstalled("5.0.3");
            EditorGUILayout.Space();

            var adjustSetting = Resources.Load<AdjustConfig>(nameof(AdjustConfig));
            if (adjustSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink_500;
                if (GUILayout.Button("Create Adjust Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<AdjustConfig>();
                    if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(AdjustConfig)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(AdjustConfig).SetColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(AdjustConfig)}.asset");
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

            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Adjust", "Are you sure you want to uninstall adjust package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.adjust.sdk");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = previousColor;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Adjust Package", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.adjust.sdk", "https://github.com/adjust/unity_sdk.git?path=Assets/Adjust#v5.0.3");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}