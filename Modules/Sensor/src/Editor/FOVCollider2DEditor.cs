using UnityEditor;

namespace Pancake.Sensor
{
    [CustomEditor(typeof(FOVCollider2D))]
    [CanEditMultipleObjects]
    public class FOVCollider2DEditor : Editor
    {
        SerializedProperty length;
        SerializedProperty nearDistance;
        SerializedProperty fovAngle;
        SerializedProperty resolution;

        void OnEnable()
        {
            if (serializedObject == null) return;

            length = serializedObject.FindProperty("Length");
            nearDistance = serializedObject.FindProperty("NearDistance");
            fovAngle = serializedObject.FindProperty("FOVAngle");
            resolution = serializedObject.FindProperty("Resolution");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(nearDistance);
            EditorGUILayout.PropertyField(fovAngle);
            EditorGUILayout.PropertyField(resolution);

            serializedObject.ApplyModifiedProperties();
        }
    }
}