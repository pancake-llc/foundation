using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [InlineDecoratorTarget(typeof(PrefixAttribute))]
    public sealed class PrefixInlineDecorator : FieldInlineDecorator, ITypeValidationCallback
    {
        public const float LabelSpace = 1.0f;

        private PrefixAttribute attribute;
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
            attribute = inlineDecoratorAttribute as PrefixAttribute;
        }

        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        public override void OnGUI(Rect position) { GUI.Label(position, attribute.label); }

        /// <summary>
        /// Get the width of the inline decorator, which required to display it.
        /// Calculate only the size of the current inline decorator, not the entire property.
        /// The inline decorator width will be added to the total size of the property with other painters.
        /// </summary>
        public override float GetWidth()
        {
            if (style == null)
            {
                style = new GUIStyle(attribute.Style);
                GUIContent content = new GUIContent(attribute.label);
                width = style.CalcSize(content).x;
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
        public override InlineDecoratorSide GetSide() { return InlineDecoratorSide.Left; }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return !property.isArray && property.propertyType != SerializedPropertyType.Generic && property.propertyType != SerializedPropertyType.ManagedReference;
        }
    }
}