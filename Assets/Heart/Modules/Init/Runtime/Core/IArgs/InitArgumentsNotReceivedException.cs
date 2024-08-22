using System;
using System.Runtime.CompilerServices;
using System.Text;
using Sisus.Init.Internal;

namespace Sisus.Init
{
	/// <summary>
	/// The exception that is thrown when arguments have been provided for an object being initialized but it fails to receive them.
	/// </summary>
	public sealed class InitArgumentsNotReceivedException : InitArgsException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
		/// </summary>
		/// <param name="methodName"> Name of the method from which the exception originates. </param>
		public InitArgumentsNotReceivedException([CallerMemberName] string methodName = null) : base(GenerateMessage(methodName, null)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
		/// </summary>
		/// <param name="clientType"> Type of the client that failed to receive the arguments. </param>
		/// /// <param name="methodName"> Name of the method from which the exception originates. </param>
		public InitArgumentsNotReceivedException(Type clientType, [CallerMemberName] string methodName = null) : base(GenerateMessage(methodName, clientType)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
		/// </summary>
		/// <param name="methodName"> Name of the method from which the exception originates. </param>
		/// <param name="client"> The client object that failed to receive the arguments. </param>
		public InitArgumentsNotReceivedException(object client, [CallerMemberName] string methodName = null) : base(GenerateMessage(methodName, client?.GetType())) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="InitArgumentsNotReceivedException"/> class.
		/// </summary>
		/// <param name="methodName"> Name of the method from which the exception originates. </param>
		/// <param name="inner">
		/// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
		/// </param>
		public InitArgumentsNotReceivedException(string methodName, Exception inner) : base(GenerateMessage(methodName, null), inner) { }

		private static string GenerateMessage(string methodName, Type clientType)
		{
			if(clientType != null)
			{
				foreach(var interfaceType in clientType.GetInterfaces())
				{
					if(!interfaceType.IsGenericType)
					{
						continue;
					}

					var genericTypeDefinition = interfaceType.IsGenericTypeDefinition ? interfaceType : interfaceType.GetGenericTypeDefinition();
					Type initializableType = null;
					if(InitializableUtility.argumentCountsByIArgsTypeDefinition.TryGetValue(genericTypeDefinition, out _))
					{
						initializableType = InitializableUtility.GetIInitializableType(interfaceType.GetGenericArguments());
						if(!initializableType.IsAssignableFrom(clientType))
						{
							var sb = new StringBuilder();
							if(!string.IsNullOrEmpty(methodName))
							{
								sb.Append(methodName);
								sb.Append(" called but ");
							}

							sb.Append(TypeUtility.ToString(clientType));
							sb.Append(" does not implement ");
							sb.Append(TypeUtility.ToString(initializableType));
							sb.Append(" and did not receive the provided arguments during initialization.");
							return sb.ToString();
						}
					}
					else if(InitializableUtility.argumentCountsByIInitializableTypeDefinition.TryGetValue(genericTypeDefinition, out _))
					{
						initializableType = interfaceType;
					}

					if(initializableType != null)
					{
						var sb = new StringBuilder();
						if(!string.IsNullOrEmpty(methodName))
						{
							sb.Append(methodName);
							sb.Append(" called for ");
							sb.Append(TypeUtility.ToString(clientType));
							sb.Append(" that implements ");
							sb.Append(TypeUtility.ToString(initializableType));
							sb.Append(" but it still somehow did not receive the provided arguments during initialization.");
						}
						else
						{
							sb.Append(TypeUtility.ToString(clientType));
							sb.Append(" implements ");
							sb.Append(TypeUtility.ToString(initializableType));
							sb.Append(" but still somehow did not receive the provided arguments during initialization.");
						}

						return sb.ToString();
					}
				}

				// TODO: Extract to a separate exception
				if(typeof(IInitializable).IsAssignableFrom(clientType))
				{
					if(!string.IsNullOrEmpty(methodName))
					{
						return $"{methodName}() called but {clientType.Name} that implements IInitializable did it failed to retrieve all the services it depends on.";
					}

					return $"{clientType.Name} does not implement IInitializable<T...> and did not receive the provided arguments during initialization.";
				}

				if(!string.IsNullOrEmpty(methodName))
				{
					return $"{methodName}() called but {clientType.Name} does not implement any IInitializable<T...> interface and did not receive the provided arguments during initialization.";
				}

				return $"{clientType.Name} does not implement any IInitializable<T...> interface and did not receive the provided arguments during initialization.";
			}
			
			if(!string.IsNullOrEmpty(methodName))
			{
				return $"{methodName}() called but client does not implement IInitializable<T...> and did not receive the provided arguments during initialization.";
			}

			return "Client does not implement IInitializable<T...> and did not receive the provided arguments during initialization.";
		}
	}
}