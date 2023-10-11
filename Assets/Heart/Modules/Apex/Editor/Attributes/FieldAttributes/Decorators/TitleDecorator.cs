using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(TitleAttribute))]
    public sealed class TitleDecorator : FieldDecorator
    {
        private const float LINE_HEIGHT = 1;
        private static Color LineColor;
        static TitleDecorator() { LineColor = new Color32(94, 94, 94, 255); }

        private TitleAttribute attribute;
        private GUIContent content;
        private GUIStyle style;
        private object target;
        private float width;
        private float height;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as TitleAttribute;
            content = new GUIContent(attribute.text);
            target = serializedField.GetDeclaringObject();
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (style == null)
            {
                style = FindStyle();
            }

            width = position.width;

            position.y += attribute.VerticalSpace;
            position.height = height;
            GUI.Label(position, content, style);
            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

            if (attribute.DrawSeparator)
            {
                position.height = LINE_HEIGHT;
                EditorGUI.DrawRect(position, LineColor);
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
                style = FindStyle();
            }

            height = style.CalcHeight(content, width);
            if (attribute.DrawSeparator)
            {
                return height + EditorGUIUtility.standardVerticalSpacing + LINE_HEIGHT + attribute.VerticalSpace;
            }

            return height + attribute.VerticalSpace;
        }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override DecoratorSide GetSide() { return DecoratorSide.Top; }

        private GUIStyle FindStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel) {fontSize = attribute.FontSize, alignment = attribute.Anchor, wordWrap = true};

            string styleName = attribute.Style;
            if (!string.IsNullOrEmpty(styleName))
            {
                if (styleName[0] == '@')
                {
                    return new GUIStyle(styleName.Remove(0, 1));
                }

                return ApexCallbackUtility.GetCallbackResult<GUIStyle>(target, styleName);
            }

            return style;
        }
    }
}