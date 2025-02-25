namespace Pancake.Common
{
    /// <summary>An object which can be converted to another type.</summary>
    public interface IConvertable<out T>
    {
        /// <summary>Returns the equivalent of this object as <typeparamref name="T"/>.</summary>
        T Convert();
    }
}