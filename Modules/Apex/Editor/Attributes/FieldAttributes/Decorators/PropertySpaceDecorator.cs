using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(PropertySpaceAttribute))]
    public sealed class PropertySpaceDecorator : FieldDecorator
    {
        private PropertySpaceAttribute attribute;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as PropertySpaceAttribute;
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position) { }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight() { return attribute.space; }

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public override DecoratorSide GetSide() { return DecoratorSide.Top; }
    }
}