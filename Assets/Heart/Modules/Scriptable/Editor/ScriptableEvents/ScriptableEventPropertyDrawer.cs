using System.Reflection;
using Pancake.Apex;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableEvent<>), true)]
    public class ScriptableEventPropertyDrawer : ScriptableBasePropertyDrawer
    {
        private SerializedObject _serializedObject;
        private ScriptableEventBase _scriptableEventBase;

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
            
            bool isNeedIndent = fieldInfo.FieldType.IsCollectionType() && fieldInfo.GetCustomAttribute<ArrayAttribute>(false) != null;
            DrawIfNotNull(position, property, label, property.objectReferenceValue, isNeedIndent);

            EditorGUI.EndProperty();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            if (_serializedObject == null || _serializedObject.targetObject != targetObject) _serializedObject = new SerializedObject(targetObject);
            _serializedObject.UpdateIfRequiredOrScript();

            if (_scriptableEventBase == null) _scriptableEventBase = _serializedObject.targetObject as ScriptableEventBase;
            var genericType = _scriptableEventBase.GetGenericType;

            if (!EditorExtend.IsSerializable(genericType))
            {
                EditorExtend.DrawSerializationError(genericType, position);
                return;
            }

            GUI.enabled = EditorApplication.isPlaying;
            if (GUI.Button(position, "Raise"))
            {
                var method = property.objectReferenceValue.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                var asset = new SerializedObject(property.objectReferenceValue);
                var valueProp = asset.FindProperty("debugValue");
                method.Invoke(targetObject, new[] {GetDebugValue(valueProp)});
            }

            GUI.enabled = true;
        }

        private object GetDebugValue(SerializedProperty property)
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField("debugValue", BindingFlags.Instance | BindingFlags.NonPublic);
            return targetField.GetValue(property.serializedObject.targetObject);
        }
    }
}