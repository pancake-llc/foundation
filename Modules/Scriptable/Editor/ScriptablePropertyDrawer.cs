using System.IO;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomPropertyDrawer(typeof(ScriptableBase), true)]
    public class ScriptablePropertyDrawer : PropertyDrawer
    {
        private SerializedObject _serializedObject;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
            {
                //Draw property and a create button
                var rectPosition = position;
                rectPosition.width = position.width * 0.8f;
                EditorGUI.PropertyField(rectPosition, property, label);

                rectPosition.x += rectPosition.width + 5f;
                rectPosition.width = position.width * 0.2f - 5f;
                if (GUI.Button(rectPosition, "Create"))
                {
                    const string path = "Assets/_Root/DataStorages/Generated";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    var instance = ScriptableObject.CreateInstance(fieldInfo.FieldType);
                    instance.name = fieldInfo.Name == "" ? fieldInfo.FieldType.ToString().Replace("Pancake.Scriptable.", "") : fieldInfo.Name;
                    string uniqueFilePath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{instance.name}.asset");
                    AssetDatabase.CreateAsset(instance, uniqueFilePath);
                    property.objectReferenceValue = instance;
                    EditorGUIUtility.PingObject(instance);
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
                GUI.backgroundColor = Uniform.Pink;
                GUILayout.BeginVertical(GUI.skin.box);
                var editor = UnityEditor.Editor.CreateEditor(targetObject);
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