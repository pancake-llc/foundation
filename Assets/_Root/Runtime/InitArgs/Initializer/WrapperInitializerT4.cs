using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can specify the four constructor arguments used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializer<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		: WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			where TWrapper : Wrapper<TWrapped>
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