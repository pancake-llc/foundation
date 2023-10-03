using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.ScriptableEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesScriptableDrawer
    {
        public static void OnInspectorGUI()
        {
            var scriptableSetting = Resources.Load<ScriptableEditorSetting>(nameof(ScriptableEditorSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Scriptable Setting", GUILayout.Height(40)))
                {
                    var setting = ScriptableObject.CreateInstance<ScriptableEditorSetting>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(ScriptableEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(ScriptableEditorSetting).TextColor("#f75369")} was created ad {path}/{nameof(ScriptableEditorSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.LabelField("Default: displays all the parameters of variables. " + "\nMinimal : only displays the value.", EditorStyles.wordWrappedLabel);

                var editor = UnityEditor.Editor.CreateEditor(scriptableSetting);
                editor.OnInspectorGUI();
                //needed to refresh directly even if not focused during play mode.
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Open Scriptable Wizard", GUILayout.MaxHeight(25f)))
                {
                    EditorApplication.ExecuteMenuItem("Tools/Pancake/Scriptable/Wizard");
                }

                GUI.backgroundColor = Color.white;
            }
        }
    }
}