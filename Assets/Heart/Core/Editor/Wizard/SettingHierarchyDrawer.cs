using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using PancakeEditor.Hierarchy;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class SettingHierarchyDrawer
    {
        public static void OnInspectorGUI()
        {
            var hierarchySetting = Resources.Load<HierarchyEditorSetting>(nameof(HierarchyEditorSetting));
            if (hierarchySetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;

                if (GUILayout.Button("Create Hierarchy Setting", GUILayout.Height(30)))
                {
                    var setting = ScriptableObject.CreateInstance<HierarchyEditorSetting>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(HierarchyEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(HierarchyEditorSetting).TextColor("#f75369")} was created ad {path}/{nameof(HierarchyEditorSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(hierarchySetting);
                editor.OnInspectorGUI();
                //needed to refresh directly even if not focused during play mode.
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}