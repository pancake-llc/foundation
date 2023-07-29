using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ProgressBarAttribute))]
    public sealed class ProgressBarView : FieldView, ITypeValidationCallback
    {
        private ProgressBarAttribute attribute;

        // Stored callback properties.
        private object target;
        private MemberGetter<object, object> getter;
        private MethodCaller<object, object> caller;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as ProgressBarAttribute;
            target = serializedField.GetDeclaringObject();

            if (attribute.text[0] == '@')
            {
                FindCallback(attribute.text.Remove(0, 1));
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
                    EditorGUI.ProgressBar(position, serializedProperty.intValue / 100, GetText());
                    break;
                case SerializedPropertyType.Float:
                    EditorGUI.ProgressBar(position, serializedProperty.floatValue / 100, GetText());
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
        /// If member info is not null return member value. Otherwise text.
        /// </summary>
        /// <param name="target">Member target.</param>
        private string GetText()
        {
            if (getter != null)
                return (string) getter.Invoke(target);
            else if (caller != null)
                return (string) caller.Invoke(target, null);
            return attribute.text;
        }

        private void FindCallback(string name)
        {
            var stringType = typeof(string);
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
            {
                if (memberInfo.Name == name)
                {
                    if (memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == stringType)
                    {
                        getter = fieldInfo.DelegateForGet();
                        break;
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.PropertyType == stringType)
                    {
                        getter = propertyInfo.DelegateForGet();
                        break;
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType == stringType && methodInfo.GetParameters().Length == 0)
                    {
                        caller = methodInfo.DelegateForCall();
                        break;
                    }
                }
            }
        }
    }
}