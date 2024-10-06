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
	public interface IServiceProvider
	{
		/// <summary>
		/// Tries to get a shared instance of <typeparamref name="TService"/> service for the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TService">
		/// Interface or class type that defines the service.
		/// <para>
		/// This should be an interface that the service implements, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </typeparam>
		/// <typeparam name="TService"></typeparam>
		/// <param name="service">
		/// When this method returns, contains service of type <typeparamref name="TService"/>,
		/// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if service was found; otherwise, <see langword="false"/>. </returns>
		public bool TryGet<TService>([NotNullWhen(true), MaybeNullWhen(false)] out TService service);

		/// <summary>
		/// Tries to get a shared instance of <typeparamref name="TService"/> service for the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TService">
		/// Interface or class type that defines the service.
		/// <para>
		/// This should be an interface that the service implements, a base type that the service derives from, or the exact type of the service.
		/// </para>
		/// </typeparam>
		/// <param name="client"> The client that needs the service. </param>
		/// <param name="service">
		/// When this method returns, contains service of type <typeparamref name="TService"/>,
		/// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if service was found; otherwise, <see langword="false"/>. </returns>
		public bool TryGetFor<TService>([DisallowNull] Component client, [NotNullWhen(true), MaybeNullWhen(false)] out TService service);
	}
}