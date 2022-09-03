#pragma warning disable CS0414

using System;
using JetBrains.Annotations;
using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;
using static Pancake.NullExtensions;
using Object = UnityEngine.Object;

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
	/// <para>
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,,,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,,,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : MonoBehaviour, IInitializer, IValueProvider<TWrapped>
		#if DEBUG
		, IInitializerEditorOnly
		#endif
		where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TWrapper target = null;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private NullArgumentGuard nullArgumentGuard = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException;

		/// <inheritdoc/>
		TWrapped IValueProvider<TWrapped>.Value => target != null ? target.WrappedObject : default;

		/// <inheritdoc/>
		object IValueProvider.Value => target != null ? target.WrappedObject : default(TWrapped);

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = (TWrapper)value; }

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TWrapper)) || type.IsAssignableFrom(typeof(TWrapped));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <summary>
		/// The first argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TFirstArgument FirstArgument { get; set; }

		/// <summary>
		/// The second argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TSecondArgument SecondArgument { get; set; }

		/// <summary>
		/// The third argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TThirdArgument ThirdArgument { get; set; }

		/// <summary>
		/// The fourth argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TFourthArgument FourthArgument { get; set; }

		#if UNITY_EDITOR
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(FirstArgument) || IsNull(SecondArgument) || IsNull(ThirdArgument) || IsNull(FourthArgument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		#endif

		protected virtual void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument, ref TThirdArgument thirdArgument, ref TFourthArgument fourthArgument) { }

		/// <summary>
		/// Creates a new Instance of <see cref="TWrapped"/> initialized using the provided arguments and returns it.
		/// </summary>
		/// <param name="firstArgument"> The first argument used to initialize the wrapped object. </param>
		/// <param name="secondArgument"> The second argument used to initialize the wrapped object. </param>
		/// <param name="thirdArgument"> The third argument used to initialize the wrapped object. </param>
		/// <param name="fourthArgument"> The fourth argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[NotNull]
		protected abstract TWrapped CreateWrappedObject(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument);

		/// <summary>
		/// Initializes the existing <see cref="target"/> or new Instance of type <see cref="TWrapper"/> using the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new Instance of type <see cref="TWrapper"/>. </returns>
		[NotNull]
		protected virtual TWrapper InitWrapper(TWrapped wrappedObject)
        {
            if(target == null)
            {
                return gameObject.AddComponent<TWrapper, TWrapped>(wrappedObject);
            }

            if(target.gameObject != gameObject)
            {
                return target.Instantiate(wrappedObject);
            }

            (target as IInitializable<TWrapped>).Init(wrappedObject);

			return target;
        }

		#if UNITY_EDITOR
        private void Reset()
		{
			var set = HandleReset(this, ref target, FirstArgument, SecondArgument, ThirdArgument, FourthArgument, OnReset);
			if(!AreEqual(FirstArgument, set.firstArgument)) FirstArgument = set.firstArgument;
			if(!AreEqual(SecondArgument, set.secondArgument)) SecondArgument = set.secondArgument;
			if(!AreEqual(ThirdArgument, set.thirdArgument)) ThirdArgument = set.thirdArgument;
			if(!AreEqual(FourthArgument, set.fourthArgument)) FourthArgument = set.fourthArgument;
		}

		private void OnValidate() => OnMainThread(Validate);
		#endif

		protected virtual void Validate()
		{
			#if UNITY_EDITOR
			ValidateOnMainThread(this);
			#endif
		}

		private void Awake() => InitTarget();

		private TWrapped InitTarget()
		{
			if(this == null)
			{
				return target;
			}

			var firstArgument = FirstArgument;
			var secondArgument = SecondArgument;
			var thirdArgument = ThirdArgument;
			var fourthArgument = FourthArgument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
			{
				if(firstArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TFirstArgument));
				if(secondArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TSecondArgument));
				if(thirdArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TThirdArgument));
				if(fourthArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TFourthArgument));
			}
			#endif

			var wrappedObject = CreateWrappedObject(firstArgument, secondArgument, thirdArgument, fourthArgument);
			target = InitWrapper(wrappedObject);
			Updater.InvokeAtEndOfFrame(DestroySelf);
			return target;
		}

		private void DestroySelf()
		{
			if(this != null)
			{
				Destroy(this);
			}
		}
    }
}