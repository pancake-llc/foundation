using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sisus.Init.Internal;
using Sisus.Init.ValueProviders;
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

		internal static MissingInitArgumentsException ForService([DisallowNull] Type serviceType, Type missingDependency, LocalServices localServices = null) => new(GenerateMessage(serviceType, true, missingDependency is null ? Type.EmptyTypes : new []{ missingDependency }, localServices));
		internal static MissingInitArgumentsException ForService([DisallowNull] Type serviceType, Type[] missingDependencies, LocalServices localServices = null) => new(GenerateMessage(serviceType, true, missingDependencies, localServices));

		private static string GenerateMessage(Type clientType, bool isService, [AllowNull] Type[] missingDependencies, LocalServices localServices = null)
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
					if(missingArgumentCount is 1)
					{
						sb.Append("Service ");
						sb.Append(clientTypeName);
						sb.Append(" requires initialization argument ");
						sb.Append(missingDependencyList);

						var dependencyType = missingDependencies[0];
						if(ServiceAttributeUtility.definingTypes.TryGetValue(dependencyType, out var dependencyServiceInfo))
						{
							sb.Append(", and the dependency has the [Service] attribute, but acquiring it has still failed.");
							sb.Append("\n\n");

							if(dependencyServiceInfo.concreteType is null)
							{
								sb.Append("The reason could be that the concrete type of the service could not be resolved.\n");
								sb.Append("Try specifying the concrete type of the service in the [Service] attribute's constructor to fix the issue.");
							}
							else if(dependencyType.IsAssignableFrom(dependencyServiceInfo.concreteType))
							{
								if(dependencyServiceInfo.FindFromScene)
								{
									sb.Append("The reason could be that the dependency has been configured with [Service(FindFromScene = true)], but was not found in the active scenes.");
									sb.Append("Try starting the game from a scene that contains the dependency to fix the issue.");
								}
								#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
								else if(dependencyServiceInfo.AddressableKey is not null)
								{
									sb.Append($"The reason could be that the dependency has been configured with [Service(AddressableKey = \"{dependencyServiceInfo.AddressableKey}\")], but no such addressable asset exists.");
									sb.Append("Ensure that an asset with such a key exists in the Addressables Groups window to fix the issue.");
								}
								#endif
								else if(dependencyServiceInfo.ResourcePath is not null)
								{
									sb.Append($"The reason could be that the dependency has been configured with [Service(ResourcePath = \"{dependencyServiceInfo.ResourcePath}\")], but no such resource exists.");
									sb.Append($"Ensure that an asset of type {TypeUtility.ToString(dependencyType)} can be found in the Project at the path \"Resources/{dependencyServiceInfo.ResourcePath}\" to fix the issue.");
								}
								else
								{
									sb.Append("Potential reasons for such failure include:\n");
									sb.Append($"1. There's a cyclical dependency between {TypeUtility.ToString(dependencyType)} and another service, and both of them receive the services they depend on via their constructor.\n");
									sb.Append($"2. {TypeUtility.ToString(dependencyType)}'s constructor required services that could not be resolved.\n");
									sb.Append($"3. An exception occurred while trying to initialize {TypeUtility.ToString(dependencyType)}.");
								}
							}
							else if(dependencyServiceInfo.concreteType.IsGenericTypeDefinition)
							{
								sb.Append($"The reason could be that the concrete type of the service ({TypeUtility.ToString(dependencyServiceInfo.concreteType)}) is a generic type definition.\n");
								sb.Append("Try adding the [Service] attribute to a closed generic type to fix the issue.");
							}
							else if(dependencyServiceInfo.concreteType.IsValueType)
							{
								sb.Append("The reason could be that the concrete type of the service is a value type.\n");
								sb.Append("Try adding the [Service] attribute to a class type to fix the issue.\n");
								sb.Append($"You can use a class that implements IValueProvider<{TypeUtility.ToString(dependencyType)}> as a middleman to provide value type dependencies to clients.");
							}
							else if(typeof(IWrapper).IsAssignableFrom(dependencyServiceInfo.serviceOrProviderType))
							{
								sb.Append($"The reason could be that the Wrapper {TypeUtility.ToString(dependencyServiceInfo.serviceOrProviderType)} did not have an instance of {TypeUtility.ToString(dependencyType)} at this time.\n");
								sb.Append("Try one of the following ways to address fix issue:\n");
								sb.Append($"1. Add the [Serializable] attribute to {TypeUtility.ToString(dependencyType)} to have Unity's serialization system create the instance.\n");
								sb.Append($"2. Add a parameterless constructor to {TypeUtility.ToString(dependencyServiceInfo.serviceOrProviderType)}, create an instance of {TypeUtility.ToString(dependencyType)}, and pass it to the base({TypeUtility.ToString(dependencyType)}) constructor.\n");
								sb.Append($"3. Generate an Initializer for {TypeUtility.ToString(dependencyServiceInfo.serviceOrProviderType)} and implement the CreateWrappedObject method.");
							}
							else if(ValueProviderUtility.IsValueProvider(dependencyServiceInfo.serviceOrProviderType))
							{
								sb.Append($"The reason could be that the value provider {dependencyServiceInfo.serviceOrProviderType} failed to provide the dependency.");
							}
							else
							{
								sb.Append($"The reason could be that {TypeUtility.ToString(dependencyType)} is not assignable from {dependencyServiceInfo.concreteType} and it is is not a wrapper, an initializer, nor a value provider.");
							}
						}
						else if(localServices is not null && localServices.TryGetInfo(dependencyType, out var localServiceInfo))
						{
							sb.Append(", and the dependency has registered as a local service, but acquiring it has still failed.");
							sb.Append("\n\n");

							if(!localServiceInfo.serviceOrProvider)
							{
								sb.Append($"Reference to the service seems to be missing in the {nameof(Services)} component.\n");
								sb.Append("Try assigning a valid reference to fix the issue.");
							}
							else if(localServiceInfo.toClients != Clients.Everywhere)
							{
								sb.Append($"The availability of the service is limited to clients {localServiceInfo.toClients}.\n");
								sb.Append($"Try setting availability to {nameof(Clients.Everywhere)} to fix the issue.");
							}
							else if(dependencyType.IsInstanceOfType(localServiceInfo.serviceOrProvider))
							{
								sb.Append($"{TypeUtility.ToString(dependencyType)} is assignable from {localServiceInfo.serviceOrProvider.GetType()}.\n");
								sb.Append("The reason for the failure cannot be determined.");
							}
							else if(localServiceInfo.serviceOrProvider.GetType().IsGenericTypeDefinition)
							{
								sb.Append($"The reason could be that the concrete type of the service ({TypeUtility.ToString(localServiceInfo.serviceOrProvider.GetType())}) is a generic type definition.\n");
								sb.Append("Try adding the [Service] attribute to a closed generic type to fix the issue.");
							}
							else if(localServiceInfo.serviceOrProvider.GetType().IsValueType)
							{
								sb.Append("The reason could be that the concrete type of the service is a value type.\n");
								sb.Append("Try adding the [Service] attribute to a class type to fix the issue.\n");
								sb.Append($"You can use a class that implements IValueProvider<{TypeUtility.ToString(dependencyType)}> as a middleman to provide value type dependencies to clients.");
							}
							else if(localServiceInfo.serviceOrProvider is IWrapper)
							{
								sb.Append($"The reason could be that the Wrapper {TypeUtility.ToString(localServiceInfo.serviceOrProvider.GetType())} did not have an instance of {TypeUtility.ToString(dependencyType)} at this time.\n");
								sb.Append("Try one of the following ways to address fix issue:\n");
								sb.Append($"1. Add the [Serializable] attribute to {TypeUtility.ToString(dependencyType)} to have Unity's serialization system create the instance.\n");
								sb.Append($"2. Add a parameterless constructor to {TypeUtility.ToString(localServiceInfo.serviceOrProvider.GetType())}, create an instance of {TypeUtility.ToString(dependencyType)}, and pass it to the base({TypeUtility.ToString(dependencyType)}) constructor.\n");
								sb.Append($"3. Generate an Initializer for {localServiceInfo.serviceOrProvider.GetType()} and implement the CreateWrappedObject method.");
							}
							else if(ValueProviderUtility.IsValueProvider(localServiceInfo.serviceOrProvider.GetType()))
							{
								sb.Append($"The reason could be that the value provider {localServiceInfo.serviceOrProvider.GetType()} failed to provide the dependency.");
							}
							else
							{
								sb.Append($"The reason could be that {TypeUtility.ToString(dependencyType)} is not assignable from {localServiceInfo.serviceOrProvider.GetType()} and it is not a wrapper, an initializer, nor a value provider.");
							}
						}
						else
						{
							sb.Append(" but it was not found among registered services.");
							sb.Append("\n\n");
							sb.Append("To fix the issue, perform one of the actions listed below:\n");
							sb.Append("1. Add the [Service] attribute to the required object's class, to generate a global service from it.\n");
							sb.Append("2. Attach the Service Tag to the required object using the Inspector, and register it as a global service.\n");
							sb.Append("3. Drag-and-drop the required object into a Services component using the Inspector, and register it as a global service.\n");
							sb.Append("4. Define a Service Initializer and manually provide all the objects that the service requires.");
						}
					}
					else
					{
						sb.Append("Service ");
						sb.Append(clientTypeName);
						sb.Append(" requires initialization arguments ");
						sb.Append(missingDependencyList);
						sb.Append(" but they were not found among registered services.");
						sb.Append("\n\n");
						sb.Append("To fix the issue, perform one of the actions listed below for each of the required objects:\n");
						sb.Append("1. Add the [Service] attribute to the required objects' class, to generate a global service from it.\n");
						sb.Append("2. Attach the Service Tag to the required object using the Inspector, and register it as a global service.\n");
						sb.Append("3. Drag-and-drop the required object into a Services component using the Inspector, and register it as a global service.\n");
						sb.Append("4. Define a Service Initializer and manually provide all the objects that the service requires.");
					}
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

				if(ServiceAttributeUtility.definingTypes.TryGetValue(clientType, out var clientServiceInfo))
				{
					ServiceInitFailedException.AppendServiceInfo(sb, clientServiceInfo);
				}

				AppendMoreHelpUrl(sb);
				return sb.ToString();
			}

			if(typeof(IInitializable).IsAssignableFrom(clientType))
			{
				sb.Append(clientTypeName);
				sb.Append(" was loaded without it having access to all the services that it depends on.\n");
				sb.Append("Have all objects that the client depends on have been registered as services using the [Service] attribute, ServiceTag or Services components?");
				AppendMoreHelpUrl(sb);
				return sb.ToString();
			}

			sb.Append(clientTypeName);
			sb.Append(" was loaded without it having access to all the services that it depends on.");
			AppendMoreHelpUrl(sb);
			return sb.ToString();
		}

		private static Type[] AsArray(Type type) => type is null ? null : Array.Empty<Type>();
	}
}