#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
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
	/// A base class for a component that can initialize a client of type <typeparamref name="TClient"/>
	/// even if the <see cref="GameObject"/> that contains them is inactive in the hierarchy.
	/// <para>
	/// After the client has been initialized the initializer is removed from the <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	public abstract class InactiveInitializerBaseInternal<TClient> : MonoBehaviour, IInitializer<TClient>, IValueProvider<TClient>, IValueByTypeProvider, ISerializationCallbackReceiver
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient>
		#endif
		where TClient : Component
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TClient target = default;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private protected NullArgumentGuard nullArgumentGuard = DefaultNullArgumentGuardFlags;

		private InitState initState = InitState.Uninitialized;

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

		/// <inheritdoc/>
		TClient IValueProvider<TClient>.Value => target;

		/// <inheritdoc/>
		bool IInitializer.ProvidesCustomInitArguments => false;

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = value as TClient; }

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>([AllowNull] Component client, out TValue value)
		{
			if(target && target is TValue result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => typeof(TValue).IsAssignableFrom(typeof(TClient));

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TClient));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget(Context.MainThread);

		/// <inheritdoc/>
		async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<object>
		#else
		System.Threading.Tasks.Task<object>
		#endif
		IInitializer.InitTargetAsync()
		{
			_ = await InitTargetAsync(Context.MainThread);
			return target;
		}

		/// <inheritdoc/>
		async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif
		IInitializer<TClient>.InitTargetAsync()
		{
			_ = await InitTargetAsync(Context.MainThread);
			return target;
		}

		private async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<bool>
		#else
		System.Threading.Tasks.Task<bool>
		#endif
		InitTargetAsync(Context context)
		{
			if(initState is InitState.Initialized or InitState.Initializing)
			{
				return true;
			}

			if(!this || initState is InitState.Failed)
			{
				return false;
			}

			try
			{
				initState = InitState.Initializing;

				if(!await InitTargetAsync(target, context))
				{
					initState = InitState.Uninitialized;
					return false;
				}

				initState = InitState.Initialized;

				if(this && IsRemovedAfterTargetInitialized)
				{
					Updater.InvokeAtEndOfFrame(DestroySelfIfNotAsset);
				}

				return true;
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
			Awaitable<bool> InitTargetAsync([AllowNull] TClient target, Context context) => AwaitableUtility.FromResult(InitTarget(target, context));
		#else
		System.Threading.Tasks.Task<bool> InitTargetAsync([AllowNull] TClient target, Context context) => System.Threading.Tasks.Task.FromResult(InitTarget(target, context));
		#endif

		/// <inheritdoc/>
		public TClient InitTarget()
		{
			_ = InitTarget(Context.MainThread);
			return target;
		}

		public bool InitTarget(Context context)
		{
			if(initState is InitState.Initialized or InitState.Initializing)
			{
				return true;
			}

			if(!this || initState is InitState.Failed)
			{
				return false;
			}

			initState = InitState.Initializing;

			try
			{
				if(!InitTarget(target, context))
				{
					initState = InitState.Uninitialized;
					return false;
				}

				initState = InitState.Initialized;

				if(IsRemovedAfterTargetInitialized)
				{
					Updater.InvokeAtEndOfFrame(DestroySelfIfNotAsset);
				}

				return true;
			}
			catch(
			Exception
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
		/// <param name="target"> The object to initialize. </param>
		/// <param name="context"> Contexts from which the method is being called. </param>
		/// <returns> <see langword="true"/> if target was initialized successfully; otherwise, <see langword="true"/>. </returns>
		[return: NotNull]
		private protected abstract bool InitTarget([DisallowNull] TClient target, Context context);

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }

		public async void OnAfterDeserialize()
		{
			await Until.UnitySafeContext();
			await OnAfterDeserializeOnMainThread();
		}

		private protected async
		#if UNITY_2023_1_OR_NEWER
		Awaitable
		#else
		System.Threading.Tasks.Task
		#endif
		OnAfterDeserializeOnMainThread()
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return;
			}
			#endif

			if(!this || !target)
			{
				return;
			}

			if(IsAsync ? await InitTargetAsync(Context.MainThread) : InitTarget(Context.MainThread))
			{
				return;
			}

			if(!ServiceInjector.AsyncServicesAreReady)
			{
				ServiceInjector.AsyncServicesBecameReady += OnAsyncServicesReady;
			}

			Updater.InvokeAtEndOfFrame(OnManuallyRegisteredServicesReady);

			void OnAsyncServicesReady()
			{
				if(this && !InitTarget(Context.MainThread))
				{
					Updater.InvokeAtEndOfFrame(OnManuallyRegisteredServicesReady);
				}
			}

			void OnManuallyRegisteredServicesReady()
			{
				if(this && !InitTarget(Context.MainThread))
				{
					initState = InitState.Failed;
					var exception = new MissingInitArgumentsException(target?.GetType() ?? typeof(TClient));
					#if UNITY_EDITOR
					((IInitializerEditorOnly)this).NullGuardFailedMessage = exception.Message;
					#endif
					throw exception;
				}
			}
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void ThrowIfMissing<TArgument>(TArgument argument)
		{
			if(argument == Null)
			{
				throw GetMissingInitArgumentsException(GetType(), typeof(TClient), typeof(TArgument));
			}
		}
		#endif

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
		bool IInitializerEditorOnly.CanInitTargetWhenInactive => true;
		bool IInitializerEditorOnly.CanGuardAgainstNull => false;
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get; set; } = "";
		NullGuardResult IInitializerEditorOnly.EvaluateNullGuard() => EvaluateNullGuard();
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		bool IInitializerEditorOnly.WasJustReset { get; set; }
		bool IInitializerEditorOnly.IsAsync => IsAsync;
		void IInitializerEditorOnly.SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) => SetReleaseArgumentOnDestroy(argument, shouldRelease);
		void IInitializerEditorOnly.SetIsArgumentAsync(Arguments argument, bool isAsync) => SetIsArgumentAsyncValueProvider(argument, isAsync);
		private protected virtual void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) { }
		private protected virtual void SetIsArgumentAsyncValueProvider(Arguments argument, bool isAsyncValueProvider) { }
		private protected virtual NullGuardResult EvaluateNullGuard() => NullGuardResult.Passed;
		private protected virtual void Reset() { }
		private protected virtual void OnValidate() { }
		#endif
	}
}