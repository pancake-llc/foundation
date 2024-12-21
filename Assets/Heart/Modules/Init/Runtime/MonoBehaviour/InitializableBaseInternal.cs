//#define DEBUG_DEFERRED_ON_AWAKE

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sisus.Init.EditorOnly;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using Debug = UnityEngine.Debug;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Base class for all MonoBehaviour{T...} classes.
	/// </summary>
	public abstract class InitializableBaseInternal : MonoBehaviour, IInitializable
	#if UNITY_EDITOR
		, IInitializableEditorOnly
	#endif
	{
		internal InitState initState;
		
		bool IInitializable.HasInitializer => HasInitializer(this);
		bool IInitializable.Init(Context context) => Init(context);
		
		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="UnityEngine.Object"/> which has been <see cref="UnityEngine.Object.Destroy">destroyed</see>.
		/// </summary>
		/// <example>
		/// <code>
		/// private IEvent trigger;
		/// 
		///	private void OnDisable()
		///	{
		///		if(trigger != Null)
		///		{
		///			trigger.RemoveListener(this);
		///		}
		///	}
		/// </code>
		/// </example>
		[NotNull]
		protected static NullExtensions.NullComparer Null => NullExtensions.Null;

		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="UnityEngine.Object"/> which is <see cref="GameObject.activeInHierarchy">inactive</see>
		/// or has been <see cref="UnityEngine.Object.Destroy">destroyed</see>.
		/// </summary>
		/// <example>
		/// <code>
		/// private ITrackable target;
		/// 
		/// private void Update()
		/// {
		/// 	if(target != NullOrInactive)
		/// 	{
		/// 		transform.LookAt(target.Position);
		/// 	}
		/// }
		/// </code>
		/// </example>
		[NotNull]
		protected static NullExtensions.NullOrInactiveComparer NullOrInactive => NullExtensions.NullOrInactive;

		/// <summary>
		/// <see cref="OnAwake"/> is called when the script instance is being loaded during the <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
		/// <para>
		/// <see cref="OnAwake"/> is called either when an active <see cref="GameObject"/> that contains the script is initialized when a <see cref="UnityEngine.SceneManagement.Scene">Scene</see> loads,
		/// or when a previously <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/> is set active, or after a <see cref="GameObject"/> created with <see cref="UnityEngine.Object.Instantiate"/>
		/// is initialized.
		/// </para>
		/// <para>
		/// Unity calls <see cref="OnAwake"/> only once during the lifetime of the script instance. A script's lifetime lasts until the Scene that contains it is unloaded.
		/// If the Scene is loaded again, Unity loads the script instance again, so <see cref="OnAwake"/> will be called again.
		/// If the Scene is loaded multiple times additively, Unity loads several script instances, so <see cref="OnAwake"/> will be called several times (once on each instance).
		/// </para>
		/// <para>
		/// For active <see cref="GameObject">GameObjects</see> placed in a Scene, Unity calls <see cref="OnAwake"/> after all active <see cref="GameObject">GameObjects</see>
		/// in the Scene are initialized, so you can safely use methods such as <see cref="GameObject.FindWithTag"/> to query other <see cref="GameObject">GameObjects</see>.
		/// </para>
		/// <para>
		/// The order that Unity calls each <see cref="GameObject"/>'s Awake (and by extension <see cref="OnAwake"/>) is not deterministic.
		/// Because of this, you should not rely on one <see cref="GameObject"/>'s Awake being called before or after another
		/// (for example, you should not assume that a reference assigned by one GameObject's <see cref="OnAwake"/> will be usable in another GameObject's <see cref="Awake"/>).
		/// Instead, you should use <see cref="Awake"/>/<see cref="OnAwake"/> to set up references between scripts, and use Start, which is called after all <see cref="Awake"/>
		/// and <see cref="OnAwake"/> calls are finished, to pass any information back and forth.
		/// </para>
		/// <para>
		/// <see cref="OnAwake"/> is always called before any Start functions. This allows you to order initialization of scripts.
		/// <see cref="OnAwake"/> is called even if the script is a disabled component of an active GameObject.
		/// <see cref="OnAwake"/> can not act as a coroutine.
		/// </para>
		/// <para>
		/// Note: Use <see cref="OnAwake"/> instead of the constructor for initialization, as the serialized state of the <see cref="Component"/> is undefined at construction time.
		/// </para>
		/// </summary>
		protected virtual void OnAwake() { }
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected abstract bool Init(Context context);
		
		internal void Awake()
		{
			if(initState is InitState.Uninitialized)
			{
				#if UNITY_EDITOR
				bool isInitialized =
				#endif
					Init(Context.Awake);

				#if UNITY_EDITOR
				if(!isInitialized && ShouldSelfGuardAgainstNull(this))
				{
					throw new MissingInitArgumentsException(this);
				}
				#endif
			}
			else if(initState is InitState.Initializing)
			{
				#if DEV_MODE && DEBUG_DEFERRED_ON_AWAKE
				Debug.Log($"{GetType().Name} - won't execute OnAwake yet because initialization is still in progress. Expecting Initializer to do it manually once it is safe to do so.");
				#endif
				return;
			}

			OnAwake();
		}
		
		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="System.ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfNull<TArgument>(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(argument == Null) throw new ArgumentNullException(typeof(TArgument).Name, $"Init argument of type {typeof(TArgument).Name} passed to {GetType().Name} was null.");
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and logs an assertion message to the console if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertNotNull<TArgument>(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(argument == Null) Debug.LogAssertion($"Init argument of type {typeof(TArgument).Name} passed to {GetType().Name} was null.", this);
			#endif
		}

		#if UNITY_EDITOR
		IInitializer IInitializableEditorOnly.Initializer { get => TryGetInitializer(this, out IInitializer Initializer) ? Initializer : null; set { if(value != Null) value.Target = this; else if(TryGetInitializer(this, out IInitializer Initializer)) Initializer.Target = null; } }
		bool IInitializableEditorOnly.CanInitSelfWhenInactive => false;
		InitState IInitializableEditorOnly.InitState => initState;
		#endif
	}
}