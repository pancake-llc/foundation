namespace Sisus.Init
{
	/// <summary>
	/// Specifies which client objects can receive services from a particular service.
	/// </summary>
	public enum Clients
	{
		/// <summary>
		/// Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
		/// <see cref="Services"/> can receive services from it.
		/// </summary>
		InGameObject = 0,

		/// <summary>
		/// Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
		/// <see cref="Services"/>, or any of its children (including nested children), can receive services from it.
		/// </summary>
		InChildren = 1,

		/// <summary>
		/// Only clients that are attached to the same <see cref="UnityEngine.GameObject"/> as the
		/// <see cref="Services"/>, or any of its parents (including nested parents), can receive services from it.
		/// </summary>
		InParents = 2,

		/// <summary>
		/// Only clients that are attached to the <see cref="UnityEngine.GameObject"/> that is at the
		/// <see cref="UnityEngine.Transform.root"/> of the <see cref="Services">Services's</see> hierarchy,
		/// or any of its children (including nested children), can receive services from the <see cref="Services"/>.
		/// </summary>
		InHierarchyRootChildren = 3,

		/// <summary>
		/// Only clients belonging to the same <see cref="UnityEngine.GameObject.scene"/> as the
		/// <see cref="Services"/> can receive services from it.
		/// </summary>
		InScene = 4,

		/// <summary>
		/// All scene objects can receive services from the <see cref="Services"/> regardless
		/// of which active <see cref="UnityEngine.GameObject.scene"/> they belong to.
		/// </summary>
		InAllScenes = 5,

		/// <summary>
		/// All clients can receive services from the <see cref="Services"/> regardless of
		/// of which active <see cref="UnityEngine.GameObject.scene"/> they belong to or if they
		/// belong a scene at all.
		/// </summary>
		Everywhere = 6
	}
}