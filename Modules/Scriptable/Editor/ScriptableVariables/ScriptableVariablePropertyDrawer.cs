using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariablePropertyDrawer : ScriptableBasePropertyDrawer
    {
        private SerializedObject _serializedObject;

        protected override string GetFieldName()
        {
            //fieldInfo.Name does not work for VariableReferences so we have to make an edge case for that.
            var isAbstract = fieldInfo.DeclaringType?.IsAbstract == true;
            var fieldName = isAbstract ? fieldInfo.FieldType.Name : fieldInfo.Name;
            return fieldName;
        }

        protected override void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label,
            Object targetObject)
        {
            if (_serializedObject == null || _serializedObject.targetObject != targetObject)
                _serializedObject = new SerializedObject(targetObject);

            _serializedObject.UpdateIfRequiredOrScript();
            base.DrawUnExpanded(position,property,label,targetObject);
            _serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            var value = _serializedObject.FindProperty("value");
            EditorGUI.PropertyField(position, value, GUIContent.none);
        }
    }
}