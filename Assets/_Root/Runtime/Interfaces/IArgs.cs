using UnityEngine;

namespace Pancake
{
	/// <summary>
	/// Represents an object which can receive an argument of type <typeparamref name="TArgument"/> as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TArgument}"/> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with their dependency using the <see cref="GameObjectExtensions.AddComponent{TComponent, TArgument}">GameObject.AddComponent{TComponent, TArgument}</see> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TArgument}"/>
	/// <seealso cref="MonoBehaviour{TArgument}"/>
	/// <seealso cref="ScriptableObject{TArgument}"/>
	/// </summary>
	/// <typeparam name="TArgument"> Type of the argument. </typeparam>
	public interface IArgs<TArgument> : IOneArgument, IFirstArgument<TArgument> { }

	/// <summary>
	/// Represents an object which can receive two arguments as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument}"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TFirstArgument, TSecondArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument> : ITwoArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument> { }

	/// <summary>
	/// Represents an object which can receive three arguments as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument> : IThreeArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument> { }

	/// <summary>
	/// Represents an object which can receive four arguments as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> : IFourArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument> { }

	/// <summary>
	/// Represents an object which can receive five arguments as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> : IFiveArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFifthArgument<TFifthArgument> { }

	/// <summary>
	/// Represents an object which can receive five arguments as part of its initialization process.
	/// <para>
	/// <see cref="Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="ObjectExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"> function.
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="GameObjectExtensions.AddComponent{TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// <para>
	/// If the class does not implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : ISixArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument> { }

	/// <summary>
	/// Represents an object which can receive one argument as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TArgument}"/>.
	/// </para>
	/// </summary>
	public interface IOneArgument { }

	/// <summary>
	/// Represents an object which can receive two arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument}"/>.
	/// </para>
	/// </summary>
	public interface ITwoArguments { }

	/// <summary>
	/// Represents an object which can receive three arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// </para>
	/// </summary>
	public interface IThreeArguments { }

	/// <summary>
	/// Represents an object which can receive four arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
	/// </para>
	/// </summary>
	public interface IFourArguments { }

	/// <summary>
	/// Represents an object which can receive five arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
	/// </para>
	/// </summary>
	public interface IFiveArguments { }

	/// <summary>
	/// Represents an object which can receive six arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
	/// </para>
	/// </summary>
	public interface ISixArguments { }

	/// <summary>
	/// Represents an object which can receive at least one argument as part of its initialization process,
	/// with the first one being an object of type <typeparamref name="TFirstArgument"/>.
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	public interface IFirstArgument<TFirstArgument> { }

	/// <summary>
	/// Represents an object which can receive at least two arguments as part of its initialization process,
	/// with the second one being an object of type <typeparamref name="TSecondArgument"/>.
	/// </summary>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	public interface ISecondArgument<TSecondArgument> { }

	/// <summary>
	/// Represents an object which can receive at least three arguments as part of its initialization process,
	/// with the third one being an object of type <typeparamref name="TThirdArgument"/>.
	/// </summary>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	public interface IThirdArgument<TThirdArgument> { }

	/// <summary>
	/// Represents an object which can receive at least four arguments as part of its initialization process,
	/// with the fourth one being an object of type <typeparamref name="TFourthArgument"/>.
	/// </summary>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	public interface IFourthArgument<TFourthArgument> { }

	/// <summary>
	/// Represents an object which can receive at least five arguments as part of its initialization process,
	/// with the fifth one being an object of type <typeparamref name="TFifthArgument"/>.
	/// </summary>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	public interface IFifthArgument<TFifthArgument> { }

	/// <summary>
	/// Represents an object which can receive at least six arguments as part of its initialization process,
	/// with the sixth one being an object of type <typeparamref name="TSixthArgument"/>.
	/// </summary>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	public interface ISixthArgument<TSixthArgument> { }
}