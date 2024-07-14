using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents a value provider (<see cref="IValueByTypeProvider"/> or
	/// <see cref="IValueByTypeProviderAsync"/>) to which clients should return
	/// the value that was provided to them, when they no longer need it
	/// (for example, when the client is destroyed).
	/// <para>
	/// Can be used, for example, to release unmanaged memory, or to return
	/// reusable objects into an object pool.
	/// </para>
	/// </summary>
	public interface IValueByTypeReleaser
	{
		/// <summary>
		/// Returns the value of type <typeparamref name="TValue"/> that was being used by the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TValue"> Type of the returned <paramref name="value"/>. </typeparam>
		/// <param name="client">
		/// The object that requested the value for the value provider, or <see langword="null"/> if the requester
		/// was unknown or was not a <see cref="Component"/>.
		/// </param>
		/// <param name="value">
		/// The value of type <typeparamref name="TValue"/> that is being returned.
		/// </param>
		void Release<TValue>([AllowNull] Component client, TValue value);
	}
}
