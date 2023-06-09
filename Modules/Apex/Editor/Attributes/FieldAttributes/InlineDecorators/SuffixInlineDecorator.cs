using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [InlineDecoratorTarget(typeof(SuffixAttribute))]
    public sealed class SuffixInlineDecorator : FieldInlineDecorator
    {
        private SuffixAttribute attribute;
        private GUIStyle style;
        private float width;

        /// <summary>
        /// Called once when initializing element inline decorator.
        /// </summary>
        /// <param name="element">Serialized element with InlineDecoratorAttribute.</param>
        /// <param name="inlineDecoratorAttribute">InlineDecoratorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public override void Initialize(SerializedField element, InlineDecoratorAttribute inlineDecoratorAttribute, GUIContent label)
        {
            attribute = inlineDecoratorAttribute as SuffixAttribute;
        }

        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        public override void OnGUI(Rect position)
        {
            EditorGUI.BeginDisabledGroup(attribute.Mute);
            GUI.Label(position, attribute.label, style);
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Get the width of the inline decorator, which required to display it.
        /// Calculate only the size of the current inline decorator, not the entire property.
        /// The inline decorator width will be added to the total size of the property with other painters.
        /// </summary>
        public override float GetWidth()
        {
            if (style == null)
            {
                style = new GUIStyle(attribute.Style) {alignment = attribute.Alignment};

                width = style.CalcSize(new GUIContent(attribute.label)).x;
            }

            return width;
        }

        /// <summary>
        /// Inline decorator visibility state.
        /// </summary>
        public override bool IsVisible() { return !string.IsNullOrEmpty(attribute.label); }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override InlineDecoratorSide GetSide() { return InlineDecoratorSide.Right; }
    }
}