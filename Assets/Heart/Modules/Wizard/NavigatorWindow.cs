using System.IO;
using Pancake.Common;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class NavigatorWindow
    {
        public static void OnInspectorGUI(Rect position)
        {
            var defaultPopupSetting = Resources.Load<DefaultNavigatorSetting>(nameof(DefaultNavigatorSetting));
            if (defaultPopupSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink_500;
                if (GUILayout.Button("Create Default Naviagtor Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<DefaultNavigatorSetting>();
                    if (!Directory.Exists(Common.Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultNavigatorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log(
                        $"{nameof(DefaultNavigatorSetting).SetColor("#f75369")} was created ad {Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(DefaultNavigatorSetting)}.asset");
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
                GUI.backgroundColor = Uniform.Green_500;
                if (GUILayout.Button("Create Type", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT))) PopupWindow.Show(new Rect(), new CreateTypeScreenWindow(position));

                GUI.backgroundColor = Color.white;
            }
        }
    }
}