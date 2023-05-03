namespace Pancake.ApexEditor
{
    public interface IInlineDecoratorWidth
    {
        /// <summary>
        /// Get the width of the inline decorator, which required to display it.
        /// Calculate only the size of the current inline decorator, not the entire property.
        /// The inline decorator width will be added to the total size of the property with other painters.
        /// </summary>
        float GetWidth();
    }
}