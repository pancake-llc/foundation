using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(SearchableEnumAttribute))]
    sealed class SearchableEnumView : FieldView, ITypeValidationCallback
    {
        private SearchableEnumAttribute attribute;
        private MethodInfo onSelectMethod;
        private object[] methodParameters;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as SearchableEnumAttribute;

            if (!string.IsNullOrEmpty(attribute.OnSelectCallback))
            {
                Type type = element.GetMemberTarget().GetType();
                foreach (MethodInfo methodInfo in type.AllMethods())
                {
                    if (methodInfo.Name == attribute.OnSelectCallback)
                    {
                        ParameterInfo[] callbackParameters = methodInfo.GetParameters();

                        if ((callbackParameters.Length == 1 && callbackParameters[0].ParameterType == typeof(SerializedProperty)) || callbackParameters.Length == 0)
                        {
                            onSelectMethod = methodInfo;
                        }

                        if (callbackParameters.Length == 1)
                        {
                            methodParameters = new object[1] {element.GetSerializedProperty().Copy()};
                        }

                        break;
                    }
                }
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
            Rect labelPosition = EditorGUI.PrefixLabel(position, label);

            Rect popupPosition = new Rect(labelPosition.x, labelPosition.y, position.x + position.width - labelPosition.x, position.height);
            if (GUI.Button(popupPosition, element.GetEnumDisplayValue(), EditorStyles.popup))
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

                            // if (SearchContentUtility.TryLoadContentImage(searchContent.Image, out Texture2D image))
                            // {
                            //     content.image = image;
                            // }
                        }
                    }

                    Action onSelect = () =>
                    {
                        element.SetEnum(indexCopy);
                        onSelectMethod?.Invoke(element.GetMemberTarget(), methodParameters);
                    };

                    searchWindow.AddEntry(content, onSelect);
                }

                searchWindow.Open(popupPosition, 0, attribute.Height);
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Enum; }
    }
}