namespace Pancake.ApexEditor
{
    public interface IDecoratorHeight
    {
        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        float GetHeight();
    }
}