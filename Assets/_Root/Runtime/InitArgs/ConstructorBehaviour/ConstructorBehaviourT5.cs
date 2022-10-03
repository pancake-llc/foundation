using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using static Pancake.Init.Reflection.InjectionUtility;
using static Pancake.Init.Internal.InitializerUtility;
#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
#endif

namespace Pancake.Init
{
    /// <summary>
    /// A base class for <see cref="MonoBehaviour">MonoBehaviours</see> that depend on receiving five arguments in their constructor during initialization.
    /// <para>
    /// Instances can be
    /// <see cref="ObjectExtensions.Instantiate{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}">instantiated</see>
    /// or <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}">added</see>
    /// to a <see cref="GameObject"/> at runtime with five arguments passed to the constructor of the created instance.
    /// </para>
    /// <para>
    /// If an object depends exclusively on classes that implement <see cref="IService"/> then it will be able to receive them
    /// in its constructor automatically.
    /// </para>
    /// <para>
    /// Instances of classes inheriting from <see cref="ConstructorBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> receive the
    /// arguments in their default constructor where they can be assigned to member fields or propertys - including ones that are read-only.
    /// </para>
    /// <para>
    /// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
    /// </para>
    /// </summary>
    /// <typeparam name="TFirstArgument"> Type of the first object that the <see cref="Component"/> depends on. </typeparam>
    /// <typeparam name="TSecondArgument"> Type of the second object that the <see cref="Component"/> depends on. </typeparam>
    /// <typeparam name="TThirdArgument"> Type of the third object that the <see cref="Component"/> depends on. </typeparam>
    /// <typeparam name="TFourthArgument"> Type of the fourth object that the <see cref="Component"/> depends on. </typeparam>
    /// <typeparam name="TFifthArgument"> Type of the fifth object that the <see cref="Component"/> depends on. </typeparam>
    public abstract class ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : MonoBehaviour, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, ISerializationCallbackReceiver
	{
		private static readonly ConcurrentDictionary<Type, (string, string, string, string, string)> initPropertyNamesCache = new ConcurrentDictionary<Type, (string, string, string, string, string)>();
		private static readonly ConcurrentDictionary<Type, bool> isAnyInitPropertySerializedCache = new ConcurrentDictionary<Type, bool>();

		/// <summary>
		/// <see langword="true"/> if this object received the arguments that it depends on in the constructor
		/// or if they were injected to it through the <see cref="Init"/> function; otherwise, <see langword="false"/>.
		/// <para>
		/// Note that this is set to <see langword="true"/> if it receives the arguments regardless of whether or not
		/// any of the received arguments are <see langword="null"/> or not.
		/// </para>
		/// </summary>
		private bool dependenciesReceived;

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
		/// Initializes the new instance of the <see cref="ConstructorBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> class and retrieves its dependencies.
		/// <para>
		/// Classes that inherit from <see cref="ConstructorBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> should always implement the parameterless
		/// default constructor, chain it to this constructor using the <see langword="base"/> keyword and then in the body of the
		/// constructor assign the values of the arguments to read-only fields or properties.
		/// </para>
		/// <example>
		/// <code>
		/// public class Player : ConstructorBehaviour<Guid, Renderer, Collider, IInputManager, IActorMotor>
		/// {
		///		public readonly Guid id;
		///		public readonly Renderer renderer;
		///		public readonly Collider collider;
		/// 
		///		private readonly IInputManager inputManager;
		///		private readonly IActorMotor actorMotor;
		/// 
		///		private Direction moveDirection = Direction.None;
		/// 
		///		public Actor() : base(out var id, out var renderer, out var collider, out var inputManager, out var actorMotor)
		/// 	{
		/// 		this.id = id;
		/// 		this.renderer = renderer;
		/// 		this.collider = collider;
		/// 		this.inputManager = inputManager;
		/// 		this.actorMotor = actorMotor;
		///		}
		///	
		///		private void OnEnable()
		///		{
		///			inputManager.OnMoveInputGiven += OnMovementInputGiven;
		///			inputManager.OnMoveInputReleased += OnMovementInputReleased;
		///			renderer.enabled = true;
		///			collider.enabled = true;
		///		}
		///		
		///		private void OnMoveInputGiven(Direction moveDirection)
		///		{
		///			moveDirection = moveDirection;
		///		}
		///		
		///		private void OnMoveInputReleased()
		///		{
		///			moveDirection = Direction.None;
		///		}
		/// 
		///		private void Update()
		///		{
		///			actorMotor.Move(this, moveDirection);
		///		}
		///		
		///		private void OnDisable()
		///		{
		///			inputManager.OnMoveInputGiven -= OnMovementInputGiven;
		///			inputManager.OnMoveInputReleased -= OnMovementInputReleased;
		///			
		///			if(renderer != null)
		///			{
		///				renderer.disabled = false;
		///			}
		///			if(collider != null)
		///			{
		///				collider.enabled = false;
		///			}
		///		}
		/// }
		/// </code>
		/// </example>
		/// </summary>
		/// <param name="firstArgument"> The first argument passed to this <see cref="Component"/>. </param>
		/// <param name="secondArgument"> The second argument passed to this <see cref="Component"/>. </param>
		/// <param name="thirdArgument"> The third argument passed to this <see cref="Component"/>. </param>
		/// <param name="fourthArgument"> The fourth argument passed to this <see cref="Component"/>. </param>
		/// <param name="fifthArgument"> The fifth argument passed to this <see cref="Component"/>. </param>
		/// <exception cref="MissingInitArgumentsException">
		/// Thrown if arguments were not provided for the object using <see cref="InitArgs.Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> prior to the constructor being called.
		/// </exception>
		protected ConstructorBehaviour(out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument)
		{
			dependenciesReceived = InitArgs.TryGet(Context.Constructor, this, out firstArgument, out secondArgument, out thirdArgument, out fourthArgument, out fifthArgument);

			#if DEBUG
			if(!dependenciesReceived)
            {
				return;
			}

			#if UNITY_EDITOR
			if(EditorOnly.ThreadSafe.Application.IsExitingPlayMode)
            {
				return;
            }
			#endif

			ValidateArguments(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			#endif
		}

		/// <summary>
        /// Default constructor of <see cref="ConstructorBehaviour"/> which should never get called.
        /// <para>
        /// Classes that inherit from <see cref="ConstructorBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> should always implement the parameterless
        /// default constructor and chain it to the <see cref="ConstructorBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}(out TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument)"/> constructor using
        /// the <see langword="base"/> keyword.
        /// </para>
        /// </summary>
        /// <exception cref="NotSupportedException"> Always thrown if this constructor is called. </exception>
        private ConstructorBehaviour()
		{
			throw new NotSupportedException(GetType().Name + " invalid constructor called. Classes that derive from ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> must implement a parameterless default constructor and chain it to the base constructor that contains the out keyword.");
		}

        /// <inheritdoc/>
        void IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
            .Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				=> Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

        private void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
        {
			(string firstField, string secondField, string thirdField, string fourthField, string fifthField) = GetInitArgumentClassMemberNames();
			Inject<ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, firstField, firstArgument);
			Inject<ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, secondField, secondArgument);
			Inject<ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, thirdField, thirdArgument);
			Inject<ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, fourthField, fourthArgument);
			Inject<ConstructorBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this, fifthField, fifthArgument);
			dependenciesReceived = true;
			ValidateArguments(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
		}

		/// <summary>
		/// Method that can be overridden and used to validate the initialization arguments that were received by this object.
		/// <para>
		/// You can use the <see cref="ThrowIfNull"/> method to throw an <see cref="ArgumentNullException"/>
		/// if an argument is <see cref="Null">null</see>.
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
		/// if an argument is <see cref="Null">null</see>.
		/// <example>
		/// <code>
		/// protected override void ValidateArguments(IInputManager inputManager, Camera camera)
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
		/// <param name="fourthArgument"> The fourth received argument to validate. </param>
		/// <param name="fifthArgument"> The fifth received argument to validate. </param>
		[Conditional("DEBUG")]
		protected virtual void ValidateArguments(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) { }

		/// <summary>
		/// Gets the names of the fields and properties into which the five
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see>
		/// arguments accepted by this object are assigned to in the constructor of this object.
		/// <para>
		/// By default uses reflection to figure out the names by searching for fields and properties with
		/// names matching the ones used in the constructor, or failing that, types being exact matches with the argument type.
		/// </para>
		/// <para>
		/// This method can also be overridden to manually specify the names.
		/// <example>
		/// <code>
		/// protected override (string, string, string, string, string) GetInitArgumentClassMemberNames()
		/// {
		///		return (nameof(inputHandler), nameof(collisionHandler), nameof(jumpHandler), nameof(fallHandler), nameof(animator));
		/// }
		/// </code>
		/// </example>
		/// </para>
		/// </summary>
		/// <returns> The names of five instance fields or properties. </returns>
		/// <exception cref="MissingMemberException">
		/// Thrown if no field or property is found with their name closely matching that of the one the constructor parameters
		/// nor is any field or property found with their type matching the type of the parameter exactly.
		/// </exception>
		/// <exception cref="System.Reflection.AmbiguousMatchException">
		/// Thrown if no field or property is found with their name closely matching that of the one the constructor parameters
		/// and more than one field or property is found with their type matching the type of the parameter exactly.
		/// </exception>
		protected virtual (string firstField, string secondField, string thirdField, string fourthField, string fifthField) GetInitArgumentClassMemberNames()
		{
			if(initPropertyNamesCache.TryGetValue(GetType(), out (string, string, string, string, string) names))
            {
				return names;
			}

			var type = GetType();

			try
			{
				names = (GetConstructorArgumentTargetFieldName(type, typeof(TFirstArgument)), GetConstructorArgumentTargetFieldName(type, typeof(TSecondArgument)), GetConstructorArgumentTargetFieldName(type, typeof(TThirdArgument)), GetConstructorArgumentTargetFieldName(type, typeof(TFourthArgument)), GetConstructorArgumentTargetFieldName(type, typeof(TFifthArgument)));
				initPropertyNamesCache[type] = names;
			}
			catch
			{
				foreach(var initializer in GetComponents<IInitializer>())
				{
					if(initializer.Target != this || !(initializer.GetType().GetNestedType(InitArgumentMetadataClassName) is Type init))
					{
						continue;
					}

					try
					{
						names = (GetConstructorArgumentTargetFieldName(init, typeof(TFirstArgument)), GetConstructorArgumentTargetFieldName(init, typeof(TSecondArgument)), GetConstructorArgumentTargetFieldName(init, typeof(TThirdArgument)), GetConstructorArgumentTargetFieldName(init, typeof(TFourthArgument)), GetConstructorArgumentTargetFieldName(init, typeof(TFifthArgument)));
						initPropertyNamesCache[type] = names;
						return names;
					}
					catch
					{
						break;
					}
				}

				throw;
			}

			return names;
		}

		/// <summary>
		/// <see cref="OnAwake"/> is called when the script instance is being loaded during the <see cref="Awake"/> event after the <see cref="Init"/> function has finished.
		/// <para>
		/// <see cref="OnAwake"/> is called either when an active <see cref="GameObject"/> that contains the script is initialized when a <see cref="UnityEngine.SceneManagement.Scene">Scene</see> loads,
		/// or when a previously <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject"/> is set active, or after a <see cref="GameObject"/> created with <see cref="Object.Instantiate"/>
		/// is initialized.
		/// </para>
		/// <para>
		/// Use <see cref="OnAwake"/> to initialize variables or states before the application starts.
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
		/// Note: Use <see cref="OnAwake"/> instead of the constructor for any initialization logic beyond assigining the received arguments into fields and properties,
		/// as the serialized state of the <see cref="Component"/> is undefined at construction time, and many classes in Unity are not thread safe and as such
		/// are not supported being called from <see cref="Component"/> constructors.
		/// </para>
		/// </summary>
		protected virtual void OnAwake() { }

        private void Awake()
        {
			if(!dependenciesReceived)
			{
				// Retry fetching arguments in Awake because scene services defined in Services components
                // can't always be fetched in the constructor or in OnAfterDeserialize because examining
                // the parent chains can only be done from the main thread.
				if(InitArgs.TryGet(Context.Awake, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
				{
					Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				}
				else
				{
					#if UNITY_EDITOR
					if(!Application.isPlaying || PrefabUtility.IsPartOfPrefabAsset(this) || PrefabStageUtility.GetPrefabStage(gameObject) != null)
					{
						return;
					}
					#endif

					DestroyImmediate(this);
					throw new MissingInitArgumentsException(this);
				}
			}

			OnAwake();
		}

        void ISerializationCallbackReceiver.OnBeforeSerialize() => OnBeforeSerialize();

        protected virtual void OnBeforeSerialize() { }

		void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
			if(dependenciesReceived && GetIsAnyInitPropertySerialized() && InitArgs.TryGet(Context.OnAfterDeserialize, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
            {
				OnAfterDeserialize(new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument));
			}
			else
            {
				OnAfterDeserialize(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.None);
			}
        }

		protected virtual void OnAfterDeserialize(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> arguments)
		{
			if(arguments.provided)
            {
				Init(arguments.firstArgument, arguments.secondArgument, arguments.thirdArgument, arguments.fourthArgument, arguments.fifthArgument);
			}
		}

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

		#if UNITY_EDITOR
		private void Reset()
		{
			if(InitArgs.TryGet(Context.Reset, this, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
			{
				Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			}

			OnInitializableReset(this);
			OnReset();
		}
		#endif

		private bool GetIsAnyInitPropertySerialized()
        {
			if(isAnyInitPropertySerializedCache.TryGetValue(GetType(), out bool isAnyInitPropertySerialized))
            {
				return isAnyInitPropertySerialized;
            }

			(string firstField, string secondField, string thirdField, string fourthField, string fifthField) = GetInitArgumentClassMemberNames();

			var type = GetType();
			isAnyInitPropertySerialized = IsClassMemberSerialized(type, firstField)
				|| IsClassMemberSerialized(type, secondField)
				|| IsClassMemberSerialized(type, thirdField)
				|| IsClassMemberSerialized(type, fourthField)
				|| IsClassMemberSerialized(type, fifthField);

			isAnyInitPropertySerializedCache[type] = isAnyInitPropertySerialized;
			return isAnyInitPropertySerialized;
		}

		private bool TryGetConstructorParameterName(int parameterNumber, out string parameterName)
        {
			try
			{
				var names = GetInitArgumentClassMemberNames();
				switch(parameterNumber)
				{
					case 1:
						parameterName = GetConstructorArgumentName(names.firstField);
						return true;
					case 2:
						parameterName = GetConstructorArgumentName(names.secondField);
						return true;
					case 3:
						parameterName = GetConstructorArgumentName(names.thirdField);
						return true;
					case 4:
						parameterName = GetConstructorArgumentName(names.fourthField);
						return true;
					case 5:
						parameterName = GetConstructorArgumentName(names.fifthField);
						return true;
					default:
						throw new IndexOutOfRangeException(parameterNumber.ToString());
				}
			}
			catch
            {
				parameterName = null;
				return false;
            }
        }

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TFirstArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			TryGetConstructorParameterName(1, out string parameterName);
			throw new ArgumentNullException(parameterName, $"First argument passed to {GetType().Name} was null.");
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
		protected void AssertNotNull(TFirstArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			string suffix = TryGetConstructorParameterName(1, out string parameterName) ? "\nParameter name: " + parameterName : "";
			Debug.LogAssertion($"First argument passed to {GetType().Name} was null.{suffix}", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TSecondArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			TryGetConstructorParameterName(2, out string parameterName);
			throw new ArgumentNullException(parameterName, $"Second argument passed to {GetType().Name} was null.");
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
		protected void AssertNotNull(TSecondArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			string suffix = TryGetConstructorParameterName(2, out string parameterName) ? "\nParameter name: " + parameterName : "";
			Debug.LogAssertion($"Second argument passed to {GetType().Name} was null.{suffix}", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TThirdArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			TryGetConstructorParameterName(3, out string parameterName);
			throw new ArgumentNullException(parameterName, $"Third argument passed to {GetType().Name} was null.");
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
		protected void AssertNotNull(TThirdArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			string suffix = TryGetConstructorParameterName(3, out string parameterName) ? "\nParameter name: " + parameterName : "";
			Debug.LogAssertion($"Third argument passed to {GetType().Name} was null.{suffix}", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TFourthArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			TryGetConstructorParameterName(4, out string parameterName);
			throw new ArgumentNullException(parameterName, $"Fourth argument passed to {GetType().Name} was null.");
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
		protected void AssertNotNull(TFourthArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			string suffix = TryGetConstructorParameterName(4, out string parameterName) ? "\nParameter name: " + parameterName : "";
			Debug.LogAssertion($"Fourth argument passed to {GetType().Name} was null.{suffix}", this);
			#endif
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws
		/// an <see cref="ArgumentNullException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG")]
		protected void ThrowIfNull(TFifthArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			TryGetConstructorParameterName(5, out string parameterName);
			throw new ArgumentNullException(parameterName, $"Fifth argument passed to {GetType().Name} was null.");
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
		protected void AssertNotNull(TFifthArgument argument)
		{
			#if DEBUG
			if(!dependenciesReceived || argument != Null)
            {
				return;
            }

			string suffix = TryGetConstructorParameterName(5, out string parameterName) ? "\nParameter name: " + parameterName : "";
			Debug.LogAssertion($"Fifth argument passed to {GetType().Name} was null.{suffix}", this);
			#endif
		}
	}
}