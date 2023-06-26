using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    public class SensorEditorActions
    {
        static double prevPingTime;
        static GameObject prevPingObject;

        public static void PingOrSelectObject(GameObject o)
        {
            var time = EditorApplication.timeSinceStartup;
            var delta = time - prevPingTime;
            if (o != null && ReferenceEquals(o, prevPingObject) && delta <= .3f)
            {
                Selection.activeGameObject = o;
            }
            else if (o != null)
            {
                EditorGUIUtility.PingObject(o);
            }

            prevPingTime = time;
            prevPingObject = o;
        }
    }

    public abstract class BaseSensorEditor<T> : BasePulsableEditor<T> where T : Sensor.Sensor
    {
        protected T sensor;

        protected override void OnEnable()
        {
            base.OnEnable();
            sensor = serializedObject.targetObject as T;
        }

        protected override void OnPulsableGUI()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            InspectorParameters();
            if (EditorGUI.EndChangeCheck())
            {
                EditorState.StopAllTesting();
            }

            serializedObject.ApplyModifiedProperties();

            if (showDetections)
            {
                EditorUtils.HorizontalLine(new Color(0.5f, 0.5f, 0.5f, 1));

                EditorGUILayout.Space();

                InspectorDetectedObjects();

                EditorGUILayout.Space();
            }
        }

        protected abstract void InspectorParameters();

        protected virtual void InspectorDetectedObjects()
        {
            if (sensor.Signals.Count == 0)
            {
                EditorGUILayout.LabelField("Nothing Detected...", new GUIStyle() {fontStyle = FontStyle.Bold});
                return;
            }

            EditorGUILayout.LabelField("Detected Signals", new GUIStyle() {fontStyle = FontStyle.Bold});
            foreach (var signal in sensor.Signals)
            {
                SignalFieldLayout(signal);
            }
        }

        protected void SignalFieldLayout(Signal signal)
        {
            var rect = EditorGUILayout.GetControlRect();
            SignalField(rect, signal);
        }

        protected void SignalField(Rect rect, Signal signal)
        {
            var width = rect.width;
            var objRect = new Rect(rect.x, rect.y, width / 2, rect.height);
            var strengthRect = new Rect(objRect.max.x, rect.y, width / 4, rect.height);
            var shapeRect = new Rect(strengthRect.max.x, rect.y, width / 4, rect.height);
            DetectedObjectField(objRect, signal.Object);
            EditorGUI.LabelField(strengthRect, $"Str: {signal.Strength.ToString("N1")}", new GUIStyle() {alignment = TextAnchor.MiddleCenter});
            EditorGUI.LabelField(shapeRect, $"Size: {signal.Shape.size.magnitude.ToString("N1")}");
        }

        protected void DetectedObjectFieldLayout(GameObject go)
        {
            var rect = EditorGUILayout.GetControlRect();
            DetectedObjectField(rect, go);
        }

        protected void DetectedObjectField(Rect rect, GameObject go)
        {
            var guiContent = EditorGUIUtility.ObjectContent(go, typeof(GameObject));

            var style = new GUIStyle("TextField");
            style.fixedHeight = 16;
            style.imagePosition = go ? ImagePosition.ImageLeft : ImagePosition.TextOnly;

            if (GUI.Button(rect, guiContent, style) && go)
            {
                SensorEditorActions.PingOrSelectObject(go);
            }
        }

        protected void BufferSizeInfo(int bufferSize)
        {
            if (bufferSize > PhysicsNonAlloc<object>.InitialSize && Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Buffer size expanded to: " + bufferSize, MessageType.Info);
            }
        }
    }
}