using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an <see cref="Object"/> that acts as a simple wrapper for a plain old class object of type <typeparamref name="TWrapped"/>.
	/// <para>
	/// An instance of a <see cref="MonoBehaviour">MonoBehaviour</see> class that implements this interface can be added to a <see cref="GameObject"/> using the
	/// <see cref="WrapperGameObjectExtensions.Add{TWrapped}">GameObject.Add</see> function with the wrapped plain old class object injected to the added instance's
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
		/// Gets a value indicating whether the wrapper is enabled.
		/// <para>
		/// If the object has a <see cref="Wrapper{TWrapped}"/>, then this returns the current
		/// value of the <see cref="Behaviour.enabled"/> property.
		/// </para>
		/// <para>
		/// If the object has a <see cref="ScriptableWrapper{TWrapped}"/>, then this returns a value
		/// indicating whether the object has not been destroyed.
		/// </para>
		/// </summary>
		bool enabled => this is MonoBehaviour monoBehaviour ? monoBehaviour.enabled : (Object)this;

		/// <summary>
		/// Gets a value indicating whether the wrapper is enabled and not attached to an inactive
		/// <seealso cref="GameObject"/>.
		/// <para>
		/// If the object has a <see cref="Wrapper{TWrapped}"/>, then this returns the current
		/// value of the <see cref="Behaviour.isActiveAndEnabled"/> property.
		/// </para>
		/// <para>
		/// If the object has a <see cref="ScriptableWrapper{TWrapped}"/>, then this returns a value
		/// indicating whether the object has not been destroyed.
		/// </para>
		/// </summary>
		bool isActiveAndEnabled => this is MonoBehaviour monoBehaviour ? monoBehaviour.isActiveAndEnabled : (Object)this;

		/// <summary>
		/// Returns the instance id of the object.
		/// <para>
		/// The instance id of an object is always guaranteed to be unique.
		/// </para>
		/// </summary>
		/// <returns> An <see cref="int"/> acting as a unique identifier for the object. </returns>
		int GetInstanceID();

		void Destroy() => Object.Destroy(this as Object);

		/// <summary>
		/// Returns <see langword="true"/> if the wrapper is attached to a <see cref="GameObject"/> that has the
		/// given tag; otherwise, <see langword="false"/>.
		/// </summary>
		/// <param name="tag"> The tag to check for. </param>
		/// <returns></returns>
		bool CompareTag(string tag) => this is MonoBehaviour monoBehaviour && monoBehaviour.CompareTag(tag);

		#if UNITY_6000_0_OR_NEWER
		/// <summary>
		/// Returns <see langword="true"/> if the wrapper is attached to a <see cref="GameObject"/> that has the
		/// given tag; otherwise, <see langword="false"/>.
		/// </summary>
		/// <param name="tag"> A <see cref="TagHandle"/> representing the tag to check for. </param>
		/// <returns></returns>
		bool CompareTag(TagHandle tag) => this is MonoBehaviour monoBehaviour && monoBehaviour.CompareTag(tag);
		#endif

		#if UNITY_6000_0_OR_NEWER
		/// <summary>
		/// Cancellation token raised when the wrapper is destroyed.
		/// </summary>
		System.Threading.CancellationToken destroyCancellationToken => this is MonoBehaviour monoBehaviour ? monoBehaviour.destroyCancellationToken : Application.exitCancellationToken;
		#endif

		/// <summary>
		/// Gets a reference to an object of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is attached to.
		/// </summary>
		/// <typeparam name="T"> The type of object to retrieve. </typeparam>
		/// <returns>
		/// An object of type <see typeparamref="T"/>, if the wrapper is attached to a <see cref="GameObject"/>
		/// that contains one; otherwise, <see langword="null"/>.
		/// </returns>
		/// <see cref="Init.Find.In{T}(GameObject)"/>
		T Find<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.In<T>(monoBehaviour.gameObject) : default;

		/// <summary>
		/// Gets references to all objects of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is attached to.
		/// </summary>
		/// <typeparam name="T"> The type of objects to retrieve. </typeparam>
		/// <returns>
		/// An array containing all matching objects of type <see paramref="T"/> found on the <see cref="GameObject"/>
		/// the wrapper is attached to, if the wrapper is attached to one; otherwise, an empty array.
		/// </returns>
		/// <see cref="Init.Find.AllIn{T}(GameObject)"/>
		T[] FindAll<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.AllIn<T>(monoBehaviour.gameObject) : Array.Empty<T>();

		/// <summary>
		/// Gets a reference to an object of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is
		/// attached to, or any of its children.
		/// </summary>
		/// <typeparam name="T"> The type of object to retrieve. </typeparam>
		/// <returns>
		/// An object of type <see typeparamref="T"/>, if the wrapper is attached to a <see cref="GameObject"/>
		/// that has one attached to it or any of its children; otherwise, <see langword="null"/>.
		/// </returns>
		/// <see cref="Init.Find.InChildren{T}(GameObject)"/>
		T FindInChildren<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.InChildren<T>(monoBehaviour.gameObject) : default;

		/// <summary>
		/// Gets references to all objects of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is
		/// attached to, or any of its children.
		/// </summary>
		/// <typeparam name="T"> The type of objects to retrieve. </typeparam>
		/// <returns>
		/// An array containing all matching objects of type <see paramref="T"/> found on the <see cref="GameObject"/>
		/// the wrapper is attached to and its children, if the wrapper is attached to one; otherwise, an empty array.
		/// </returns>
		/// <see cref="Init.Find.AllInChildren{T}(GameObject, bool)"/>
		T[] FindAllInChildren<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.AllInChildren<T>(monoBehaviour.gameObject) : Array.Empty<T>();

		/// <summary>
		/// Gets a reference to an object of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is
		/// attached to, or any of its parents.
		/// </summary>
		/// <typeparam name="T"> The type of object to retrieve. </typeparam>
		/// <returns>
		/// An object of type <see typeparamref="T"/>, if the wrapper is attached to a <see cref="GameObject"/>
		/// that has one attached to it or any of its parents; otherwise, <see langword="null"/>.
		/// </returns>
		/// <see cref="Init.Find.InParents{T}(GameObject, bool)"/>
		T FindInParents<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.InParents<T>(monoBehaviour.gameObject) : default;

		/// <summary>
		/// Gets references to all objects of type <see cref="T"/> found on the <see cref="GameObject"/> the wrapper is
		/// attached to, or any of its parents.
		/// </summary>
		/// <typeparam name="T"> The type of objects to retrieve. </typeparam>
		/// <returns>
		/// An array containing all matching objects of type <see paramref="T"/> found on the <see cref="GameObject"/>
		/// the wrapper is attached to and its parents, if the wrapper is attached to one; otherwise, an empty array.
		/// </returns>
		/// <see cref="Init.Find.AllInParents{T}(GameObject, bool)"/>
		T[] FindAllInParents<T>() => this is MonoBehaviour monoBehaviour ? Init.Find.AllInParents<T>(monoBehaviour.gameObject) : Array.Empty<T>();
	}
}