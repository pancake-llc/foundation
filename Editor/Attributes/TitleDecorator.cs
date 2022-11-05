using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(TitleAttribute))]
    sealed class TitleDecorator : FieldDecorator
    {
        private object target;
        private float height;
        private TitleAttribute attribute;
        private GUIContent content;
        private GUIStyle style;
        private MemberInfo styleMember;
        private Color separatorColor;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as TitleAttribute;
            separatorColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

            target = serializedField.GetMemberTarget();
            Type type = target.GetType();
            if (!string.IsNullOrEmpty(attribute.Style))
            {
                foreach (MemberInfo memberInfo in type.AllMembers())
                {
                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        if (fieldInfo.FieldType == typeof(GUIStyle))
                        {
                            styleMember = memberInfo;
                        }
                        else if (fieldInfo.FieldType == typeof(Color))
                        {
                            separatorColor = (Color) fieldInfo.GetValue(target);
                        }
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                    {
                        if (propertyInfo.PropertyType == typeof(GUIStyle))
                        {
                            styleMember = memberInfo;
                        }
                        else if (propertyInfo.PropertyType == typeof(Color))
                        {
                            separatorColor = (Color) propertyInfo.GetValue(target);
                        }
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.GetParameters().Length == 0)
                    {
                        if (methodInfo.ReturnType == typeof(GUIStyle))
                        {
                            styleMember = memberInfo;
                        }
                        else if (methodInfo.ReturnType == typeof(Color))
                        {
                            separatorColor = (Color) methodInfo.Invoke(target, null);
                        }
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
                return 17 + 3 + attribute.SeparatorHeight;
            }

            return 17;
        }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override DecoratorSide GetSide() { return DecoratorSide.Top; }

        private void CreateStyle()
        {
            if (string.IsNullOrEmpty(attribute.Style) || styleMember == null)
            {
                if (style == null)
                {
                    style = new GUIStyle(GUI.skin.label) {fontSize = attribute.FontSize, alignment = attribute.Anchor, fontStyle = FontStyle.Bold, wordWrap = true};
                }
            }
            else if (styleMember != null)
            {
                if (styleMember is FieldInfo fieldInfo)
                {
                    style = (GUIStyle) fieldInfo.GetValue(target);
                }
                else if (styleMember is PropertyInfo propertyInfo)
                {
                    style = (GUIStyle) propertyInfo.GetValue(target);
                }
                else if (styleMember is MethodInfo methodInfo)
                {
                    style = (GUIStyle) methodInfo.Invoke(target, null);
                }
            }
        }
    }
}