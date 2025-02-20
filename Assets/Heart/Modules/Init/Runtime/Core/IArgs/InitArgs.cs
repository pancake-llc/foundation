//#define DEBUG_ENABLED

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Internal;
using Sisus.Init.ValueProviders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init
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
		private readonly struct Arg<TArgument>
		{
			public static readonly ConcurrentDictionary<Type, Arg<TArgument>> arg = new(1, 1);

			public readonly TArgument argument;
			public readonly bool received;

			public Arg(TArgument argument, bool received)
			{
				this.argument = argument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, bool received)
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

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly TEighthArgument eighthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.eighthArgument = eighthArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly TEighthArgument eighthArgument;
			public readonly TNinthArgument ninthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.eighthArgument = eighthArgument;
				this.ninthArgument = ninthArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly TEighthArgument eighthArgument;
			public readonly TNinthArgument ninthArgument;
			public readonly TTenthArgument tenthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.eighthArgument = eighthArgument;
				this.ninthArgument = ninthArgument;
				this.tenthArgument = tenthArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly TEighthArgument eighthArgument;
			public readonly TNinthArgument ninthArgument;
			public readonly TTenthArgument tenthArgument;
			public readonly TEleventhArgument eleventhArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.eighthArgument = eighthArgument;
				this.ninthArgument = ninthArgument;
				this.tenthArgument = tenthArgument;
				this.eleventhArgument = eleventhArgument;
				this.received = received;
			}
		}

		private readonly struct Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			public static readonly ConcurrentDictionary<Type, Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>> args = new(1, 1);

			public readonly TFirstArgument firstArgument;
			public readonly TSecondArgument secondArgument;
			public readonly TThirdArgument thirdArgument;
			public readonly TFourthArgument fourthArgument;
			public readonly TFifthArgument fifthArgument;
			public readonly TSixthArgument sixthArgument;
			public readonly TSeventhArgument seventhArgument;
			public readonly TEighthArgument eighthArgument;
			public readonly TNinthArgument ninthArgument;
			public readonly TTenthArgument tenthArgument;
			public readonly TEleventhArgument eleventhArgument;
			public readonly TTwelfthArgument twelfthArgument;
			public readonly bool received;

			public Args(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument, bool received)
			{
				this.firstArgument = firstArgument;
				this.secondArgument = secondArgument;
				this.thirdArgument = thirdArgument;
				this.fourthArgument = fourthArgument;
				this.fifthArgument = fifthArgument;
				this.sixthArgument = sixthArgument;
				this.seventhArgument = seventhArgument;
				this.eighthArgument = eighthArgument;
				this.ninthArgument = ninthArgument;
				this.tenthArgument = tenthArgument;
				this.eleventhArgument = eleventhArgument;
				this.twelfthArgument = twelfthArgument;
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
			Arg<TArgument>.arg[typeof(TClient)] = new(argument, received:false);

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
			Args<TFirstArgument, TSecondArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, received:false);

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
			Args<TFirstArgument, TSecondArgument, TThirdArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, received:false);

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
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, received:false);

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
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, received:false);

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
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}, {typeof(TEleventhArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument}, {eleventhArgument});");
			#endif
		}

		public static void Set<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			(TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument, TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument, TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>.args[typeof(TClient)] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TClient).Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}, {typeof(TEleventhArgument).Name}, {typeof(TTwelfthArgument).Name}>({firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument}, {eleventhArgument}, {twelfthArgument});");
			#endif
		}

		/// <summary>
		/// Retrieves initialization argument that has been provided for the <see cref="client"/>.
		/// <para>
		/// The argument can be provided using the <see cref="Set{TClient, TArgument}"/> method, or by registering it as a <see cref="Service">service</see>.
		/// </para>
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for the argument it accepts,
		/// then the argument can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// or <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TArgument"> The type of the argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="argument"> The argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if an argument had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException"> Thrown if <paramref name="client"/> argument is <see langword="null"/>. </exception>
		public static bool TryGet<TClient, TArgument>(Context context, [DisallowNull] TClient client, out TArgument argument) where TClient : IArgs<TArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Arg<TArgument>.arg;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependency))
			{
				argument = dependency.argument;
				args[clientType] = new(argument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext() && Service.TryGetFor(client, out argument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TArgument).Name}>: <color=red>False</color> - args of size {args.Count} did contain an entry for the client, but ignoring because HasCustomInitArguments was true for the client.", client as Object);
					#endif

					argument = default;
					return false;
				}

				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"Argument of type {TypeUtility.ToString(typeof(TArgument))} for client {TypeUtility.ToString(typeof(TClient))} found via Service.TryGetFor: {argument}", client as Object);
				#endif

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentForAutoInit<TClient, TArgument>(client, context))
				{
					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Argument {typeof(TArgument)} of {typeof(TClient).Name} is auto-inititable. Fetching via GetAutoInitArgument.", client as Object);
					#endif

					argument = GetAutoInitArgument<TClient, TArgument>(client, 0);
					return true;
				}

				argument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{TypeUtility.ToString(clientType)}, {TypeUtility.ToString(typeof(TArgument))}>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			argument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				args[clientType] = new(firstArgument, secondArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext() && Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
				&& Service.TryGetFor(client, out firstArgument)
				&& Service.TryGetFor(client, out secondArgument)
				&& Service.TryGetFor(client, out thirdArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
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
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <typeparam name="TEighthArgument"> The type of the eighth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <param name="eighthArgument"> The eighth argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				eighthArgument = dependencies.eighthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument) && Service.TryGetFor(client, out eighthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					eighthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					eighthArgument = GetAutoInitArgument<TClient, TEighthArgument>(client, 7);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				eighthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			eighthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <typeparam name="TEighthArgument"> The type of the eighth argument. </typeparam>
		/// <typeparam name="TNinthArgument"> The type of the ninth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <param name="eighthArgument"> The eighth argument received, or default value if no stored argument was found. </param>
		/// <param name="ninthArgument"> The ninth argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				eighthArgument = dependencies.eighthArgument;
				ninthArgument = dependencies.ninthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument) && Service.TryGetFor(client, out eighthArgument)
			&& Service.TryGetFor(client, out ninthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					eighthArgument = default;
					ninthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					eighthArgument = GetAutoInitArgument<TClient, TEighthArgument>(client, 7);
					ninthArgument = GetAutoInitArgument<TClient, TNinthArgument>(client, 8);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				eighthArgument = default;
				ninthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			eighthArgument = default;
			ninthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <typeparam name="TEighthArgument"> The type of the eighth argument. </typeparam>
		/// <typeparam name="TNinthArgument"> The type of the ninth argument. </typeparam>
		/// <typeparam name="TTenthArgument"> The type of the tenth argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <param name="eighthArgument"> The eighth argument received, or default value if no stored argument was found. </param>
		/// <param name="ninthArgument"> The ninth argument received, or default value if no stored argument was found. </param>
		/// <param name="tenthArgument"> The tenth argument received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				eighthArgument = dependencies.eighthArgument;
				ninthArgument = dependencies.ninthArgument;
				tenthArgument = dependencies.tenthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument) && Service.TryGetFor(client, out eighthArgument)
			&& Service.TryGetFor(client, out ninthArgument) && Service.TryGetFor(client, out tenthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					eighthArgument = default;
					ninthArgument = default;
					tenthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					eighthArgument = GetAutoInitArgument<TClient, TEighthArgument>(client, 7);
					ninthArgument = GetAutoInitArgument<TClient, TNinthArgument>(client, 8);
					tenthArgument = GetAutoInitArgument<TClient, TTenthArgument>(client, 9);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				eighthArgument = default;
				ninthArgument = default;
				tenthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			eighthArgument = default;
			ninthArgument = default;
			tenthArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <typeparam name="TEighthArgument"> The type of the eighth argument. </typeparam>
		/// <typeparam name="TNinthArgument"> The type of the ninth argument. </typeparam>
		/// <typeparam name="TTenthArgument"> The type of the tenth argument. </typeparam>
		/// <typeparam name="TEleventhArgument"> The type of the eleventh argument. </typeparam>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <param name="eighthArgument"> The eighth argument received, or default value if no stored argument was found. </param>
		/// <param name="ninthArgument"> The ninth argument received, or default value if no stored argument was found. </param>
		/// <param name="tenthArgument"> The tenth argument received, or default value if no stored argument was found. </param>
		/// <param name="eleventhArgument"> The eleventh received, or default value if no stored argument was found. </param>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument, out TEleventhArgument eleventhArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				eighthArgument = dependencies.eighthArgument;
				ninthArgument = dependencies.ninthArgument;
				tenthArgument = dependencies.tenthArgument;
				eleventhArgument = dependencies.eleventhArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument) && Service.TryGetFor(client, out eighthArgument)
			&& Service.TryGetFor(client, out ninthArgument) && Service.TryGetFor(client, out tenthArgument)
			&& Service.TryGetFor(client, out eleventhArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					eighthArgument = default;
					ninthArgument = default;
					tenthArgument = default;
					eleventhArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					eighthArgument = GetAutoInitArgument<TClient, TEighthArgument>(client, 7);
					ninthArgument = GetAutoInitArgument<TClient, TNinthArgument>(client, 8);
					tenthArgument = GetAutoInitArgument<TClient, TTenthArgument>(client, 9);
					eleventhArgument = GetAutoInitArgument<TClient, TEleventhArgument>(client, 0);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				eighthArgument = default;
				ninthArgument = default;
				tenthArgument = default;
				eleventhArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			eighthArgument = default;
			ninthArgument = default;
			tenthArgument = default;
			eleventhArgument = default;
			return false;
		}

		/// <summary>
		/// Retrieves arguments provided for the client object using the <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> function.
		/// <para>
		/// When called in the context of the <see cref="Context.Reset">Reset</see> event in edit mode and the <typeparamref name="TClient"/> class has the
		/// the <see cref="InitOnResetAttribute">AutoInit attribute</see> or a <see cref="RequireComponent">RequireComponent attribute</see> for each argument it accepts,
		/// then the arguments can also be retrieved autonomously by this method using methods such as <see cref="GetComponent">GetComponent</see>
		/// and <see cref="FindAnyObjectByType"/>.
		/// </para>
		/// <para>
		/// The <paramref name="client"/> must implement the <see cref="IArgs{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/> interface in order for it to be used with this function.
		/// </para>
		/// </summary>
		/// <param name="context"> Initialization phase during which the method is being called. </param>
		/// <param name="client"> The object whose dependencies to retrieve. </param>
		/// <param name="firstArgument"> The first argument received, or default value if no stored argument was found. </param>
		/// <param name="secondArgument"> The second argument received, or default value if no stored argument was found. </param>
		/// <param name="thirdArgument"> The third argument received, or default value if no stored argument was found. </param>
		/// <param name="fourthArgument"> The fourth argument received, or default value if no stored argument was found. </param>
		/// <param name="fifthArgument"> The five argument received, or default value if no stored argument was found. </param>
		/// <param name="sixthArgument"> The sixth argument received, or default value if no stored argument was found. </param>
		/// <param name="seventhArgument"> The seventh argument received, or default value if no stored argument was found. </param>
		/// <param name="eighthArgument"> The eighth argument received, or default value if no stored argument was found. </param>
		/// <param name="ninthArgument"> The ninth argument received, or default value if no stored argument was found. </param>
		/// <param name="tenthArgument"> The tenth argument received, or default value if no stored argument was found. </param>
		/// <param name="eleventhArgument"> The eleventh received, or default value if no stored argument was found. </param>
		/// <param name="twelfthArgument"> The twelfth received, or default value if no stored argument was found. </param>
		/// <typeparam name="TClient"> The type of the <paramref name="client"/> object. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth argument. </typeparam>
		/// <typeparam name="TSixthArgument"> The type of the sixth argument. </typeparam>
		/// <typeparam name="TSeventhArgument"> The type of the seventh argument. </typeparam>
		/// <typeparam name="TEighthArgument"> The type of the eighth argument. </typeparam>
		/// <typeparam name="TNinthArgument"> The type of the ninth argument. </typeparam>
		/// <typeparam name="TTenthArgument"> The type of the tenth argument. </typeparam>
		/// <typeparam name="TEleventhArgument"> The type of the eleventh argument. </typeparam>
		/// <typeparam name="TTwelfthArgument"> The type of the twelfth argument. </typeparam>
		/// <returns> <see langword="true"/> if arguments had been provided for the object; otherwise, <see langword="false"/>. </returns>
		/// <exception cref="ArgumentNullException" > Thrown if client argument is null. </exception>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			(Context context, [DisallowNull] TClient client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument, out TEleventhArgument eleventhArgument, out TTwelfthArgument twelfthArgument)
				where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is Object obj ? !obj : client is null)
			{
				throw new ArgumentNullException($"The {typeof(TClient).Name} whose dependencies you are trying to get is null.", null as Exception);
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>.args;
			var clientType = client.GetType();
			if(args.TryGetValue(clientType, out var dependencies))
			{
				firstArgument = dependencies.firstArgument;
				secondArgument = dependencies.secondArgument;
				thirdArgument = dependencies.thirdArgument;
				fourthArgument = dependencies.fourthArgument;
				fifthArgument = dependencies.fifthArgument;
				sixthArgument = dependencies.sixthArgument;
				seventhArgument = dependencies.seventhArgument;
				eighthArgument = dependencies.eighthArgument;
				ninthArgument = dependencies.ninthArgument;
				tenthArgument = dependencies.tenthArgument;
				eleventhArgument = dependencies.eleventhArgument;
				twelfthArgument = dependencies.twelfthArgument;
				args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument, received:true);
				return true;
			}

			if(context.IsUnitySafeContext()
			&& Service.TryGetFor(client, out firstArgument) && Service.TryGetFor(client, out secondArgument)
			&& Service.TryGetFor(client, out thirdArgument) && Service.TryGetFor(client, out fourthArgument)
			&& Service.TryGetFor(client, out fifthArgument) && Service.TryGetFor(client, out sixthArgument)
			&& Service.TryGetFor(client, out seventhArgument) && Service.TryGetFor(client, out eighthArgument)
			&& Service.TryGetFor(client, out ninthArgument) && Service.TryGetFor(client, out tenthArgument)
			&& Service.TryGetFor(client, out eleventhArgument) && Service.TryGetFor(client, out twelfthArgument))
			{
				if(client is Component component && InitializerUtility.HasCustomInitArguments(component))
				{
					firstArgument = default;
					secondArgument = default;
					thirdArgument = default;
					fourthArgument = default;
					fifthArgument = default;
					sixthArgument = default;
					seventhArgument = default;
					eighthArgument = default;
					ninthArgument = default;
					tenthArgument = default;
					eleventhArgument = default;
					twelfthArgument = default;
					return false;
				}

				return true;
			}

			#if UNITY_EDITOR
			if(context.IsEditMode())
			{
				// When a client has an initializer attached, the initializer takes over the responsibility
				// of determining the initialization arguments -  even if the client has the InitInEditModeAttribute.
				if(TryPrepareArgumentsForAutoInit<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(client, context))
				{
					firstArgument = GetAutoInitArgument<TClient, TFirstArgument>(client, 0);
					secondArgument = GetAutoInitArgument<TClient, TSecondArgument>(client, 1);
					thirdArgument = GetAutoInitArgument<TClient, TThirdArgument>(client, 2);
					fourthArgument = GetAutoInitArgument<TClient, TFourthArgument>(client, 3);
					fifthArgument = GetAutoInitArgument<TClient, TFifthArgument>(client, 4);
					sixthArgument = GetAutoInitArgument<TClient, TSixthArgument>(client, 5);
					seventhArgument = GetAutoInitArgument<TClient, TSeventhArgument>(client, 6);
					eighthArgument = GetAutoInitArgument<TClient, TEighthArgument>(client, 7);
					ninthArgument = GetAutoInitArgument<TClient, TNinthArgument>(client, 8);
					tenthArgument = GetAutoInitArgument<TClient, TTenthArgument>(client, 9);
					eleventhArgument = GetAutoInitArgument<TClient, TEleventhArgument>(client, 0);
					twelfthArgument = GetAutoInitArgument<TClient, TTwelfthArgument>(client, 11);
					return true;
				}

				firstArgument = default;
				secondArgument = default;
				thirdArgument = default;
				fourthArgument = default;
				fifthArgument = default;
				sixthArgument = default;
				seventhArgument = default;
				eighthArgument = default;
				ninthArgument = default;
				tenthArgument = default;
				eleventhArgument = default;
				twelfthArgument = default;
				return false;
			}
			#endif

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.TryGet<{clientType.Name}, {typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}...>: <color=red>False</color> - args of size {args.Count} did not contain an entry for the client.\ndictionary contents: {string.Join(", ", args.Keys)}.", client as Object);
			#endif

			firstArgument = default;
			secondArgument = default;
			thirdArgument = default;
			fourthArgument = default;
			fifthArgument = default;
			sixthArgument = default;
			seventhArgument = default;
			eighthArgument = default;
			ninthArgument = default;
			tenthArgument = default;
			eleventhArgument = default;
			twelfthArgument = default;
			return false;
		}

		/// <summary>
		/// Acquire the service that the <see paramref="client"/> depends on and pass it to its
		/// <see cref="IInitializable{TArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TArgument>(TClient client, Context context = Context.MainThread) where TClient : IInitializable<TArgument>
		{
			if(!TryGet(context, client, out TArgument argument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TArgument>)
			{
				client.Validate(argument);
			}
			#endif

			client.Init(argument);
			return true;
		}

		/// <summary>
		/// Acquire the two services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument>(TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument>)
			{
				client.Validate(firstArgument, secondArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument);
			return true;
		}

		/// <summary>
		/// Acquire the three services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument);
			return true;
		}

		/// <summary>
		/// Acquire the four services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the five services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the six services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the seven services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument);
			return true;
		}

		/// <summary>
		/// Acquire the eight services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the nine services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the ten services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument);
			return true;
		}

		/// <summary>
		/// Acquire the eleven services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument, out TEleventhArgument eleventhArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument);
			return true;
		}

		/// <summary>
		/// Acquire the twelve services that the <see paramref="client"/> depends on and pass them to its
		/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}.Init"/> method.
		/// </summary>
		/// <param name="client">
		/// The object to initialize. It must implement <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument}"/>.
		/// </param>
		/// <param name="context"> The context from which a method is being called. <para>
		/// Many objects that implement <see cref="IInitializable"/> are only able to acquire their own dependencies
		/// when <see cref="Context.EditMode"/> or <see cref="Context.Reset"/> is used in Edit Mode. For performance and
		/// reliability reasons it is recommended to do these operations in Edit Mode only, and cache the results.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if was able to locate all dependencies and initialize the client with them; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool TryGet<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>(this TClient client, Context context = Context.MainThread) where TClient : IInitializable<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			if(!TryGet(context, client, out TFirstArgument firstArgument, out TSecondArgument secondArgument, out TThirdArgument thirdArgument, out TFourthArgument fourthArgument, out TFifthArgument fifthArgument, out TSixthArgument sixthArgument, out TSeventhArgument seventhArgument, out TEighthArgument eighthArgument, out TNinthArgument ninthArgument, out TTenthArgument tenthArgument, out TEleventhArgument eleventhArgument, out TTwelfthArgument twelfthArgument))
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(client is not MonoBehaviour<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>)
			{
				client.Validate(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			}
			#endif

			client.Init(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument);
			return true;
		}

		/// <summary>
		/// Clears argument provided for client of type <typeparamref name="TClient"/> using the <see cref="Set{TClient, TArgument}"/> function
		/// and returns a value indicating if the argument was not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose argument is cleared. </typeparam>
		/// <typeparam name="TArgument"> The type of the dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if argument for <typeparamref name="TClient"/> class was provided
		/// using <see cref="Set{TClient, TArgument}"/> but never retrieved using
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> but never retrieved using
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> but never retrieved using
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function
		/// and returns a value indicating if the arguments were not received by the client.
		/// </summary>
		/// <typeparam name="TClient"> The type of the object whose arguments are cleared. </typeparam>
		/// <typeparam name="TFirstArgument"> The type of the first argument. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second argument. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third argument. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth argument. </typeparam>
		/// <returns>
		/// <see langword="true"/> if arguments for <typeparamref name="TClient"/> class were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> but never retrieved using
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function
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
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> but never retrieved using
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
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
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> but never retrieved using
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

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		public static bool Clear<TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>()
			where TClient : IArgs<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
		{
			if(Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>.args.TryGetValue(typeof(TClient), out var dependency))
			{
				Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>.args.TryRemove(typeof(TClient), out _);
				return !dependency.received;
			}

			return false;
		}

		/// <summary>
		/// Clears dependency previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TClient, TArgument}"/> function.
		/// </summary>
		/// <typeparam name="TArgument"> The type of the dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependency is cleared. </param>
		/// <returns> <see langword="true"/> if dependencies were cleared, which means that they had been stored for the <typeparamref name="TClient"/>
		/// using the <see cref="Set{TClient, TArgument}"/> function but never retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// <returns>
		/// <see langword="true"/> if argument for <paramref name="clientType">client type</paramref> was provided
		/// using <see cref="Set{TClient, TArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException" > Thrown if the <paramref name="clientType"/> argument is <see langword="null"/>. </exception>
		public static bool Clear<TArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> function.
		/// </summary>
		/// <typeparam name="TFirstArgument"> The type of the first dependency. </typeparam>
		/// <typeparam name="TSecondArgument"> The type of the second dependency. </typeparam>
		/// <typeparam name="TThirdArgument"> The type of the third dependency. </typeparam>
		/// <typeparam name="TFourthArgument"> The type of the fourth dependency. </typeparam>
		/// <typeparam name="TFifthArgument"> The type of the fifth dependency. </typeparam>
		/// <param name="clientType"> The type of the object whose dependencies are cleared. </param>
		/// <returns>
		/// <see langword="true"/> if arguments for <paramref name="clientType">client type</paramref> were provided
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			var args = Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args;
			if(args.TryGetValue(clientType, out var dependency))
			{
				args.TryRemove(clientType, out _);
				return !dependency.received;
			}

			return false;
		}

		/// <summary>
		/// Clears dependencies previously injected for client of type <paramref name="clientType"/> using the
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> function.
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
		/// using <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/> but never retrieved using
		/// <see cref="TryGet{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool Clear<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>([DisallowNull] Type clientType)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
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
		/// <see cref="Set{TClient, TArgument}"/> function was received by the client.
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument}"/> function were received by the client.
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument}"/>
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument}"/>
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument}"/>
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
		/// <see cref="Set{TClient, TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument}"/>
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

		internal static void Set<TArgument>([DisallowNull] Type clientType, TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Arg<TArgument>.arg[clientType] = new(argument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TArgument).Name}>({clientType.Name}, {argument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument>.args[clientType] = new(firstArgument, secondArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument,
			TThirdArgument thirdArgument, TFourthArgument fourthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument, TSeventhArgument seventhArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			TSeventhArgument seventhArgument, TEighthArgument eighthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>.args[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument,
			TTenthArgument tenthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>.args[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument,
			TTenthArgument tenthArgument, TEleventhArgument eleventhArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>.args[clientType] = new Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument>(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}, {typeof(TEleventhArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument}, {eleventhArgument});");
			#endif
		}

		internal static void Set<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>
			([DisallowNull] Type clientType, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument,
			TFourthArgument fourthArgument, TFifthArgument fifthArgument, TSixthArgument sixthArgument,
			TSeventhArgument seventhArgument, TEighthArgument eighthArgument, TNinthArgument ninthArgument,
			TTenthArgument tenthArgument, TEleventhArgument eleventhArgument, TTwelfthArgument twelfthArgument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(clientType is null)
			{
				throw new ArgumentNullException(nameof(clientType));
			}
			#endif

			Args<TFirstArgument, TSecondArgument, TThirdArgument, TFourthArgument, TFifthArgument, TSixthArgument, TSeventhArgument, TEighthArgument, TNinthArgument, TTenthArgument, TEleventhArgument, TTwelfthArgument>.args[clientType] = new(firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument, seventhArgument, eighthArgument, ninthArgument, tenthArgument, eleventhArgument, twelfthArgument, received:false);

			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"InitArgs.Set<{typeof(TFirstArgument).Name}, {typeof(TSecondArgument).Name}, {typeof(TThirdArgument).Name}, {typeof(TFourthArgument).Name}, {typeof(TFifthArgument).Name}, {typeof(TSixthArgument).Name}, {typeof(TSeventhArgument).Name}, {typeof(TEighthArgument).Name}, {typeof(TNinthArgument).Name}, {typeof(TTenthArgument).Name}, {typeof(TEleventhArgument).Name}, {typeof(TTwelfthArgument).Name}>({clientType.Name}, {firstArgument}, {secondArgument}, {thirdArgument}, {fourthArgument}, {fifthArgument}, {sixthArgument}, {seventhArgument}, {eighthArgument}, {ninthArgument}, {tenthArgument}, {eleventhArgument}, {twelfthArgument});");
			#endif
		}
	}
}