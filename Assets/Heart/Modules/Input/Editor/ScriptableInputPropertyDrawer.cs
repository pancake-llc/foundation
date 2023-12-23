using System.Reflection;
using Pancake.Apex;
using Pancake.ExLibEditor;
using Pancake.MobileInput;
using Pancake.ScriptableEditor;
using UnityEditor;

namespace Pancake.MobileInputEditor
{
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ScriptableInput), true)]
    public class ScriptableInputPropertyDrawer : ScriptableBasePropertyDrawer
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

            DrawUnExpanded(position, property, label, targetObject);

            EditorGUI.EndProperty();
        }

        protected override void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            EditorGUI.PropertyField(position, property, label);
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject) { }
    }
}