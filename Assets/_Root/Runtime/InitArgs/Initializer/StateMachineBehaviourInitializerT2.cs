using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can be used to specify the two arguments used to
	/// initialize a state machine behaviour of type <typeparamref name="TStateMachineBehaviour"/>
	/// that implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The argument gets injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the argument via the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see>
	/// method where it can assign them to a member field or property.
	/// </para>
	/// <para>
	/// After the argument has been injected the <see cref="StateMachineBehaviourInitializer{,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized state machine behaviour client. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client's Init function. </typeparam>
	public abstract class StateMachineBehaviourInitializer<TStateMachineBehaviour, TFirstArgument, TSecondArgument>
		: StateMachineBehaviourInitializerBase<TStateMachineBehaviour, TFirstArgument, TSecondArgument>
			where TStateMachineBehaviour : StateMachineBehaviour, IInitializable<TFirstArgument, TSecondArgument>
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