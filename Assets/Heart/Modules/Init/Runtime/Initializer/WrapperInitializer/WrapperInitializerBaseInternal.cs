#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif

namespace Sisus.Init.Internal
{
	/// <summary>
	/// A base class for a component that can specify the constructor arguments used to initialize a plain old class object
	/// of type <typeparamref name="TClient"/>, which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	public abstract class WrapperInitializerBaseInternal<TWrapper, TWrapped> : Initializer, IInitializer<TWrapped>, IValueProvider<TWrapped>, IValueByTypeProvider //, IServiceProvider
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TWrapped>
		#endif
		where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TWrapper target = default;

		private protected InitState initState = InitState.Uninitialized;
		private TWrapper initTargetResult = default;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private protected NullArgumentGuard nullArgumentGuard = DefaultNullArgumentGuardFlags;

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

		/// <inheritdoc/>
		TWrapped IValueProvider<TWrapped>.Value => initTargetResult ? initTargetResult.WrappedObject : default;

		internal override Object GetTarget() => initTargetResult ? initTargetResult : target;

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>([AllowNull] Component client, out TValue value)
		{
			if(initTargetResult && initTargetResult.WrappedObject is TValue result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}

		/// <inheritdoc/>
		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => typeof(TValue).IsAssignableFrom(typeof(TWrapped));

		/// <inheritdoc/>
		Object IInitializer.Target
		{
			get => initTargetResult ? initTargetResult : target;
			set => target = value as TWrapper;
		}

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TWrapper)) || type.IsAssignableFrom(typeof(TWrapped));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <inheritdoc/>
		#if UNITY_2023_1_OR_NEWER
		async Awaitable<object>
		#else
		async System.Threading.Tasks.Task<object>
		#endif
		IInitializer.InitTargetAsync() => await InitTargetAsync();

		/// <inheritdoc/>
		public async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TWrapped>
		#else
		System.Threading.Tasks.Task<TWrapped>
		#endif
		InitTargetAsync()
		{
			if(initState != InitState.Uninitialized)
			{
				return initTargetResult;
			}

			if(!this)
			{
				return default;
			}

			try
			{
				initState = InitState.Initializing;
				
				var task = InitTargetAsync(target);
				bool disableUntilReady;
				if(!task.GetAwaiter().IsCompleted)
				{
					disableUntilReady = target && target.enabled && target.gameObject.activeInHierarchy;
					if(disableUntilReady)
					{
						target.enabled = false;
					}
				}
				else
				{
					disableUntilReady = false;
				}
				
				initTargetResult = await InitTargetAsync(target);
				initState = InitState.Initialized;
				
				if(disableUntilReady && target)
				{
					target.enabled = true;
					
					if(!ReferenceEquals(initTargetResult, target) && initTargetResult)
					{
						initTargetResult.enabled = true;
					}
				}

				if(IsRemovedAfterTargetInitialized && this)
				{
					Updater.InvokeAtEndOfFrame(DestroySelfIfNotAsset);
				}

				return initTargetResult;
			}
			catch(Exception
			#if UNITY_EDITOR
			exception
			#endif
			)
			{
				initState = InitState.Failed;
				#if UNITY_EDITOR
				((IInitializerEditorOnly)this).NullGuardFailedMessage = exception.ToString();
				#endif
				throw;
			}
		}

		/// <summary>
		/// Returns an awaitable task for initializing the wrapped object with the arguments specified by this initializer.
		/// <para>
		/// The method will wait until all dependencies of the client are ready, and only then initialize the target.
		/// For example, if one of the dependencies is an addressable asset, the method can wait until the asset
		/// has finished loading, and then pass it to the client's Init method.
		/// </para>
		/// <para>
		/// Note that if any dependency requires asynchronous loading, and the <paramref name="target"/> is a
		/// component attached to an active scene object, then initialization event functions like Awake, OnEnable
		/// and Start can get called for the target before initialization has finished.
		/// </para>
		/// <para>
		/// If <paramref name="target"/> is attached to the same <see cref="GameObject"/> as the initializer,
		/// then this method initializes the <paramref name="target"/> and returns it.
		/// </para>
		/// <para>
		/// If <paramref name="target"/> is attached to a different <see cref="GameObject"/> than the initializer, like for example a different prefab,
		/// then this method clones the <paramref name="target"/>, initializes the clone, and returns it. 
		/// </para>
		/// <para>
		/// If <paramref name="target"/> is <see langword="null"/>,
		/// then this method attaches a new component to this <see cref="GameObject"/>,
		/// initializes it, and returns it.
		/// </para>
		/// </summary>
		/// <param name="target">
		/// An object to initialize, a prefab to clone, or <see langword="null"/>.
		/// </param>
		/// <returns> The initialized object. </returns>
		private protected virtual
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TWrapper> InitTargetAsync(TWrapper target) => AwaitableUtility.FromResult(InitTarget(target));
		#else
		System.Threading.Tasks.Task<TWrapper> InitTargetAsync(TWrapper target) => System.Threading.Tasks.Task.FromResult(InitTarget(target));
		#endif

		/// <inheritdoc/>
		public TWrapped InitTarget()
		{
			if(initState != InitState.Uninitialized)
			{
				return initTargetResult;
			}

			if(!this)
			{
				return default;
			}

			try
			{
				initState = InitState.Initializing;
				initTargetResult = InitTarget(target);
				initState = InitState.Initialized;

				if(IsRemovedAfterTargetInitialized && this)
				{
					Updater.InvokeAtEndOfFrame(DestroySelfIfNotAsset);
				}

				return initTargetResult.WrappedObject;
			}
			catch(Exception
			#if UNITY_EDITOR
			exception
			#endif
			)
			{
				initState = InitState.Failed;
				#if UNITY_EDITOR
				((IInitializerEditorOnly)this).NullGuardFailedMessage = exception.ToString();
				#endif
				throw;
			}
		}

		/// <summary>
		/// Resets the initializer to its default state.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// </para>
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </para>
		/// </summary>
		protected virtual void OnReset() { }

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the configured initialization arguments
		/// and wraps it inside the <paramref name="target"/> components.
		/// </summary>
		/// <param name="target"> The wrapper that should hold the created object. </param>
		/// <returns> The new instance of type <see cref="TClient"/>. </returns>
		[return: NotNull]
		private protected abstract TWrapper InitTarget([AllowNull] TWrapper target);

		async private protected void Awake()
		{
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying = Application.isPlaying;
			#endif

			#if DEBUG && UNITY_2023_1_OR_NEWER
			if(target && target.didAwake && !target.GetType().IsDefined(typeof(ExecuteAlways), false))
			{
				Debug.LogWarning($"{GetType().Name}.Awake was called after target {target.GetType().Name}.Awake has already executed! You can add the [InitAfter(typeof({target.GetType().Name}))] attribute to the {GetType().Name} class to make sure it is initialized before its client.");
			}
			#endif

			if(
			#if UNITY_EDITOR
			!gameObject.IsAsset(resultIfSceneObjectInEditMode: true) &&
			#endif	
			IsAsync)
			{
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif
					
					await InitTargetAsync();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception e)
				{
					if(e is not OperationCanceledException)
					{
						throw;
					}
				}
				#endif

				return;
			}

			InitTarget();
		}

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the configured initialization arguments and returns it.
		/// <para>
		/// Note: If you need support circular dependencies between your objects then you need to also override
		/// <see cref="GetOrCreateUninitializedWrappedObject"/>.
		/// </para>
		/// </summary>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[return: NotNull]
		private protected abstract TWrapped CreateWrappedObject();

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> using the default constructor
		/// or retrieves an existing instance of it contained in <see cref="TWrapper"/>.
		/// <para>
		/// By default, this method returns <see langword="null"/>. When this is the case then
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
		[return: MaybeNull]
		protected virtual TWrapped GetOrCreateUninitializedWrappedObject() => target && ReferenceEquals(target.gameObject, gameObject) ? target.wrapped : default;

		/// <summary>
		/// Initializes the existing <see cref="target"/> or new instance of type <see cref="TWrapper"/> using the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TWrapper"/>. </returns>
		[return: NotNull]
		protected virtual TWrapper InitWrapper(TWrapped wrappedObject)
		{
			if(!target)
			{
				return gameObject.AddComponent<TWrapper, TWrapped>(wrappedObject);
			}

			if(!ReferenceEquals(target.gameObject, gameObject))
			{
				return target.Instantiate(wrappedObject);
			}

			(target as IInitializable<TWrapped>).Init(wrappedObject);

			return target;
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ThrowIfMissing<TArgument>(TArgument argument)
		{
			if(argument == Null)
			{
				throw GetMissingInitArgumentsException(GetType(), typeof(TWrapped), typeof(TArgument));
			}
		}
		#endif

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private protected bool IsRuntimeNullGuardActive => nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException)
			#if UNITY_EDITOR
			&& Application.isPlaying
			#endif
			;
		#endif

		private void DestroySelf()
		{
			if(this)
			{
				Destroy(this);
			}
		}

		#if UNITY_EDITOR
		bool IInitializerEditorOnly.ShowNullArgumentGuard => true;
		bool IInitializerEditorOnly.CanInitTargetWhenInactive => false;
		bool IInitializerEditorOnly.CanGuardAgainstNull => true;
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get; set; } = "";
		NullGuardResult IInitializerEditorOnly.EvaluateNullGuard() => EvaluateNullGuard();
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		bool IInitializerEditorOnly.WasJustReset { get; set; }
		bool IInitializerEditorOnly.IsAsync => IsAsync;
		void IInitializerEditorOnly.SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) => SetReleaseArgumentOnDestroy(argument, shouldRelease);
		void IInitializerEditorOnly.SetIsArgumentAsync(Arguments argument, bool isAsync) => SetIsArgumentAsync(argument, isAsync);
		private protected virtual void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) { }
		private protected virtual void SetIsArgumentAsync(Arguments argument, bool isAsyncValueProvider) { }
		private protected abstract NullGuardResult EvaluateNullGuard();

		private protected abstract void Reset();
		private protected abstract void OnValidate();
		#endif
	}
}