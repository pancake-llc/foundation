using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(ArcSensor2D))]
    [CanEditMultipleObjects]
    public class ArcSensor2DEditor : BaseSensorEditor<ArcSensor2D>
    {
        SerializedProperty parameterisation;
        SerializedProperty bezier;
        SerializedProperty ballistic;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectsOnLayers;
        SerializedProperty detectionMode;
        SerializedProperty ignoreTriggerColliders;
        SerializedProperty obstructedByLayers;
        SerializedProperty worldSpace;
        SerializedProperty pulseMode;
        SerializedProperty pulseUpdateFunction;
        SerializedProperty pulseInterval;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;
        SerializedProperty onSomeDetection;
        SerializedProperty onNoDetection;
        SerializedProperty onObstructed;
        SerializedProperty onClear;

        bool showEvents = false;

        protected override bool canTest => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null) return;

            parameterisation = serializedObject.FindProperty("Parameterisation");
            bezier = serializedObject.FindProperty("Bezier");
            ballistic = serializedObject.FindProperty("Ballistic");
            ignoreList = serializedObject.FindProperty("signalFilter.IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("signalFilter.EnableTagFilter");
            tagFilter = serializedObject.FindProperty("signalFilter.AllowedTags");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            ignoreTriggerColliders = serializedObject.FindProperty("IgnoreTriggerColliders");
            obstructedByLayers = serializedObject.FindProperty("ObstructedByLayers");
            worldSpace = serializedObject.FindProperty("WorldSpace");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseUpdateFunction = serializedObject.FindProperty("pulseRoutine.UpdateFunction");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");
            onObstructed = serializedObject.FindProperty("onObstruction");
            onClear = serializedObject.FindProperty("onClear");
        }

        protected override void InspectorParameters()
        {
            EditorGUILayout.PropertyField(parameterisation);
            if (sensor.Parameterisation == ArcSensor2D.ParameterisationType.Bezier)
            {
                EditorUtils.InlinePropertyField(bezier);
            }
            else if (sensor.Parameterisation == ArcSensor2D.ParameterisationType.Ballistic)
            {
                EditorUtils.InlinePropertyField(ballistic);
            }

            EditorGUILayout.PropertyField(worldSpace);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreList, true);
            EditorGUILayout.PropertyField(tagFilterEnabled);
            if (tagFilterEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(tagFilter, true);
            }

            EditorGUILayout.PropertyField(detectsOnLayers);
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(ignoreTriggerColliders);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(obstructedByLayers);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode != PulseRoutine.Modes.Manual)
            {
                EditorGUILayout.PropertyField(pulseUpdateFunction);
            }

            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval)
            {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events"))
            {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onSomeDetection);
                EditorGUILayout.PropertyField(onNoDetection);
                EditorGUILayout.PropertyField(onObstructed);
                EditorGUILayout.PropertyField(onClear);
            }

            EditorGUILayout.Space();

            BufferSizeInfo(sensor.CurrentBufferSize);
        }

        protected override void InspectorDetectedObjects()
        {
            base.InspectorDetectedObjects();

            if (!sensor.IsObstructed) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arc is Obstructed", new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState() {textColor = Color.red}});
            DetectedObjectFieldLayout(sensor.GetObstructionRayHit().GameObject);
        }
    }
}