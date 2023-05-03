using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableListBase), true)]
    public class ScriptableListDrawer : UnityEditor.Editor
    {
        private ScriptableListBase _targetScriptableList;

        public override void OnInspectorGUI()
        {
            if (_targetScriptableList == null)
                _targetScriptableList = target as ScriptableListBase;

            var isMonoBehaviourOrGameObject = _targetScriptableList.GetElementType.IsSubclassOf(typeof(MonoBehaviour)) ||
                                              _targetScriptableList.GetElementType == typeof(GameObject);
            if (isMonoBehaviourOrGameObject)
            {
                Uniform.DrawOnlyField(serializedObject, "m_Script", true);
                Uniform.DrawOnlyField(serializedObject, "_resetOn", false);
            }
            else
            {
                //we still want to display the native list for non MonoBehaviors (like SO for examples)
                DrawDefaultInspector();
            }

            if (!EditorApplication.isPlaying)
                return;

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            Uniform.DrawLine();

            if (gameObjects.Count > 0)
                DisplayAll(gameObjects);
        }

        private void DisplayAll(List<Object> objects)
        {
            GUILayout.Space(15);
            var title = $"List Count : {objects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in objects)
                Uniform.DrawSelectableObject(obj, new[] {obj.name, "Select"});

            GUILayout.EndVertical();
        }
    }
}