using Pancake.Tag;
using UnityEditor;
using UnityEngine;

namespace Pancake.TagEditor
{
    [CustomEditor(typeof(ScriptableTag), true, isFallback = false)]
    [CanEditMultipleObjects]
    public class ScriptableTagEditor : TagEditorBase
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawDivider();
            base.OnInspectorGUI();
        }

        private void DrawDivider()
        {
            var rect = EditorGUILayout.BeginHorizontal();
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Handles.DrawLine(new Vector2(rect.x - 17, rect.y), new Vector2(rect.width + 20, rect.y));
            EditorGUILayout.EndHorizontal();
        }
    }
}