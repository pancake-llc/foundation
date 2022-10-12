using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(HelpBoxAttribute))]
    sealed class HelpBoxDecorator : FieldDecorator
    {
        private HelpBoxAttribute attribute;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="element">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as HelpBoxAttribute;
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI(Rect position) { EditorGUI.HelpBox(position, attribute.text, CovertStyleToType(attribute.Style)); }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight()
        {
            if (attribute.Height == 0)
            {
                GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                GUIContent content = new GUIContent(attribute.text);
                Vector2 size = style.CalcSize(content);
                attribute.Height = style.CalcHeight(content, size.x) + EditorGUIUtility.standardVerticalSpacing;
            }

            return attribute.Height;
        }

        #region [Static Methods]

        public static MessageType CovertStyleToType(MessageStyle style)
        {
            switch (style)
            {
                default:
                case MessageStyle.None:
                    return MessageType.None;
                case MessageStyle.Info:
                    return MessageType.Info;
                case MessageStyle.Warning:
                    return MessageType.Warning;
                case MessageStyle.Error:
                    return MessageType.Error;
            }
        }

        #endregion
    }
}