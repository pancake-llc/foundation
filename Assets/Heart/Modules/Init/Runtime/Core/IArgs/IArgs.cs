using Sisus.Init.Internal;
using Sisus.Init.ValueProviders;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Represents an object which can receive an argument of type <typeparamref name="TArgument"/> as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// with their dependency using the <see cref="AddComponent">GameObject.AddComponent{TComponent, TArgument}</see> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TArgument}"/>.
	/// </para>
	/// </summary>
	/// <typeparam name="TArgument"> Type of the argument. </typeparam>
	/// <seealso cref="IInitializable{TArgument}"/>
	/// <seealso cref="MonoBehaviour{TArgument}"/>
	/// <seealso cref="ScriptableObject{TArgument}"/>
	public interface IArgs<TArgument> : IOneArgument, IFirstArgument<TArgument>
	{
		/// <summary>
		/// Validates the initialization argument that was provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if the argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="argument"> Received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if the argument is invalid. </exception>
		void Validate(TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, argument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive two arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/>
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument> : ITwoArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if either argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive three arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>, or if you you would like to retrieve the dependency at an earlier
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
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive four arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
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
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive five arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
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
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive six arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> : ISixArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive seven arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> : ISevenArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive eight arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> : IEightArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>, IEighthArgument<TEighthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <param name="eighthArgument"> Eighth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eighthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive nine arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> : INineArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>, IEighthArgument<TEighthArgument>, INinthArgument<TNinthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <param name="eighthArgument"> Eighth received argument to validate. </param>
		/// <param name="ninthArgument"> Ninth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eighthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, ninthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive ten arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> : ITenArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>, IEighthArgument<TEighthArgument>, INinthArgument<TNinthArgument>, ITenthArgument<TTenthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <param name="eighthArgument"> Eighth received argument to validate. </param>
		/// <param name="ninthArgument"> Ninth received argument to validate. </param>
		/// <param name="tenthArgument"> Tenth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eighthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, ninthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, tenthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}
	
	/// <summary>
	/// Represents an object which can receive eleven arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument. </typeparam>
	/// <typeparam name="TEleventhArgument"> Type of the eleventh argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> : IElevenArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>, IEighthArgument<TEighthArgument>, INinthArgument<TNinthArgument>, ITenthArgument<TTenthArgument>, IEleventhArgument<TEleventhArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <param name="eighthArgument"> Eighth received argument to validate. </param>
		/// <param name="ninthArgument"> Ninth received argument to validate. </param>
		/// <param name="tenthArgument"> Tenth received argument to validate. </param>
		/// <param name="eleventhArgument"> Eleventh received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eighthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, ninthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, tenthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eleventhArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

	/// <summary>
	/// Represents an object which can receive twelve arguments as part of its initialization process.
	/// <para>
	/// <see cref="UnityEngine.Object"/>-derived classes that implement this interface can be instantiated with their dependency using the 
	/// <see cref="InstantiateExtensions.Instantiate{TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> function.
	/// </para>
	/// <para>
	/// <see cref="MonoBehaviour"/>-derived classes that implement this interface can be added to a <see cref="GameObject"/> with their dependency using the
	/// <see cref="AddComponent"/> function.
	/// </para>
	/// <para>
	/// If the class also implements <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> then these functions can automatically inject the dependency to
	/// the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init"/> function at the end of the initialization
	/// process; after the Awake and OnEnable events but before the Start event.
	/// </para>
	/// <para>
	/// If the class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>, or if you you would like to retrieve the dependency at an earlier
	/// stage of the initialization process, you can use <see cref="InitArgs.TryGet{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
	/// </para>
	/// <seealso cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
	/// <seealso cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
	/// <seealso cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	/// <typeparam name="TSeventhArgument"> Type of the seventh argument. </typeparam>
	/// <typeparam name="TEighthArgument"> Type of the eighth argument. </typeparam>
	/// <typeparam name="TNinthArgument"> Type of the ninth argument. </typeparam>
	/// <typeparam name="TTenthArgument"> Type of the tenth argument. </typeparam>
	/// <typeparam name="TEleventhArgument"> Type of the eleventh argument. </typeparam>
	/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument. </typeparam>
	public interface IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> : ITwelveArguments,
		IFirstArgument<TFirstArgument>, ISecondArgument<TSecondArgument>, IThirdArgument<TThirdArgument>, IFourthArgument<TFourthArgument>, IFifthArgument<TFifthArgument>, ISixthArgument<TSixthArgument>, ISeventhArgument<TSeventhArgument>, IEighthArgument<TEighthArgument>, INinthArgument<TNinthArgument>, ITenthArgument<TTenthArgument>, IEleventhArgument<TEleventhArgument>, ITwelfthArgument<TTwelfthArgument>
	{
		/// <summary>
		/// Validates the initialization arguments that were provided to this client.
		/// <remarks>
		/// By default, this returns <see langword="false"/> if any argument is <see langword="null"/>.
		/// </remarks> 
		/// </summary>
		/// <param name="firstArgument"> First received argument to validate. </param>
		/// <param name="secondArgument"> Second received argument to validate. </param>
		/// <param name="thirdArgument"> Third received argument to validate. </param>
		/// <param name="fourthArgument"> Fourth received argument to validate. </param>
		/// <param name="fifthArgument"> Fifth received argument to validate. </param>
		/// <param name="sixthArgument"> Sixth received argument to validate. </param>
		/// <param name="seventhArgument"> Seventh received argument to validate. </param>
		/// <param name="eighthArgument"> Eighth received argument to validate. </param>
		/// <param name="ninthArgument"> Ninth received argument to validate. </param>
		/// <param name="tenthArgument"> Tenth received argument to validate. </param>
		/// <param name="eleventhArgument"> Eleventh received argument to validate. </param>
		/// <param name="twelfthArgument"> Twelfth received argument to validate. </param>
		/// <exception cref="InvalidInitArgumentsException"> Thrown if one or more the arguments are invalid. </exception>
		void Validate(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			InvalidInitArgumentsException exception = null;
			InvalidInitArgumentsException.ValidateNotNull(this, firstArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, secondArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, thirdArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fourthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, fifthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, sixthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, seventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eighthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, ninthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, tenthArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, eleventhArgument, ref exception);
			InvalidInitArgumentsException.ValidateNotNull(this, twelfthArgument, ref exception);
			if(exception is not null && InitializerUtility.ShouldSelfGuardAgainstNull(this))
			{
				throw exception;
			}
			#endif
		}
	}

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
	/// Represents an object which can receive seven arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>.
	/// </para>
	/// </summary>
	public interface ISevenArguments { }

	/// <summary>
	/// Represents an object which can receive eight arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>.
	/// </para>
	/// </summary>
	public interface IEightArguments { }

	/// <summary>
	/// Represents an object which can receive nine arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>.
	/// </para>
	/// </summary>
	public interface INineArguments { }

	/// <summary>
	/// Represents an object which can receive ten arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>.
	/// </para>
	/// </summary>
	public interface ITenArguments { }

	/// <summary>
	/// Represents an object which can receive eleven arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>.
	/// </para>
	/// </summary>
	public interface IElevenArguments { }

	/// <summary>
	/// Represents an object which can receive twelve arguments as part of its initialization process.
	/// <para>
	/// Base interface of <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
	/// </para>
	/// </summary>
	public interface ITwelveArguments { }

	/// <summary>
	/// Represents an object which can receive one or more arguments as part of its initialization process,
	/// with the first one being an object of type <typeparamref name="TFirstArgument"/>.
	/// </summary>
	/// <typeparam name="TFirstArgument"> Type of the first argument. </typeparam>
	public interface IFirstArgument<TFirstArgument> { }

	/// <summary>
	/// Represents an object which can receive two or more arguments as part of its initialization process,
	/// with the second one being an object of type <typeparamref name="TSecondArgument"/>.
	/// </summary>
	/// <typeparam name="TSecondArgument"> Type of the second argument. </typeparam>
	public interface ISecondArgument<TSecondArgument> { }

	/// <summary>
	/// Represents an object which can receive three or more arguments as part of its initialization process,
	/// with the third one being an object of type <typeparamref name="TThirdArgument"/>.
	/// </summary>
	/// <typeparam name="TThirdArgument"> Type of the third argument. </typeparam>
	public interface IThirdArgument<TThirdArgument> { }

	/// <summary>
	/// Represents an object which can receive four or more arguments as part of its initialization process,
	/// with the fourth one being an object of type <typeparamref name="TFourthArgument"/>.
	/// </summary>
	/// <typeparam name="TFourthArgument"> Type of the fourth argument. </typeparam>
	public interface IFourthArgument<TFourthArgument> { }

	/// <summary>
	/// Represents an object which can receive five or more arguments as part of its initialization process,
	/// with the fifth one being an object of type <typeparamref name="TFifthArgument"/>.
	/// </summary>
	/// <typeparam name="TFifthArgument"> Type of the fifth argument. </typeparam>
	public interface IFifthArgument<TFifthArgument> { }

	/// <summary>
	/// Represents an object which can receive six or more arguments as part of its initialization process,
	/// with the sixth one being an object of type <typeparamref name="TSixthArgument"/>.
	/// </summary>
	/// <typeparam name="TSixthArgument"> Type of the sixth argument. </typeparam>
	public interface ISixthArgument<TSixthArgument> { }
	
	/// <summary>
	/// Represents an object which can receive seven or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TSeventhArgument"/>.
	/// </summary>
	/// <typeparam name="TSeventhArgument"> Type of the twelfth argument. </typeparam>
	public interface ISeventhArgument<TSeventhArgument> { }
	
	/// <summary>
	/// Represents an object which can receive eight or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TEighthArgument"/>.
	/// </summary>
	/// <typeparam name="TEighthArgument"> Type of the twelfth argument. </typeparam>
	public interface IEighthArgument<TEighthArgument> { }
	
	/// <summary>
	/// Represents an object which can receive nine or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TNinthArgument"/>.
	/// </summary>
	/// <typeparam name="TNinthArgument"> Type of the twelfth argument. </typeparam>
	public interface INinthArgument<TNinthArgument> { }
	
	/// <summary>
	/// Represents an object which can receive ten or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TTenthArgument"/>.
	/// </summary>
	/// <typeparam name="TTenthArgument"> Type of the twelfth argument. </typeparam>
	public interface ITenthArgument<TTenthArgument> { }
	
	/// <summary>
	/// Represents an object which can receive eleven or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TEleventhArgument"/>.
	/// </summary>
	/// <typeparam name="TEleventhArgument"> Type of the twelfth argument. </typeparam>
	public interface IEleventhArgument<TEleventhArgument> { }

	/// <summary>
	/// Represents an object which can receive twelve or more arguments as part of its initialization process,
	/// with the twelfth one being an object of type <typeparamref name="TTwelfthArgument"/>.
	/// </summary>
	/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument. </typeparam>
	public interface ITwelfthArgument<TTwelfthArgument> { }
}