using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object that can asynchronously retrieve a value of a given type for a client <see cref="Component"/>.
	/// <para>
	/// If a class derives from <see cref="Object"/> and implements <see cref="IValueByTypeProviderAsync"/> then
	/// <see cref="Any{T}"/> can wrap an instance of this class and return its value when <see cref="Any{T}.GetValueAsync{TClient}"/> is called.
	/// </para>
	/// </summary>
	/// <seealso cref="IValueProvider{TValue}"/>
	/// <seealso cref="IValueProviderAsync{TValue}"/>
	/// <seealso cref="IValueByTypeProvider"/>
	[RequireImplementors]
	public interface IValueByTypeProviderAsync
	{
		/// <summary>
		/// Asynchronously retrieves a value of type <typeparamref name="TValue"/> for the <paramref name="client"/>.
		/// </summary>
		/// <typeparam name="TValue"> Type of the requested <paramref name="value"/>. </typeparam>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see cref="Awaitable{TValue}"/> that can be <see langword="await">awaited</see> to get the value of type <typeparamref name="TValue"/>, if available;
		/// otherwise, <see langword="default"/>.
		/// </returns>
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TValue>
		#else
		System.Threading.Tasks.Task<TValue>
		#endif
		GetForAsync<TValue>([AllowNull] Component client);

		/// <summary>
		/// Gets a value indicating whether this value provider can provide a value of type
		/// <typeparamref name="TValue"/> for the <paramref name="client"/> at this time.
		/// </summary>
		/// <param name="client">
		/// The component requesting the value, if request is coming from a component; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if can provide a value for the client at this time; otherwise, <see langword="false"/>.
		/// </returns>
		bool HasValueFor<TValue>(Component client)
		{
			var awaitable = GetForAsync<TValue>(client);
			#if UNITY_2023_1_OR_NEWER
			return !awaitable.GetAwaiter().IsCompleted || awaitable.GetAwaiter().GetResult() is not null;
			#else
			return !awaitable.IsFaulted;
			#endif
		}

		/// <summary>
		/// Gets a value indicating whether this value provider can potentially provide
		/// a value of the given type to the client at runtime.
		/// <para>
		/// Used by the Inspector to determine if the value provider can be assigned to an Init argument field.
		/// </para>
		/// </summary>
		/// <typeparam name="TValue"> Type of the value that would be provided. </typeparam>
		/// <param name="client"> The client component that would receive the value. </param>
		/// <returns>
		/// <see langword="true"/> if can potentially provide a value of the given type to the client
		/// at runtime; otherwise, <see langword="false"/>.
		/// </returns>
		bool CanProvideValue<TValue>([AllowNull] Component client);
	}
}