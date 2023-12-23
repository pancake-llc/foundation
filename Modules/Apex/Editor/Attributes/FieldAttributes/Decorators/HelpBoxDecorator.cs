using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(HelpBoxAttribute))]
    public sealed class HelpBoxDecorator : FieldDecorator
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
        public override float GetHeight() { return attribute.Height; }

        /// <summary>
        /// Field decorator visibility state.
        /// </summary>
        public override bool IsVisible() { return !string.IsNullOrEmpty(attribute.text); }

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