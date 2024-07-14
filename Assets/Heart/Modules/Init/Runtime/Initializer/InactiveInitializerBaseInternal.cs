#pragma warning disable CS0414

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif

[assembly: InternalsVisibleTo("InitArgs.UIToolkit")]

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

		/// <inheritdoc/>
		TClient IValueProvider<TClient>.Value => target;

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
		Object IInitializer.Target { get => target; set => target = value as TClient; }

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
		IInitializer.InitTargetAsync() => await InitTargetAsync();

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

		/// <inheritdoc/>
		public
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif
		InitTargetAsync() => InitTargetAsync(Context.MainThread);

		/// <inheritdoc/>
		public async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif
		InitTargetAsync(Context context)
		{
			if(!this)
			{
				return target;
			}

			target = await InitTargetAsync(target, context);

			if(
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying &&
			#endif
			IsRemovedAfterTargetInitialized)
			{
				Updater.InvokeAtEndOfFrame(DestroySelf);

				void DestroySelf()
				{
					if(this != null)
					{
						Destroy(this);
					}
				}
			}

			return target;
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
		private protected virtual ValueTask<TClient> InitTargetAsync([AllowNull] TClient target, Context context) => new ValueTask<TClient>(InitTarget(ref target, context) ? target : null);

		/// <inheritdoc/>
		public TClient InitTarget()
		{
			InitTarget(Context.MainThread);
			return target;
		}

		public bool InitTarget(Context context)
		{
			if(this == null)
			{
				return false;
			}

			if(
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying &&
			#endif
			IsRemovedAfterTargetInitialized)
			{
				Updater.InvokeAtEndOfFrame(DestroySelf);

				void DestroySelf()
				{
					if(this != null)
					{
						Destroy(this);
					}
				}
			}

			return InitTarget(ref target, context);
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		private protected bool IsRuntimeNullGuardActive => nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException)
			#if UNITY_EDITOR
			&& Application.isPlaying
			#endif
			;
		#endif

		/// <summary>
		/// Resets the initializer to its default state.
		/// <para>
		/// <see cref="OnReset"/> is called when the user hits the Reset button in the Inspector's
		/// context menu or when adding the component to a GameObject the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
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
		private protected abstract bool InitTarget([DisallowNull] ref TClient target, Context context);

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }
		//public void OnBeforeSerialize() { }

		#if DEBUG
		async
		#endif
		public void OnAfterDeserialize()
		{
			#if DEBUG
			if(!target)
			{
				return;
			}
			#endif

			if(
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying &&
			#endif
			IsAsync)
			{
				#if DEBUG
				await
				#else
				_ = 
				#endif
				InitTargetAsync(Context.OnAfterDeserialize);
				return;
			}

			if(!InitTarget(Context.OnAfterDeserialize))
			{
				if(!ServiceInjector.ServicesAreReady)
				{
					ServiceInjector.ServicesBecameReady += OnServicesBecameReady;
				}
			}
		}

		private void OnServicesBecameReady()
		{
			if(!this)
			{
				return;
			}

			if(!InitTarget(Context.MainThread))
			{
				throw new MissingInitArgumentsException(target?.GetType() ?? typeof(TClient));
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

		#if UNITY_EDITOR
		bool IInitializerEditorOnly.ShowNullArgumentGuard => true;
		bool IInitializerEditorOnly.CanInitTargetWhenInactive => true;
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get; set; } = "";
		NullGuardResult IInitializerEditorOnly.EvaluateNullGuard() => EvaluateNullGuard();
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		bool IInitializerEditorOnly.WasJustReset { get; set; }
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