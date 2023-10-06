using System;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [CustomPropertyDrawer(typeof(GuidAttribute))]
    public class GuidAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrEmpty(property.stringValue)) Recreate(property);
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, "Id field must be a string");
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
            {
                var context = new GenericMenu();
                context.AddItem(new GUIContent("Copy"), false, _ => Copy(property), property);
                if (GUIUtility.systemCopyBuffer != null)
                {
                    context.AddItem(new GUIContent("Paste"), false, _ => Paste(property), property);
                }
                else
                {
                    context.AddDisabledItem(new GUIContent("Paste"));
                }

                context.AddItem(new GUIContent("Paste"), false, _ => Paste(property), property);
                context.AddItem(new GUIContent("Recreate"), false, _ => Recreate(property), property);
                context.ShowAsContext();
            }
        }

        private void Recreate(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String) return;
            property.stringValue = Guid.NewGuid().ToString("N")[..15]; // slower than Guid.NewGuid().ToString() but shorter
            property.serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void Copy(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String) return;

            var textEditor = new TextEditor {text = property.stringValue};
            textEditor.SelectAll();
            textEditor.Copy();
        }

        private void Paste(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String) return;

            property.stringValue = GUIUtility.systemCopyBuffer;
            property.serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}