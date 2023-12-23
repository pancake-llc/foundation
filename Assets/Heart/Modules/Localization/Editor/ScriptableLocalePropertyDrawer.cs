using Pancake.ExLibEditor;
using Pancake.Localization;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Pancake.Apex;

namespace Pancake.LocalizationEditor
{
    [CustomPropertyDrawer(typeof(ScriptableLocaleBase), true)]
    public class ScriptableLocalePropertyDrawer : PropertyDrawer
    {
        private Editor _editor;
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

            bool isNeedIndent = fieldInfo.FieldType.IsCollectionType() && fieldInfo.GetCustomAttribute<ArrayAttribute>(false) != null;
            DrawIfNotNull(position,
                property,
                label,
                targetObject,
                isNeedIndent);

            EditorGUI.EndProperty();
        }

        private void DrawIfNull(Rect position, SerializedProperty property, GUIContent label)
        {
            //Draw property and a create button
            var rect = DrawPropertyField(position, property, label);
            var guiContent = new GUIContent("Create", "Creates the SO at default storage generate path");
            if (GUI.Button(rect, guiContent))
            {
                string newName = fieldInfo.Name.ToSnakeCase();
                var typeCreate = fieldInfo.FieldType;

                var elementType = fieldInfo.FieldType.GetCorrectElementType();
                if (elementType != null) typeCreate = elementType;
#pragma warning disable CS0612
                property.objectReferenceValue = EditorCreator.CreateScriptableAt(typeCreate,
                    newName,
                    ProjectDatabase.DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED,
                    HeartSettings.EditorNameCreationMode == ENameAssetCreationMode.Auto);
#pragma warning restore CS0612
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

        private void DrawIfNotNull(Rect position, SerializedProperty property, GUIContent label, Object targetObject, bool isNeedIndent)
        {
            var rect = position;
            var labelRect = position;
            labelRect.width = position.width * 0.4f; //only expands on the first half on the window when clicked

            if (isNeedIndent) labelRect.position += new Vector2(10, 0);
            if (isNeedIndent) label.text = $"   {label.text.Trim()}";
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
                if (_editor == null) UnityEditor.Editor.CreateCachedEditor(targetObject, null, ref _editor);
                _editor.OnInspectorGUI();
                GUILayout.Space(28f); // for help box
                GUI.backgroundColor = cacheBgColor;
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}