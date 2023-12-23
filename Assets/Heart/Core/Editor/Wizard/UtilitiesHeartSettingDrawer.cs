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
            if (heartSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Heart Setting", GUILayout.Height(30f)))
                {
                    var setting = ScriptableObject.CreateInstance<HeartSettings>();
                    if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(HeartSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(HeartSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(HeartSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editor = UnityEditor.Editor.CreateEditor(heartSetting);
                editor.OnInspectorGUI();
            }
        }
    }
}