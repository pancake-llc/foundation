using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sisus.Init.Internal;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// The exception that is thrown when invalid arguments are passed to an object during its initialization 
	/// </summary>
	public sealed class InvalidInitArgumentsException : InitArgsException
	{
		[return: NotNull]
		public static InvalidInitArgumentsException Null(object client, Type argumentType, InvalidInitArgumentsException previousException = null)
		{
			var newException = new InvalidInitArgumentsException($"Init argument of type {TypeUtility.ToString(argumentType)} passed to {TypeUtility.ToString(client.GetType())} was null.", previousException, GetContext(client));
			return previousException is null ? newException : Aggregate(client, newException);
		}

		[return: NotNull]
		public static InvalidInitArgumentsException Invalid(object client, Type argumentType, InvalidInitArgumentsException previousException = null)
		{
			var newException = new InvalidInitArgumentsException($"Init argument of type {TypeUtility.ToString(argumentType)} passed to {TypeUtility.ToString(client.GetType())} was invalid.", previousException, GetContext(client));
			return previousException is null ? newException : Aggregate(client, newException);
		}
		
		[return: NotNull]
		public static InvalidInitArgumentsException Invalid(object client, string message, InvalidInitArgumentsException previousException = null)
		{
			var newException = new InvalidInitArgumentsException(message, previousException, GetContext(client));
			return previousException is null ? newException : Aggregate(client, newException);
		}

		/// <summary>
		/// Checks if the <paramref name="argument"/> is <see langword="null"/> and throws an <see cref="InvalidInitArgumentsException"/> if it is.
		/// <para>
		/// This method call is ignored in non-development builds.
		/// </para>
		/// </summary>
		/// <param name="client"> Client receiving the argument. </param>
		/// <param name="argument"> The argument to test. </param>
		[Conditional("DEBUG"), Conditional("INIT_ARGS_SAFE_MODE"), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowIfNull<TArgument>([DisallowNull] object client, TArgument argument)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(argument as Object ?? argument is not null)
			{
				return;
			}
			
			throw Null(client, typeof(TArgument));
			#endif
		}
		
		internal static void ValidateNotNull<TArgument>([DisallowNull] object client, [MaybeNull] TArgument argument, [MaybeNull] ref InvalidInitArgumentsException exception)
		{
			if(argument as Object ?? argument is not null)
			{
				return;
			}

			exception = Null(client, typeof(TArgument), exception);
		}
		
		[return: NotNull]
		private static InvalidInitArgumentsException Aggregate(object client, InvalidInitArgumentsException innerException) => new($"Some Init arguments passed to {TypeUtility.ToString(client.GetType())} were invalid.", innerException, GetContext(client));

		private InvalidInitArgumentsException(string message, [MaybeNull] Exception innerException, [MaybeNull] Object context) : base(message, innerException, context) { }
	}
}