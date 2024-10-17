using Pancake;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Editor = UnityEditor.Editor;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ScriptableObjectPropertyDrawer : PropertyDrawer
    {
        private Editor _editor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var targetObject = property.objectReferenceValue;
            var isInCollection = true;
            if (fieldInfo != null) isInCollection = fieldInfo.FieldType.IsCollectionType();
            
            if (targetObject == null)
            {
                DrawIfNull(position, property, label, isInCollection);
                return;
            }
            
            DrawIfNotNull(position,
                property,
                label,
                targetObject,
                isInCollection);

            EditorGUI.EndProperty();
        }

        private void DrawIfNull(Rect position, SerializedProperty property, GUIContent label, bool isInCollection)
        {
            if (fieldInfo != null && (fieldInfo.FieldType.IsAbstract || isInCollection && fieldInfo.FieldType.GetCorrectElementType().IsAbstract))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            //Draw property and a create button
            var rect = DrawPropertyField(position, property, label);
            var guiContent = new GUIContent("Create", "Creates the SO at default storage generate path");

            if (GUI.Button(rect, guiContent))
            {
                string newName = GetFieldName(property).ToSnakeCase();
                var typeCreate = fieldInfo != null ? fieldInfo.FieldType : GetTypeFromSerializedProperty(property);

                var elementType = typeCreate.GetCorrectElementType();
                if (elementType != null) typeCreate = elementType;

#pragma warning disable CS0612
                property.objectReferenceValue = EditorCreator.CreateScriptableAt(typeCreate,
                    newName,
                    ProjectDatabase.DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED,
                    HeartEditorSettings.EditorNameCreationMode == ECreationMode.Auto);
#pragma warning restore CS0612
            }

            EditorGUI.EndProperty();
        }

        private Rect DrawPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            var rectPosition = position;
            const float propertyWidthRatio = 0.82f;
            rectPosition.width = position.width * propertyWidthRatio;
            EditorGUI.PropertyField(rectPosition, property, label);

            rectPosition.x += rectPosition.width + 5f;
            rectPosition.width = position.width * (1 - propertyWidthRatio) - 5f;
            return rectPosition;
        }

        private void DrawIfNotNull(Rect position, SerializedProperty property, GUIContent label, Object targetObject, bool isInCollection)
        {
            var rect = position;
            var labelRect = position;
            labelRect.width = position.width * 0.4f; //only expands on the first half on the window when clicked

            if (!isInCollection)
            {
                labelRect.position -= new Vector2(15, 0);
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, new GUIContent(""), true);
                int indent = EditorGUI.indentLevel;
                if (property.isExpanded)
                {
                    //Draw an embedded inspector
                    rect.width = position.width;
                    EditorGUI.PropertyField(rect, property, label);
                    var cacheBgColor = GUI.backgroundColor;
                    GUI.backgroundColor = EditorGUIUtility.isProSkin ? Uniform.Sky_300 : Uniform.Pink_300;
                    GUILayout.BeginVertical(GUI.skin.box);
                    if (_editor == null) Editor.CreateCachedEditor(targetObject, null, ref _editor);
                    _editor.OnInspectorGUI();
                    GUI.backgroundColor = cacheBgColor;
                    GUILayout.EndVertical();
                    EditorGUI.indentLevel = indent;
                }
                else
                {
                    _editor = null;
                    DrawUnExpanded(position, property, label, targetObject);
                }
            }
            else DrawUnExpanded(position, property, label, targetObject);
        }

        protected virtual string GetFieldName(SerializedProperty property)
        {
            if (fieldInfo == null)
            {
                string[] pathParts = property.propertyPath.Split('.');
                return pathParts[^1];
            }

            return fieldInfo.Name;
        }

        protected virtual void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label, Object targetObject)
        {
            if (property.serializedObject.isEditingMultipleObjects || property.objectReferenceValue == null)
            {
                base.OnGUI(position, property, label);
                return;
            }

            var inner = new SerializedObject(property.objectReferenceValue);
            var valueProp = inner.FindProperty("value");

            if (valueProp == null || valueProp.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(position, property, label);
                inner.ApplyModifiedProperties();
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            var rect = DrawPropertyField(position, property, label);
            if (valueProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                GUI.enabled = false;
                EditorGUI.ObjectField(rect,
                    GUIContent.none,
                    valueProp.objectReferenceValue,
                    typeof(Object),
                    true);
                GUI.enabled = true;
            }
            else
            {
                EditorGUI.PropertyField(rect, valueProp, GUIContent.none, false);
            }

            inner.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        private System.Type GetTypeFromSerializedProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var obj = property.objectReferenceValue;
                if (obj != null) return obj.GetType();
            }

            var t = property.serializedObject.targetObject.GetType();
#if ODIN_INSPECTOR
            const string emittedKeyPrefix = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedSOProperty_Key_";
            const string emittedValuePrefix = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedSOProperty_Value_";
            if (t.FullName != null)
            {
                if (t.FullName.StartsWith(emittedKeyPrefix))
                {
                    string typePart = t.FullName[emittedKeyPrefix.Length..];
                    TypeExtensions.FindTypeByFullName(typePart, out var type);
                    t = type;
                }
                else if (t.FullName.StartsWith(emittedValuePrefix))
                {
                    string typePart = t.FullName[emittedValuePrefix.Length..];
                    TypeExtensions.FindTypeByFullName(typePart, out var type);
                    t = type;
                }
            }
#endif

            return t;
        }
    }
}