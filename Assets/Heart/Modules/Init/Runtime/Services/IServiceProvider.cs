using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents a class responsible for providing <see cref="ServiceAttribute">service</see>
	/// objects on request to any clients that need them.
	/// <para>
	/// A benefit of using <see cref="IServiceProvider"/> instead of a concrete class directly,
	/// is that it makes possible to create mock implementations of the interface for unit tests.
	/// </para>
	/// <para>
	/// Additionally, it makes it easier to swap your service provider with another implementation at a later time.
	/// </para>
	/// <para>
	/// A third benefit is that it makes your code less coupled with other classes, making it easier
	/// to do things such as port the code over to another project.
	/// </para>
	/// </summary>
	[RequireImplementors]
	public interface IServiceProvider : INullGuardByType
	{
		/// <summary>
		/// Tries to get a service of type <typeparamref name="TService"/> that is accessible to clients <see cref="Clients.Everywhere"/> .
		/// </summary>
		/// <typeparam name="TService">
		/// Type of the requested service.
		/// <para>
		/// This can be an interface that the returned <see paramref="service"/> implements, a base type that it derives
		/// from, or its exact concrete type.
		/// </para>
		/// </typeparam>
		/// <param name="service">
		/// When this method returns, contains the service of type <typeparamref name="TService"/>,
		/// if available; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if a service was provided; otherwise, <see langword="false"/>. </returns>
		public bool TryGet<TService>([NotNullWhen(true), MaybeNullWhen(false)] out TService service);

		/// <summary>
		/// Tries to get a service of type <typeparamref name="TService"/> for the given <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TService">
		/// Type of the service that the client needs.
		/// <para>
		/// This can be an interface that the returned <see paramref="service"/> implements, a base type that it derives
		/// from, or its exact concrete type.
		/// </para>
		/// </typeparam>
		/// <param name="client"> The client that needs the service. </param>
		/// <param name="service">
		/// When this method returns, contains the service of type <typeparamref name="TService"/>,
		/// if available for the client; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if service was found; otherwise, <see langword="false"/>. </returns>
		public bool TryGetFor<TService>([DisallowNull] Component client, [NotNullWhen(true), MaybeNullWhen(false)] out TService service);
	}
}