using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.ValueProviders;
using UnityEngine;
using static Sisus.NullExtensions;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to <see cref="AddComponent">add components</see>
	/// that implement one of the <see cref="IArgs{TArgument}">IArgs</see> interfaces
	/// with the required dependencies passed to the component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public static class AddComponentExtensions
	{
		private const string GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE = "The GameObject to which you want to add the component {0} is null.";

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> the component using the provided <paramref name="argument"/>.
		/// <para>
		/// The argument should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be
		/// provided using the <see cref="IInitializable{TArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument passed to the component's <see cref="IArgs{TArgument}">Init</see> function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="argument"> The argument passed to the component's <see cref="IArgs{TArgument}">Init</see> function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TArgument}"/> and did not manually handle receiving the provided argument. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TArgument>([DisallowNull] this GameObject gameObject, TArgument argument)
			where TComponent : MonoBehaviour, IArgs<TArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TArgument>(argument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			var client =
			#if UNITY_EDITOR
				Application.isPlaying
				? gameObject.AddComponent<TComponent>()
				: UnityEditor.Undo.AddComponent<TComponent>(gameObject);
			#else
				gameObject.AddComponent<TComponent>();
			#endif

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did receive the arguments during initialization. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of eleventh argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <param name="eleventhArgument"> The eleventh argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of eleventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTwelfthArgument"> Type of twelfth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <param name="eleventhArgument"> The eleventh argument passed to the component's Init function. </param>
		/// <param name="twelfthArgument"> The twelfth argument passed to the component's Init function. </param>
		/// <returns> The added component. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TComponent AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			ThrowIfNull(gameObject, GAME_OBJECT_IS_NULL_EXCEPTION_MESSAGE, typeof(TComponent));

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> the component using the provided argument.
		/// <para>
		/// The argument should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be
		/// provided using the <see cref="IInitializable{TArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the argument will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="argument"> The argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TArgument argument)
				where TComponent : MonoBehaviour, IArgs<TArgument>
				 => component = gameObject.AddComponent<TComponent, TArgument>(argument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of eleventh argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <param name="eleventhArgument"> The eleventh argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event)
		/// or if the component class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="gameObject"/> is <see cref="GameObject.activeSelf">inactive</see>, while if the the <paramref name="gameObject"/> is
		/// inactive the arguments will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TComponent"> Type of the component to add. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TSecondArgument"> Type of the second argument passed to the component's Init function. </typeparam>
		/// <typeparam name="TThirdArgument"> Type of third argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFourthArgument"> Type of fourth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TFifthArgument"> Type of fifth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSixthArgument"> Type of sixth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TSeventhArgument"> Type of seventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEighthArgument"> Type of eighth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TNinthArgument"> Type of ninth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTenthArgument"> Type of tenth argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TEleventhArgument"> Type of eleventh argument which is passed to the component's Init function. </typeparam>
		/// <typeparam name="TTwelfthArgument"> Type of twelfth argument which is passed to the component's Init function. </typeparam>
		/// <param name="gameObject"> The GameObject to which the component is added. </param>
		/// <param name="component">
		/// When this method returns, contains the component of type <typeparamref name="TComponent"/> that was added to the <paramref name="gameObject"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first argument passed to the component's Init function. </param>
		/// <param name="secondArgument"> The second argument passed to the component's Init function. </param>
		/// <param name="thirdArgument"> The third argument passed to the component's Init function. </param>
		/// <param name="fourthArgument"> The fourth argument passed to the component's Init function. </param>
		/// <param name="fifthArgument"> The fifth argument passed to the component's Init function. </param>
		/// <param name="sixthArgument"> The sixth argument passed to the component's Init function. </param>
		/// <param name="seventhArgument"> The seventh argument passed to the component's Init function. </param>
		/// <param name="eighthArgument"> The eighth argument passed to the component's Init function. </param>
		/// <param name="ninthArgument"> The ninth argument passed to the component's Init function. </param>
		/// <param name="tenthArgument"> The tenth argument passed to the component's Init function. </param>
		/// <param name="eleventhArgument"> The eleventh argument passed to the component's Init function. </param>
		/// <param name="twelfthArgument"> The twelfth argument passed to the component's Init function. </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <see cref="this"/> <see cref="GameObject"/> is <see langword="null"/>. 
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TComponent"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
	}
}