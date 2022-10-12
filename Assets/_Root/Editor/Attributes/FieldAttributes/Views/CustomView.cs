using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(CustomViewAttribute))]
    sealed class CustomView : FieldView
    {
        private MethodInfo onInitialization;
        private MethodInfo onGUI;
        private MethodInfo getHeight;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            SearchCallbacks(element, viewAttribute);
            if (onInitialization != null)
            {
                onInitialization.Invoke(element.serializedObject.targetObject, new object[2] {element.serializedProperty, label});
            }
        }

        /// <summary>
        /// Searching custom method callbacks.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        private void SearchCallbacks(SerializedField element, ViewAttribute viewAttribute)
        {
            CustomViewAttribute attribute = viewAttribute as CustomViewAttribute;

            Type type = element.serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            if (!string.IsNullOrEmpty(attribute.OnInitialization))
            {
                onInitialization = type.GetAllMembers(attribute.OnInitialization, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty) && method.GetParameters()[1].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.OnGUI))
            {
                onGUI = type.GetAllMembers(attribute.OnGUI, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 3 &&
                                method.GetParameters()[0].ParameterType == typeof(Rect) && method.GetParameters()[1].ParameterType == typeof(SerializedProperty) &&
                                method.GetParameters()[2].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }

            if (!string.IsNullOrEmpty(attribute.GetHeight))
            {
                getHeight = type.GetAllMembers(attribute.GetHeight, flags)
                    .Where(m => m is MethodInfo method && method.ReturnType == typeof(void) && method.GetParameters().Length == 2 &&
                                method.GetParameters()[0].ParameterType == typeof(SerializedProperty) && method.GetParameters()[1].ParameterType == typeof(GUIContent))
                    .FirstOrDefault() as MethodInfo;
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            if (onGUI != null)
            {
                onGUI.Invoke(element.serializedObject.targetObject, new object[3] {position, element.serializedProperty, label});
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label)
        {
            if (getHeight != null)
            {
                getHeight.Invoke(element.serializedObject.targetObject, new object[2] {element.serializedProperty, label});
            }

            return base.GetHeight(element, label);
        }
    }
}