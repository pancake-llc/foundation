using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(TitleAttribute))]
    sealed class TitleDecorator : FieldDecorator
    {
        private TitleAttribute attribute;
        private GUIContent content;
        private GUIStyle style;
        private Color separatorColor;
        private MemberInfo styleMember;
        private object targetObject;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as TitleAttribute;

            targetObject = element.serializedObject.targetObject;
            Type type = targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            if (!string.IsNullOrEmpty(attribute.Style))
            {
                styleMember = type.GetAllMembers(attribute.Style, flags).Where(SearchStylePredicate).FirstOrDefault();
            }

            separatorColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            if (!string.IsNullOrEmpty(attribute.SeparatorColor))
            {
                MemberInfo separatorColorMember = type.GetAllMembers(attribute.SeparatorColor, flags).Where(SearchSeparatorColorPredicate).FirstOrDefault();

                if (separatorColorMember != null)
                {
                    if (separatorColorMember is FieldInfo fieldInfo)
                    {
                        separatorColor = (Color) fieldInfo.GetValue(targetObject);
                    }
                    else if (separatorColorMember is PropertyInfo propertyInfo)
                    {
                        separatorColor = (Color) propertyInfo.GetValue(targetObject);
                    }
                    else if (separatorColorMember is MethodInfo methodInfo)
                    {
                        separatorColor = (Color) methodInfo.Invoke(targetObject, null);
                    }
                }
            }
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            position.height -= attribute.SeparatorHeight + 3;
            GUI.Label(position, content, style);
            if (attribute.DrawSeparator)
            {
                Rect separatorPosition = new Rect(position.x, position.yMax + 1, position.width, attribute.SeparatorHeight);
                EditorGUI.DrawRect(separatorPosition, separatorColor);
            }
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight()
        {
            if (style == null)
            {
                content = new GUIContent(attribute.text);
                CreateStyle();
            }

            Vector2 size = style.CalcSize(content);
            if (attribute.DrawSeparator)
            {
                return style.CalcHeight(content, size.x) + 3 + attribute.SeparatorHeight;
            }

            return style.CalcHeight(content, size.x);
        }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override DecoratorSide GetSide() { return DecoratorSide.Top; }

        private void CreateStyle()
        {
            if (string.IsNullOrEmpty(attribute.Style) || styleMember == null)
            {
                style = new GUIStyle(GUI.skin.label) {fontSize = attribute.FontSize, alignment = attribute.Anchor, fontStyle = FontStyle.Bold, wordWrap = true};
            }
            else if (styleMember != null)
            {
                if (styleMember is FieldInfo fieldInfo)
                {
                    style = (GUIStyle) fieldInfo.GetValue(targetObject);
                }
                else if (styleMember is PropertyInfo propertyInfo)
                {
                    style = (GUIStyle) propertyInfo.GetValue(targetObject);
                }
                else if (styleMember is MethodInfo methodInfo)
                {
                    style = (GUIStyle) methodInfo.Invoke(targetObject, null);
                }
            }
        }


        private bool SearchStylePredicate(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType == typeof(GUIStyle);
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.CanRead && propertyInfo.PropertyType == typeof(GUIStyle);
            }
            else if (memberInfo is MethodInfo methodInfo)
            {
                return methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(GUIStyle);
            }

            return false;
        }

        private bool SearchSeparatorColorPredicate(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType == typeof(Color);
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.CanRead && propertyInfo.PropertyType == typeof(Color);
            }
            else if (memberInfo is MethodInfo methodInfo)
            {
                return methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(Color);
            }

            return false;
        }
    }
}