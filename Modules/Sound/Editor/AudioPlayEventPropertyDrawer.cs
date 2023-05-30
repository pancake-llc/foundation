using Pancake.ScriptableEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Sound
{
    [CustomPropertyDrawer(typeof(AudioPlayEvent), true)]
    public class AudioPlayEventPropertyDrawer : ScriptableBasePropertyDrawer
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

            DrawIfNotNull(position, property, label, property.objectReferenceValue);

            EditorGUI.EndProperty();
        }

        protected override void DrawShortcut(Rect position, SerializedProperty property, Object targetObject) { }
    }
}