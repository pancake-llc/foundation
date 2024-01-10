using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesHeartSettingDrawer
    {
        public static void OnInspectorGUI()
        {
            var heartSetting = Resources.Load<HeartSettings>(nameof(HeartSettings));
            var heartEditorSetting = Resources.Load<HeartEditorSettings>(nameof(HeartEditorSettings));
            if (heartSetting == null || heartEditorSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Missing Heart Setting", GUILayout.Height(30f)))
                {
                    if (heartSetting == null)
                    {
                        var setting = ScriptableObject.CreateInstance<HeartSettings>();
                        if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                        AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(HeartSettings)}.asset");
                        Debug.Log($"{nameof(HeartSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(HeartSettings)}.asset");
                    }

                    if (heartEditorSetting == null)
                    {
                        var editorSetting = ScriptableObject.CreateInstance<HeartEditorSettings>();
                        if (!Directory.Exists(Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                        AssetDatabase.CreateAsset(editorSetting, $"{Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(HeartEditorSettings)}.asset");
                        Debug.Log(
                            $"{nameof(HeartEditorSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(HeartEditorSettings)}.asset");
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editorHeartSetting = UnityEditor.Editor.CreateEditor(heartSetting);
                editorHeartSetting.OnInspectorGUI();
                EditorGUILayout.Space();
                var editorHeartEditorSetting = UnityEditor.Editor.CreateEditor(heartEditorSetting);
                editorHeartEditorSetting.OnInspectorGUI();
            }
        }
    }
}