using System.Collections.Generic;
using Pancake.Component;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;

namespace Pancake.ComponentEditor
{
    using UnityEngine;

    [CustomEditor(typeof(ScriptableEventVfxMagnet))]
    public class ScriptableEventVfxMagnetDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Uniform.DrawLine();

            var goContainer = (IDrawObjectsInInspector) target;
            var gameObjects = goContainer.GetAllObjects();
            if (gameObjects.Count > 0) DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> objects)
        {
            GUILayout.Space(15);
            var title = $"Listener Amount : {objects.Count}";

            GUILayout.BeginVertical(title, "window");
            foreach (var obj in objects) Uniform.DrawSelectableObject(obj, new[] {obj.name, "Select"});
            GUILayout.EndVertical();
        }
    }
}