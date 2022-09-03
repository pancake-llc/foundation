using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static Pancake.Init.Reflection.InjectionUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for <see cref="StateMachineBehaviour">state machine behaviours</see> that
	/// can receive three objects they depend on during their initialization.
	/// <para>
	/// Arguments can be specified using a <see cref="StateMachineBehaviourInitializer{TStateMachineBehaviour, TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// attached to the <see cref="Animator"/> that contains the <see cref="StateMachineBehaviour"/>.
	/// <para>
	/// The arguments will be received through the <see cref="Init"/> function where they can be assigned to member fields and properties.
	/// </para>
	/// <typeparam name="TFirstArgument"> Type of the first argument received in the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument received in the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument received in the <see cref="Init"/> function. </typeparam>
	public abstract class StateMachineBehaviour<TFirstArgument, TSecondArgument, TThirdArgument> : StateMachineBehaviour,
		IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		#if DEBUG
		/// <summary>
		/// <see langword="true"/> if object is currently in the process of being initialized with an argument; otherwise, <see langword="false"/>.
		/// </summary>
		private bool initializing;
		#endif

		/// <summary>
		/// Provides the <see cref="StateMachineBehaviour">StateMachineBehaviour</see> with the object that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the <see cref="StateMachineBehaviour"/>.
		/// </para>
		/// <para>
		/// <see cref="Init"/> is called at the beginning of the <see cref="Awake"/> event function when the script is being loaded,
		/// before <see cref="OnAwake"/>, OnEnable and Start.
		/// </para>
		/// </summary>
		/// <param name="argument"> object that this <see cref="StateMachineBehaviour"/> depends on. </param>
		protected abstract void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);

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
		protected object this[[NotNull] string memberName]
        {
			set
			{
				#if DEBUG
				if(!initializing) throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
				#endif

				Inject<StateMachineBehaviour<TFirstArgument, TSecondArgument, TThirdArgument>, TFirstArgument, TSecondArgument, TThirdArgument>(this, memberName, value);
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
		/// <see cref="OnAwake"/> is called when the script Instance is being loaded during the
		/// <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
		/// This happens as the game is launched and is similar to MonoBehavior.Awake.
		/// <para>
		/// Use <see cref="OnAwake"/> to initialize variables or states before the application starts.
		/// </para>
		/// <para>
		/// Unity calls <see cref="OnAwake"/> only once during the lifetime of the script Instance.
		/// A script's lifetime lasts until it is manually destroyed using <see cref="UnityEngine.Object.Destroy"/>.
		/// </para>
		/// <para>
		/// <see cref="OnAwake"/> is always called before any Start functions. This allows you to order initialization of scripts.
		/// <see cref="OnAwake"/> can not act as a coroutine.
		/// </para>
		/// <para>
		/// Note: Use <see cref="OnAwake"/> instead of the constructor for initialization, as the serialized state
		/// of the <see cref="StateMachineBehaviour"/> is undefined at construction time.
		/// </para>
		/// </summary>
		protected virtual void OnAwake() { }

        private void Awake()
		{
			if(InitArgs.TryGet(Context.Awake, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument))
			{
				#if DEBUG
				initializing = true;
				ValidateArguments(firstArgument, secondArgument, thirdArgument);
				#endif

                Init(firstArgument, secondArgument, thirdArgument);

				#if DEBUG
				initializing = false;
				#endif
			}

			OnAwake();
		}

		/// <inheritdoc/>
		void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>.Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
        {
			#if DEBUG
			initializing = true;
			ValidateArguments(firstArgument, secondArgument, thirdArgument);
			#endif

            Init(firstArgument, secondArgument, thirdArgument);

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
		/// protected override void ValidateArguments(IInputManager inputManager, Camera camera)
		/// {
		///		ThrowIfNull(inputManager);
		///		ThrowIfNull(camera);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// You can use the <see cref="AssertNotNull"/> method to log an assertion to the Console
		/// if the argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArgument(IInputManager inputManager, Camera camera)
		/// {
		///		AssertNotNull(inputManager);
		///		AssertNotNull(camera);
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// <para>
		/// Calls to this method are ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> The first received argument to validate. </param>
		/// <param name="secondArgument"> The second received argument to validate. </param>
		/// <param name="thirdArgument"> The third received argument to validate. </param>
		[Conditional("DEBUG")]
		protected virtual void ValidateArguments(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) { }

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TFirstArgument argument)
		{
			#if DEBUG
			if(argument == Null) throw new ArgumentNullException(null, $"First {nameof(Init)} argument of type {typeof(TFirstArgument).Name} passed to {GetType().Name} was null.");
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and logs an assertion message to the console if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void AssertNotNull(TFirstArgument argument)
		{
			#if DEBUG
			if(argument == Null) Debug.LogAssertion($"First {nameof(Init)} argument of type {typeof(TFirstArgument).Name} passed to {GetType().Name} was null.", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TSecondArgument argument)
		{
			#if DEBUG
			if(argument == Null) throw new ArgumentNullException(null, $"Second {nameof(Init)} argument of type {typeof(TSecondArgument).Name} passed to {GetType().Name} was null.");
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and logs an assertion message to the console if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void AssertNotNull(TSecondArgument argument)
		{
			#if DEBUG
			if(argument == Null) Debug.LogAssertion($"Second {nameof(Init)} argument of type {typeof(TSecondArgument).Name} passed to {GetType().Name} was null.", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TThirdArgument argument)
		{
			#if DEBUG
			if(argument == Null) throw new ArgumentNullException(null, $"Third {nameof(Init)} argument of type {typeof(TThirdArgument).Name} passed to {GetType().Name} was null.");
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and logs an assertion message to the console if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void AssertNotNull(TThirdArgument argument)
		{
			#if DEBUG
			if(argument == Null) Debug.LogAssertion($"Third {nameof(Init)} argument of type {typeof(TThirdArgument).Name} passed to {GetType().Name} was null.", this);
			#endif
		}
	}
}