using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// A global service that clients can use to acquire other global and local services by their defining types.
	/// <para>
	/// Implements the <see cref="Sisus.Init.IServiceProvider"/> and <see cref="System.IServiceProvider"/> interfaces.
	/// </para>
	/// <para>
	/// This is just a simple proxy for the <see cref="Service.TryGet{TService}"/>
	/// and <see cref="Service.TryGetFor{TService}(Component, out TService)"/> methods.
	/// </para>
	/// </summary>
	public sealed class ServiceProvider : ScriptableObject, IServiceProvider, System.IServiceProvider, IValueByTypeProvider
	{
		/// <inheritdoc/>
		public bool TryGet<TService>(out TService service) => Service.TryGet(out service);

		/// <inheritdoc/>
		public bool TryGetFor<TService>(Component client, out TService service) => Service.TryGetFor(client, out service);

		/// <inheritdoc/>
		public bool CanProvideValue<TService>([AllowNull] Component client) => !typeof(TService).IsValueType && typeof(TService) != typeof(string) && typeof(TService) != typeof(object);

		/// <summary>
		/// Returns shared instance of <typeparamref name="TService"/> service.
		/// </summary>
		/// <typeparam name="TService">
		/// Interface or class type that defines the service.
		/// <para>
		/// This should be an interface that the service implements, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </typeparam>
		/// <returns>
		/// An instance of a class that derives from <typeparamref name="TService"/>
		/// or is <typeparamref name="TService"/> and has the <see cref="ServiceAttribute"/>,
		/// if one is found in the project; otherwise, <see langword="null"/>.
		/// </returns>
		bool IValueByTypeProvider.TryGetFor<TService>([AllowNull] Component client, [MaybeNullWhen(false), NotNullWhen(true)] out TService service)
			=> client ? Service.TryGetFor(client, out service) : Service.TryGet(out service);

		/// <returns>
		/// Returns always <see langword="true"/> as long as <typeparamref name="TService"/> is not a value type.
		/// <para>
		/// We will always assume that the service will be available at runtime to avoid warnings being shown
		/// to the user about missing arguments.
		/// </para>
		/// </returns>
		NullGuardResult INullGuardByType.EvaluateNullGuard<TService>(Component client) => NullGuardResult.Passed;

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType"> The type of service object to get. </param>
		/// <returns>
		/// A service object of type <paramref name="serviceType"/> or <see langword="null"/>
		/// if there is no service object of type <paramref name="serviceType"/>.
		/// </returns>
		object System.IServiceProvider.GetService(Type serviceType) => ServiceUtility.Get(serviceType);
	}
}