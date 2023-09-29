using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesPopupSettingDrawer
    {
        public static void OnInspectorGUI()
        {
            var defaultPopupSetting = Resources.Load<DefaultPopupSetting>(nameof(DefaultPopupSetting));
            if (defaultPopupSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Default Popup Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<DefaultPopupSetting>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(DefaultPopupSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(DefaultPopupSetting).TextColor("#f75369")} was created ad {path}/{nameof(DefaultPopupSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editor = UnityEditor.Editor.CreateEditor(defaultPopupSetting);
                editor.OnInspectorGUI();
            }
        }
    }
}