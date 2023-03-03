using Pancake.Scriptable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BindComparisonToUnityEvent))]
    public class BindComparisonToUnityEventDrawer : UnityEditor.Editor
    {
        private BindComparisonToUnityEvent _bindComparison;
        private SerializedProperty _boolVariable;
        private SerializedProperty _boolComparer;
        private SerializedProperty _intVariable;
        private SerializedProperty _intComparer;
        private SerializedProperty _floatVariable;
        private SerializedProperty _floatComparer;
        private SerializedProperty _stringVariable;
        private SerializedProperty _stringComparer;
        private SerializedProperty _unityEvent;

        private void OnEnable()
        {
            _bindComparison = (BindComparisonToUnityEvent) target;
            _boolVariable = serializedObject.FindProperty("boolVariable");
            _boolComparer = serializedObject.FindProperty("boolComparer");
            _intVariable = serializedObject.FindProperty("intVariable");
            _intComparer = serializedObject.FindProperty("intComparer");
            _floatVariable = serializedObject.FindProperty("floatVariable");
            _floatComparer = serializedObject.FindProperty("floatComparer");
            _stringVariable = serializedObject.FindProperty("stringVariable");
            _stringComparer = serializedObject.FindProperty("stringComparer");
            _unityEvent = serializedObject.FindProperty("unityEvent");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(_bindComparison, "Modified Custom Inspector");
            _bindComparison.type = (CustomVariableType) EditorGUILayout.EnumPopup("Variable Type", _bindComparison.type);

            switch (_bindComparison.type)
            {
                case CustomVariableType.None:
                    break;
                case CustomVariableType.Bool:
                    EditorGUILayout.PropertyField(_boolVariable, new GUIContent("Bool Variable"));
                    EditorGUILayout.PropertyField(_boolComparer, new GUIContent("Bool Comparer"));
                    EditorGUILayout.PropertyField(_unityEvent, new GUIContent("Event"));
                    break;
                case CustomVariableType.Int:
                    EditorGUILayout.PropertyField(_intVariable, new GUIContent("Int Variable"));
                    _bindComparison.comparison = (BindComparisonToUnityEvent.Comparator) EditorGUILayout.EnumPopup("Operation", _bindComparison.comparison);
                    EditorGUILayout.PropertyField(_intComparer, new GUIContent("Int Comparer"));
                    EditorGUILayout.PropertyField(_unityEvent, new GUIContent("Event"));
                    break;
                case CustomVariableType.Float:
                    EditorGUILayout.PropertyField(_floatVariable, new GUIContent("Float Variable"));
                    _bindComparison.comparison = (BindComparisonToUnityEvent.Comparator) EditorGUILayout.EnumPopup("Operation", _bindComparison.comparison);
                    EditorGUILayout.PropertyField(_floatComparer, new GUIContent("Float Comparer"));
                    EditorGUILayout.PropertyField(_unityEvent, new GUIContent("Event"));
                    break;
                case CustomVariableType.String:
                    EditorGUILayout.PropertyField(_stringVariable, new GUIContent("String Variable"));
                    EditorGUILayout.PropertyField(_stringComparer, new GUIContent("String Comparer"));
                    EditorGUILayout.PropertyField(_unityEvent, new GUIContent("Event"));
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }
    }
}