using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// The exception that is thrown when the initialization of a service has failed.
	/// </summary>
	public class ServiceInitFailedException : InitArgsException
	{
		internal ServiceInfo ServiceInfo { get; }
		public ServiceInitFailReason Reason { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceInitFailedException"/> class.
		/// </summary>
		/// <param name="reason"> Reason for the exception. </param>
		/// <param name="message"> The error message that explains the reason for the exception. </param>
		/// <param name="exception">
		/// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
		/// </param>
		private protected ServiceInitFailedException(ServiceInfo serviceInfo, ServiceInitFailReason reason, string message, Exception exception = null) : base(message, exception)
		{
			ServiceInfo = serviceInfo;
			Reason = reason;
		}

		private protected ServiceInitFailedException(ServiceInfo serviceInfo, ServiceInitFailReason reason, Object asset, Object sceneObject, object initializerOrWrapper) : base(GenerateMessage(serviceInfo, reason, asset, sceneObject, initializerOrWrapper, null, null))
		{
			ServiceInfo = serviceInfo;
			Reason = reason;
		}

		private protected ServiceInitFailedException(ServiceInfo serviceInfo, ServiceInitFailReason reason, Exception exception, Type concreteType = null) : base(GenerateMessage(serviceInfo, reason, null, null, null, null, exception), exception)
		{
			ServiceInfo = serviceInfo;
			Reason = reason;
		}

		private protected ServiceInitFailedException(ServiceInfo serviceInfo, ServiceInitFailReason reason, Object asset, Object sceneObject, object initializerOrWrapper, Type concreteType, Exception exception) : base(GenerateMessage(serviceInfo, reason, asset, sceneObject, initializerOrWrapper, concreteType, exception), exception, sceneObject ? sceneObject : asset ? asset : initializerOrWrapper is Object obj && obj ? obj :
		#if UNITY_EDITOR
		Find.Script(serviceInfo.ConcreteOrDefiningType)
		#else
		default
		#endif
		)
		{
			ServiceInfo = serviceInfo;
			Reason = reason;
		}

		internal static InitArgsException Create([DisallowNull] ServiceInfo serviceInfo, ServiceInitFailReason reason, Exception exception = null)
		 => Create(serviceInfo, reason, null, null, null, exception);

		internal static InitArgsException Create([DisallowNull] ServiceInfo serviceInfo, ServiceInitFailReason reason, Object asset, Object sceneObject = null, object initializerOrWrapper = null, Exception exception = null, Type concreteType = null, Type missingDependencyType = null, LocalServices localServices = null)
		{
			InitArgsException result;
			var dependencyChain = new List<ServiceInfo>();
			if(reason is ServiceInitFailReason.CircularDependencies)
			{
				#if DEV_MODE
				bool success =
				#endif
					TryGetCircularDependencyChain(dependencyChain, serviceInfo);

				result = new CircularDependenciesException(serviceInfo, dependencyChain, exception);

				#if DEV_MODE
				if(!success) { Debug.LogWarning($"Failed to get circular dependency chain for {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)}"); }
				#endif
			}
			else if(reason is ServiceInitFailReason.MissingDependency)
			{
				if(TryGetCircularDependencyChain(dependencyChain, serviceInfo))
				{
					result = new CircularDependenciesException(serviceInfo, dependencyChain, exception);
				}
				else
				{
					result = MissingInitArgumentsException.ForService(concreteType ?? serviceInfo.ConcreteOrDefiningType, missingDependencyType, localServices);
				}
			}
			else
			{
				result = new ServiceInitFailedException(serviceInfo, reason, asset, sceneObject, initializerOrWrapper, concreteType, exception);
			}

			Service.HandleInitializationFailed(result, serviceInfo, reason, asset, sceneObject, initializerOrWrapper, concreteType);
			return result;
		}

		private static bool TryGetCircularDependencyChain(List<ServiceInfo> dependencyChain, ServiceInfo currentServiceInfo)
		{
			bool usesConstructorInjection = !currentServiceInfo.FindFromScene
				&& (!currentServiceInfo.ShouldInstantiate(true)
				|| (string.IsNullOrEmpty(currentServiceInfo.ResourcePath)
				#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
				&& string.IsNullOrEmpty(currentServiceInfo.AddressableKey)
				#endif
				));

			if(!usesConstructorInjection)
			{
				return false;
			}

			if(dependencyChain.Contains(currentServiceInfo))
			{
				return true;
			}

			int index = dependencyChain.Count;
			dependencyChain.Add(currentServiceInfo);

			foreach(var parameterTypes in ServiceInjector.GetParameterTypesForAllInitMethods(currentServiceInfo.concreteType))
			{
				foreach(var parameterType in parameterTypes)
				{
					if(ServiceInjector.TryGetServiceInfo(parameterType, out var nextServiceInfo) && TryGetCircularDependencyChain(dependencyChain, nextServiceInfo))
					{
						return true;
					}
				}
			}

			dependencyChain.RemoveAt(index);
			return false;
		}

		private static string GenerateMessage([DisallowNull] ServiceInfo serviceInfo, ServiceInitFailReason reason, [AllowNull] Object asset, [AllowNull] Object sceneObject, object initializerOrWrapper, [AllowNull] Type concreteType, Exception exception)
		{
			var sb = new StringBuilder();
			sb.Append("Service Init Failed: ");

			concreteType ??= serviceInfo.concreteType;

			// Disable non-exhaustive switch expression warnings, so that switch expressions that only handle all the members
			// defined in an enumeration type can be used. This way, if new members should ever get added to the enum, the
			// compiler can help make sure that all the switch expression in this class are updated accordingly.
			#pragma warning disable CS8524

			sb.Append(reason switch
			{
				ServiceInitFailReason.None => "Unable to initialize service.",
				ServiceInitFailReason.MissingSceneObject => $"{nameof(ServiceAttribute.FindFromScene)} was set to True, but service was not found from the initially active scene(s): {GetLoadedSceneNames()}.",
				#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
				ServiceInitFailReason.MissingAddressable => $"No asset was found in the Addressable registry under the address '{serviceInfo.AddressableKey}', but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.AddressableKey)} set to the aforementioned address. Either make sure an instance with the address exists in the project, or don't specify a {nameof(ServiceAttribute.AddressableKey)}, to have a new instance be created automatically instead.",
				#endif
				ServiceInitFailReason.MissingResource => $"No asset was found at the resource path 'Resources/{serviceInfo.ResourcePath}', but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.ResourcePath)} set to the aforementioned path. Either make sure an asset exists in the project at the specified path, or don't specify a {nameof(ServiceAttribute.ResourcePath)}, to have a new instance be created automatically instead.",
				ServiceInitFailReason.MissingComponent => $"No component matching all specified service defining types was found on '{(sceneObject ?? asset).name}'.",
				ServiceInitFailReason.AssetNotConvertible => $"Asset '{(sceneObject ?? asset).name}' of type {TypeUtility.ToString((sceneObject ?? asset)?.GetType())} was not convertible to all the defining types specified for the service.",
				ServiceInitFailReason.CreatingServiceProviderFailed => $"An exception occurred while trying to create service initializer {TypeUtility.ToString(serviceInfo.serviceOrProviderType)}:\n{exception}",
				ServiceInitFailReason.ServiceProviderThrewException => $"An exception was thrown by {TypeUtility.ToString(initializerOrWrapper?.GetType())}:\n{exception}",
				ServiceInitFailReason.ServiceProviderResultNull => $"Service provider {TypeUtility.ToString(initializerOrWrapper?.GetType())} returned a Null result.",
				ServiceInitFailReason.InitializerThrewException => $"An exception was thrown by {TypeUtility.ToString(initializerOrWrapper?.GetType())}:\n{exception}",
				ServiceInitFailReason.InitializerReturnedNull => $"{TypeUtility.ToString(initializerOrWrapper?.GetType())} returned a Null result.",
				ServiceInitFailReason.WrapperReturnedNull => $"{TypeUtility.ToString(initializerOrWrapper?.GetType())}.{nameof(IWrapper.WrappedObject)} was Null.",
				ServiceInitFailReason.InvalidDefiningType => $"Service '{concreteType?.Name}' is a scriptable object type but has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. Scriptable objects can not exist in scenes and as such can't be retrieved using this method.",
				ServiceInitFailReason.ScriptableObjectWithFindFromScene => $"Service '{concreteType?.Name}' is a scriptable object type but has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. Scriptable objects can not exist in scenes and as such can't be retrieved using this method.",
				ServiceInitFailReason.UnresolveableConcreteType when serviceInfo.concreteType?.IsGenericTypeDefinition ?? false => "Unable to determine closed generic type to use for creating the service instance.",
				ServiceInitFailReason.UnresolveableConcreteType => "Unable to determine concrete type to use for creating the service instance.",
				ServiceInitFailReason.ExceptionWasThrown => "An exception was thrown during service initialization.",
				ServiceInitFailReason.CircularDependencies => CircularDependenciesException.GenerateMessage(serviceInfo, new()),
				ServiceInitFailReason.MissingDependency => new MissingInitArgumentsException(concreteType)
			});

			AppendServiceInfo(sb, serviceInfo, asset:asset, sceneObject:sceneObject);
			AppendMoreHelpUrl(sb);
			return sb.ToString();
		}

		internal static void AppendServiceInfo([DisallowNull] StringBuilder sb, [DisallowNull] ServiceInfo serviceInfo, [AllowNull] Object asset = null, [AllowNull] Object sceneObject = null)
		{
			sb.Append("\n\nService Info:");

			if(serviceInfo.concreteType != null)
			{
				sb.Append("\nConcrete Type: ");
				sb.Append(TypeUtility.ToString(serviceInfo.concreteType));
				sb.Append(".");
			}

			if(serviceInfo.definingTypes.Length > 0)
			{
				sb.Append("\nDefining Types: ");
				sb.Append(string.Join(", ", serviceInfo.definingTypes.Select(t => TypeUtility.ToString(t))));
				sb.Append(".");
			}

			if(serviceInfo.serviceOrProviderType != null && serviceInfo.concreteType != serviceInfo.serviceOrProviderType)
			{
				if(ServiceAttributeUtility.definingTypes.ContainsValue(serviceInfo))
				{
					sb.Append("\nClass with [Service] attribute: ");
				}
				else
				{
					sb.Append("\nService provider type: ");
				}

				sb.Append(TypeUtility.ToString(serviceInfo.serviceOrProviderType));
				sb.Append(".");
			}

			#if UNITY_EDITOR
			if(asset)
			{
				sb.Append("\nAsset: '");
				sb.Append(AssetDatabase.GetAssetPath(asset));
				sb.Append("'.");
			}
			#endif

			if(sceneObject)
			{
				sb.Append("\nScene Object: ");
				AddHierarchyPath(sceneObject is Component component ? component.transform : ((GameObject)sceneObject).transform);
				sb.Append(".");
			}

			void AddHierarchyPath(Transform transform)
			{
				sb.Append(transform.name);
				sb.Append('\'');

				while(transform.parent)
				{
					transform = transform.parent;
					sb.Insert(0, '/');
					sb.Insert(0, transform.name);
				}

				sb.Insert(0, " > '");
				sb.Insert(0, transform.gameObject.scene.name);
			}
		}

		private static string GetLoadedSceneNames()
		{
			var sb = new StringBuilder();


			int count =
				#if UNITY_2022_2_OR_NEWER
				SceneManager.loadedSceneCount;
				#else
				SceneManager.sceneCount;
				#endif

			if(count == 0)
			{
				return "None";
			}

			sb.Append(SceneManager.GetSceneAt(0).name);

			for(int i = 1; i < count; i++)
			{
				sb.Append(", ");
				sb.Append(SceneManager.GetSceneAt(i).name);
			}

			return sb.ToString();
		}
	}
}