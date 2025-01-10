using Pancake;
using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(HeartEditorSettings), true)]
    public class HeartEditorSettingDrawer : Editor
    {
        private SerializedProperty _nameCreationModeProperty;
        private SerializedProperty _targetFrameRateProperty;
        private SerializedProperty _toolbarTimeScaleProperty;

        private void OnEnable()
        {
            _nameCreationModeProperty = serializedObject.FindProperty("nameCreationMode");
            _targetFrameRateProperty = serializedObject.FindProperty("targetFrameRate");
            _toolbarTimeScaleProperty = serializedObject.FindProperty("toolbarTimeScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_nameCreationModeProperty);
            EditorGUILayout.PropertyField(_targetFrameRateProperty);
            EditorGUILayout.PropertyField(_toolbarTimeScaleProperty);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}