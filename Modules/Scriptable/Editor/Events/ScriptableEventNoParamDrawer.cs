using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomEditor(typeof(ScriptableEventNoParam))]
    public class ScriptableEventNoParamDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Raise"))
            {
                var eventNoParam = (ScriptableEventNoParam) target;
                eventNoParam.Raise();
            }

            if (!EditorApplication.isPlaying) return;

            Uniform.DrawLine();

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            if (gameObjects.Count > 0) DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> gameObjects)
        {
            GUILayout.Space(15);
            var title = $"Listener Amount : {gameObjects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in gameObjects)
            {
                Uniform.DrawSelectableObject(obj, new[] {obj.name, "Select"});
            }

            GUILayout.EndVertical();
        }
    }
}