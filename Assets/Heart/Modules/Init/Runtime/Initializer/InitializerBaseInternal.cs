#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif

namespace Sisus.Init.Internal
{
    // Base class for all Initializers; targeted by InitializerEditor.
	public abstract class InitializerBaseInternal : MonoBehaviour { }

	/// <summary>
	/// A base class for a component that can specify the arguments used to initialize a client of type <typeparamref name="TClient"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// After the arguments have been injected the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	public abstract class InitializerBaseInternal<TClient> : InitializerBaseInternal, IInitializer<TClient>, IValueProvider<TClient>, IValueByTypeProvider
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient>
		#endif
		where TClient : Object
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TClient target = default;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private protected NullArgumentGuard nullArgumentGuard = DefaultNullArgumentGuardFlags;

		private protected InitState initState = InitState.Uninitialized;
		private TClient initTargetResult = default;

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

		/// <inheritdoc/>
		TClient IValueProvider<TClient>.Value => initTargetResult;

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>([AllowNull] Component client, out TValue value)
		{
			if(initTargetResult is TValue result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => typeof(TValue).IsAssignableFrom(typeof(TClient));

		/// <inheritdoc/>
		Object IInitializer.Target
		{
			get => initTargetResult ? initTargetResult : target;
			set => target = value as TClient;
		}

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TClient));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <inheritdoc/>
		async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<object>
		#else
		System.Threading.Tasks.Task<object>
		#endif
		IInitializer.InitTargetAsync() => await InitTargetAsync();

		/// <inheritdoc/>
		public async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif
		InitTargetAsync()
		{
			if(initState != InitState.Uninitialized)
			{
				return initTargetResult;
			}

			if(!this)
			{
				return null;
			}

			initState = InitState.Initializing;

			var initTargetAsync = InitTargetAsync(target);

			try
			{
				initTargetResult = await initTargetAsync;

				initState = InitState.Initialized;

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
		/// Returns an awaitable task for initializing the client object of type <see cref="TClient"/>
		/// with the arguments specified by this initializer.
		/// <para>
		/// The method will wait until all dependencies of the client are ready, and only then initialize the target.
		/// For example, if one of the dependencies is an addressable asset, the method can wait until the asset
		/// has finished loading, and then pass it to the client's Init method.
		/// </para>
		/// <para>
		/// Note that if any dependency requires asynchronous loading, and the <paramref name="client"/> is a
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
		/// then this method attaches a new component of type <see cref="TClient"/> to this <see cref="GameObject"/>,
		/// initializes it, and returns it.
		/// </para>
		/// </summary>
		/// <param name="target">
		/// An object to initialize, a prefab to clone, or <see langword="null"/>.
		/// </param>
		/// <returns> The initialized object. </returns>
		private protected virtual
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient> InitTargetAsync([AllowNull] TClient target) => AwaitableUtility.FromResult(InitTarget(target));
		#else
		System.Threading.Tasks.Task<TClient> InitTargetAsync([AllowNull] TClient target) => System.Threading.Tasks.Task.FromResult(InitTarget(target));
		#endif

		/// <inheritdoc/>
		public TClient InitTarget()
		{
			if(initState != InitState.Uninitialized)
			{
				return initTargetResult;
			}

			if(!this)
			{
				return null;
			}

			initState = InitState.Initializing;

			try
			{

				initTargetResult = InitTarget(target);
				initState = InitState.Initialized;

				if(IsRemovedAfterTargetInitialized)
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

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private protected bool IsRuntimeNullGuardActive
		{
			get
			{
				if(!nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
				{
					return false;
				}

				#if UNITY_EDITOR
				if(gameObject.IsAsset(resultIfSceneObjectInEditMode: true))
				{
					return false;
				}
				#endif

				return true;
			}
		}
		#endif

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
		/// Initializes the client object of type <see cref="TClient"/> with the arguments specified by this initializer.
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
		/// then this method attaches a new component of type <see cref="TClient"/> to this <see cref="GameObject"/>,
		/// initializes it, and returns it.
		/// </para>
		/// </summary>
		/// <param name="target">
		/// An object to initialize, a prefab to clone, or <see langword="null"/>.
		/// </param>
		/// <returns> The initialized object. </returns>
		[return: NotNull]
		private protected abstract TClient InitTarget([AllowNull] TClient target);

		async private protected void Awake()
		{
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying = Application.isPlaying;
			#endif

			#if (DEBUG || INIT_ARGS_SAFE_MODE) && UNITY_2023_1_OR_NEWER
			if(target is MonoBehaviour targetMonoBehaviour && targetMonoBehaviour && targetMonoBehaviour.didAwake && targetMonoBehaviour.GetType().GetCustomAttribute<ExecuteAlways>() is null)
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
				await InitTargetAsync();
				return;
			}

			InitTarget();
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#else
		[System.Diagnostics.Conditional("FALSE")]
		#endif
		private protected void ThrowIfMissing<TArgument>(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(argument == Null)
			{
				GetMissingInitArgumentsException(this, typeof(TArgument)).LogAsError();
				throw new ArgumentNullException(typeof(TArgument).Name);
			}
			#endif
		}

		private void DestroySelf()
		{
			if(this)
			{
				Destroy(this);
			}
		}

		private void DestroySelfIfNotAsset()
		{
			if(!this)
			{
				return;
			}

			#if UNITY_EDITOR
			if(gameObject.IsAsset(resultIfSceneObjectInEditMode: true))
			{
				return;
			}
			#endif

			Destroy(this);
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
		void IInitializerEditorOnly.SetIsArgumentAsyncValueProvider(Arguments argument, bool isAsyncValueProvider) => SetIsArgumentAsyncValueProvider(argument, isAsyncValueProvider);
		private protected virtual void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) { }
		private protected virtual void SetIsArgumentAsyncValueProvider(Arguments argument, bool isAsyncValueProvider) { }
		private protected abstract NullGuardResult EvaluateNullGuard();
		private protected abstract void Reset();
		private protected abstract void OnValidate();
		#endif
	}
}