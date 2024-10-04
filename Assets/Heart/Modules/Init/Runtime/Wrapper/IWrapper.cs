using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an <see cref="Object"/> that acts as a simple wrapper for a plain old class object of type <typeparamref name="TWrapped"/>.
	/// <para>
	/// An instance of a <see cref="MonoBehaviour">MonoBehaviour</see> class that implements this interface can be added to a <see cref="GameObject"/> using the
	/// <see cref="ICoroutinesExtensions.AddWrapped">GameObject.AddWrapped</see> function with the wrapped plain old class object injected to the added instance's
	/// <see cref="IInitializable{TWrapped}.Init">Init</see> function.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapped"> Type of the wrapped plain old class object. </typeparam>
	[RequireImplementors]
	public interface IWrapper<TWrapped> : IWrapper, IInitializable<TWrapped>
	{
		/// <summary>
		/// The plain old class <see cref="object"/> wrapped by this <see cref="UnityEngine.Object"/>.
		/// </summary>
		new TWrapped WrappedObject { get; }

		/// <summary>
		/// The plain old class <see cref="object"/> wrapped by this <see cref="UnityEngine.Object"/>.
		/// </summary>
		object IWrapper.WrappedObject => WrappedObject;
	}

	/// <summary>
	/// Represents a <see cref="UnityEngine.Object">Object</see> that acts as a simple wrapper for a plain old class object of type <typeparamref name="TWrapped"/>.
	/// <para>
	/// Base interface of <see cref="IWrapper{TWrapped}"/>.
	/// </para>
	/// </summary>
	[RequireImplementors]
	public interface IWrapper : ICoroutineRunner
	{
		/// <summary>
		/// The plain old class <see cref="object"/> wrapped by this <see cref="UnityEngine.Object"/>.
		/// </summary>
		object WrappedObject { get; }

		/// <summary>
		/// The <see cref="GameObject"/> this wrapper is attached to, if any.
		/// <para>
		/// <see cref="Component"/> wrappers are always attached to a <see cref="GameObject"/>,
		/// while <see cref="ScriptableObject"/> wrappers are never.
		/// </para>
		/// </summary>
		[MaybeNull]
		GameObject gameObject { get; }

		/// <summary>
		/// This wrapper as a <see cref="MonoBehaviour"/>, or <see langword="null"/>
		/// if the wrapper class does not derive from <see cref="MonoBehaviour"/>.
		/// </summary>
		[MaybeNull]
		MonoBehaviour AsMonoBehaviour => this as MonoBehaviour;

		/// <summary>
		/// This wrapper as an <see cref="Object"/>.
		/// </summary>
		[NotNull]
		Object AsObject => this as Object;

		/// <summary>
		/// Should the object be hidden, saved with the Scene or modifiable by the user?
		/// </summary>
		HideFlags hideFlags { get; set; }

		/// <summary>
		/// The name of the object.
		/// </summary>
		string name { get; set; }

		/// <summary>
		/// Returns the instance id of the object.
		/// <para>
		/// The instance id of an object is always guaranteed to be unique.
		/// </para>
		/// </summary>
		/// <returns> An <see cref="int"/> acting as an unique identifier for the object. </returns>
		int GetInstanceID();

		#if UNITY_2022_2_OR_NEWER
		/// <summary>
		/// Cancellation token raised when the wrapper is destroyed.
		/// </summary>
		System.Threading.CancellationToken destroyCancellationToken { get; }
		#endif
	}
}