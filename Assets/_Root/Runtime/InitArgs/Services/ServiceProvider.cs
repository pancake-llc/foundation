using System;

namespace Pancake.Init
{
    /// <summary>
    /// Class that can provide an Instance of any class that has the <see cref="ServiceAttribute"/> on demand.
    /// <para>
    /// This is a simple proxy for the static <see cref="Service{TDefiningClassOrInterface}"/> class;
    /// Calling the <see cref="Get"/> method on any Instance of this class will return the shared
    /// service Instance stored in <see cref="Service{TDefiningClassOrInterface}.Instance"/>.
    /// </para>
    /// <para>
    /// A benefit of using <see cref="ServiceProvider"/> instead of <see cref="Service{}"/> directly,
    /// is the ability to call <see cref="Get"/> through the <see cref="IServiceProvider"/> interface.
    /// This makes it possible to create mock implementations of the interface for unit tests.
    /// </para>
    /// <para>
    /// Additionally, it makes it easier to swap your service provider with another implementation at a later time.
    /// </para>
    /// A third benefit is that it makes your code less coupled with other classes, making it much easier to
    /// port the code over to another project for example.
    /// </para>
    /// <para>
    /// The <see cref="ServiceProvider"/> class is a <see cref="ServiceAttribute">service</see> itself.
    /// This means that an Instance of the class can be automatically received by any classes that derive from
    /// <see cref="MonoBehaviour{IServiceProvider}"/> or <see cref="ScriptableObject{IServiceProvider}"/>.
    /// </para>
    /// </summary>
    [Service(typeof(IServiceProvider))]
    public sealed class ServiceProvider : IServiceProvider, System.IServiceProvider
    {
        /// <inheritdoc/>
        public TDefiningClassOrInterface Get<TDefiningClassOrInterface>() => Service<TDefiningClassOrInterface>.Instance;

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"> The type of service object to get. </param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/> or <see langword="null"/>
        /// if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        object System.IServiceProvider.GetService(Type serviceType) => ServiceUtility.GetService(serviceType);
    }
}