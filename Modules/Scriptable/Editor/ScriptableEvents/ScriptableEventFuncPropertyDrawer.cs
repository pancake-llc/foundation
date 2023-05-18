using System.Reflection;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableEventFunc<,>), true)]
    public class ScriptableEventFuncPropertyDrawer : ScriptableBasePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
            {
                DrawIfNull(position, property, label);
                return;
            }

            //TODO: make this more robust. Disable foldout fo all arrays of serializable class that contain ScriptableBase
            var isEventListener = property.serializedObject.targetObject is EventListenerBase;
            if (isEventListener)
            {
                DrawUnExpanded(position, property, label, targetObject);
                EditorGUI.EndProperty();
                return;
            }

            DrawIfNotNull(position, property, label, property.objectReferenceValue);

            EditorGUI.EndProperty();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            GUI.enabled = EditorApplication.isPlaying;
            if (GUI.Button(position, "Raise"))
            {
                var method = property.objectReferenceValue.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                var asset = new SerializedObject(property.objectReferenceValue);
                var valueProp = asset.FindProperty("_debugValue");
                method.Invoke(targetObject, new[] {GetDebugValue(valueProp)});
            }

            GUI.enabled = true;
        }

        private object GetDebugValue(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField("_debugValue", BindingFlags.Instance | BindingFlags.NonPublic);
            return targetField.GetValue(property.serializedObject.targetObject);
        }
    }
}