using System;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Methods that can be used to <see cref="Instantiate">instantiate</see> new copies <see cref="Object">Objects</see>
	/// that implement one of the <see cref="IArgs{TArgument}">IArgs</see> interfaces with the required dependencies passed
	/// to the clone's <see cref="IInitializable{TArgument}.Init">Init</see> function.
	/// </summary>
	public static class Create
	{
		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> it with the provided <paramref name="argument"/>.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TArgument}"/>
		/// <see cref="IInitializable{TArgument}.Init"/> gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TArgument}"/>
		/// <see cref="IInitializable{TArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TArgument}"/> can also manually acquire the dependency using the
		/// <see cref="InitArgs.TryGet{TClient, TArgument}"/> function during any stage of initialization,
		/// including the constructor - in which case it is even possible to assign the dependency into a readonly field.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="argument"> The object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TArgument>(TArgument argument) where TScriptableObject : ScriptableObject, IArgs<TArgument>
		{
			InitArgs.Set<TScriptableObject, TArgument>(argument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TArgument>())
			{
				return client;
			}

			if(client is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see> it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument}"/> can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument}"/> function during any stage of initialization,
		/// including the constructor - in which case it is even possible to assign the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TFirstArgument, TSecondArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> gets called
		/// at the beginning of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/> can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> function during any stage of initialization,
		/// including the constructor - in which case it is even possible to assign the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function during any stage of initialization,
		/// including the constructor - in which case it is even possible to assign the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fifthArgument"> The fifth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fifthArgument"> The fifth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="sixthArgument"> The sixth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <returns> The created <see cref="ScriptableObject"/> instance. </returns>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static TScriptableObject Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			
			var client = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return client;
			}

			if(client is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return client;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TArgument}.Init">initializes</see> it with the provided argument.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TArgument}"/>
		/// <see cref="IInitializable{TArgument}.Init"/> gets called at the beginning of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TArgument}"/>
		/// <see cref="IInitializable{TArgument}.Init"/> is called at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TArgument}"/> can also manually acquire the dependency using the
		/// <see cref="InitArgs.TryGet{TClient, TArgument}"/> function during any stage of initialization, including
		/// the constructor - in which case it is even possible to assign the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance">
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="argument"> The object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TArgument>(out TScriptableObject instance, TArgument argument) where TScriptableObject : ScriptableObject, IArgs<TArgument>
		{
			InitArgs.Set<TScriptableObject, TArgument>(argument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TArgument>())
			{
				return;
			}

			if(instance is IInitializable<TArgument> initializable)
			{
				initializable.Init(argument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance"> 
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TFirstArgument, TSecondArgument>(out TScriptableObject instance, TFirstArgument firstArgument, TSecondArgument secondArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument>(firstArgument, secondArgument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument>())
			{
				return;
			}

			if(instance is IInitializable<TFirstArgument, TSecondArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance"> 
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>(out TScriptableObject instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument>())
			{
				return;
			}

			if(instance is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance"> 
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(out TScriptableObject instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>())
			{
				return;
			}

			if(instance is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance"> 
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fifthArgument"> The fifth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(out TScriptableObject instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>())
			{
				return;
			}

			if(instance is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}

		/// <summary>
		/// Creates an instance of <see cref="ScriptableObject"/> of type <typeparamref name="TScriptableObject"/>
		/// and <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init">initializes</see>
		/// it with the provided arguments.
		/// <para>
		/// For classes that derive from <see cref="ScriptableObject{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/>
		/// gets called at the beginning
		/// of the Awake event.
		/// </para>
		/// <para>
		/// For classes that implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/> is called
		/// at the end of initialization, after Awake and OnEnable but before Start.
		/// </para>
		/// <para>
		/// Classes that implement <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// can also manually acquire the dependencies using the
		/// <see cref="InitArgs.TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// function during any stage of initialization, including the constructor - in which case it is even possible to assign
		/// the dependencies into readonly fields.
		/// </para>
		/// </summary>
		/// <typeparam name="TScriptableObject"> Type of the <see cref="ScriptableObject"/> to create. </typeparam>
		/// <param name="instance"> 
		/// When this method returns, contains the scriptable object of type <typeparamref name="TScriptableObject"/> that was created.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="firstArgument"> The first object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="secondArgument"> The second object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="thirdArgument"> The third object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fourthArgument"> The fourth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="fifthArgument"> The fifth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <param name="sixthArgument"> The sixth object that the created <see cref="ScriptableObject"/> depends on. </param>
		/// <exception cref="InitArgumentsNotReceivedException">
		/// Thrown if <typeparamref name="TScriptableObject"/> class does not implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> and did not manually handle receiving the provided arguments. 
		/// </exception>
		public static void Instance<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(out TScriptableObject instance, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument) where TScriptableObject : ScriptableObject, IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			InitArgs.Set<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			
			instance = ScriptableObject.CreateInstance<TScriptableObject>();

			if(!InitArgs.Clear<TScriptableObject, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>())
			{
				return;
			}

			if(instance is IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument> initializable)
			{
				initializable.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
				return;
			}

			throw new InitArgumentsNotReceivedException(typeof(TScriptableObject), $"{nameof(Create)}.{nameof(Instance)}<{typeof(TScriptableObject).Name}>");
		}
	}
}