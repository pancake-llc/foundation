using Pancake.Apex;
using Pancake.ExLib.Reflection;
using Pancake.ExLibEditor.Windows;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(SearchableEnumAttribute))]
    sealed class SearchableEnumView : FieldView, ITypeValidationCallback
    {
        private SearchableEnumAttribute attribute;

        // Stored validate properties.
        private float width;
        private bool isFlags;
        private GUIContent helpBoxContent;

        // Stored callback properties.
        private object target;
        private object arg;
        private MethodCaller<object, object> onSelect;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            isFlags = serializedField.GetMemberType().GetCustomAttribute<System.FlagsAttribute>() != null;
            if (isFlags)
            {
                helpBoxContent = new GUIContent("[SearchableEnum] attribute does not support enum types with [Flags] attribute.");
            }
            else
            {
                attribute = viewAttribute as SearchableEnumAttribute;
                target = serializedField.GetDeclaringObject();
                FindCallback(serializedField.GetSerializedProperty());
                helpBoxContent = GUIContent.none;
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
            if (!isFlags)
            {
                position = EditorGUI.PrefixLabel(position, label);

                if (GUI.Button(position, element.GetEnumDisplayValue(), EditorStyles.popup))
                {
                    ExSearchWindow searchWindow = ExSearchWindow.Create();
                    for (int i = 0; i < element.GetEnumValueCount(); i++)
                    {
                        int indexCopy = i;
                        string enumName = element.GetEnumValue(indexCopy);

                        if (attribute.HideValues?.Any(v => v == enumName) ?? false)
                        {
                            continue;
                        }

                        GUIContent content = new GUIContent(element.GetEnumDisplayValue(indexCopy));

                        Type type = element.GetMemberType();
                        MemberInfo[] memberInfo = type.GetMember(enumName);
                        object[] attributes = memberInfo[0].GetCustomAttributes(typeof(SearchContent), false);
                        if (attributes.Length > 0)
                        {
                            SearchContent searchContent = attributes[0] as SearchContent;
                            if (searchContent != null)
                            {
                                content.text = searchContent.name;

                                if (SearchContentUtility.TryLoadContentImage(searchContent.Image, out Texture2D image))
                                {
                                    content.image = image;
                                }
                            }
                        }

                        searchWindow.AddEntry(content,
                            () =>
                            {
                                element.SetEnum(indexCopy);
                                element.GetSerializedObject().ApplyModifiedProperties();
                                onSelect.SafeInvoke(target, arg);
                            });
                    }

                    searchWindow.Open(position, 0, attribute.Height);
                }
            }
            else
            {
                width = position.width;
                EditorGUI.HelpBox(position, helpBoxContent.text, MessageType.Error);
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            if (isFlags)
            {
                return EditorStyles.helpBox.CalcHeight(helpBoxContent, width);
            }

            return base.GetHeight(serializedField, label);
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Enum; }

        private void FindCallback(SerializedProperty property)
        {
            if (!string.IsNullOrEmpty(attribute.OnSelect))
            {
                Type type = target.GetType();
                foreach (MethodInfo methodInfo in type.AllMethods())
                {
                    if (methodInfo.Name == attribute.OnSelect)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        if (parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(SerializedProperty)))
                        {
                            onSelect = methodInfo.DelegateForCall();
                        }

                        if (onSelect != null && parameters.Length == 1)
                        {
                            arg = new object[1] {property};
                        }

                        break;
                    }
                }
            }
        }
    }
}