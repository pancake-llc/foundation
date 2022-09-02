using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the argument used to
	/// initialize an object that implements <see cref="IInitializable{TArgument}"/>.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="Initializer{,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TArgument> : InitializerBase<TClient, TArgument> where TClient : MonoBehaviour, IInitializable<TArgument>
	{
		[SerializeField]
		private Any<TArgument> argument = default;

		/// <inheritdoc/>
		protected override TArgument Argument { get => argument.GetValueOrDefault(this, Context.MainThread); set => argument = value; }

		#if UNITY_EDITOR
		protected override bool HasNullArguments => IsNull(this, argument);
		#endif
	}
}