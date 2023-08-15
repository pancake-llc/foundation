using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEditor;
using UnityEngine;

namespace Pancake.SoundEditor
{
    [CustomEditor(typeof(ScriptableEventAudioHandle))]
    public class ScriptableEventAudioHandleDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!EditorApplication.isPlaying) return;

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
            foreach (var obj in objects)
                Uniform.DrawSelectableObject(obj, new[] {obj.name, "Select"});
            GUILayout.EndVertical();
        }
    }
}