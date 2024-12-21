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
	/// A base class for an initializer that can specify the arguments used to initialize a <see cref="ScriptableObject"/> of type <typeparamref name="TClient"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the client's <see cref="Awake"/> event, or when services become ready (whichever occurs later).
	/// </para>
	/// <para>
	/// The client receives the arguments via the Init function where it can assign them to member fields and properties.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client scriptable object. </typeparam>
	public abstract class ScriptableObjectInitializerBaseInternal<TClient> : ScriptableObject, IInitializer<TClient>, IValueProvider<TClient>, IValueByTypeProvider
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TClient>
		#endif
		where TClient : ScriptableObject
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TClient target = default;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private protected NullArgumentGuard nullArgumentGuard = DefaultNullArgumentGuardFlags;

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

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
		Awaitable<TClient>
		#else
		System.Threading.Tasks.Task<TClient>
		#endif
		InitTargetAsync()
		{
			if(!this)
			{
				return target;
			}

			target = await InitTargetAsync(target);

			if(
			#if UNITY_EDITOR
			Application.isPlaying &&
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
		/// Returns an awaitable Task for initializing the client object of type <see cref="TClient"/>
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
		[return: NotNull]
		private protected virtual
		#if UNITY_2023_1_OR_NEWER
		Awaitable<TClient> InitTargetAsync([AllowNull] TClient target) => AwaitableUtility.FromResult(target);
		#else
		System.Threading.Tasks.Task<TClient> InitTargetAsync([AllowNull] TClient target) => System.Threading.Tasks.Task.FromResult(target);
		#endif

		/// <inheritdoc/>
		public TClient InitTarget()
		{
			if(this == null)
			{
				return target;
			}

			target = InitTarget(target);

			#if !UNITY_EDITOR
			if(IsRemovedAfterTargetInitialized)
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
			#endif

			return target;
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
		/// context menu or when adding the initializer to a scriptable object the first time.
		/// <para>
		/// This function is only called in the editor in edit mode.
		/// </summary>
		protected virtual void OnReset() { }

		/// <summary>
		/// Initializes <paramref name="target"/>, or a new instance of it if it is a prefab, with the arguments specified on this initializer.
		/// </summary>
		/// <param name="target"> The target to initialize. </param>
		/// <returns> The initialized object; <paramref name="target"/>, if it is not a prefab, otherwise a new clone of target. </returns>
		[return: NotNull]
		private protected abstract TClient InitTarget([AllowNull] TClient target);

		#if DEBUG || INIT_ARGS_SAFE_MODE
		async 
		#endif
		private protected void Awake()
		{
			#if UNITY_EDITOR
			ThreadSafe.Application.IsPlaying = Application.isPlaying;
			#endif

			if(
			#if UNITY_EDITOR
			Application.isPlaying &&
			#endif
			IsAsync)
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				await
				#else
				_ = 
				#endif
				InitTargetAsync();

				return;
			}

			InitTarget();
		}

		protected void ThrowMissingInitArgumentsException<TArgument>() => throw GetMissingInitArgumentsException(GetType(), typeof(TClient), typeof(TArgument));

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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
		bool IInitializerEditorOnly.CanInitTargetWhenInactive => false;
		bool IInitializerEditorOnly.CanGuardAgainstNull => true;
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
		private protected abstract NullGuardResult EvaluateNullGuard();

		private protected abstract void Reset();
		private protected abstract void OnValidate();
		#endif
	}
}