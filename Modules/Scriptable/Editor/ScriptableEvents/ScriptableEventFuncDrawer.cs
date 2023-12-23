using System.Collections.Generic;
using System.Reflection;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableEventFunc<>), true)]
    public class ScriptableEventFuncDrawer : UnityEditor.Editor
    {
        private MethodInfo _methodInfo;

        private void OnEnable() { _methodInfo = target.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public); }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                _methodInfo.Invoke(target, null);
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