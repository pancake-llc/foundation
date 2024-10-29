using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Defines a class that needs to receive a callback during the Awake event function.
	/// <para>
	/// An object that is wrapped by a Wrapper receives the event when the wrapper component is being loaded.
	/// </para>
	/// A class that has the Service attribute receives the event when services are being initialized,
	/// after all service instances have been created.
	/// </summary>
	[RequireImplementors]
	public interface IAwake
	{
		/// <summary>
		/// <see cref="Awake"/> is called when the object is being loaded.
		/// <para>
		/// If the object has been attached to an <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/>
		/// at edit time, then <see cref="Awake"/> is called when the scene or prefab that contains the <see cref="GameObject"/>
		/// is being loaded. <see cref="Awake"/> is called regardless of whether the component is
		/// <see cref="MonoBehaviour.enabled">enabled</see>.
		/// </para>
		/// <para>
		/// If the object is attached to an <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/>
		/// at runtime, then <see cref="Awake"/> is called immediately.
		/// </para>
		/// <para>
		/// If the object is attached to an <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/>,
		/// then <see cref="Awake"/> will only be called when the <see cref="GameObject"/> becomes
		/// <see cref="GameObject.activeInHierarchy">active</see>.
		/// </para>
		/// <para>
		/// A class that has the Service attribute receives the event when services are being initialized,
		/// after all service instances have been created.
		/// </para>
		/// <para>
		/// <see cref="Awake"/> and <see cref="IOnEnable.OnEnable"/> are called (in that order)
		/// immediately for all enabled and active objects when a scene or a prefab is being loaded,
		/// <see cref="IStart.Start"/> only gets called after a short delay, just before the first
		/// <see cref="IUpdate.Update"/> event.
		/// </para>
		/// </summary>
		void Awake();
	}
}