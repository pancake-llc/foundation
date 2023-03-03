using Pancake.Scriptable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BindTextMeshPro))]
    public class BindTextMeshProDrawer : UnityEditor.Editor
    {
        private BindTextMeshPro _bindText;
        private SerializedProperty _boolVariableProperty;
        private SerializedProperty _intVariableProperty;
        private SerializedProperty _floatVariableProperty;
        private SerializedProperty _stringVariableProperty;

        private void OnEnable()
        {
            _bindText = (BindTextMeshPro) target;
            _boolVariableProperty = serializedObject.FindProperty("boolVariable");
            _intVariableProperty = serializedObject.FindProperty("intVariable");
            _floatVariableProperty = serializedObject.FindProperty("floatVariable");
            _stringVariableProperty = serializedObject.FindProperty("stringVariable");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(_bindText, "Modified Custom Inspector");
            _bindText.type = (CustomVariableType) EditorGUILayout.EnumPopup("Variable Type", _bindText.type);
            _bindText.prefix = EditorGUILayout.TextField(new GUIContent("Prefix", "Adds a text in front of the value"), _bindText.prefix);
            _bindText.suffix = EditorGUILayout.TextField(new GUIContent("Suffix", "Adds a text after the value"), _bindText.suffix);

            switch (_bindText.type)
            {
                case CustomVariableType.None:
                    break;
                case CustomVariableType.Bool:
                    EditorGUILayout.PropertyField(_boolVariableProperty, new GUIContent("Bool"));
                    break;
                case CustomVariableType.Int:
                    EditorGUILayout.PropertyField(_intVariableProperty, new GUIContent("Int"));
                    _bindText.increment = EditorGUILayout.IntField(new GUIContent("Increment",
                            "Useful to add an offset, for example for Level counts. If your level index is  0, add 1, so it displays Level : 1"),
                        _bindText.increment);
                    _bindText.isClamped = EditorGUILayout.Toggle(new GUIContent("Is Clamped", "Clamps the value shown to a minimum and a maximum."),
                        _bindText.isClamped);
                    if (_bindText.isClamped)
                    {
                        var minMaxInt = EditorGUILayout.Vector2IntField("Min Max", _bindText.minMaxInt);
                        _bindText.minMaxInt = minMaxInt;
                    }

                    break;
                case CustomVariableType.Float:
                    EditorGUILayout.PropertyField(_floatVariableProperty, new GUIContent("Float"));
                    int decimalAmount = EditorGUILayout.IntField(new GUIContent("Decimal", "Round the float to a decimal"), _bindText.decimalAmount);
                    _bindText.decimalAmount = Mathf.Clamp(decimalAmount, 0, 5);
                    _bindText.isClamped = EditorGUILayout.Toggle(new GUIContent("Is Clamped", "Clamps the value shown to a minimum and a maximum."),
                        _bindText.isClamped);
                    if (_bindText.isClamped)
                    {
                        var minMaxFloat = EditorGUILayout.Vector2Field("Min Max", _bindText.minMaxFloat);
                        _bindText.minMaxFloat = minMaxFloat;
                    }

                    break;
                case CustomVariableType.String:
                    EditorGUILayout.PropertyField(_stringVariableProperty, new GUIContent("String"));
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