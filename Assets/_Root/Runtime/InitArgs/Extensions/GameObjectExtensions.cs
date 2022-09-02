using System;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
	/// <summary>
	/// Extensions methods for <see cref="GameObject"/> that can be used to <see cref="AddComponent">add components</see>
	/// that implement one of the <see cref="IArgs{TArgument}">IArgs</see> interfaces
	/// with the required dependencies passed to the component's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> the component using the provided <paramref name="argument"/>.
		/// <para>
		/// The argument should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
		public static TComponent AddComponent<TComponent, TArgument>([NotNull] this GameObject gameObject, TArgument argument)
			where TComponent : MonoBehaviour, IArgs<TArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

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

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

			InitArgs.Set<TComponent, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);
			var client = gameObject.AddComponent<TComponent>();

			if(!InitArgs.Clear<TComponent, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

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

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

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

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

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

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {nameof(TComponent)} is null.");
			}
			#endif

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

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), typeof(TComponent));
		}

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> the component using the provided argument.
		/// <para>
		/// The argument should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TArgument argument)
				where TComponent : MonoBehaviour, IArgs<TArgument>
				 => component = gameObject.AddComponent<TComponent, TArgument>(argument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

		/// <summary>
		/// Adds a component of type <typeparamref name="TComponent"/> to the <paramref name="gameObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see> the component using the provided arguments.
		/// <para>
		/// Arguments should either be received by the added component during its initialization (such during the Awake event function or in the constructor)
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
			([NotNull] this GameObject gameObject, out TComponent component, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TComponent : MonoBehaviour, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
				 => component = gameObject.AddComponent<TComponent, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

        /// <summary>
        /// Returns the object of Type <typeparamref name="T"/> if the game object has one attached,
        /// <see langword="null"/> if it doesn't.
        /// <para>
        /// This will return the first object that is found and the order is undefined.
        /// If you expect there to be more than one component of the same type you can
        /// use gameObject.GetComponents instead, and cycle through the returned components
        /// testing for some unique property.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to retrieve.
        /// <para>
        /// This can be the exact type of the object's class, a type of a class that the
        /// object's class derives from, or the type of an interface that the object's class implements.
        /// </para>
        /// <para>
        /// It is also possible to get objects wrapped by <see cref="IWrapper">wrappers</see>
        /// using the type of the wrapped object or using any interface implemented by the wrapped object.
        /// </para>
        /// </typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [CanBeNull]
        public static T Get<T>([NotNull] this GameObject gameObject) => Find.In<T>(gameObject);

        [CanBeNull]
        public static T Get<T>([NotNull] this Component component) => Find.In<T>(component.gameObject);

        public static bool TryGet<T>([NotNull] this GameObject gameObject, out T result) => Find.In(gameObject, out result);

        [CanBeNull]
        public static T[] GetAll<T>([NotNull] this GameObject gameObject) => Find.AllIn<T>(gameObject);

        [CanBeNull]
        public static T GetInChildren<T>([NotNull] this GameObject gameObject, bool includeInactive = false) => Find.InChildren<T>(gameObject, includeInactive);

        [CanBeNull]
        public static T[] GetAllInChildren<T>([NotNull] this GameObject gameObject, bool includeInactive = false) => Find.AllInChildren<T>(gameObject, includeInactive);

        [CanBeNull]
        public static T GetInParents<T>([NotNull] this GameObject gameObject, bool includeInactive = false) => Find.InParents<T>(gameObject, includeInactive);

        [CanBeNull]
        public static T[] GetAllInParents<T>([NotNull] this GameObject gameObject, bool includeInactive = false) => Find.AllInParents<T>(gameObject, includeInactive);

        [CanBeNull]
        public static TWrapped GetWrappedInChildren<TWrapped>([NotNull] this Component component, bool includeInactive = false) => Find.InChildren<TWrapped>(component.gameObject, includeInactive);

        [CanBeNull]
        public static TWrapped GetWrappedInParents<TWrapped>([NotNull] this Component component, bool includeInactive = false) => Find.InParents<TWrapped>(component.gameObject, includeInactive);

        #region Internal

        internal static Component AddComponent<TArgument>([NotNull] this GameObject gameObject, [NotNull] Type componentType, TArgument argument)
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {componentType} is null.");
			}

			if(componentType == null)
			{
				throw new ArgumentNullException($"The component type which you want to add to GameObject {gameObject.name} is null.");
			}
			#endif

			InitArgs.Set(componentType, argument);
			var client = gameObject.AddComponent(componentType);

			if(!InitArgs.Clear<TArgument>(componentType))
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), componentType);
		}

		internal static Component AddComponent<TFirstArgument, TSecondArgument>([NotNull] this GameObject gameObject, [NotNull] Type componentType, TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {componentType} is null.");
			}

			if(componentType == null)
			{
				throw new ArgumentNullException($"The component type which you want to add to GameObject {gameObject.name} is null.");
			}
			#endif

			InitArgs.Set(componentType, firstArgument, secondArgument);
			var client = gameObject.AddComponent(componentType);

			if(!InitArgs.Clear<TFirstArgument, TSecondArgument>(componentType))
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), componentType);
		}

		internal static Component AddComponent<TFirstArgument, TSecondArgument, TThirdArgument>([NotNull] this GameObject gameObject, [NotNull] Type componentType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {componentType} is null.");
			}

			if(componentType == null)
			{
				throw new ArgumentNullException($"The component type which you want to add to GameObject {gameObject.name} is null.");
			}
			#endif

			InitArgs.Set(componentType, firstArgument, secondArgument, thirdArgument);
			var client = gameObject.AddComponent(componentType);

			if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument>(componentType))
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), componentType);
		}

		internal static Component AddComponent<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([NotNull] this GameObject gameObject, [NotNull] Type componentType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {componentType} is null.");
			}

			if(componentType == null)
			{
				throw new ArgumentNullException($"The component type which you want to add to GameObject {gameObject.name} is null.");
			}
			#endif

			InitArgs.Set(componentType, firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = gameObject.AddComponent(componentType);

			if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(componentType))
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), componentType);
		}

		internal static Component AddComponent<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([NotNull] this GameObject gameObject, [NotNull] Type componentType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			#if DEBUG
			if(gameObject == null)
			{
				throw new ArgumentNullException($"The GameObject to which you want to add the component {componentType} is null.");
			}

			if(componentType == null)
			{
				throw new ArgumentNullException($"The component type which you want to add to GameObject {gameObject.name} is null.");
			}
			#endif

			InitArgs.Set(componentType, firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = gameObject.AddComponent(componentType);

			if(!InitArgs.Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(componentType))
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(nameof(AddComponent), componentType);
		}

		#if UNITY_EDITOR
		internal static bool IsPartOfPrefabAssetOrOpenInPrefabStage([NotNull] this GameObject gameObject)
        {
			return gameObject.IsPartOfPrefabAsset() || gameObject.IsOpenInPrefabStage();
        }
		#endif

		#if UNITY_EDITOR
        internal static bool IsPartOfPrefabAsset([NotNull] this GameObject gameObject)
        {
			return !gameObject.scene.IsValid();
		}
		#endif

		#if UNITY_EDITOR
        internal static bool IsOpenInPrefabStage([NotNull] this GameObject gameObject)
        {
		    #if UNITY_2020_1_OR_NEWER
		    return UnityEditor.SceneManagement.StageUtility.GetStage(gameObject) != null;
		    #else
		    return UnityEditor.SceneManagement.StageUtility.GetStageHandle(gameObject).IsValid();
		    #endif
        }
		#endif

        #endregion
    }
}