using System.Collections.Generic;
using System.Reflection;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableEventBase), true)]
    public class ScriptableEventGenericDrawer : UnityEditor.Editor
    {
        private MethodInfo _methodInfo;

        private void OnEnable()
        {
            _methodInfo = target.GetType().BaseType.GetMethod("Raise",
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Raise"))
            {
                var property = serializedObject.FindProperty("_debugValue");
                _methodInfo.Invoke(target, new [] { GetDebugValue(property) });
            }
            GUI.enabled = true;

            if (!EditorApplication.isPlaying)
                return;

            Uniform.DrawLine();

            var goContainer = (IDrawObjectsInInspector)target;
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
                Uniform.DrawSelectableObject(obj, new[] { obj.name, "Select" });
            GUILayout.EndVertical();
        }

        private object GetDebugValue(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField("_debugValue", BindingFlags.Instance | BindingFlags.NonPublic);
            return targetField.GetValue(property.serializedObject.targetObject);
        }
    }
}