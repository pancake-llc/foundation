using System.IO;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.ScriptableEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class ScriptableWindow
    {
        public static void OnInspectorGUI()
        {
            var scriptableSetting = Resources.Load<ScriptableEditorSetting>(nameof(ScriptableEditorSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Scriptable Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<ScriptableEditorSetting>();
                    if (!Directory.Exists(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(ScriptableEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(ScriptableEditorSetting).TextColor("#f75369")} was created ad {Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(ScriptableEditorSetting)}.asset");
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

                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Open Scriptable Wizard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    EditorApplication.ExecuteMenuItem("Tools/Pancake/Scriptable/Wizard");
                }

                GUI.backgroundColor = Color.white;
            }
        }
    }
}