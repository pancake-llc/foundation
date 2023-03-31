using System.IO;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class AllBuildDrawer
    {
        public static void OnInspectorGUI()
        {
            var buildSetting = Resources.Load<EditorPreBuildSettings>(nameof(EditorPreBuildSettings));
            if (buildSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create PreBuild Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<EditorPreBuildSettings>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(EditorPreBuildSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(EditorPreBuildSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(EditorPreBuildSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(buildSetting);
                editor.OnInspectorGUI();
                EditorGUILayout.Space();
                
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Build", GUILayout.Height(40f)))
                {
                    
                }
                GUI.backgroundColor = Color.white;
            }
        }
    }
}