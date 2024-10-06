#if ODIN_INSPECTOR
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Sisus.Init.Internal;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.Init.Reflection.InjectionUtility;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sisus.Init
{
	/// <summary>
	/// A base class for scriptable objects that are serialized by the Sirenix serialization system, and can be
	/// <see cref="Create.Instance{TScriptableObject, TArgument}">created</see>
	/// or <see cref="ObjectExtensions.Instantiate{TScriptableObject, TArgument}">instantiated</see>
	/// with an argument passed to the <see cref="Init"/> function of the created instance.
	/// <para>
	/// If the object depends on a class that has the <see cref="ServiceAttribute"/> then
	/// it will receive it in its <see cref="Init"/> function automatically during initialization.
	/// </para>
	/// <para>
	/// If the object depends on a class that doesn't have the <see cref="ServiceAttribute"/>,
	/// then an <see cref="ScriptableObjectInitializer{TScriptableObject, TArgument}"/>
	/// can be used to specify its initialization argument.
	/// </para>
	/// <para>
	/// Instances of classes inheriting from <see cref="SerializedScriptableObject{TArgument}"/> receive the argument
	/// via the <see cref="Init"/> method where it can be assigned to a member field or property.
	/// </para>
	/// <typeparam name="TArgument"> Type of the argument received in the <see cref="Init"/> function. </typeparam>
	public abstract class SerializedScriptableObject<TArgument> : SerializedScriptableObject, IInitializable<TArgument>, IInitializable
		#if UNITY_EDITOR
		, EditorOnly.IInitializableEditorOnly
		#endif
	{
		[SerializeField, HideInInspector, MaybeNull]
		private ScriptableObject initializer = null;

		#if DEBUG || INIT_ARGS_SAFE_MODE
		/// <summary>
		/// <see langword="true"/> if object is currently in the process of being initialized with an argument; otherwise, <see langword="false"/>.
		/// </summary>
		[NonSerialized]
		private InitState initState;
		#endif

		#if UNITY_EDITOR
		IInitializer EditorOnly.IInitializableEditorOnly.Initializer { get => initializer as IInitializer; set => initializer = value as ScriptableObject; }
		bool EditorOnly.IInitializableEditorOnly.CanInitSelfWhenInactive => true;
		InitState EditorOnly.IInitializableEditorOnly.InitState => initState;
		#endif

		/// <summary>
		/// Provides the scriptable object with the object that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the <see cref="SerializedScriptableObject"/>.
		/// </para>
		/// <para>
		/// <see cref="Init"/> is called at the beginning of the <see cref="Awake"/> event function when the script is being loaded,
		/// or when services become ready (whichever occurs later).
		/// </para>
		/// </summary>
		/// <param name="argument"> object that this <see cref="SerializedScriptableObject"/> depends on. </param>
		protected abstract void Init(TArgument argument);

		/// <summary>
		/// Assigns an argument received during initialization to a field or property by the <paramref name="memberName">given name</paramref>.
		/// <para>
		/// Because reflection is used to set the value it is possible to use this to assign to init only fields and properties.
		/// Properties that do not have a set accessor and are not auto-implemented are not supported however.
		/// </para>
		/// </summary>
		/// <param name="memberName"> Name of the field or property to which to assign the value. </param>
		/// <exception cref="InvalidOperationException">
		/// Thrown if this method is called outside of the context of the client object being <see cref="Init">initialized</see>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the provided <paramref name="memberName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property by the provided name is found or if property by given name is not auto-implemented
		/// and does not have a set accessor.
		/// </exception>
		protected TArgument this[[DisallowNull] string memberName]
		{
			set
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(initState == InitState.Initialized) throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
				#endif

				Inject(this, memberName, value);
			}
		}

		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="Object"/> which has been <see cref="Object.Destroy">destroyed</see>.
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
		/// </summary>
		[NotNull]
		protected static NullExtensions.NullComparer Null => NullExtensions.Null;

		/// <summary>
		/// A value against which any <see cref="object"/> can be compared to determine whether or not it is
		/// <see langword="null"/> or an <see cref="Object"/> which is <see cref="GameObject.activeInHierarchy">inactive</see>
		/// or has been <see cref="Object.Destroy">destroyed</see>.
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
		/// Reset state to default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user selects Reset in the Inspector context menu or creating an instance for the first time.
		/// </para>
		/// <para>
		/// This function is only called in edit mode.
		/// </para>
		/// </summary>
		protected virtual void OnReset() { }

		/// <summary>
		/// <see cref="OnAwake"/> is called when the script instance is being loaded during the <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
		/// This happens as the game is launched and is similar to MonoBehavior.Awake.
		/// <para>
		/// Use <see cref="OnAwake"/> to initialize variables or states before the application starts.
		/// </para>
		/// <para>
		/// Unity calls <see cref="OnAwake"/> only once during the lifetime of the script instance.
		/// A script's lifetime lasts until it is manually destroyed using <see cref="UnityEngine.Object.Destroy"/>.
		/// </para>
		/// <para>
		/// <see cref="OnAwake"/> is always called before any Start functions. This allows you to order initialization of scripts.
		/// <see cref="OnAwake"/> can not act as a coroutine.
		/// </para>
		/// <para>
		/// Note: Use <see cref="OnAwake"/> instead of the constructor for initialization, as the serialized state of the <see cref="SerializedScriptableObject"/> is undefined at construction time.
		/// </para>
		/// </summary>
		protected virtual void OnAwake() { }

		bool IInitializable.HasInitializer => initializer != null;

		bool IInitializable.Init(Context context)
		{
			#if UNITY_EDITOR
			if(context.IsEditMode() && initializer is IInitializable initializable)
			{
				return initializable.Init(context);
			}
			#endif
			
			if(initializer is IInitializer iinitializer)
			{
				iinitializer.InitTarget();
				return true;
			}

			return false;
		}

		#if UNITY_EDITOR
		private void Reset()
		{
			if(InitArgs.TryGet(Context.Reset, this, out TArgument argument))
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				initState = InitState.Initializing;
				#endif

				Init(argument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				initState = InitState.Initialized;
				#endif
			}

			OnReset();
		}
		#endif

		private protected void Awake()
		{
			#if !UNITY_EDITOR
			if(!ServiceUtility.ServicesAreReady)
			{
				ServiceUtility.ServicesBecameReady += Awake;
				return;
			}
			#endif
			
			#if !UNITY_EDITOR
			if(initializer is IInitializer iinitializer)
			#else
			if(initializer is IInitializer iinitializer && Application.isPlaying)
			#endif
			{
				iinitializer.InitTarget();

				#if !UNITY_EDITOR
				Destroy(initializer);
				initializer = null;
				#endif
			}
			else if(InitArgs.TryGet(Context.Awake, this, out TArgument argument))
			{
				#if DEBUG || INIT_ARGS_SAFE_MODE
				initState = InitState.Initializing;
				ValidateArgumentIfPlayMode(argument);
				#endif

				Init(argument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				initState = InitState.Initialized;
				#endif
			}

			OnAwake();
		}

		#if UNITY_EDITOR
		private void OnValidate()
		{
			Validate();

			if(initializer == null)
			{
				return;
			}

			var initializersGenericArguments = initializer.GetType().GetInterfaces().Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<,>)).Select(t => t.GetGenericArguments()).ToList();
			if(!initializersGenericArguments.Any())
			{
				Debug.LogWarning($"Initializer {initializer.GetType().Name} is attached to {GetType().Name} at '{AssetDatabase.GetAssetOrScenePath(this)}' but it does not implement IInitializer<{GetType().Name}, {typeof(TArgument).Name}>.", this);
				return;
			}

			if(!initializersGenericArguments.Any(args => args[0].IsInstanceOfType(this)))
			{
				Debug.LogWarning($"Initializer {initializer.GetType().Name} of {GetType().Name} at '{AssetDatabase.GetAssetOrScenePath(this)}' invalid client type {initializersGenericArguments.First()[0].Name}: not assignable from {GetType().Name}.", this);
				return;
			}

			if(!initializersGenericArguments.Any(args => typeof(TArgument).IsAssignableFrom(args[1])))
			{
				Debug.LogWarning($"Initializer {initializer.GetType().Name} of {GetType().Name} at '{AssetDatabase.GetAssetOrScenePath(this)}' invalid argument type {initializersGenericArguments.First()[1].Name}: not assignable to {typeof(TArgument).Name}.", this);
			}
		}
		#endif

		[Conditional("UNITY_EDITOR")]
		protected virtual void Validate() { }

		/// <inheritdoc/>
		void IInitializable<TArgument>.Init(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			initState = InitState.Initializing;
			ValidateArgumentIfPlayMode(argument);
			#endif

			Init(argument);

			#if DEBUG || INIT_ARGS_SAFE_MODE
			initState = InitState.Initialized;
			#endif
		}

		internal void InitInternal(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			initState = InitState.Initializing;
			ValidateArgumentIfPlayMode(argument);
			#endif

			Init(argument);

			#if DEBUG || INIT_ARGS_SAFE_MODE
			initState = InitState.Initialized;
			#endif
		}

		/// <summary>
		/// Method that can be overridden and used to validate the initialization argument that was received by this object.
		/// <para>
		/// You can use the <see cref="ThrowIfNull"/> method to throw an <see cref="ArgumentNullException"/>
		/// if the argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArgument(IInputManager inputManager)
		/// {
		///		ThrowIfNull(inputManager);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// You can use the <see cref="AssertNotNull"/> method to log an assertion to the Console
		/// if the argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArgument(IInputManager inputManager)
		/// {
		///		AssertNotNull(inputManager);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// Calls to this method are ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The received argument to validate. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ValidateArgument(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			ThrowIfNull(argument);
			#endif
		}

		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ValidateArgumentIfPlayMode(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			#if UNITY_EDITOR
			if(!EditorOnly.ThreadSafe.Application.IsPlaying)
			{
				return;
			}
			#endif

			if(!ShouldSelfGuardAgainstNull(this))
			{
				return;
			}

			ValidateArgument(argument);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfNull(TArgument argument)
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
		protected void AssertNotNull(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(argument == Null) Debug.LogAssertion($"Init argument of type {typeof(TArgument).Name} passed to {GetType().Name} was null.", this);
			#endif
		}
	}
}
#endif