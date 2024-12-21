#pragma warning disable CS0414

using System;
using System.Threading.Tasks;
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
	/// A base class for an initializer that can specify the arguments used to initialize a <see cref="ScriptableObject"/> of type <typeparamref name="TStateMachineBehaviour"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TStateMachineBehaviour">client</typeparamref>
	/// during the client's <see cref="Awake"/> event, or when services become ready (whichever occurs later).
	/// </para>
	/// <para>
	/// The client receives the arguments via the Init function where it can assign them to member fields and properties.
	/// </para>
	/// </summary>
	/// <typeparam name="TStateMachineBehaviour"> Type of the initialized client scriptable object. </typeparam>
	public abstract class StateMachineBehaviourInitializerBaseInternal<TStateMachineBehaviour> : MonoBehaviour, IInitializer<TStateMachineBehaviour>, IValueProvider<TStateMachineBehaviour>, IValueByTypeProvider
		#if UNITY_EDITOR
		, IInitializerEditorOnly<TStateMachineBehaviour>
		#endif
		where TStateMachineBehaviour : StateMachineBehaviour
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected Animator target = null;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private protected NullArgumentGuard nullArgumentGuard = DefaultNullArgumentGuardFlags;

		protected virtual bool IsRemovedAfterTargetInitialized => true;
		private protected virtual bool IsAsync => false;

		/// <inheritdoc/>
		TStateMachineBehaviour IValueProvider<TStateMachineBehaviour>.Value => target != null ? target.GetBehaviour<TStateMachineBehaviour>() : null;

		/// <inheritdoc/>
		bool IValueByTypeProvider.TryGetFor<TValue>([AllowNull] Component client, out TValue value)
		{
			if(target)
			{
				TStateMachineBehaviour stateMachineBehaviour = target.GetBehaviour<TStateMachineBehaviour>();
				if(stateMachineBehaviour && stateMachineBehaviour is TValue result)
				{
					value = result;
					return true;
				}
			}

			value = default;
			return false;
		}

		bool IValueByTypeProvider.CanProvideValue<TValue>(Component client) => typeof(TValue).IsAssignableFrom(typeof(TStateMachineBehaviour));

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = value as Animator; }

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TStateMachineBehaviour));

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
		Awaitable<TStateMachineBehaviour>
		#else
		System.Threading.Tasks.Task<TStateMachineBehaviour>
		#endif	
		InitTargetAsync()
		{
			if(!this)
			{
				return target ? target.GetBehaviour<TStateMachineBehaviour>() : null;
			}

			TStateMachineBehaviour result = await InitTargetAsync(target);

			if(
			#if UNITY_EDITOR
			Application.isPlaying &&
			#endif
			IsRemovedAfterTargetInitialized)
			{
				Updater.InvokeAtEndOfFrame(DestroySelf);
			}

			return result;
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
		[return: NotNull]
		private protected abstract ValueTask<TStateMachineBehaviour> InitTargetAsync([AllowNull] Animator target);

		/// <inheritdoc/>
		public TStateMachineBehaviour InitTarget()
		{
			if(!this)
			{
				return target ? target.GetBehaviour<TStateMachineBehaviour>() : null;
			}

			if(IsRemovedAfterTargetInitialized
			#if UNITY_EDITOR
			&& Application.isPlaying
			#endif
			)
			{
				Updater.InvokeAtEndOfFrame(DestroySelf);
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(target == null)
			{
				#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					return null;
				}
				#endif

				throw new MissingComponentException($"No {nameof(Animator)} was found on the GameObject '{name}'.", null);
			}
			#endif

			return InitTarget(target);
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
		/// Initializes the <see cref="TStateMachineBehaviour"/> behaviour inside the <see cref="target"/> <see cref="Animator"/> using the provided argument.
		/// </summary>
		/// <param name="target"> The target to initialize. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TStateMachineBehaviour"/>. </returns>
		[return: NotNull]
		private protected abstract TStateMachineBehaviour InitTarget([DisallowNull] Animator target);

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

		protected void ThrowMissingInitArgumentsException<TArgument>() => throw GetMissingInitArgumentsException(GetType(), typeof(TStateMachineBehaviour), typeof(TArgument));

		void DestroySelf()
		{
			if(this)
			{
				Destroy(this);
			}
		}

		#if DEBUG || INIT_ARGS_SAFE_MODE
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private protected void ThrowIfMissing<TArgument>(TArgument argument)
		{
			if(argument == Null)
			{
				throw GetMissingInitArgumentsException(GetType(), typeof(TStateMachineBehaviour), typeof(TArgument));
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
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => true;
		bool IInitializerEditorOnly.WasJustReset { get; set; }
		bool IInitializerEditorOnly.IsAsync => IsAsync;
		void IInitializerEditorOnly.SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) => SetReleaseArgumentOnDestroy(argument, shouldRelease);
		void IInitializerEditorOnly.SetIsArgumentAsync(Arguments argument, bool isAsync) => SetReleaseArgumentOnDestroy(argument, isAsync);
		private protected virtual void SetReleaseArgumentOnDestroy(Arguments argument, bool shouldRelease) { }
		private protected virtual void SetIsArgumentAsyncValueProvider(Arguments argument, bool isAsyncValueProvider) { }
		private protected abstract NullGuardResult EvaluateNullGuard();

		private protected abstract void Reset();
		private protected abstract void OnValidate();
		#endif
	}
}