using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Monetization;
using UnityEditor;
using UnityEngine;

namespace Pancake.MonetizationEditor
{
    [CustomPropertyDrawer(typeof(AdClient), true)]
    public class AdClientDrawer : PropertyDrawer
    {
        private UnityEditor.Editor _editor;
        private const float PROPERTY_WIDTH_RATIO = 0.82f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            if (fieldInfo.FieldType.IsArray || fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // ignore draw when inside list or array
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (targetObject == null)
            {
                if (DrawAbstractPropertyField(fieldInfo.FieldType, position, property, label)) return;
                DrawIfNull(position, property, label);
                return;
            }

            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }

        protected void DrawIfNull(Rect position, SerializedProperty property, GUIContent label)
        {
            //Draw property and a create button
            var rect = DrawPropertyField(position, property, label);
            var guiContent = new GUIContent("Create", "Creates the SO at default script generate path");
            if (GUI.Button(rect, guiContent))
            {
                string newName = fieldInfo.Name.ToSnackCase();
                property.objectReferenceValue = EditorCreator.CreateScriptableAt(fieldInfo.FieldType, newName, ProjectDatabase.DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED);
                var serializedObject = new SerializedObject(property.objectReferenceValue);
                serializedObject.UpdateIfRequiredOrScript();
                var prop = serializedObject.FindProperty("adSettings");
                prop.objectReferenceValue = ProjectDatabase.FindAll<AdSettings>().First();
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        private Rect DrawPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            var rectPosition = position;
            rectPosition.width = position.width * PROPERTY_WIDTH_RATIO;
            EditorGUI.PropertyField(rectPosition, property, label);

            rectPosition.x += rectPosition.width + 5f;
            rectPosition.width = position.width * (1 - PROPERTY_WIDTH_RATIO) - 5f;
            return rectPosition;
        }

        private bool DrawAbstractPropertyField(Type type, Rect position, SerializedProperty property, GUIContent label)
        {
            if (type == typeof(AdUnitVariable))
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return true;
            }

            return false;
        }
    }
}