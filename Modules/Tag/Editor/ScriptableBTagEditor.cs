using Pancake.BTag;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTagEditor
{
    [CustomEditor(typeof(ScriptableBTag), true, isFallback = false)]
    [CanEditMultipleObjects]
    public class ScriptableBTagEditor : BTagEditorBase
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
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