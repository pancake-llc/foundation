using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.IAP;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.IAPEditor
{
    [CustomEditor(typeof(ScriptableEventIAPNoParam))]
    public class ScriptableEventIAPNoParamDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                var eventNoParam = (ScriptableEventIAPNoParam) target;
                eventNoParam.Raise();
            }

            GUI.enabled = true;

            if (!EditorApplication.isPlaying) return;

            Uniform.DrawLine();

            var goContainer = (IDrawObjectsInInspector) target;
            var gameObjects = goContainer.GetAllObjects();
            if (gameObjects.Count > 0)
                DisplayAll(gameObjects);
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