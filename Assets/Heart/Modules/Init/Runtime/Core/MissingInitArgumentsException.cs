using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sisus.Init.Internal;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// The exception that is thrown when an object is initialized
	/// without all the services that it depends on having been provided to it.
	/// </summary>
	public sealed class MissingInitArgumentsException : InitArgsException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		internal MissingInitArgumentsException([DisallowNull] Type clientType, Object context = null) : base(GenerateMessage(clientType, false, Array.Empty<Type>()), context) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		/// <param name="client"> The client object that was initialized without the necessary arguments. </param>
		internal MissingInitArgumentsException([DisallowNull] object client, [DisallowNull] Type missingDependency, Object context = null) : this(client.GetType(), missingDependency, context ? context : client as Object) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		internal MissingInitArgumentsException([DisallowNull] Object client) : this(client.GetType(), client) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		internal MissingInitArgumentsException([DisallowNull] object client, Object context = null) : this(client.GetType(), context) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		/// <param name="message"> The error message that explains the reason for the exception. </param>
		internal MissingInitArgumentsException(string message, Object context = null) : base(message, context) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MissingInitArgumentsException"/> class.
		/// </summary>
		/// <param name="clientType"> Type of the client that was initialized without the necessary arguments. </param>
		private MissingInitArgumentsException([DisallowNull] Type clientType, [DisallowNull] Type missingDependency, Object context = null) : base(GenerateMessage(clientType, false, AsArray(missingDependency)), context) { }

		internal static MissingInitArgumentsException ForService([DisallowNull] Type serviceType, params Type[] missingDependencies) => new(GenerateMessage(serviceType, true, missingDependencies));

		private static string GenerateMessage(Type clientType, bool isService, [AllowNull] params Type[] missingDependencies)
		{
			var sb = new StringBuilder();

			int missingArgumentCount;
			string missingDependencyList;
			if(missingDependencies is { Length: > 0 } || InitializableUtility.TryGetParameterTypes(clientType, out missingDependencies))
			{
				missingArgumentCount = missingDependencies.Length;
				sb.Append(TypeUtility.ToString(missingDependencies[0]));
				for(int i = 1, count = missingDependencies.Length; i < count; i++)
				{
					sb.Append(", ");
					sb.Append(TypeUtility.ToString(missingDependencies[i]));
				}
				missingDependencyList = sb.ToString();
				sb.Clear();
			}
			else
			{
				missingArgumentCount = 0;
				missingDependencyList = "";
			}

			string clientTypeName = TypeUtility.ToString(clientType);

			if(missingArgumentCount > 0)
			{
				if(isService)
				{
					sb.Append("Service ");
					sb.Append(clientTypeName);
					sb.Append(" requires initialization argument");
					sb.Append(missingDependencyList);
					sb.Append(" but it was not found among registered services.");

					sb.Append("\n\n");
					sb.Append("To fix the issue, perform one of the actions listed below:\n");
					sb.Append("1. Add the [Service] attribute to the required object's class, to generate a global service from it.\n");
					sb.Append("2. Attach the Service Tag to the required object using the Inspector, and register it as a global service.\n");
					sb.Append("3. Drag-and-drop the required object into a Services component using the Inspector, and register it as a global service.\n");
					sb.Append("4. Define a Service Initializer and manually provide all the objects that the service requires.");
				}
				else
				{
					sb.Append("Client ");
					sb.Append(clientTypeName);

					if(missingArgumentCount == 1)
					{
						sb.Append(" requires initialization argument ");
						sb.Append(missingDependencyList);
						sb.Append(" but was loaded without having been provided it.");
					}
					else if(missingArgumentCount == 2)
					{
						sb.Append(" requires initialization arguments ");
						sb.Append(missingDependencyList);
						sb.Append(" but was loaded without having been provided both of them.");
					}
					else
					{
						sb.Append(" requires initialization arguments ");
						sb.Append(missingDependencyList);
						sb.Append(" but was loaded without having been provided all of them.");
					}

					sb.Append("\n\n");
					sb.Append("To fix a missing dependency, perform one of the actions listed below:\n");
					sb.Append("1. Add the [Service] attribute to a required object's class, to generate a global service from it.\n");
					sb.Append("2. Attach the Service Tag to a required object using the Inspector, to register it as a local or global service.\n");
					sb.Append("3. Drag-and-drop the required object into a Services component using the Inspector, to register it as a local or global service.\n");
					sb.Append("4. Generate an Initializer for the client using the Inspector, and then drag-and-drop the required object into it.\n");
					sb.Append("5. Manually pass the required objects using ");
						sb.Append(clientTypeName);
						sb.Append(".Instantiate<");
						sb.Append(missingDependencyList);
						sb.Append(">.\n");
					sb.Append("6. Manually pass the required objects using ");
						sb.Append("GameObject.AddComponent<");
						sb.Append(clientTypeName);
						sb.Append(", ");
						sb.Append(missingDependencyList);
						sb.Append(">.");
				}

				return sb.ToString();
			}

			if(typeof(IInitializable).IsAssignableFrom(clientType))
			{
				sb.Append(clientTypeName);
				sb.Append(" was loaded without it having access to all the services that it depends on.\n");
				sb.Append("Have all objects that the client depends on have been registered as services using the [Service] attribute, ServiceTag or Services components?");
				return sb.ToString();
			}

			sb.Append(clientTypeName);
			sb.Append(" was loaded without it having access to all the services that it depends on.");
			return sb.ToString();
		}

		private static Type[] AsArray(Type type) => type is null ? null : Array.Empty<Type>();
	}
}