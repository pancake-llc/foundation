using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    public static class EditorState
    {
        public static event System.Action OnStopTesting;
        public static Observable<BasePulsableSensor> ActivePulsable = new Observable<BasePulsableSensor>();

        public static void StopAllTesting()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                return;
            }

            OnStopTesting?.Invoke();
            ActivePulsable.Value = null;
        }
    }

    public abstract class BasePulsableEditor<T> : Editor where T : BasePulsableSensor
    {
        T pulsable;
        protected bool IsTesting = false;
        protected bool IsActivePulsable => EditorState.ActivePulsable.Value == pulsable;
        protected abstract bool canTest { get; }

        protected bool showDetections => EditorApplication.isPlaying || EditorApplication.isPaused || IsTesting;
        bool showTestButton => !showDetections;

        protected virtual void OnEnable()
        {
            if (serializedObject == null)
            {
                return;
            }

            pulsable = serializedObject.targetObject as T;
            pulsable.OnPulsed += OnPulsedHandler;
            EditorState.OnStopTesting += OnStopTestingHandler;
            EditorState.ActivePulsable.OnChanged += ActivePulsableChangedHandler;

            if ((EditorApplication.isPlaying || EditorApplication.isPaused) && EditorState.ActivePulsable.Value == null)
            {
                EditorState.ActivePulsable.Value = pulsable;
            }
        }

        protected virtual void OnDisable()
        {
            EditorState.StopAllTesting();
            if (IsActivePulsable)
            {
                EditorState.ActivePulsable.Value = null;
            }

            pulsable.OnPulsed -= OnPulsedHandler;
            EditorState.OnStopTesting -= OnStopTestingHandler;
            EditorState.ActivePulsable.OnChanged -= ActivePulsableChangedHandler;
        }

        public override void OnInspectorGUI()
        {
            var mb = pulsable as MonoBehaviour;
            if (mb != null && mb.transform.hasChanged)
            {
                EditorState.StopAllTesting();
                mb.transform.hasChanged = false;
            }

            serializedObject.Update();

            var rect = EditorGUILayout.BeginVertical();
            rect.xMin -= 12;
            rect.xMax += 2;
            if (IsActivePulsable)
            {
                DrawActive(rect);
            }

            OnPulsableGUI();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (canTest && showTestButton)
            {
                if (GUILayout.Button("Test", GUILayout.Width(100)))
                {
                    StartTesting();
                }
            }
            else if (!showTestButton && !IsActivePulsable)
            {
                if (GUILayout.Button("Show Gizmos", GUILayout.Width(100)))
                {
                    EditorState.ActivePulsable.Value = pulsable;
                }
            }
        }

        void DrawActive(Rect rect) { EditorGUI.DrawRect(rect, new Color(0.2f, 1f, 1f, 0.427451f)); }

        protected abstract void OnPulsableGUI();

        void OnPulsedHandler()
        {
            Repaint();
            if (IsTesting || Application.isPlaying || pulsable == null)
            {
                return;
            }

            IsTesting = true;
            SceneView.RepaintAll();
        }

        void StartTesting()
        {
            if (IsTesting || Application.isPlaying || pulsable == null)
            {
                return;
            }

            pulsable.PulseAll();
            EditorState.ActivePulsable.Value = pulsable;
        }

        void OnStopTestingHandler()
        {
            if (!IsTesting || Application.isPlaying || pulsable == null)
            {
                return;
            }

            IsTesting = false;
            SceneView.RepaintAll();
        }

        void ActivePulsableChangedHandler()
        {
            pulsable.ShowDetectionGizmos = IsActivePulsable;
            SceneView.RepaintAll();
        }
    }
}