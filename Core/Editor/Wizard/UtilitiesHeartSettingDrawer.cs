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
                if (GUILayout.Button("Create Heart Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<HeartSettings>();
                    const string path = "Assets/_Root/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(HeartSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(HeartSettings).TextColor("#f75369")} was created ad {path}/{nameof(HeartSettings)}.asset");
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