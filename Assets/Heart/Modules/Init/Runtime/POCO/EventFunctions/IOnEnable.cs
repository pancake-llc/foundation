using UnityEngine;
using UnityEngine.Scripting;

namespace Sisus.Init
{
	/// <summary>
	/// Defines a class that needs to receive a callback during the OnEnable event function.
	/// <para>
	/// An object that is wrapped by a Wrapper receives the event every time the wrapper component
	/// becomes <see cref="Behaviour.isActiveAndEnabled">active and enabled</see>.
	/// </para>
	/// A class that has the Service attribute receives the event when services are being initialized,
	/// after all service instances have been created.
	/// </summary>
	[RequireImplementors]
	public interface IOnEnable : IEnableable
	{
		/// <summary>
		/// <see cref="OnEnable"/> is called every time the object becomes
		/// <see cref="Behaviour.isActiveAndEnabled">active and enabled</see>.
		/// <para>
		/// If the object has been attached to an <see cref="Behaviour.enabled"/> component on an
		/// <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/> at edit time,
		/// then <see cref="OnEnable"/> is first called when the scene or prefab that contains the <see cref="GameObject"/>
		/// is being loaded.
		/// </para>
		/// <para>
		/// If the object is attached to an <see cref="GameObject.activeInHierarchy">active</see> <see cref="GameObject"/>
		/// at runtime, then <see cref="OnEnable"/> is first called immediately, right after the <see cref="IAwake.Awake"/> event.
		/// </para>
		/// <para>
		/// If the object is attached to a <see cref="Behaviour.enabled">disabled</see> component or an
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/>,
		/// then <see cref="OnEnable"/> will be called when the object becomes
		/// <see cref="Behaviour.isActiveAndEnabled">active and enabled</see>.
		/// </para>
		/// <para>
		/// A class that has the Service attribute receives the event when services are being initialized,
		/// after all service instances have been created.
		/// </para>
		/// <para>
		/// While <see cref="Awake"/> and <see cref="IOnEnable.OnEnable"/> are called (in that order)
		/// immediately for all enabled and active objects when a scene or a prefab is being loaded,
		/// <see cref="IStart.Start"/> only gets called after a short delay, just before the first <see cref="IUpdate.Update"/> event.
		/// </para>
		/// </summary>
		void OnEnable();
	}
}