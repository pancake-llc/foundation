namespace Pancake
{
    /// <summary>
    /// Represents a class responsible for providing <see cref="ServiceAttribute">service</see>
    /// objects on request to any to any clients that need them.
    /// <para>
    /// A benefit of using <see cref="IServiceProvider"/> instead of a concrete class directly,
    /// is that it makes possible to create mock implementations of the interface for unit tests.
    /// </para>
    /// <para>
    /// Additionally, it makes it easier to swap your service provider with another implementation at a later time.
    /// </para>
    /// A third benefit is that it makes your code less coupled with other classes, making it easier
    /// to do things such as port the code over to another project.
    /// </para>
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Returns shared instance of <typeparamref name="TDefiningClassOrInterface"/> service.
        /// </summary>
        /// <typeparam name="TDefiningClassOrInterface">
        /// Interface or class type that defines the service.
        /// <para>
        /// This should be an interface that the service implements, a base type that the service derives from, or the exact type of the service.
        /// </para>
        /// </typeparam>
        /// <returns>
        /// An instance of a class that derives from <typeparamref name="TDefiningClassOrInterface"/>
        /// or is <typeparamref name="TDefiningClassOrInterface"/> and has the <see cref="ServiceAttribute"/>,
        /// if one is found in the project; otherwise, <see langword="null"/>.
        /// </returns>
        TDefiningClassOrInterface Get<TDefiningClassOrInterface>();
    }
}