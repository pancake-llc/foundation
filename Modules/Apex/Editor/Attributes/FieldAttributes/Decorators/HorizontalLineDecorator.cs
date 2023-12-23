using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(HorizontalLineAttribute))]
    sealed class HorizontalLineDecorator : FieldDecorator
    {
        private HorizontalLineAttribute attribute;
        private Color color;

        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = (HorizontalLineAttribute) decoratorAttribute;
            color = new Color(attribute.r, attribute.g, attribute.b, attribute.a);
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            position.y += (attribute.Space / 2);

            position.height = attribute.height;
            EditorGUI.DrawRect(position, color);
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight() { return attribute.height + attribute.Space; }
    }
}