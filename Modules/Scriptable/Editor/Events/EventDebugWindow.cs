using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PancakeEditor.Scriptable
{
    public class EventDebugWindow : EditorWindow
    {
        private string _methodName = string.Empty;
        private bool _hasClicked;
        private bool _wasFound;


        [MenuItem("Tools/Pancake/Scriptable/Event Debug Window")]
        private new static void Show() => GetWindow<EventDebugWindow>("Event Debug Window");

        private void OnLostFocus() { GUI.FocusControl(null); }

        private void OnGUI()
        {
            Uniform.DrawHeader("Event Debug Window");

            EditorGUILayout.LabelField("Find which events are calling a method in currently open scenes.");
            Uniform.DrawLine(2);

            EditorGUI.BeginChangeCheck();
            _methodName = EditorGUILayout.TextField("Method Name", _methodName, EditorStyles.textField);
            if (EditorGUI.EndChangeCheck()) _hasClicked = false;

            if (GUILayout.Button("Find", GUILayout.MaxHeight(40)))
            {
                _hasClicked = true;
                int invocationCount = FindMethodInvocationCount(_methodName);
                _wasFound = invocationCount > 0;
            }

            if (!_hasClicked) return;

            var feedbackText = _methodName;
            var guiStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = _wasFound ? Color.white : Uniform.Green}, fontStyle = FontStyle.Bold};
            feedbackText += _wasFound ? " was found!" : " was not found!";
            EditorGUILayout.LabelField(feedbackText, guiStyle);
            if (_wasFound) EditorGUILayout.LabelField("Check the console for more details.", guiStyle);
        }

        private static List<T> FindAllInOpenScenes<T>()
        {
            var results = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;

                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }
            }

            return results;
        }

        private int FindMethodInvocationCount(string methodName)
        {
            var eventListeners = FindAllInOpenScenes<EventListenerBase>();
            var count = 0;
            foreach (var listener in eventListeners)
            {
                if (listener.ContainsCallToMethod(methodName)) count++;
            }

            return count;
        }
    }
}