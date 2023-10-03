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
        private ScriptableListBase _scriptableListBase;
        private ScriptableBase _scriptableBase;
        private static bool repaintFlag;

        public override void OnInspectorGUI()
        {
            if (_scriptableListBase == null)
                _scriptableListBase = target as ScriptableListBase;

            var isMonoBehaviourOrGameObject = _scriptableListBase.GetGenericType.IsSubclassOf(typeof(MonoBehaviour)) ||
                                              _scriptableListBase.GetGenericType == typeof(GameObject);
            if (isMonoBehaviourOrGameObject)
            {
                Uniform.DrawOnlyField(serializedObject, "m_Script", true);
                Uniform.DrawOnlyField(serializedObject, "resetOn", false);
            }
            else
            {
                //we still want to display the native list for non MonoBehaviors (like SO for examples)
                DrawDefaultInspector();

                //Check for Serializable
                var genericType = _scriptableListBase.GetGenericType;
                if (!EditorExtend.IsSerializable(genericType))
                {
                    EditorExtend.DrawSerializationError(genericType);
                    return;
                }
            }

            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            Uniform.DrawLine();

            if (gameObjects.Count > 0) DisplayAll(gameObjects);
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

        #region Repaint

        private void OnEnable()
        {
            if (repaintFlag)
                return;

            _scriptableBase = target as ScriptableBase;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            repaintFlag = true;
        }

        private void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                if (_scriptableBase == null)
                {
                    if (target == null) return;
                    _scriptableBase = (ScriptableBase) target;
                }

                _scriptableBase.repaintRequest += OnRepaintRequested;
            }

            else if (obj == PlayModeStateChange.EnteredEditMode)
            {
                if (_scriptableBase != null) _scriptableBase.repaintRequest -= OnRepaintRequested;
            }
        }

        private void OnRepaintRequested() => Repaint();

        #endregion
    }
}