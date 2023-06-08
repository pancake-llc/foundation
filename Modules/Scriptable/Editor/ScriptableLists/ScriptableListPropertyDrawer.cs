using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableListBase), true)]
    public class ScriptableListPropertyDrawer : ScriptableBasePropertyDrawer
    {
        private SerializedObject _serializedObject;

        protected override void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            if (_serializedObject == null || _serializedObject.targetObject != targetObject) _serializedObject = new SerializedObject(targetObject);

            _serializedObject.UpdateIfRequiredOrScript();
            base.DrawUnExpanded(position, property, label, targetObject);
            _serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            var value = _serializedObject.FindProperty("list");
            if (value != null)
            {
                int count = value.arraySize;
                EditorGUI.LabelField(position, "Count: " + count);
            }
        }
    }
}