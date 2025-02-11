namespace Sisus.Init.Internal
{
    /// <summary>
    /// Represents an object that has a globally unique identifier.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets the globally unique identifier of the object.
        /// </summary>
        Id Id { get; }
    }
}