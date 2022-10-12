#pragma warning disable CS0414

using System;
using JetBrains.Annotations;
#if UNITY_EDITOR
using Pancake.Init.EditorOnly;
#endif
using UnityEngine;
using static Pancake.Init.Internal.InitializerUtility;
using static Pancake.Init.NullExtensions;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for a component that can specify the two constructor arguments used to initialize
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
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument> : MonoBehaviour
		, IInitializer<TWrapped, TFirstArgument, TSecondArgument>, IValueProvider<TWrapped>
		#if UNITY_EDITOR
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

		#if UNITY_EDITOR
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(FirstArgument) || IsNull(SecondArgument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		#endif

		/// <inheritdoc/>
		public TWrapped InitTarget()
		{
			if(this == null)
			{
				return target;
			}

			// Handle instance first creation method, which supports cyclical dependencies (A requires B, and B requires A).
			if(target is IInitializable<TFirstArgument, TSecondArgument> initializable && GetOrCreateUnitializedWrappedObject() is var wrappedObject)
			{
				target = InitWrapper(wrappedObject);

				var firstArgument = FirstArgument;
				OnAfterUnitializedWrappedObjectArgumentRetrieved(this, ref firstArgument);
				var secondArgument = SecondArgument;
				OnAfterUnitializedWrappedObjectArgumentRetrieved(this, ref secondArgument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
				{
					if(firstArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TFirstArgument));
					if(secondArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TSecondArgument));
				}
				#endif

				initializable.Init(firstArgument, secondArgument);
			}
			// Handle arguments first creation method, which supports constructor injection.
			else
			{
				var firstArgument = FirstArgument;
				var secondArgument = SecondArgument;

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
				{
					if(firstArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TFirstArgument));
					if(secondArgument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TSecondArgument));
				}
				#endif

				wrappedObject = CreateWrappedObject(firstArgument, secondArgument);
				target = InitWrapper(wrappedObject);
			}
			
			Updater.InvokeAtEndOfFrame(DestroySelf);
			return target;
		}

		protected virtual void OnReset(ref TFirstArgument firstArgument, ref TSecondArgument secondArgument) { }

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> using the default constructor
		/// or retrieves an existing instance of it contained in <see cref="TWrapper"/>.
		/// <para>
		/// By default this method returns <see langword="null"/>. When this is the case then
		/// the <see cref="CreateWrappedObject"/> overload will be used to create the
		/// <see cref="TWrapped"/> instance during initialization.
		/// </para>
		/// <para>
		/// If <see cref="TWrapped"/> is a serializable class, or this method is overridden to return
		/// a non-null value, and <see cref="TWrapped"/> implements <see cref="IInitializable{,}"/>,
		/// then this overload will be used to create the instance during initialization instead
		/// of <see cref="CreateWrappedObject"/>.
		/// The instance will be created and injected to the <see cref="TWrapper"/>
		/// component first, and only then will all the initialization arguments be retrieved and injected
		/// to the Wrapped object through its <see cref="IInitializable{,}.Init"/> function.
		/// </para>
		/// <para>
		/// The main benefit with this form of two-part initialization (first create and inject the instance,
		/// then retrieve the arguments and inject them to the instance), is that it makes it possible to
		/// have cyclical dependencies between your objects. Normally if A requires B during its initialization,
		/// and B requires A during its initialization, both will fail to initialize as the cyclical dependency
		/// is unresolvable. With two-part initialization it is possible to initialize both objects, because A
		/// can be created without its dependencies injected at first, then B can be created and initialized with A,
		/// and finally B can be injected to A.
		/// is that 
		/// </para>
		/// </summary>
		/// <returns> Instance of the <see cref="TWrapped"/> class or <see langword="null"/>. </returns>
		[CanBeNull]
		protected virtual TWrapped GetOrCreateUnitializedWrappedObject() => target != null && target.gameObject == gameObject ? target.wrapped : default;

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the provided arguments and returns it.
		/// <para>
		/// Note: If you need support circular dependencies between your objects then you need to also override
		/// <see cref="GetOrCreateUnitializedWrappedObject()"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> The first argument used to initialize the wrapped object. </param>
		/// <param name="secondArgument"> The second argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[JetBrains.Annotations.NotNull]
		protected abstract TWrapped CreateWrappedObject(TFirstArgument firstArgument, TSecondArgument secondArgument);

		/// <summary>
		/// Initializes the existing <see cref="target"/> or new instance of type <see cref="TWrapper"/> using the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TWrapper"/>. </returns>
		[JetBrains.Annotations.NotNull]
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
			var set = HandleReset(this, ref target, FirstArgument, SecondArgument, OnReset);
			if(!AreEqual(FirstArgument, set.firstArgument)) FirstArgument = set.firstArgument;
			if(!AreEqual(SecondArgument, set.secondArgument)) SecondArgument = set.secondArgument;
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

		private void DestroySelf()
		{
			if(this != null)
			{
				Destroy(this);
			}
		}
    }
}