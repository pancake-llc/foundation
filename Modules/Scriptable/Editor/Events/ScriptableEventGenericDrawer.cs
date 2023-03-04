using System.Collections.Generic;
using System.Reflection;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor.Scriptable
{
    [CustomEditor(typeof(ScriptableEventBase), true)]
    public class ScriptableEventGenericDrawer : UnityEditor.Editor
    {
        private MethodInfo _methodInfo;

        private void OnEnable() { _methodInfo = target.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public); }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                var property = serializedObject.FindProperty("debugValue");
                _methodInfo.Invoke(target, new[] {GetDebugValue(property)});
            }

            GUI.enabled = true;

            if (!EditorApplication.isPlaying) return;

            Uniform.DrawLine();

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            if (gameObjects.Count > 0) DisplayAll(gameObjects);
        }

        private object GetDebugValue(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField("debugValue", BindingFlags.Instance | BindingFlags.NonPublic);
            return targetField?.GetValue(property.serializedObject.targetObject);
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