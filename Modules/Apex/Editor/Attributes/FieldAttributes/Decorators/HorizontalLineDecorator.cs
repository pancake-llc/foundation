using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(HorizontalLineAttribute))]
    sealed class HorizontalLineDecorator : FieldDecorator
    {
        private HorizontalLineAttribute _attribute;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            _attribute = decoratorAttribute as HorizontalLineAttribute;
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            float lineHeight = _attribute.Style == LineStyle.Thin ? 1f : 2f;
            var rect = new Rect(position.x, position.y + 5f, position.width, position.height) {height = lineHeight, xMin = 0, xMax = EditorGUIUtility.currentViewWidth};
            EditorGUI.DrawRect(rect, new Color(_attribute.R, _attribute.G, _attribute.B, _attribute.A));
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight() { return _attribute.Style == LineStyle.Thin ? 4f : 5f; }
    }
}