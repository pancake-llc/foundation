using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can can specify the four arguments used to
	/// initialize an object that implements
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see>
	/// method where it can assigned them to member fields or properties.
	/// </para>
	/// <para>
	/// After the arguments have been injected the <see cref="Initializer{,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		: InitializerBase<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			where TClient : MonoBehaviour, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		[SerializeField]
		private Any<TFirstArgument> firstArgument = default;
		[SerializeField]
		private Any<TSecondArgument> secondArgument = default;
		[SerializeField]
		private Any<TThirdArgument> thirdArgument = default;
		[SerializeField]
		private Any<TFourthArgument> fourthArgument = default;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValueOrDefault(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValueOrDefault(this, Context.MainThread); set => secondArgument = value; }
		/// <inheritdoc/>
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValueOrDefault(this, Context.MainThread); set => thirdArgument = value; }
		/// <inheritdoc/>
		protected override TFourthArgument FourthArgument { get => fourthArgument.GetValueOrDefault(this, Context.MainThread); set => fourthArgument = value; }

		#if UNITY_EDITOR
		protected override bool HasNullArguments
			=> IsNull(this, firstArgument) || IsNull(this, secondArgument)
			|| IsNull(this, thirdArgument) || IsNull(this, fourthArgument);
		#endif
    }
}