using Pancake.MobileInput;
using Pancake.ScriptableEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.MobileInputEditor
{
    [CustomPropertyDrawer(typeof(ScriptableEventVector3Transform), true)]
    public class ScriptableEventVector3TransformPropertyDrawer : ScriptableBasePropertyDrawer
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