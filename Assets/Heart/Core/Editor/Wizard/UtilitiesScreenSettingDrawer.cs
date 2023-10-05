using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesScreenSettingDrawer
    {
        public static void OnInspectorGUI(Rect position)
        {
            var defaultPopupSetting = Resources.Load<DefaultTransitionSetting>(nameof(DefaultTransitionSetting));
            if (defaultPopupSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Default Popup Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<DefaultTransitionSetting>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(DefaultTransitionSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(DefaultTransitionSetting).TextColor("#f75369")} was created ad {path}/{nameof(DefaultTransitionSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editor = UnityEditor.Editor.CreateEditor(defaultPopupSetting);
                editor.OnInspectorGUI();
                
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Create Type", GUILayout.MaxHeight(40f)))
                    PopupWindow.Show(new Rect(), new CreateTypeScreenWindow(position));

                GUI.backgroundColor = Color.white;
            }
        }
    }
}