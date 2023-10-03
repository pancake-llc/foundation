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
        public static void OnInspectorGUI()
        {
            var scriptableSetting = Resources.Load<LevelSystemEditorSetting>(nameof(LevelSystemEditorSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Install Level System", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<LevelSystemEditorSetting>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(LevelSystemEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    RegistryManager.Add("com.unity.addressables", "1.21.17");
                    RegistryManager.Resolve();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(LevelSystemEditorSetting).TextColor("#f75369")} was created ad {path}/{nameof(LevelSystemEditorSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("Open Level Editor", GUILayout.MaxHeight(40)))
                {
                    var window = EditorWindow.GetWindow<LevelEditor>("Level Editor", true);
                    if (window)
                    {
                        window.minSize = new Vector2(275, 0);
                        window.Show(true);
                    }
                }
            }
        }
    }
}