using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PancakeEditor.Common;

using Pancake.Scriptable;
using UnityEngine.SceneManagement;

namespace PancakeEditor.Scriptable
{
    public class EventsDebugWindow : WindowBase
    {
        private string _methodName = string.Empty;
        private bool _hasClicked;
        private bool _wasFound;

        private new static void Show() => GetWindow<EventsDebugWindow>("Events Debug Window");

        [MenuItem("Tools/Pancake/Scriptable/Event Debug Window")]
        private static void OpenEventDebugWindow() => Show();

        private void OnLostFocus() { GUI.FocusControl(null); }

        protected override void OnGUI()
        {
            base.OnGUI();
            EditorGUILayout.LabelField("Find which events are calling a method in currently open scenes.");
            Uniform.DrawLine(2);

            EditorGUI.BeginChangeCheck();
            _methodName = EditorGUILayout.TextField("Method Name", _methodName, EditorStyles.textField);
            if (EditorGUI.EndChangeCheck()) _hasClicked = false;

            if (GUILayout.Button("Find", GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
            {
                _hasClicked = true;
                var invocationCount = FindMethodInvocationCount(_methodName);
                _wasFound = invocationCount > 0;
            }

            if (!_hasClicked) return;

            DrawFeedbackText();
        }

        private void DrawFeedbackText()
        {
            var feedbackText = _methodName;
            var guiStyle = new GUIStyle(EditorStyles.label);
            guiStyle.normal.textColor = _wasFound ? Color.white : Uniform.SunsetOrange;
            guiStyle.fontStyle = FontStyle.Bold;
            feedbackText += _wasFound ? " was found!" : " was not found!";
            EditorGUILayout.LabelField(feedbackText, guiStyle);
            if (_wasFound) EditorGUILayout.LabelField("Check the console for more details.", guiStyle);
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
    }
}