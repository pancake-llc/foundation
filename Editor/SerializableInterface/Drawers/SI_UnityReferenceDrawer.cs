using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    internal class SI_UnityReferenceDrawer : SI_ReferenceDrawer, SI_IReferenceDrawer
    {
        private GUIContent _label;

        public void Initialize(SerializedProperty property, Type genericType, GUIContent label, FieldInfo fieldInfo)
        {
            Initialize(property, genericType, fieldInfo);
            this._label = label;
        }

        public float GetHeight() { return EditorGUIUtility.singleLineHeight; }

        public void OnGUI(Rect position)
        {
            Object unityReference = UnityReferenceProperty.objectReferenceValue;
            Type referenceType = unityReference == null ? typeof(Object) : unityReference.GetType();
            GUIContent objectContent = EditorGUIUtility.ObjectContent(unityReference, referenceType);
            customObjectDrawer.OnGUI(position, _label, objectContent, Property);
            HandleDragAndDrop(position);
        }

        protected override void PingObject(SerializedProperty property) { EditorGUIUtility.PingObject((Object) GetPropertyValue(property)); }

        protected override void OnPropertiesClicked(SerializedProperty property) { SI_PropertyEditorUtility.Show(property.UnityReferenceProperty().objectReferenceValue); }
    }
}