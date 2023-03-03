using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomEditor(typeof(ScriptableListBase), true)]
    public class ScriptableListDrawer : UnityEditor.Editor
    {
        private SerializedObject _serializedObject;
        private ScriptableListBase _list;

        public override void OnInspectorGUI()
        {
            if (_list == null) _list = target as ScriptableListBase;

            bool isMonoOrGameObject = _list.GetElementType.IsSubclassOf(typeof(MonoBehaviour)) || _list.GetElementType == typeof(GameObject);
            if (isMonoOrGameObject)
            {
                //Do not draw the native list.
                if (_serializedObject == null) _serializedObject = new SerializedObject(target);

                Uniform.DrawOnlyField(_serializedObject, "m_Script", true);
                Uniform.DrawOnlyField(_serializedObject, "resetOn", false);
            }
            else
            {
                //we still want to display the native list for non MonoBehaviors (like scriptableObject for examples)
                DrawDefaultInspector();
            }

            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            Uniform.DrawLine();

            if (gameObjects.Count > 0) DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> gameObjects)
        {
            GUILayout.Space(15);
            var title = $"List Count : {gameObjects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in gameObjects)
            {
                Uniform.DrawSelectableObject(obj, new[] {obj.name, "Select"});
            }

            GUILayout.EndVertical();
        }
    }
}