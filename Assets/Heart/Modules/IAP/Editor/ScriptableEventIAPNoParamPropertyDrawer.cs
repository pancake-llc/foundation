#if PANCAKE_IAP
using System.Reflection;
using Pancake.Apex;
using Pancake.ExLibEditor;
using Pancake.IAP;
using Pancake.Scriptable;
using UnityEditor;
using Pancake.ScriptableEditor;
using UnityEngine;

namespace Pancake.IAPEditor
{
    [CustomPropertyDrawer(typeof(ScriptableEventIAPNoParam), true)]
    public class ScriptableEventIAPNoParamPropertyDrawer : ScriptableBasePropertyDrawer
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
            DrawIfNotNull(position,
                property,
                label,
                property.objectReferenceValue,
                isNeedIndent);

            EditorGUI.EndProperty();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
            GUI.enabled = EditorApplication.isPlaying;
            if (GUI.Button(position, "Raise"))
            {
                var eventNoParam = (ScriptableEventIAPNoParam) property.objectReferenceValue;
                eventNoParam.Raise();
            }

            GUI.enabled = true;
        }
    }
}

#endif
