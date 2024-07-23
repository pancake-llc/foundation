using System.Diagnostics.CodeAnalysis;

namespace Sisus.Init
{
	/// <summary>
	/// Represents a method that will handle the event of the globally shared instance of
	/// service of type <typeparamref name="TService"/> having been replaced by another instance.
	/// </summary>
	/// <typeparam name="TService">
	/// The defining type of the service class, which is the type specified in its <see cref="ServiceAttribute"/>,
	/// or - if no other type has been explicitly specified - the exact type of the service class.
	/// </typeparam>
	/// <param name="oldInstance"> The old service instance, or <see langword="null"/> if there was no instance of the service before. </param>
	/// <param name="newInstance"> The new service instance, or <see langword="null"/> if the service was set to be <see langword="null"/>. </param>
	public delegate void ServiceChangedHandler<TService>(Clients clients, [AllowNull] TService oldInstance, [AllowNull] TService newInstance);

	/// <summary>
	/// Class that holds the current subscribers to the <see cref="ServiceChangedHandler{TService}"/> event.
	/// </summary>
	/// <typeparam name="TService">
	/// The defining type of the service class, which is the type specified in its <see cref="ServiceAttribute"/>,
	/// or - if no other type has been explicitly specified - the exact type of the service class.
	/// </typeparam>
	internal static class ServiceChanged<TService>
	{
		/// <summary>
		/// Event that is invoked when a globally available shared instance of service of type
		/// <typeparamref name="TService"/> has changed to another one.
		/// </summary>
		internal static ServiceChangedHandler<TService> listeners;
	}
}