using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (ISave) target;
                so.ResetToInitialValue();
            }

            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            Uniform.DrawLine();
            if (gameObjects.Count > 0) DisplayALl(gameObjects);
        }

        private void DisplayALl(List<Object> gameObjects)
        {
            GUILayout.Space(15);
            var title = $"Objects reacting to OnValueChanged Event : {gameObjects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in gameObjects)
            {
                var text = $"{obj.name}  ({obj.GetType().Name})";
                Uniform.DrawSelectableObject(obj, new[] {text, "Select"});
            }

            GUILayout.EndVertical();
        }
    }
}