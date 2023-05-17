﻿using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomPropertyDrawer(typeof(ScriptableBase), true)]
    public abstract class ScriptableBasePropertyDrawer : PropertyDrawer
    {
        private UnityEditor.Editor _editor;
        private const float PROPERTY_WIDTH_RATIO = 0.82f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
            {
                DrawIfNull(position, property, label);
                return;
            }

            DrawIfNotNull(position, property, label, targetObject);

            EditorGUI.EndProperty();
        }

        protected void DrawIfNull(Rect position, SerializedProperty property, GUIContent label)
        {
            //Draw property and a create button
            var rect = DrawPropertyField(position, property, label);
            var guiContent = new GUIContent("Create", "Creates the SO at default script generate path");
            if (GUI.Button(rect, guiContent))
            {
                string newName = GetFieldName();
                property.objectReferenceValue = EditorCreator.CreateScriptableAt(fieldInfo.FieldType, newName, ProjectDatabase.DEFAULT_PATH_SCRIPT_GENERATED);
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

        protected void DrawIfNotNull(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            var rect = position;
            var labelRect = position;
            labelRect.width = position.width * 0.4f; //only expands on the first half on the window when clicked

            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, new GUIContent(""), true);
            if (property.isExpanded)
            {
                //Draw an embedded inspector 
                rect.width = position.width;
                EditorGUI.PropertyField(rect, property, label);
                EditorGUI.indentLevel++;
                var cacheBgColor = GUI.backgroundColor;
                GUI.backgroundColor = Uniform.FieryRose;
                GUILayout.BeginVertical(GUI.skin.box);
                if (_editor == null) _editor = UnityEditor.Editor.CreateEditor(targetObject);
                _editor.OnInspectorGUI();
                GUI.backgroundColor = cacheBgColor;
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            else
                DrawUnExpanded(position, property, label, targetObject);
        }

        protected virtual string GetFieldName() { return fieldInfo.Name; }

        protected virtual void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            var rect = DrawPropertyField(position, property, label);
            DrawShortcut(rect, property, targetObject);
        }

        protected abstract void DrawShortcut(Rect position, SerializedProperty property, Object targetObject);
    }
}