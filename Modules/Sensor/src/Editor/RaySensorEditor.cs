using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(RaySensor))]
    [CanEditMultipleObjects]
    public class RaySensorEditor : BaseSensorEditor<RaySensor>
    {
        SerializedProperty length;
        SerializedProperty shape;
        SerializedProperty sphere;
        SerializedProperty box;
        SerializedProperty capsule;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectsOnLayers;
        SerializedProperty detectionMode;
        SerializedProperty ignoreTriggerColliders;
        SerializedProperty minimumSlopeAngle;
        SerializedProperty slopeUpDirection;
        SerializedProperty obstructedByLayers;
        SerializedProperty direction;
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

        protected override bool canTest { get { return true; } }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null) return;

            length = serializedObject.FindProperty("Length");
            shape = serializedObject.FindProperty("Shape");
            sphere = serializedObject.FindProperty("Sphere");
            box = serializedObject.FindProperty("Box");
            capsule = serializedObject.FindProperty("Capsule");
            ignoreList = serializedObject.FindProperty("signalFilter.IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("signalFilter.EnableTagFilter");
            tagFilter = serializedObject.FindProperty("signalFilter.AllowedTags");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            ignoreTriggerColliders = serializedObject.FindProperty("IgnoreTriggerColliders");
            minimumSlopeAngle = serializedObject.FindProperty("MinimumSlopeAngle");
            slopeUpDirection = serializedObject.FindProperty("SlopeUpDirection");
            obstructedByLayers = serializedObject.FindProperty("ObstructedByLayers");
            direction = serializedObject.FindProperty("Direction");
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
            EditorGUILayout.PropertyField(shape);
            if (sensor.Shape == RaySensor.CastShapeType.Sphere)
            {
                EditorUtils.InlinePropertyField(sphere);
            }
            else if (sensor.Shape == RaySensor.CastShapeType.Box)
            {
                EditorUtils.InlinePropertyField(box);
            }
            else if (sensor.Shape == RaySensor.CastShapeType.Capsule)
            {
                EditorUtils.InlinePropertyField(capsule);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(direction);
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

            EditorGUILayout.PropertyField(minimumSlopeAngle);
            if (sensor.MinimumSlopeAngle > 0f)
            {
                EditorGUILayout.PropertyField(slopeUpDirection);
            }

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
            EditorGUILayout.LabelField("Ray is Obstructed", new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState() {textColor = Color.red}});
            DetectedObjectFieldLayout(sensor.GetObstructionRayHit().GameObject);
        }
    }
}