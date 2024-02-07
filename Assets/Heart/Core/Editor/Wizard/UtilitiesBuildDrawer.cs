using System.IO;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesBuildDrawer
    {
        public static void OnInspectorGUI(ref AndroidBuildPipelineSettings pipelineSettings)
        {
            if (pipelineSettings == null) pipelineSettings = Resources.Load<AndroidBuildPipelineSettings>(nameof(AndroidBuildPipelineSettings));

            EditorGUILayout.BeginHorizontal();
            pipelineSettings = EditorGUILayout.ObjectField("Build Pipeline", pipelineSettings, typeof(AndroidBuildPipelineSettings), true) as AndroidBuildPipelineSettings;

            if (pipelineSettings == null)
            {
                if (GUILayout.Button("Create", GUILayout.Width(80)))
                {
                    pipelineSettings = ScriptableObject.CreateInstance<AndroidBuildPipelineSettings>();
                    if (!Directory.Exists(Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(pipelineSettings, $"{Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(AndroidBuildPipelineSettings)}.asset");
                    Debug.Log(
                        $"{nameof(AndroidBuildPipelineSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(AndroidBuildPipelineSettings)}.asset");
                }
            }

            EditorGUILayout.EndHorizontal();

            if (pipelineSettings != null)
            {
                var editorpipelineSetting = UnityEditor.Editor.CreateEditor(pipelineSettings);
                editorpipelineSetting.OnInspectorGUI();
            }
        }
    }
}