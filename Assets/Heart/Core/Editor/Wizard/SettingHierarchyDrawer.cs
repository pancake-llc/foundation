using System.IO;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;

using PancakeEditor.Hierarchy;
using UnityEditor;
using UnityEngine;
using Editor = PancakeEditor.Common.Editor;

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
                    if (!Directory.Exists(Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(HierarchyEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(HierarchyEditorSetting).TextColor("#f75369")} was created ad {Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(HierarchyEditorSetting)}.asset");
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