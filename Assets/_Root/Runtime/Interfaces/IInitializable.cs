using UnityEngine;

namespace Pancake
{
	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using an argument of type <typeparamref name="TArgument"/>.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with an argument using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with an argument using the <see cref="GameObjectExtensions.AddComponent{TComponent}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the argument using the <see cref="IInitializable{TArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the argument at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TArgument}"/>
	/// <seealso cref="ScriptableObject{TArgument}"/>
	/// </summary>
	public interface IInitializable<TArgument> : IArgs<TArgument>
	{
		/// <summary>
		/// Initializes the object with an object that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// For other classes that implement <see cref="IInitializable{TArgument}"/> <see cref="Init"/> gets called automatically at the end of initialization
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
		/// </para>
		/// </summary>
		/// <param name="argument"> Argument used during initialization of the component. </param>
		void Init(TArgument argument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using two arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with arguments using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with arguments using the <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the arguments at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> function. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument> : IArgs<TFirstArgument, TSecondArgument>
	{
		/// <summary>
		/// Initializes the object with two objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
		/// </para>
		/// </summary>
		/// <param name="firstArgument"> First argument used during initialization of the component. </param>
		/// <param name="secondArgument"> Second argument used during initialization of the component. </param>
		void Init(TFirstArgument firstArgument, TSecondArgument secondArgument);
	}

	/// <summary>
	/// Represents an object which can be <see cref="Init">initialized</see> using three arguments.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with arguments using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with arguments using the <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> function at the end of the
	/// initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the arguments at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> function. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		/// <summary>
		/// Initializes the object with three objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
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
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with arguments using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with arguments using the <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the arguments at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> function. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
	{
		/// <summary>
		/// Initializes the object with four objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
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
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with arguments using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with arguments using the <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the arguments at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the <see cref="Init"/> function. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
	{
		/// <summary>
		/// Initializes the object with five objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
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
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with arguments using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with arguments using the <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}">GameObject.AddComponent</see> function.
	/// </para>
	/// <para>
	/// These functions can automatically inject the arguments using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/>
	/// function at the end of the initialization process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// It is also possible for the object to retrieve the arguments at any earlier stage of its initialization process,
	/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
	/// </para>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument passed to the <see cref="Init"/> function. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument passed to the <see cref="Init"/> function. </typeparam>
	public interface IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
	{
		/// <summary>
		/// Initializes the object with six objects that it depends on.
		/// <para>
		/// You can think of the <see cref="Init"/> function as a parameterized constructor alternative for <see cref="Object"/>-derived classes.
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
		/// (after Awake and OnEnable but before Start) when an instance is created using the <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> or the
		/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}">GameObject.AddComponent</see> function.
		/// </para>
		/// <para>
		/// It is also possible for the initialized object to retrieve the argument at any earlier stage of its initialization process,
		/// such as in the constructor or during the Awake event, using <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
		/// If the arguments are retrieved independently by the object during its initialization process like this, then the argument will not get
		/// injected to the object again separately through the <see cref="Init"/> method being called by the injection code.
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
}