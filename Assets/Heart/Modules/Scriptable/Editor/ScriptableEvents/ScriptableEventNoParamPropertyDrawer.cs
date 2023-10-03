using System.Reflection;
using Pancake.Apex;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableEventNoParam), true)]
    public class ScriptableEventNoParamPropertyDrawer : ScriptableBasePropertyDrawer
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
            
            bool isNeedIndent = fieldInfo.FieldType.IsCollectionType() && fieldInfo.GetCustomAttribute<ArrayAttribute>(false) != null;
            DrawIfNotNull(position, property, label, property.objectReferenceValue, isNeedIndent);

            EditorGUI.EndProperty();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            GUI.enabled = EditorApplication.isPlaying;
            if (GUI.Button(position, "Raise"))
            {
                var eventNoParam = (ScriptableEventNoParam) property.objectReferenceValue;
                eventNoParam.Raise();
            }

            GUI.enabled = true;
        }
    }
}