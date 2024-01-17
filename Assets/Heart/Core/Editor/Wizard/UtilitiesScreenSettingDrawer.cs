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
                if (GUILayout.Button("Create Default Popup Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<DefaultTransitionSetting>();
                    if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultTransitionSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(DefaultTransitionSetting).TextColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultTransitionSetting)}.asset");
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
                if (GUILayout.Button("Create Type", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                    PopupWindow.Show(new Rect(), new CreateTypeScreenWindow(position));

                GUI.backgroundColor = Color.white;
            }
        }
    }
}