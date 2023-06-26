using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(NavMeshSensor))]
    [CanEditMultipleObjects]
    public class NavMeshSensorEditor : BasePulsableEditor<NavMeshSensor>
    {
        SerializedProperty test;
        SerializedProperty ray;
        SerializedProperty sphere;
        SerializedProperty areaMask;
        SerializedProperty pulseMode;
        SerializedProperty pulseInterval;
        SerializedProperty onObstructed;
        SerializedProperty onClear;

        bool showEvents = false;

        protected NavMeshSensor sensor;

        protected override bool canTest => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null) return;
            sensor = serializedObject.targetObject as NavMeshSensor;

            test = serializedObject.FindProperty("Test");
            ray = serializedObject.FindProperty("Ray");
            sphere = serializedObject.FindProperty("Sphere");
            areaMask = serializedObject.FindProperty("AreaMask");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            onObstructed = serializedObject.FindProperty("onObstruction");
            onClear = serializedObject.FindProperty("onClear");
        }

        protected override void OnPulsableGUI()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(test);
            if (sensor.Test == NavMeshSensor.TestType.Ray)
            {
                EditorUtils.InlinePropertyField(ray);
            }
            else if (sensor.Test == NavMeshSensor.TestType.Sample)
            {
                EditorUtils.InlinePropertyField(sphere);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(areaMask);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval)
            {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events"))
            {
                EditorGUILayout.PropertyField(onObstructed);
                EditorGUILayout.PropertyField(onClear);
            }

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

        void InspectorDetectedObjects()
        {
            if (!sensor.IsObstructed) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ray is Obstructed", new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState() {textColor = Color.red}});
        }
    }
}