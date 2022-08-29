namespace Pancake.Database
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePropertyDrawer : PropertyDrawer
    {
        private const float SIZE = 70;

        public override float GetPropertyHeight(SerializedProperty p, GUIContent label)
        {
            return p.objectReferenceValue != null ? SIZE : base.GetPropertyHeight(p, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue != null)
            {
                position.width = EditorGUIUtility.labelWidth;
                GUI.Label(position, property.displayName);
                position.x += position.width;
                position.width = SIZE;
                position.height = SIZE;
                property.objectReferenceValue = EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(Sprite), false);
            }
            else
            {
                GUI.Label(position, property.displayName);
                EditorGUI.PropertyField(position, property, true);
            }

            EditorGUI.EndProperty();
        }
    }
}