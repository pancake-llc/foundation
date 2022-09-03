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
	/// A base class for a component that can specify the constructor argument used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TArgument> : MonoBehaviour, IInitializer, IValueProvider<TWrapped>
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
		/// The argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		#if UNITY_EDITOR
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(Argument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		#endif

		protected virtual void OnReset(ref TArgument argument) { }

		/// <summary>
		/// Creates a new Instance of <see cref="TWrapped"/> initialized using the provided arguments and returns it.
		/// </summary>
		/// <param name="argument"> The argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[NotNull]
		protected abstract TWrapped CreateWrappedObject(TArgument argument);

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
			var set = HandleReset(this, ref target, Argument, OnReset);
			if(!AreEqual(Argument, set)) Argument = set;
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

			var argument = Argument;

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
			{
				if(argument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TArgument));
			}
			#endif

			var wrappedObject = CreateWrappedObject(argument);
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