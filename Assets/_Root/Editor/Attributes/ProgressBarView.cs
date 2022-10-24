using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ProgressBarAttribute))]
    sealed class ProgressBarView : FieldView, ITypeValidationCallback
    {
        private ProgressBarAttribute attribute;
        private MemberInfo memberInfo;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ProgressBarAttribute;

            string text = attribute.text;
            if (!string.IsNullOrEmpty(text) && text[0] == '@')
            {
                text = text.Remove(0, 1);
                memberInfo = serializedField.GetMemberTarget().GetType().AllMembers().Where(m => Predicate(text, m)).FirstOrDefault();
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
            SerializedProperty serializedProperty = serializedField.GetSerializedProperty();
            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    EditorGUI.ProgressBar(position, serializedProperty.intValue / 100, GetText(serializedField.GetMemberTarget()));
                    break;
                case SerializedPropertyType.Float:
                    EditorGUI.ProgressBar(position, serializedProperty.floatValue / 100, GetText(serializedField.GetMemberTarget()));
                    break;
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label) { return attribute.Height; }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;
        }

        /// <summary>
        /// Linq predicate to search specified member info.
        /// </summary>
        /// <param name="name">Member info name.</param>
        /// <param name="memberInfo">Member info reference.</param>
        /// <returns>Result of predicate.</returns>
        private bool Predicate(string name, MemberInfo memberInfo)
        {
            Type stringType = typeof(string);
            return memberInfo.Name == name && ((memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == stringType) ||
                                               (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.PropertyType == stringType) ||
                                               (memberInfo is MethodInfo methodInfo && methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == stringType));
        }

        /// <summary>
        /// If member info is not null return member value. Otherwise text.
        /// </summary>
        /// <param name="target">Member target.</param>
        private string GetText(object target)
        {
            if (memberInfo != null)
            {
                if (memberInfo is FieldInfo fieldInfo)
                {
                    return fieldInfo.GetValue(target) as string;
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    return propertyInfo.GetValue(target) as string;
                }
                else if (memberInfo is MethodInfo methodInfo)
                {
                    return methodInfo.Invoke(target, null) as string;
                }
            }

            return attribute.text;
        }
    }
}