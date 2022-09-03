using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static Pancake.Init.Reflection.InjectionUtility;

namespace Pancake.Init
{
	/// <summary>
	/// A base class for <see cref="ScriptableObject">ScriptableObjects</see> that can be <see cref="CreateInstance">created</see>
	/// or <see cref="ObjectExtensions.Instantiate{TArgument}">instantiated</see>
	/// with an argument passed to the <see cref="Init"/> function of the created Instance.
	/// <para>
	/// Instances of classes inheriting from <see cref="ScriptableObject{TArgument}"/> receive the argument
	/// via the <see cref="Init"/> method where it can be assigned to a member field or property.
	/// </para>
	/// <typeparam name="TArgument"> Type of the argument received in the <see cref="Init"/> function. </typeparam>
	public abstract class ScriptableObject<TArgument> : ScriptableObject, IInitializable<TArgument>
	{
		#if DEBUG
		/// <summary>
		/// <see langword="true"/> if object is currently in the process of being initialized with an argument; otherwise, <see langword="false"/>.
		/// </summary>
		private bool initializing;
		#endif

		/// <summary>
		/// Provides the <see cref="ScriptableObject">ScriptableObject</see> with the object that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for the <see cref="ScriptableObject"/>.
		/// </para>
		/// <para>
		/// <see cref="Init"/> is called at the beginning of the <see cref="Awake"/> event function when the script is being loaded,
		/// before <see cref="OnAwake"/>, OnEnable and Start.
		/// </para>
		/// </summary>
		/// <param name="argument"> object that this <see cref="ScriptableObject"/> depends on. </param>
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
				if(!initializing)
                {
					throw new InvalidOperationException($"Unable to assign to member {GetType().Name}.{memberName}: Values can only be injected during initialization.");
				}
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
		/// <see cref="OnReset"/> is called when the user selects Reset in the Inspector context menu or creating an Instance for the first time.
		/// </para>
		/// <para>
		/// This function is only called in edit mode.
		/// </para>
		/// </summary>
		protected virtual void OnReset() { }

		/// <summary>
		/// <see cref="OnAwake"/> is called when the script Instance is being loaded during the <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
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
		/// Note: Use <see cref="OnAwake"/> instead of the constructor for initialization, as the serialized state of the <see cref="ScriptableObject"/> is undefined at construction time.
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