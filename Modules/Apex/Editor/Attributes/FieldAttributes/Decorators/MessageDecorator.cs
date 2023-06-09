using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(MessageAttribute))]
    public sealed class MessageDecorator : FieldDecorator
    {
        private static readonly Color InfoColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        private static readonly Color WarningColor = new Color(1.0f, 0.7f, 0.3f, 0.1f);
        private static readonly Color ErrorColor = new Color(1.0f, 0.07f, 0.125f, 0.1f);

        private MessageAttribute attribute;
        private GUIStyle style;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as MessageAttribute;
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = attribute.FontSize, fontStyle = attribute.FontStyle, alignment = attribute.Alignment, richText = attribute.RichText
                };
            }

            float lineWidth = attribute.Style == MessageStyle.None ? 0 : 4;
            Color color = GetStyleColor(attribute.Style);

            Rect backgroundPosition = new Rect(position.x + lineWidth, position.y, position.width - lineWidth, position.height);
            EditorGUI.DrawRect(backgroundPosition, color);

            Rect linePosition = new Rect(position.x, position.y, lineWidth, position.height);
            color.a = 0.7f;
            EditorGUI.DrawRect(linePosition, color);

            Rect labelPosition = new Rect(backgroundPosition.xMin + 3, backgroundPosition.y, backgroundPosition.width, backgroundPosition.height);
            GUI.Label(labelPosition, attribute.text, style);
        }

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

        public static Color GetStyleColor(MessageStyle style)
        {
            switch (style)
            {
                default:
                case MessageStyle.None:
                case MessageStyle.Info:
                    return InfoColor;
                case MessageStyle.Warning:
                    return WarningColor;
                case MessageStyle.Error:
                    return ErrorColor;
            }
        }
    }
}