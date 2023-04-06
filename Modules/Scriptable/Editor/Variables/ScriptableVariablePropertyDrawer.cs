using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomPropertyDrawer(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariablePropertyDrawer : UnityEditor.PropertyDrawer
    {
        private SerializedObject _serializedObject;
        private readonly Color _bgColor = new Color(0.1f, 0.8352f, 1f, 1f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            if (_serializedObject == null || _serializedObject.targetObject != targetObject) _serializedObject = new SerializedObject(targetObject);

            _serializedObject.UpdateIfRequiredOrScript();

            var rect = position;
            var labelRect = position;
            labelRect.width = position.width * 0.4f; //prevent to only expand on the first half on the window
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, new GUIContent(""), true);

            if (property.isExpanded)
            {
                //Draw property on the full width
                rect.width = position.width;
                EditorGUI.PropertyField(rect, property, label);

                EditorGUI.indentLevel++;
                var cacheBgColor = GUI.backgroundColor;
                GUI.backgroundColor = _bgColor;
                GUILayout.BeginVertical(GUI.skin.box);
                var propertiesToHide = CanShowMinMaxProperty(targetObject) ? new[] {"m_Script"} : new[] {"m_Script", "minMax"};
                Uniform.DrawInspectorExcept(_serializedObject, propertiesToHide);
                GUI.backgroundColor = cacheBgColor;
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            else
            {
                rect.width = position.width * 0.8f;
                EditorGUI.PropertyField(rect, property, label);

                var offset = 5f;
                rect.x += rect.width + offset;
                rect.width = position.width * 0.2f - offset;
                var value = _serializedObject.FindProperty("value");
                EditorGUI.PropertyField(rect, value, GUIContent.none);
            }

            _serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        private bool CanShowMinMaxProperty(Object targetObject) { return IsIntClamped(targetObject) || IsFloatClamped(targetObject); }

        private bool IsIntClamped(Object targetObject)
        {
            var intVariable = targetObject as IntVariable;
            return intVariable != null && intVariable.IsClamped;
        }

        private bool IsFloatClamped(Object targetObject)
        {
            var floatVariable = targetObject as FloatVariable;
            return floatVariable != null && floatVariable.IsClamped;
        }
    }
}