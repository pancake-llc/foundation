//#define DEBUG_ENABLED

using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
	#if UNITY_EDITOR
	using static EditorOnly.AutoInitUtility;
	#endif

	/// <summary>
	/// Utility class containing methods related to providing and retrieving arguments for objects that implement
	/// one of the <see cref="IArgs{}">IArgs</see> interfaces such as <see cref="MonoBehaviour{TArgument}"/>
	/// and <see cref="ScriptableObject{TArgument}"/>.
	/// </summary>
	public static class InitArgs
    {
        private struct Arg<TArgument>
		{
			public static readonly ConcurrentDictionary<Type, Arg<TArgument>> arg
				= new ConcurrentDictionary<Type, Arg<TArgument>>(1, 1);

			public readonly TArgument argument;
			public readonly bool received;

			public Arg(TArgument argument, bool received = false)
			{
				this.argument = argument;
				this.received = received;
			}
		}

		private struct Args<TFirstArgument, TSecondArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument>> args
				= new ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument>>(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, bool received = false)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.received = received;
			}
		}

		private struct Args<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument>> args
				= new ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument>>(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, bool received = false)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.received = received;
			}
		}

		private struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>> args
				= new ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>>(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, bool received = false)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.received = received;
			}
		}

		private struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>> args
				= new ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>>(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, bool received = false)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.received = received;
			}
		}

		private struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>> args
				= new ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>>(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, bool received = false)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.received = received;
			}
		}

		/// <summary>
		/// Provides an argument for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The argument can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the argument has been received it can be cleared from the temporary argument cache using <see cref="Clear{TClient, TArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the argument. </typeparam>
		/// <typeparam name="TArgument"> The type of the argument. </typeparam>
		/// <param name="argument"> The argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TArgument>(TArgument argument) where TClient : IArgs<TArgument>
		{
			Arg<TArgument>.arg[typeof(TClient)] = new Arg<TArgument>(argument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TArgument).Name}>({argument});");
			#endif
		}

		/// <summary>
		/// Provides two arguments for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The arguments can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the arguments have been received it can be cleared from the temporary argument cache using
		/// <see cref="Clear{TClient, TFirstArgument, TSecondArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the arguments. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <param name="firstArgument"> The first argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="secondArgument"> The second argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TFirstArgument, TSecondArgument>(TFirstArgument firstArgument, TSecondArgument secondArgument)
			where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			Args<TFirstArgument, TSecondArgument>.args[typeof(TClient)] = new Args<TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}>({firstArgument}, {secondArgument});");
			#endif
		}

		/// <summary>
		/// Provides three arguments for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The arguments can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the arguments have been received it can be cleared from the temporary argument cache using
		/// <see cref="Clear{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the arguments. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <param name="firstArgument"> The first argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="secondArgument"> The second argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="thirdArgument"> The third  argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument>.args[typeof(TClient)]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument});");
			#endif
		}

		/// <summary>
		/// Provides four arguments for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The arguments can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the arguments have been received it can be cleared from the temporary argument cache using
		/// <see cref="Clear{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the arguments. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <param name="firstArgument"> The first argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="secondArgument"> The second argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="thirdArgument"> The third  argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="fourthArgument"> The fourth argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args[typeof(TClient)]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument});");
			#endif
		}

		/// <summary>
		/// Provides five arguments for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The arguments can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the arguments have been received it can be cleared from the temporary argument cache using
		/// <see cref="Clear{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the arguments. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <param name="firstArgument"> The first argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="secondArgument"> The second argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="thirdArgument"> The third  argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="fourthArgument"> The fourth argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="fifthArgument"> The fifth argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args[typeof(TClient)]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument});");
			#endif
		}

		/// <summary>
		/// Provides six arguments for use with initializing a client of type <typeparamref name="TClient"/>.
		/// <para>
		/// The arguments can be retrieved by the initializing object during any phase of initialization using the
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
		/// </para>
		/// <para>
		/// Once the arguments have been received it can be cleared from the temporary argument cache using
		/// <see cref="Clear{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
		/// </para>
		/// <para>
		/// The <typeparamref name="TClient"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the client that will receive the arguments. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <param name="firstArgument"> The first argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="secondArgument"> The second argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="thirdArgument"> The third  argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="fourthArgument"> The fourth argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="fifthArgument"> The fifth argument provided for the <typeparamref name="TClient"/>. </param>
		/// <param name="sixthArgument"> The sixth argument provided for the <typeparamref name="TClient"/>. </param>
		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args[typeof(TClient)]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument});");
			#endif
		}

		/// <summary>
		/// Retrieves an argument provided for the client object using the <see cref="Set{TArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for the argument it accepts,
		/// then the argument can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// or <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="argument"> The argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if an argument had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException"> Thrown if <paramref name="client"/> argument is <see langword="null"/>. </exception>
		public static bool TryGet<TClient, TArgument>(Context context, [NotNull] TClient client, out TArgument argument) where TClient : IArgs<TArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Arg<TArgument>.arg;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependency))
			{
				argument = dependency.argument;
				dictionary[clientType] = new Arg<TArgument>(argument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentForAutoInit<TClient, TArgument>(client))
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"Argument {typeof(TArgument)} of {clientType.Name} is auto-inititable. Fetching via GetAutoInitArgument.", client as Object);
				#endif

				argument = GetAutoInitArgument<TClient, TArgument>(client, 0);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out argument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TArgument).Name}>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			argument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TFirstArgument, TSecondArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument>
			(Context context, [NotNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The component {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Args<TFirstArgument, TSecondArgument>.args;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				dictionary[clientType] = new Args<TFirstArgument, TSecondArgument>(firstArgument, secondArgument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument>(client))
			{
				firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
				secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out firstArgument) && Service.TryGet(client, out secondArgument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(Context context, [NotNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The component {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Args<TFirstArgument, TSecondArgument, TThirdArgument>.args;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				dictionary[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument>(client))
			{
				firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
				secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
				thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out firstArgument) && Service.TryGet(client, out secondArgument) && Service.TryGet(client, out thirdArgument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(Context context, [NotNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The component {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				dictionary[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(client))
			{
				firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
				secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
				thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
				fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out firstArgument) && Service.TryGet(client, out secondArgument)
				&& Service.TryGet(client, out thirdArgument) && Service.TryGet(client, out fourthArgument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(Context context, [NotNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The component {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				dictionary[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(client))
			{
				firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
				secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
				thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
				fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
				fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out firstArgument) && Service.TryGet(client, out secondArgument)
				&& Service.TryGet(client, out thirdArgument) && Service.TryGet(client, out fourthArgument) && Service.TryGet(client, out fifthArgument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GameObject.GetComponent">GetComponent</see>
		/// and <see cref="Object.FindObjectOfType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </typeparam>
		/// <param name="client"> The object whose dependencies to retrieve. </typeparam>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </typeparam>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(Context context, [NotNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			#if DEBUG
			if(ReferenceEquals(client, null) || (client is Object obj && obj == null))
            {
				throw new ArgumentNullException($"The component {typeof(TClient).Name} whose dependencies you are trying to get is null.");
			}
			#endif

			var dictionary = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args;
			var clientType = client.GetType();
			if(dictionary.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				dictionary[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, true);
				return true;
			}

			#if UNITY_EDITOR
			if(context == Context.Reset && TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(client))
			{
				firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
				secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
				thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
				fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
				fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
				sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
				return true;
			}
			#endif

			if(context == Context.MainThread && Service.TryGet(client, out firstArgument) && Service.TryGet(client, out secondArgument)
				&& Service.TryGet(client, out thirdArgument) && Service.TryGet(client, out fourthArgument) && Service.TryGet(client, out fifthArgument) && Service.TryGet(client, out sixthArgument))
			{
				return true;
			}

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - dictionary of size {dictionary.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", dictionary.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			return false;
		}

		/// <summary>
		/// Clears argument provided for client of type <typeparamref name="TClient"/> using the <see cref="Set{TArgument}"/> function
		/// and returns a value indicating if the argument was not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose argument is cleared. </typeparam>
		/// <typeparam name="TArgument"> The type of the dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if argument for <typeparamref name="TClient"/> class was provided
		/// using <see cref="Set{TArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TArgument>()
			where TClient : IArgs<TArgument>
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Clear<{typeof(TClient).Name}, {typeof(TArgument).Name}>()");
			#endif

			if(Arg<TArgument>.arg.TryGetValue(typeof(TClient), out var dependency))
			{
				Arg<TArgument>.arg.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TFirstArgument, TSecondArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			if(Args<TFirstArgument, TSecondArgument>.args.TryGetValue(typeof(TClient), out var dependency))
            {
				Args<TFirstArgument, TSecondArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument>.args.TryGetValue(typeof(TClient), out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears arguments previously provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
            }
			return false;
		}

		/// <summary>
		/// Clears arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// function and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
            }
			return false;
		}

		/// <summary>
		/// Clears dependency previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TArgument}"/> function.
		/// </summary>
		/// <typeparam name="TArgument"> The type of the dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependency is cleared. </param>
		/// <returns> <see langword="true"/> if dependencies were cleared, which means that they had been stored for the <typeparamref name="TClient"/>
		/// using the <see cref="Set{TArgument}"/> function but never retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// <returns>
		/// <see langword="true"/> if argument for <paramref name="clientType">client type</paramref> was provided
		/// using <see cref="Set{TArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException" > Thrown if the <paramref name="clientType"/> argument is <see langword="null"/>. </exception>
		public static bool Clear<TArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Clear<{typeof(TArgument).Name}>({clientType.Name});");
			#endif

			if(Arg<TArgument>.arg.TryGetValue(clientType, out var dependency))
            {
				Arg<TArgument>.arg.TryRemove(clientType, out _);
				return !dependency.received;
            }
			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			if(Args<TFirstArgument, TSecondArgument>.args.TryGetValue(clientType, out var dependency))
            {
				Args<TFirstArgument, TSecondArgument>.args.TryRemove(clientType, out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			if(Args<TFirstArgument, TSecondArgument, TThirdArgument>.args.TryGetValue(clientType, out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument>.args.TryRemove(clientType, out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args.TryGetValue(clientType, out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args.TryRemove(clientType, out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth dependency. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args.TryGetValue(clientType, out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args.TryRemove(clientType, out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth dependency. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth dependency. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([NotNull] Type clientType)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args.TryGetValue(clientType, out var dependency))
            {
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args.TryRemove(clientType, out _);
				return !dependency.received;
			}
			return false;
		}

		/// <summary>
		/// Returns a value indicating if argument provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TArgument}"/> function was received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the argument was provided. </typeparam>
		/// <typeparam name="TArgument"> The type of the argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TArgument>(TClient client)
			where TClient : IArgs<TArgument>
		{
			return Arg<TArgument>.arg.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		/// <summary>
		/// Returns a value indicating if arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument}"/> function were received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the arguments were provided. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TFirstArgument, TSecondArgument>(TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			return Args<TFirstArgument, TSecondArgument>.args.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		/// <summary>
		/// Returns a value indicating if arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument}"/>
		/// function were received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the arguments were provided. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TFirstArgument, TSecondArgument, TThirdArgument>(TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			return Args<TFirstArgument, TSecondArgument, TThirdArgument>.args.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		/// <summary>
		/// Returns a value indicating if arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
		/// function were received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the arguments were provided. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			return Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		/// <summary>
		/// Returns a value indicating if arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
		/// function were received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the arguments were provided. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			return Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		/// <summary>
		/// Returns a value indicating if arguments provided for client of type <typeparamref name="TClient"/> using the
		/// <see cref="Set{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
		/// function were received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object to which the arguments were provided. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> were retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Received<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(TClient client)
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			return Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args.TryGetValue(client.GetType(), out var dependency) && dependency.received;
		}

		internal static void Set<TArgument>([NotNull] Type clientType, TArgument argument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Arg<TArgument>.arg[clientType] = new Arg<TArgument>(argument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TArgument).Name}>({clientType.Name}, {argument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument>
			([NotNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Args<TFirstArgument, TSecondArgument>.args[clientType] = new Args<TFirstArgument, TSecondArgument>(firstArgument, secondArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument>
			([NotNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument>.args[clientType]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument>(firstArgument, secondArgument, thirdArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([NotNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args[clientType]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([NotNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args[clientType]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([NotNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
		{
			#if DEBUG
			if(clientType is null)
            {
				throw new ArgumentNullException($"The provided {nameof(clientType)} argument was null.");
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args[clientType]
				= new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument});");
			#endif
		}
	}
}