using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(CustomViewAttribute))]
    sealed class CustomView : FieldView
    {
        private object target;
        private MethodInfo onInitialization;
        private MethodInfo onGUI;
        private MethodInfo getHeight;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            target = serializedField.GetMemberTarget();
            SearchCallbacks(target, viewAttribute);
            if (onInitialization != null)
            {
                onInitialization.Invoke(target, new object[2] {serializedField.GetSerializedProperty(), label});
            }
        }

        /// <summary>
        /// Searching custom method callbacks.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        private void SearchCallbacks(object target, ViewAttribute viewAttribute)
        {
            CustomViewAttribute attribute = viewAttribute as CustomViewAttribute;
            Type type = target.GetType();
            foreach (MethodInfo methodInfo in type.AllMethods())
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                if (!string.IsNullOrEmpty(attribute.OnInitialization) && methodInfo.Name == attribute.OnInitialization && methodInfo.ReturnType == typeof(void) &&
                    parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializedProperty) && parameters[1].ParameterType == typeof(GUIContent))
                {
                    onInitialization = methodInfo;
                }

                if (!string.IsNullOrEmpty(attribute.OnGUI) && methodInfo.Name == attribute.OnGUI && methodInfo.ReturnType == typeof(void) && parameters.Length == 3 &&
                    parameters[0].ParameterType == typeof(Rect) && parameters[1].ParameterType == typeof(SerializedProperty) &&
                    parameters[2].ParameterType == typeof(GUIContent))
                {
                    onGUI = methodInfo;
                }

                if (!string.IsNullOrEmpty(attribute.GetHeight) && methodInfo.Name == attribute.GetHeight && methodInfo.ReturnType == typeof(void) &&
                    parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializedProperty) && parameters[1].ParameterType == typeof(GUIContent))
                {
                    getHeight = methodInfo;
                }

                if (onInitialization != null && onGUI != null && getHeight != null)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (onGUI != null)
            {
                onGUI.Invoke(target, new object[3] {position, serializedField.GetSerializedProperty(), label});
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            if (getHeight != null)
            {
                getHeight.Invoke(target, new object[2] {serializedField.GetSerializedProperty(), label});
            }

            return base.GetHeight(serializedField, label);
        }
    }
}