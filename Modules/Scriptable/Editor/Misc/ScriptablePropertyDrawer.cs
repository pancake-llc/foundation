using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Scriptable;

namespace Pancake.ScriptableEditor
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ScriptableBase), true)]
    public class ScriptablePropertyDrawer : PropertyDrawer
    {
        private SerializedObject _serializedObject;

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
                //Draw property and a create button
                var rectPosition = position;
                rectPosition.width = position.width * 0.8f;
                EditorGUI.PropertyField(rectPosition, property, label);

                rectPosition.x += rectPosition.width + 5f;
                rectPosition.width = position.width * 0.2f - 5f;
                var guiContent = new GUIContent("Create", "Creates the SO at default script generate path");
                if (GUI.Button(rectPosition, guiContent))
                {
                    //fieldInfo.Name does not work for VariableReferences so we have to make an edge case for that.
                    bool isAbstract = fieldInfo.DeclaringType?.IsAbstract == true;
                    string newName = isAbstract ? fieldInfo.FieldType.Name : fieldInfo.Name;
                    property.objectReferenceValue = EditorCreator.CreateScriptableAt(fieldInfo.FieldType, newName, ProjectDatabase.DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED);;
                }

                EditorGUI.EndProperty();
                return;
            }

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
                var editor = Editor.CreateEditor(targetObject);
                editor.OnInspectorGUI();
                GUI.backgroundColor = cacheBgColor;
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
                //needed for list to refresh during play mode when modified
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            else
            {
                if (targetObject is ScriptableVariableBase)
                    DrawValueOnTheSameLine(position, property, label, targetObject);
                else
                    EditorGUI.PropertyField(rect, property, label);
            }

            EditorGUI.EndProperty();
        }

        private void DrawValueOnTheSameLine(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            if (_serializedObject == null || _serializedObject.targetObject != targetObject)
                _serializedObject = new SerializedObject(targetObject);

            _serializedObject.UpdateIfRequiredOrScript();
            var rect = position;
            rect.width = position.width * 0.8f;
            EditorGUI.PropertyField(rect, property, label);

            rect.x += rect.width + 5f;
            rect.width = position.width * 0.2f - 5f;
            var value = _serializedObject.FindProperty("_value");
            EditorGUI.PropertyField(rect, value, GUIContent.none);
            _serializedObject.ApplyModifiedProperties();
        }
    }
}