using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can can specify the two arguments used to
	/// initialize an object that implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see>
	/// method where it can assigned them to member fields or properties.
	/// </para>
	/// <para>
	/// After the arguments have been injected the <see cref="Initializer{,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TFirstArgument, TSecondArgument> : InitializerBase<TClient, TFirstArgument, TSecondArgument> where TClient : MonoBehaviour, IInitializable<TFirstArgument, TSecondArgument>
	{
		[SerializeField]
		private Any<TFirstArgument> firstArgument = default;
		[SerializeField]
		private Any<TSecondArgument> secondArgument = default;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValueOrDefault(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValueOrDefault(this, Context.MainThread); set => secondArgument = value; }

		#if UNITY_EDITOR
		protected override bool HasNullArguments => IsNull(this, firstArgument) || IsNull(this, secondArgument);
		#endif
	}
}