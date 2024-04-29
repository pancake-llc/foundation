using System.IO;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class ScreenSettingWindow
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
                    if (!Directory.Exists(Common.Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultTransitionSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(DefaultTransitionSetting).TextColor("#f75369")} was created ad {Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultTransitionSetting)}.asset");
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