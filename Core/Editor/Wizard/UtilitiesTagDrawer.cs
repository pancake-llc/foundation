using System.IO;
using Pancake;
using Pancake.Tag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesTagDrawer
    {
        public static void OnInspectorGUI()
        {
            var scriptableSetting = Resources.Load<TagSetting>(nameof(TagSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Scriptable Setting", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<TagSetting>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(TagSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(TagSetting).TextColor("#f75369")} was created ad {path}/{nameof(TagSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(scriptableSetting);
                editor.OnInspectorGUI();
                //needed to refresh directly even if not focused during play mode.
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}