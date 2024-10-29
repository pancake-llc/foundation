using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to get components without allocating any garbage.
	/// </summary>
	internal static class GetComponentExtensions
	{
		/// <summary>
		/// Gets references to all components of type <typeparamref name="TComponent"/> on the <paramref name="gameObject"/>,
		/// without allocating any garbage.
		/// <para>
		/// The result of this method should never be cached or reused; it should either be used with a using statement,
		/// so that it gets disposed when leaving the method scope, or iterated once with a foreach statement,
		/// in which case it gets disposed automatically at the end of the iteration.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> The type of component to search for; either a <see cref="Component"/> or an interface type. </typeparam>
		/// <param name="gameObject"> The game object to search. </param>
		/// <returns> A list containing all matching components of type <typeparamref name="TComponent"/>. </returns>
		/// <example>
		/// <code>
		/// foreach(var component in gameObject.GetComponentsNonAlloc{Component}())
		/// {
		///		// do something with component
		/// }
		/// </code>
		/// </example>
		/// <example>
		/// <code>
		/// using var components = gameObject.GetComponentsNonAlloc{Component}();
		/// // do something with components
		/// </code>
		/// </example>
		public static ComponentCollection<TComponent> GetComponentsNonAlloc<TComponent>(this GameObject gameObject) => ComponentCollection<TComponent>.GetFrom(gameObject);

		/// <summary>
		/// Gets references to all components of type <typeparamref name="TComponent"/> on the <paramref name="gameObject"/>,
		/// without allocating any garbage.
		/// <para>
		/// The result of this method should never be cached or reused; it should either be used with a using statement,
		/// so that it gets disposed when leaving the method scope, or iterated once with a foreach statement,
		/// in which case it gets disposed automatically at the end of the iteration.
		/// </para>
		/// </summary>
		/// <param name="gameObject"> The game object to search. </param>
		/// <param name="type"> The type of component to search for; either a <see cref="Component"/> or an interface type. </param>
		/// <returns> A list containing all matching components of type <typeparamref name="TComponent"/>. </returns>
		/// <example>
		/// <code>
		/// foreach(var component in gameObject.GetComponentsNonAlloc{Component}())
		/// {
		///		// do something with component
		/// }
		/// </code>
		/// </example>
		/// <example>
		/// <code>
		/// using var components = gameObject.GetComponentsNonAlloc{Component}();
		/// // do something with components
		/// </code>
		/// </example>
		public static ComponentCollection<Component> GetComponentsNonAlloc([DisallowNull] this GameObject gameObject, [DisallowNull] Type type) => ComponentCollection<Component>.GetFrom(gameObject, type);
	}
}