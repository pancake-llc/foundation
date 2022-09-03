using System;
using System.Diagnostics;
using UnityEngine;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;
using static Pancake.Init.Reflection.InjectionUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for <see cref="MonoBehaviour">MonoBehaviours</see> that can be
	/// <see cref="ObjectExtensions.Instantiate{TArgument}">instantiated</see>
	/// or <see cref="GameObjectExtensions.AddComponent{TComponent, TArgument}">added</see>
	/// to a <see cref="GameObject"/> with an argument passed to the <see cref="Init"/> function of the created Instance.
	/// <para>
	/// If the object depends on a class that has the <see cref="ServiceAttribute"/> then
	/// it will be able to receive it in its constructor automatically during initialization.
	/// </para>
	/// <para>
	/// Instances of classes inheriting from <see cref="MonoBehaviour{TArgument}"/> receive the argument
	/// via the <see cref="Init"/> method where it can be assigned to a member field or property.
	/// </para>
	/// </summary>
	/// <typeparam name="TArgument"> Type of the argument received in the <see cref="Init"/> function. </typeparam>
	public abstract class MonoBehaviour<TArgument> : MonoBehaviour, IInitializable<TArgument>
	{
		#if DEBUG
		/// <summary>
		/// <see langword="true"/> if object is currently in the process of being initialized with an argument; otherwise, <see langword="false"/>.
		/// </summary>
		private bool initializing;
		#endif

		/// <summary>
		/// Provides the <see cref="Component"/> with the <paramref name="argument">object</paramref> that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the component.
		/// </para>
		/// <para>
		/// <see cref="Init"/> get called when the script is being loaded, before the <see cref="OnAwake"/>, OnEnable and Start events when
		/// the component is created using <see cref="ObjectExtensions.Instantiate{TArgument}"/> or
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TArgument}">GameObject.AddComponent{TComponent, TArgument}</see>.
		/// </para>
		/// <para>
		/// In edit mode <see cref="Init"/> can also get called during the <see cref="Reset"/> event when the script is added to a <see cref="GameObject"/>,
		/// if the component has the <see cref="InitOnResetAttribute"/> or <see cref="RequireComponent"/> attribute for each argument.
		/// </para>
		/// <para>
		/// Note that <see cref="Init"/> gets called immediately following object creation even if the <see cref="GameObject"/> to which the component
		/// was added is <see cref="GameObject.activeInHierarchy">inactive</see> unlike some other initialization event functions such as Awake and OnEnable.
		/// </para>
		/// </summary>
		/// <param name="argument"> Object that this component depends on. </param>
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
		protected TArgument this[[NotNull] string memberName]
        {
			set
			{
				#if DEBUG
				if(!initializing) throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
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
		protected static NullExtensions.NullOrInactiveComparer NullOrInactive => NullExtensions.NullOrInactive;

		/// <summary>
		/// Reset state to default values.
		/// <para>
		/// <see cref="OnReset"/> is called when the user selects Reset in the Inspector context menu or when adding the component the first time.
		/// </para>
		/// <para>
		/// This function is only called in edit mode.
		/// </para>
		/// </summary>
		protected virtual void OnReset() { }

		/// <summary>
		/// <see cref="OnAwake"/> is called when the script Instance is being loaded during the <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
		/// <para>
		/// <see cref="OnAwake"/> is called either when an active <see cref="GameObject"/> that contains the script is initialized when a <see cref="UnityEngine.SceneManagement.Scene">Scene</see> loads,
		/// or when a previously <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/> is set active, or after a <see cref="GameObject"/> created with <see cref="Object.Instantiate"/>
		/// is initialized.
		/// </para>
		/// <para>
		/// Unity calls <see cref="OnAwake"/> only once during the lifetime of the script Instance. A script's lifetime lasts until the Scene that contains it is unloaded.
		/// If the Scene is loaded again, Unity loads the script Instance again, so <see cref="OnAwake"/> will be called again.
		/// If the Scene is loaded multiple times additively, Unity loads several script instances, so <see cref="OnAwake"/> will be called several times (once on each Instance).
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

		#if UNITY_EDITOR
        private void Reset()
        {
			if(InitArgs.TryGet(Context.Reset, this, out TArgument argument))
			{
				#if DEBUG
				initializing = true;
				#endif

                Init(argument);

				#if DEBUG
				initializing = false;
				#endif
            }

			OnInitializableReset(this);
            OnReset();
        }
		#endif

        private void Awake()
		{
			if(InitArgs.TryGet(Context.Awake, this, out TArgument argument))
			{
				#if DEBUG
				initializing = true;
				ValidateArgument(argument);
				#endif

				Init(argument);

				#if DEBUG
				initializing = false;
				#endif
			}

			OnAwake();
		}

		/// <inheritdoc/>
		void IInitializable<TArgument>.Init(TArgument argument)
        {
			#if DEBUG
			initializing = true;
			ValidateArgument(argument);
			#endif

			Init(argument);

			#if DEBUG
			initializing = false;
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
		[Conditional("DEBUG")]
		protected virtual void ValidateArgument(TArgument argument) { }

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TArgument argument)
		{
			#if DEBUG
			if(argument == Null) throw new ArgumentNullException(null, $"First argument passed to {GetType().Name} was null.");
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and logs
		/// an assertion message to the console if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void AssertNotNull(TArgument argument)
		{
			#if DEBUG
			if(argument == Null) Debug.LogAssertion($"First argument passed to {GetType().Name} was null.", this);
			#endif
		}
	}
}