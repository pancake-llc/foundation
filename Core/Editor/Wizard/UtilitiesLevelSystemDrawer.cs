using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.LevelSystemEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesLevelSystemDrawer
    {
        public static void OnInspectorGUI(Rect position)
        {
            var scriptableSetting = Resources.Load<ScriptableLevelSystemSetting>(nameof(ScriptableLevelSystemSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Scriptable Level System Setting", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<ScriptableLevelSystemSetting>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(ScriptableLevelSystemSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(ScriptableLevelSystemSetting).TextColor("#f75369")} was created ad {path}/{nameof(ScriptableLevelSystemSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
            }
        }
    }
}