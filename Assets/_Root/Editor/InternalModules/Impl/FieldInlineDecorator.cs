using UnityEngine;

namespace Pancake.Editor
{
    public abstract class FieldInlineDecorator : IInlineDecoratorInitialize, IInlineDecoratorGUI, IInlineDecoratorWidth, IInlineDecoratorSide
    {
        /// <summary>
        /// Called once when initializing element inline decorator.
        /// </summary>
        /// <param name="element">Serialized element with InlineDecoratorAttribute.</param>
        /// <param name="inlineDecoratorAttribute">InlineDecoratorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public virtual void Initialize(SerializedField element, InlineDecoratorAttribute inlineDecoratorAttribute, GUIContent label) { }

        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        public abstract void OnGUI(Rect position);

        /// <summary>
        /// Get the width of the inline decorator, which required to display it.
        /// Calculate only the size of the current inline decorator, not the entire property.
        /// The inline decorator width will be added to the total size of the property with other painters.
        /// </summary>
        public abstract float GetWidth();

        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        public virtual InlineDecoratorSide GetSide() { return InlineDecoratorSide.Right; }
    }
}