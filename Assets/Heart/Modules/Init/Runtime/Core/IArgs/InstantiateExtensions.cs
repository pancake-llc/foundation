using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// Extensions methods for <see cref="Object"/> that can be used to <see cref="Instantiate">instantiate</see>
	/// new copies <see cref="Object">Objects</see> that implement one of the <see cref="IArgs{TArgument}">IArgs</see> interfaces
	/// with the required dependencies passed to the clone's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public static class InstantiateExtensions
	{
		private const string ORIGINAL_IS_NULL_EXCEPTION_MESSAGE = "The {0} you want to instantiate is null.";

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// <para>
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be provided using
		/// the <see cref="IInitializable{TArgument}.Init">Init</see> function immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the argument will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="argument"> The argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TArgument>
			([DisallowNull] this TObject original, TArgument argument)
				where TObject : Object, IArgs<TArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TArgument>(argument);

			var client = Object.Instantiate(original);

			if(!InitArgs.Clear<TObject, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// <para>
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be provided using
		/// the <see cref="IInitializable{TArgument}.Init">Init</see> function immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the argument will be received during the Awake event function.
		/// </para>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="argument"> The argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TArgument>
			([DisallowNull] this TObject original, TArgument argument, [AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TArgument>(argument);

			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);

			if(!InitArgs.Clear<TObject, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// <para>
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be provided using
		/// the <see cref="IInitializable{TArgument}.Init">Init</see> function immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the argument will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="argument"> The argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TArgument>
			([DisallowNull] this TObject original, TArgument argument, Vector3 position, Quaternion rotation)
				where TObject : Object, IArgs<TArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TArgument>(argument);

			var client = Object.Instantiate(original, position, rotation);

			if(!InitArgs.Clear<TObject, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// <para>
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TArgument}"/> interface the argument can be provided using
		/// the <see cref="IInitializable{TArgument}.Init">Init</see> function immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the argument will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TArgument"> Type of the argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="argument"> The argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="rotation"> The orientation of the new GameObject. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TArgument>
			([DisallowNull] this TObject original, TArgument argument, Vector3 position, Quaternion rotation, [AllowNull] Transform parent)
				where TObject : Object, IArgs<TArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TArgument>(argument);

			var client = Object.Instantiate(original, position, rotation, parent);

			if(!InitArgs.Clear<TObject, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, [AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, [AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
				[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
				Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			var client = Object.Instantiate(original);

			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="twelfthArgument"> The twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			var client = Object.Instantiate(original);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument,
				[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument,
				Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);

			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="twelfthArgument"> The twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <param name="instantiateInWorldSpace"> When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the object’s position relative to its new parent. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument,
			[AllowNull] Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			var client = Object.Instantiate(original, parent, instantiateInWorldSpace);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}

		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such as during the Awake event)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init">Init</see> function
		/// immediately after initialization has finished (before the Start event function).
		/// </para>
		/// <para>
		/// For classes deriving from <see cref="MonoBehaviour{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> the latter method will be used in cases
		/// where the <paramref name="original"/> object is a <see cref="Component"/> in an <see cref="GameObject.activeSelf">inactive</see> <see cref="GameObject"/>,
		/// while if the the <paramref name="gameObject"/> is inactive the arguments will be received during the Awake event function.
		/// </para>
		/// <seealso cref="Object.Instantiate"/>
		/// </summary>
		/// <typeparam name="TObject"> Type of the <see cref="Object"/> that is being cloned. </typeparam>
		/// <typeparam name="TFirstArgument"> Type of the first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSecondArgument"> Type of the second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TThirdArgument"> Type of the third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFourthArgument"> Type of the fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TFifthArgument"> Type of the fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSixthArgument"> Type of the sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TSeventhArgument"> Type of the seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEighthArgument"> Type of the eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TNinthArgument"> Type of the ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTenthArgument"> Type of the tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TEleventhArgument"> Type of the eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <typeparam name="TTwelfthArgument"> Type of the twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="original"> Original <typeparamref name="TObject"/> to clone. </param>
		/// <param name="firstArgument"> The first argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="secondArgument"> The second argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="thirdArgument"> The third argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fourthArgument"> The fourth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="fifthArgument"> The fifth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="sixthArgument"> The sixth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="seventhArgument"> The seventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eighthArgument"> The eighth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="ninthArgument"> The ninth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="tenthArgument"> The tenth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="eleventhArgument"> The eleventh argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="twelfthArgument"> The twelfth argument used during initialization of the cloned <typeparamref name="TObject"/>. </param>
		/// <param name="position"> The position for the new instance. </param>
		/// <param name="rotation"> The rotation for the new instance. </param>
		/// <param name="parent"> Parent that will be assigned to the new object. </param>
		/// <returns> The cloned <typeparamref name="TObject"/>. </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="original"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		[return: NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument,
			Vector3 position, Quaternion rotation, [AllowNull] Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			ThrowIfNull(original, ORIGINAL_IS_NULL_EXCEPTION_MESSAGE);

			InitArgs.Set<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			var client = Object.Instantiate(original, position, rotation, parent);
			
			if(!InitArgs.Clear<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(client);
		}
    }
}