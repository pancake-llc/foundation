using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal partial class SI_RawReferenceDrawer : SI_ReferenceDrawer, SI_IReferenceDrawer
    {
        private GUIContent _label;
        private FieldInfo _fieldInfo;

        private object _previousReferenceValue;
        private string _previousPropertyPath;

        public void Initialize(SerializedProperty property, Type genericType, GUIContent label, FieldInfo fieldInfo)
        {
            Initialize(property, genericType, fieldInfo);
            _label = label;
        }

        /// <inheritdoc />
        public float GetHeight()
        {
            if (RawReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUI.GetPropertyHeight(RawReferenceProperty, true);
        }

        /// <inheritdoc />
        public void OnGUI(Rect position)
        {
            AvoidDuplicateReferencesInArray();

            Rect objectFieldRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};

            object rawReferenceValue = RawReferenceValue;

            GUIContent content = rawReferenceValue == null
                ? EditorGUIUtility.ObjectContent((MonoScript) null, typeof(MonoScript))
                : new GUIContent(rawReferenceValue.GetType().Name, SI_IconUtility.ScriptIcon);

            customObjectDrawer.OnGUI(objectFieldRect, _label, content, Property);

            HandleDragAndDrop(objectFieldRect);

            if (rawReferenceValue == null)
                return;

            DrawProperty(position);
            _previousPropertyPath = Property.propertyPath;
        }

        private void DrawProperty(Rect position) { EditorGUI.PropertyField(position, RawReferenceProperty, GUIContent.none, true); }

        protected override void PingObject(SerializedProperty property)
        {
            // No support for pinging raw objects for now (I guess this would ping the MonoScript?)
        }

        /// <inheritdoc />
        protected override void OnPropertiesClicked(SerializedProperty property)
        {
            if (RawReferenceValue == null)
                return;

            Type type = RawReferenceValue.GetType();
            string typeName = type.Name;

            string[] guids = AssetDatabase.FindAssets($"t:Script {typeName}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if (monoScript.GetClass() == type)
                {
                    SI_PropertyEditorUtility.Show(monoScript);
                    return;
                }
            }
        }

        private void AvoidDuplicateReferencesInArray()
        {
            if (!ShouldCheckProperty())
                return;

            object currentReferenceValue = RawReferenceValue;

            if (currentReferenceValue == null)
                return;

            if (_previousReferenceValue == currentReferenceValue)
            {
                currentReferenceValue = CreateInstance(currentReferenceValue);
                PropertyValue = currentReferenceValue;
            }

            _previousReferenceValue = currentReferenceValue;
        }

        private bool ShouldCheckProperty() { return IsPropertyInArray(Property) && _previousPropertyPath != null && _previousPropertyPath != Property.propertyPath; }

        private static bool IsPropertyInArray(SerializedProperty prop) { return prop.propertyPath.Contains(".Array.data["); }

        private static object CreateInstance(object source)
        {
            object instance = Activator.CreateInstance(source.GetType());
            EditorUtility.CopySerializedManagedFieldsOnly(source, instance);
            return instance;
        }
    }
}