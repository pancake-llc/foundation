using Pancake.Scriptable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(BindText))]
    [CanEditMultipleObjects]
    public class BindTextDrawer : UnityEditor.Editor
    {
        BindText _targetScript;
        SerializedProperty _boolVariableProperty;
        SerializedProperty _intVariableProperty;
        SerializedProperty _floatVariableProperty;
        SerializedProperty _stringVariableProperty;

        void OnEnable()
        {
            _targetScript = (BindText) target;
            _boolVariableProperty = serializedObject.FindProperty("_boolVariable");
            _intVariableProperty = serializedObject.FindProperty("_intVariable");
            _floatVariableProperty = serializedObject.FindProperty("_floatVariable");
            _stringVariableProperty = serializedObject.FindProperty("_stringVariable");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(_targetScript, "Modified Custom Inspector");
            _targetScript.type = (CustomVariableType) EditorGUILayout.EnumPopup("Variable Type", _targetScript.type);
            _targetScript.prefix = EditorGUILayout.TextField(new GUIContent("Prefix", "Adds a text in front of the value"), _targetScript.prefix);
            _targetScript.suffix = EditorGUILayout.TextField(new GUIContent("Suffix", "Adds a text after the value"), _targetScript.suffix);

            switch (_targetScript.type)
            {
                case CustomVariableType.None:
                    break;
                case CustomVariableType.Bool:
                    EditorGUILayout.PropertyField(_boolVariableProperty, new GUIContent("Bool"));
                    break;
                case CustomVariableType.Int:
                    EditorGUILayout.PropertyField(_intVariableProperty, new GUIContent("Int"));
                    _targetScript.increment = EditorGUILayout.IntField(new GUIContent("Increment",
                            "Useful to add an offset, for example for Level counts. If your level index is  0, add 1, so it displays Level : 1"),
                        _targetScript.increment);
                    _targetScript.isClamped = EditorGUILayout.Toggle(new GUIContent("Is Clamped", "Clamps the value shown to a minimum and a maximum."),
                        _targetScript.isClamped);
                    if (_targetScript.isClamped)
                    {
                        var minMaxInt = EditorGUILayout.Vector2IntField("Min Max", _targetScript.minMaxInt);
                        _targetScript.minMaxInt = minMaxInt;
                    }

                    break;
                case CustomVariableType.Float:
                    EditorGUILayout.PropertyField(_floatVariableProperty, new GUIContent("Float"));
                    var decimalAmount = EditorGUILayout.IntField(new GUIContent("Decimal", "Round the float to a decimal"), _targetScript.decimalAmount);
                    _targetScript.decimalAmount = Mathf.Clamp(decimalAmount, 0, 5);
                    _targetScript.isClamped = EditorGUILayout.Toggle(new GUIContent("Is Clamped", "Clamps the value shown to a minimum and a maximum."),
                        _targetScript.isClamped);
                    if (_targetScript.isClamped)
                    {
                        var minMaxFloat = EditorGUILayout.Vector2Field("Min Max", _targetScript.minMaxFloat);
                        _targetScript.minMaxFloat = minMaxFloat;
                    }

                    break;
                case CustomVariableType.String:
                    EditorGUILayout.PropertyField(_stringVariableProperty, new GUIContent("String"));
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }
    }
}