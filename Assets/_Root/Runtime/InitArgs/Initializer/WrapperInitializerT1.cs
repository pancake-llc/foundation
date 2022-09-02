using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can specify the constructor argument used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument value can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializer<TWrapper, TWrapped, TArgument> : WrapperInitializerBase<TWrapper, TWrapped, TArgument> where TWrapper : Wrapper<TWrapped>
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