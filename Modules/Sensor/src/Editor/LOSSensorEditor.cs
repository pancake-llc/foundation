using System.Collections.Generic;
using Pancake.Sensor;
using UnityEngine;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(LOSSensor))]
    [CanEditMultipleObjects]
    public class LOSSensorEditor : BaseSensorEditor<LOSSensor>
    {
        SerializedProperty inputSensor;
        SerializedProperty blocksLineOfSight;
        SerializedProperty ignoreTriggerColliders;
        SerializedProperty pulseMode;
        SerializedProperty pulseUpdateFunction;
        SerializedProperty pulseInterval;
        SerializedProperty pointSamplingMethod;
        SerializedProperty testLOSTargetsOnly;
        SerializedProperty numberOfRays;
        SerializedProperty minimumVisibility;

        SerializedProperty movingAverage;
        SerializedProperty windowSize;

        SerializedProperty limitDistance;
        SerializedProperty maxDistance;
        SerializedProperty visibilityByDistance;

        SerializedProperty limitViewAngle;
        SerializedProperty maxHorizAngle;
        SerializedProperty visibilityByHorizAngle;
        SerializedProperty maxVertAngle;
        SerializedProperty visibilityByVertAngle;
        SerializedProperty fovConstraintMethod;

        SerializedProperty onDetected;
        SerializedProperty onLostDetection;
        SerializedProperty onSomeDetection;
        SerializedProperty onNoDetection;

        bool showEvents = false;

        protected override bool canTest { get { return true; } }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (serializedObject == null)
            {
                return;
            }

            inputSensor = serializedObject.FindProperty("inputSensor");
            blocksLineOfSight = serializedObject.FindProperty("BlocksLineOfSight");
            ignoreTriggerColliders = serializedObject.FindProperty("IgnoreTriggerColliders");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseUpdateFunction = serializedObject.FindProperty("pulseRoutine.UpdateFunction");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            pointSamplingMethod = serializedObject.FindProperty("PointSamplingMethod");
            testLOSTargetsOnly = serializedObject.FindProperty("TestLOSTargetsOnly");
            numberOfRays = serializedObject.FindProperty("NumberOfRays");
            minimumVisibility = serializedObject.FindProperty("MinimumVisibility");

            movingAverage = serializedObject.FindProperty("MovingAverageEnabled");
            windowSize = serializedObject.FindProperty("MovingAverageWindowSize");

            limitDistance = serializedObject.FindProperty("LimitDistance");
            maxDistance = serializedObject.FindProperty("MaxDistance");
            visibilityByDistance = serializedObject.FindProperty("VisibilityByDistance");

            limitViewAngle = serializedObject.FindProperty("LimitViewAngle");
            maxHorizAngle = serializedObject.FindProperty("MaxHorizAngle");
            visibilityByHorizAngle = serializedObject.FindProperty("VisibilityByHorizAngle");
            maxVertAngle = serializedObject.FindProperty("MaxVertAngle");
            visibilityByVertAngle = serializedObject.FindProperty("VisibilityByVertAngle");

            fovConstraintMethod = serializedObject.FindProperty("FOVConstraintMethod");

            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");

            sensor.ShowRayCastDebug = new HashSet<GameObject>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            sensor.ShowRayCastDebug = null;
        }

        protected override void InspectorParameters()
        {
            EditorGUILayout.PropertyField(inputSensor);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(blocksLineOfSight);
            EditorGUILayout.PropertyField(ignoreTriggerColliders);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(testLOSTargetsOnly);
            if (!testLOSTargetsOnly.boolValue)
            {
                EditorGUILayout.PropertyField(numberOfRays);
                EditorGUILayout.PropertyField(pointSamplingMethod);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(movingAverage, new GUIContent("Moving Average"));
            if (sensor.MovingAverageEnabled)
            {
                EditorGUILayout.PropertyField(windowSize, new GUIContent("Window Size"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(limitDistance);
            if (sensor.LimitDistance)
            {
                EditorGUILayout.PropertyField(maxDistance);
                ScalingFunctionProperty(visibilityByDistance);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(limitViewAngle);
            if (sensor.LimitViewAngle)
            {
                EditorGUILayout.PropertyField(maxHorizAngle);
                ScalingFunctionProperty(visibilityByHorizAngle);
                EditorGUILayout.PropertyField(maxVertAngle);
                ScalingFunctionProperty(visibilityByVertAngle);
            }

            EditorGUILayout.Space();

            if (sensor.LimitDistance || sensor.LimitViewAngle)
            {
                EditorGUILayout.PropertyField(fovConstraintMethod);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(minimumVisibility);

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
            }
        }

        protected override void InspectorDetectedObjects()
        {
            EditorGUILayout.Space();

            VisibilityTable();

            SceneView.RepaintAll();
        }

        void ScalingFunctionProperty(SerializedProperty property)
        {
            var mode = property.FindPropertyRelative("Mode");
            var curve = property.FindPropertyRelative("Curve");
            EditorGUILayout.PropertyField(mode, new GUIContent(property.displayName));
            var modeVal = (ScalingMode) mode.intValue;
            if (modeVal == ScalingMode.Curve)
            {
                EditorGUILayout.PropertyField(curve, new GUIContent(" "));
            }
        }

        void VisibilityTable()
        {
            var headerRow = EditorGUILayout.GetControlRect();
            VisibilityHeaders(headerRow);

            var losResults = sensor.GetAllResults();
            losResults.Sort(delegate(ILOSResult r1, ILOSResult r2)
            {
                if (r1.Visibility < r2.Visibility)
                {
                    return 1;
                }
                else if (r1.Visibility == r2.Visibility)
                {
                    return 0;
                }

                return -1;
            });

            foreach (var result in losResults)
            {
                var rect = EditorGUILayout.GetControlRect();
                VisibilityRow(rect, result);
            }
        }

        void VisibilityColumns(Rect rect, out Rect showRaysCol, out Rect visibilityCol, out Rect signalCol)
        {
            var width = rect.width;
            var showRaysWidth = Mathf.Min(50f, width / 3);
            showRaysCol = new Rect(rect.x, rect.y, showRaysWidth, rect.height);

            width -= showRaysWidth;
            var visibilityWidth = Mathf.Min(80f, width / 2);
            visibilityCol = new Rect(showRaysCol.xMax, rect.y, visibilityWidth, rect.height);

            width -= visibilityWidth;
            signalCol = new Rect(visibilityCol.xMax, rect.y, width, rect.height);
        }

        void VisibilityHeaders(Rect rect)
        {
            Rect rRaysCol, rVisCol, rSigCol;
            VisibilityColumns(rect, out rRaysCol, out rVisCol, out rSigCol);

            var headerColumnStyle = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold, padding = new RectOffset(0, 16, 0, 0)};

            if (IsActivePulsable)
            {
                GUI.Label(rRaysCol, "Show", new GUIStyle(headerColumnStyle) {alignment = TextAnchor.UpperCenter});
            }

            GUI.Label(rVisCol, "Visibility", new GUIStyle(headerColumnStyle) {alignment = TextAnchor.UpperRight});
            GUI.Label(rSigCol, "Output Signal", new GUIStyle(headerColumnStyle) {alignment = TextAnchor.UpperLeft});
        }

        void VisibilityRow(Rect rect, ILOSResult losResult)
        {
            Rect rRaysCol, rVisCol, rSigCol;
            VisibilityColumns(rect, out rRaysCol, out rVisCol, out rSigCol);

            rRaysCol.xMin += rRaysCol.width / 2f - 16;
            rVisCol.xMax -= 16;

            var signal = losResult.OutputSignal;

            if (IsActivePulsable)
            {
                var debug = sensor.ShowRayCastDebug.Contains(signal.Object);
                if (debug = EditorGUI.Toggle(rRaysCol, debug))
                {
                    sensor.ShowRayCastDebug.Add(signal.Object);
                }
                else
                {
                    sensor.ShowRayCastDebug.Remove(signal.Object);
                }
            }

            var visStyle = new GUIStyle(EditorStyles.label) {alignment = TextAnchor.MiddleRight};
            if (!losResult.IsVisible)
            {
                visStyle.normal.textColor = Color.red;
            }

            GUI.Label(rVisCol, string.Format("{0:P1}", losResult.Visibility), visStyle);

            SignalField(rSigCol, signal);
        }
    }
}