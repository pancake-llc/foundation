using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(SteeringSensor2D))]
    [CanEditMultipleObjects]
    public class SteeringSensor2DEditor : BasePulsableEditor<SteeringSensor2D>
    {
        SerializedProperty resolution;
        SerializedProperty interpolationSpeed;
        SerializedProperty seek;
        SerializedProperty avoid;
        SerializedProperty pulseMode;
        SerializedProperty pulseUpdateFunction;
        SerializedProperty pulseInterval;
        SerializedProperty locomotionMode;
        SerializedProperty rigidBody;
        SerializedProperty locomotion;

        protected SteeringSensor2D sensor;

        protected override bool canTest => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null) return;
            sensor = serializedObject.targetObject as SteeringSensor2D;

            resolution = serializedObject.FindProperty("resolution");
            interpolationSpeed = serializedObject.FindProperty("InterpolationSpeed");
            seek = serializedObject.FindProperty("seek");
            avoid = serializedObject.FindProperty("avoid");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseUpdateFunction = serializedObject.FindProperty("pulseRoutine.UpdateFunction");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            locomotionMode = serializedObject.FindProperty("LocomotionMode");
            rigidBody = serializedObject.FindProperty("RigidBody");
            locomotion = serializedObject.FindProperty("locomotion");
        }

        protected override void OnPulsableGUI()
        {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(resolution);
            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode != PulseRoutine.Modes.Manual)
            {
                EditorGUILayout.PropertyField(pulseUpdateFunction);
            }

            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval)
            {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            if (sensor.PulseMode != PulseRoutine.Modes.Manual)
            {
                EditorGUILayout.PropertyField(interpolationSpeed);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(seek, true);
            EditorGUILayout.PropertyField(avoid, true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(locomotionMode);

            var locmode = (LocomotionMode2D) locomotionMode.enumValueIndex;

            if (locmode != LocomotionMode2D.None)
            {
                if (locmode == LocomotionMode2D.RigidBody2D)
                {
                    EditorGUILayout.PropertyField(rigidBody);
                }

                EditorGUILayout.PropertyField(locomotion, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorState.StopAllTesting();
            }

            displayErrors();

            serializedObject.ApplyModifiedProperties();
        }

        void displayErrors() { }
    }
}