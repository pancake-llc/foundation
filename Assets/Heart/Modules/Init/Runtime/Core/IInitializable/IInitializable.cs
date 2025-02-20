using Sisus.Init.ValueProviders;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object which might be able to try and locate its own dependencies and initialize itself.
	/// </summary>
	public interface IInitializable
	{
		/// <summary>
		/// Gets a value indicating whether this object has an initializer attached to it.
		/// <para>
		/// If the object itself is an initializer, then returns <see langword="false"/>.
		/// </para>
		/// </summary>
		bool HasInitializer { get; }

		/// <summary>
		/// Requests the object to try and acquire all the objects that it depends on and initialize itself.
		/// <remarks>
		/// <seealso cref="IInitializableExtensions.Init{TInitializable, TArgument}"/> can be used to implement this method
		/// in a single line.
		/// </remarks>
		/// </summary>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize itself, or has already
		/// successfully initialized itself previously; otherwise, <see langword="false"/>.
		/// </returns>
		bool Init(Context context);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using an argument of type <typeparamref name="TArgument"/>.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TArgument}(TObject, TArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TArgument}(GameObject, TArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions will pass the argument to the <see cref="IInitializable{TArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided argument at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TArgument}"/>.
	/// </para>
	/// </summary>
	/// <seealso cref="MonoBehaviour{TArgument}"/>
	/// <seealso cref="ScriptableObject{TArgument}"/>
	public interface IInitializable<TArgument> : IArgs<TArgument>
	{
		/// <summary>
		/// Initializes the object with an object that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TArgument}"/> <see cref="Init"/> gets called the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TArgument}(TObject, TArgument)"/>
		/// or <see cref="AddComponentExtensions.AddComponent{TComponent, TArgument}(GameObject, TArgument)"/>.
		/// </para>
		/// <para>
		/// It is also possible for the object to receive the argument at an earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="argument"> Argument used during initialization of the component. </param>
		void Init(TArgument argument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using two arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}(TObject, TFirstArgument, TSecondArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}(GameObject, TFirstArgument, TSecondArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument> : IArgs<TFirstArgument, TSecondArgument>
	{
		/// <summary>
		/// Initializes the object with two objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using three arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		/// <summary>
		/// Initializes the object with three objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using four arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		/// <summary>
		/// Initializes the object with four objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using five arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		/// <summary>
		/// Initializes the object with five objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using six arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument passed to the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
	{
		/// <summary>
		/// Initializes the object with six objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using seven arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
	{
		/// <summary>
		/// Initializes the object with seven objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is being created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument);
	}
	
	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using eight arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
	{
		/// <summary>
		/// Initializes the object with eight objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		/// <param name="eighthArgument"> Eighth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using nine arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
	{
		/// <summary>
		/// Initializes the object with nine objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		/// <param name="eighthArgument"> Eighth argument used during initialization of the component. </param>
		/// <param name="ninthArgument"> Ninth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument);
	}
	
	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using ten arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
	{
		/// <summary>
		/// Initializes the object with ten objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		/// <param name="eighthArgument"> Eighth argument used during initialization of the component. </param>
		/// <param name="ninthArgument"> Ninth argument used during initialization of the component. </param>
		/// <param name="tenthArgument"> Tenth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using eleven arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEleventhArgument"> Type of the eleventh argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
	{
		/// <summary>
		/// Initializes the object with eleven objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		/// <param name="eighthArgument"> Eighth argument used during initialization of the component. </param>
		/// <param name="ninthArgument"> Ninth argument used during initialization of the component. </param>
		/// <param name="tenthArgument"> Tenth argument used during initialization of the component. </param>
		/// <param name="eleventhArgument"> Eleventh argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using twelve arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}(TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument)"/> method.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can also be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="AddComponentExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}(GameObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument)"/> method.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to receive the provided arguments at an earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using
	/// <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TEleventhArgument"> Type of the eleventh argument accepted by the <see cref="Init"/> method. </typeparam>
	/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument accepted by the <see cref="Init"/> method. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
	{
		/// <summary>
		/// Initializes the object with twelve objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> method as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> <see cref="Init"/> gets called during the Awake event
		/// when the instance is being loaded at runtime and during the Reset event when the script is added to a GameObject in edit mode.
		/// If the GameObject is inactive causing the Awake event to never get fired, then the <see cref="Init"/> method is called immediately
		/// after the object has been created.
		/// </para>
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> <see cref="Init"/> also gets called during the Awake event.
		/// </para>
		/// <para>
		/// For other classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> or
		/// <see cref="AddComponent"/>.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		/// <param name="thirdArgument"> Third argument used during initialization of the component. </param>
		/// <param name="fourthArgument"> Fourth argument used during initialization of the component. </param>
		/// <param name="fifthArgument"> Fifth argument used during initialization of the component. </param>
		/// <param name="sixthArgument"> Sixth argument used during initialization of the component. </param>
		/// <param name="seventhArgument"> Seventh argument used during initialization of the component. </param>
		/// <param name="eighthArgument"> Eighth argument used during initialization of the component. </param>
		/// <param name="ninthArgument"> Ninth argument used during initialization of the component. </param>
		/// <param name="tenthArgument"> Tenth argument used during initialization of the component. </param>
		/// <param name="twelfthArgument"> Twelfth argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument);
	}
}