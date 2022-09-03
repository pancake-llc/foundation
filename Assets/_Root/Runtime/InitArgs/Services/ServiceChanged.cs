using JetBrains.Annotations;

namespace Pancake.Init
{
    /// <summary>
    /// Represents a method that will handle the event of the globally shared Instance of
    /// service of type <typeparamref name="TService"/> having been replaced by another Instance.
    /// </summary>
    /// <typeparam name="TService">
    /// The defining type of the service class, which is the type specified in its <see cref="ServiceAttribute"/>,
    /// or - if no other type has been explicitly specified - the exact type of the service class.
    /// </typeparam>
    /// <param name="oldInstance"> The old service Instance, or <see langword="null"/> if there was no Instance of the service before. </param>
    /// <param name="newInstance"> The new service Instance, or <see langword="null"/> if the service was set to be <see langword="null"/>. </param>
    public delegate void ServiceChangedHandler<TService>([CanBeNull] TService oldInstance, [CanBeNull] TService newInstance);

    /// <summary>
    /// Class that holds the current subscribers to the <see cref="ServiceChangedHandler{TService}"/> event.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    internal static class ServiceChanged<TService>
    {
        /// <summary>
        /// Event that is invoked when the shared Instance of service of type
        /// <typeparamref name="TService"/> has been changed to another one.
        /// </summary>
        internal static ServiceChangedHandler<TService> Listeners;
    }
}