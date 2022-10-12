using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
	/// <summary>
	/// Extensions methods for <see cref="Object"/> that can be used to <see cref="Instantiate">instantiate</see>
	/// new copies <see cref="Object">Objects</see> that implement one of the <see cref="IArgs{TArgument}">IArgs</see> interfaces
	/// with the required dependencies passed to the clone's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TArgument}.Init">initializes</summary>
		/// it with the given argument and then returns the clone.
		/// <para>
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TArgument argument)
				where TObject : Object, IArgs<TArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TArgument>
			(this TObject original, TArgument argument, Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TArgument>
			(this TObject original, TArgument argument, Vector3 position, Quaternion rotation)
				where TObject : Object, IArgs<TArgument>
		{
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}

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
		/// The argument should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TArgument>
			(this TObject original, TArgument argument, Vector3 position, Quaternion rotation, Transform parent)
				where TObject : Object, IArgs<TArgument>
		{
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([JetBrains.Annotations.NotNull]TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, Transform parent, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, Vector3 position, Quaternion rotation, Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([JetBrains.Annotations.NotNull]TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, Transform parent = null, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, Vector3 position, Quaternion rotation, Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
				Transform parent = null, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument,
				Vector3 position, Quaternion rotation, Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
		/// or if the <see cref="Object"/> class implements the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface the arguments can be
		/// provided using the <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">Init</see> function
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([JetBrains.Annotations.NotNull] this TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Clones the original <typeparamref name="TObject"/>, <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</summary>
		/// it with the given arguments and then returns the clone.
		/// <para>
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument,
				Transform parent = null, bool instantiateInWorldSpace = false)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
		/// Arguments should either be received by the created <see cref="Object"/> during its initialization (such during the Awake event function or in the constructor)
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
		[JetBrains.Annotations.NotNull]
		public static TObject Instantiate<TObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([JetBrains.Annotations.NotNull] TObject original, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument,
				Vector3 position, Quaternion rotation, Transform parent = null)
				where TObject : Object, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG
			if(original == null)
			{
				throw new ArgumentNullException($"The {typeof(TObject).Name} you want to instantiate is null.");
			}
			#endif

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
    }
}