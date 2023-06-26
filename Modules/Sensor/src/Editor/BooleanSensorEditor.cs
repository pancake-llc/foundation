using Pancake.Sensor;
using UnityEditor;

namespace Pancake.SensorEditor
{
    [CustomEditor(typeof(BooleanSensor))]
    [CanEditMultipleObjects]
    public class BooleanSensorEditor : BaseSensorEditor<BooleanSensor>
    {
        SerializedProperty inputSensors;
        SerializedProperty operation;
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

            inputSensors = serializedObject.FindProperty("InputSensors");
            operation = serializedObject.FindProperty("operation");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");
        }

        protected override void InspectorParameters()
        {
            EditorGUILayout.PropertyField(inputSensors, true);

            EditorGUILayout.PropertyField(operation);

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events"))
            {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onSomeDetection);
                EditorGUILayout.PropertyField(onNoDetection);
            }
        }
    }
}