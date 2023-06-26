using UnityEditor;

namespace Pancake.Sensor
{
    [CustomEditor(typeof(FOVCollider))]
    [CanEditMultipleObjects]
    public class FOVColliderEditor : Editor
    {
        SerializedProperty length;
        SerializedProperty nearDistance;
        SerializedProperty fovAngle;
        SerializedProperty elevationAngle;
        SerializedProperty resolution;

        void OnEnable()
        {
            if (serializedObject == null) return;

            length = serializedObject.FindProperty("Length");
            nearDistance = serializedObject.FindProperty("NearDistance");
            fovAngle = serializedObject.FindProperty("FOVAngle");
            elevationAngle = serializedObject.FindProperty("ElevationAngle");
            resolution = serializedObject.FindProperty("Resolution");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(nearDistance);
            EditorGUILayout.PropertyField(fovAngle);
            EditorGUILayout.PropertyField(elevationAngle);
            EditorGUILayout.PropertyField(resolution);

            serializedObject.ApplyModifiedProperties();
        }
    }
}