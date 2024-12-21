/*
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using Object = UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

namespace Sisus.Init.Internal
{
	public static class ServiceInitUtility
	{
		public sealed class InitFailResult
		{
			public readonly InitFailReason failReason;
			public readonly Exception exception;
			public readonly Type requestedType;
			public readonly Type concreteType;
			public readonly Type missingDependencyType;

			internal readonly ServiceInfo globalServiceInfo;
			internal readonly LocalServiceInfo localServiceInfo;
			internal readonly Object asset;
			internal readonly Object sceneObject;
			internal readonly object initializerOrWrapper;
			internal readonly LocalServices localServices;

			internal ServiceInitFailReason ServiceInitFailReason => (ServiceInitFailReason)(int)failReason;

			internal InitFailResult(InitFailReason failReason, Type requestedType, Type concreteType = null, Type missingDependencyType = null, Exception exception = null, ServiceInfo globalServiceInfo = null, LocalServiceInfo localServiceInfo = null, Object asset = null, Object sceneObject = null, object initializerOrWrapper = null, LocalServices localServices = null)
			{
				this.failReason = failReason;
				this.requestedType = requestedType;
				this.exception = exception;
				this.concreteType = concreteType;
				this.missingDependencyType = missingDependencyType;
				this.globalServiceInfo = globalServiceInfo;
				this.localServiceInfo = localServiceInfo;
				this.asset = asset;
				this.sceneObject = sceneObject;
				this.initializerOrWrapper = initializerOrWrapper;
				this.localServices = localServices;
			}
		}

		public readonly struct InitResult
		{
			public readonly object result;
			public bool IsSuccess => result is not InitFailResult;
			public InitFailResult failure => result as InitFailResult;

			internal InitResult(object service) => result = service;
			private InitResult(InitFailResult failure) => result = failure;

			internal static InitResult Failure(InitFailReason reason, [DisallowNull] Type requestedType, Type concreteType, Type missingDependencyType = null, Exception exception = null, ServiceInfo globalServiceInfo = null, LocalServiceInfo localServiceInfo = null, Object asset = null, Object sceneObject = null, object initializerOrWrapper = null, LocalServices localServices = null)
			{
				if(globalServiceInfo is null)
				{
					if (concreteType is null)
					{
						ServiceAttributeUtility.definingTypes.TryGetValue(requestedType, out globalServiceInfo);
					}
					else if(!ServiceAttributeUtility.concreteTypes.TryGetValue(concreteType, out globalServiceInfo))
					{
						ServiceAttributeUtility.definingTypes.TryGetValue(requestedType, out globalServiceInfo);
					}
				}

				if(localServiceInfo is null)
				{
					if(concreteType is null)
					{
						localServices.TryGetInfo(requestedType, out localServiceInfo);
					}
					else if(!localServices.TryGetInfo(concreteType, out localServiceInfo))
					{
						localServices.TryGetInfo(requestedType, out localServiceInfo);
					}
				}

				return new(new InitFailResult(reason, requestedType, concreteType, missingDependencyType, exception, globalServiceInfo, localServiceInfo, asset, sceneObject, initializerOrWrapper, localServices));
			}

			public static implicit operator bool(InitResult result) => result.IsSuccess;
			public static implicit operator InitFailReason(InitResult result) => result.failure.failReason;
		}

		private static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable
		#else
		System.Threading.Tasks.Task
		#endif
		<InitResult> CreateAsync(IServiceInitializer serviceInitializer, Type concreteType, Type initializerType, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(initialized.Contains(concreteType))
			{
				return new(InitFailReason.CircularDependencies);
			}

			object result;

			var interfaceTypes = initializerType.GetInterfaces();
			int parameterCount = 0;
			for(int interfaceIndex = interfaceTypes.Length - 1; interfaceIndex >= 0; interfaceIndex--)
			{
				var interfaceType = interfaceTypes[interfaceIndex];
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var typeDefinition = interfaceType.GetGenericTypeDefinition();
				if(!argumentCountsByIServiceInitializerTypeDefinition.TryGetValue(typeDefinition, out parameterCount))
				{
					continue;
				}

				initialized.Add(concreteType);

				var parameterTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
				var arguments = new object[parameterCount];
				int failedToGetArgumentAtIndex = await ServiceInjector.GetOrInitializeServices(parameterTypes, initialized, localServices, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					return InitResult.Failure(InitFailReason.MissingDependency, requestedType:concreteType, concreteType:concreteType, missingDependencyType:parameterTypes[failedToGetArgumentAtIndex], initializerOrWrapper:serviceInitializer, localServices:localServices);
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif
					result = serviceInitializer.InitTarget(arguments);

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception exception)
				{
					return InitResult.Failure(InitFailReason.ServiceInitializerThrewException, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:exception, initializerOrWrapper:serviceInitializer);
				}
				#endif

				if(serviceInitializer is IAsyncDisposable asyncDisposable)
				{
					_ = asyncDisposable.DisposeAsync();
				}
				else if(serviceInitializer is IDisposable disposable)
				{
					disposable.Dispose();
				}

				// NOTE: Service Initializer returning null is a supported use case; it communicates that the framework should create the instance instead.
				if(result is null)
				{
					// Create an instance of the object - but make sure to not try to use a service initializer, to avoid infinite loops.
					return new(CreateAsync(concreteType:concreteType, initializerType:concreteType, initialized:initialized, localServices:localServices));
				}


				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via service initializer {serviceInitializer.GetType().Name} successfully.");
				#endif

				return new(result);
			}

			try
			{
				initialized.Add(concreteType);
				result = serviceInitializer.InitTarget(Array.Empty<object>());
			}
			catch(Exception ex)
			{
				return InitResult.Failure(InitFailReason.ServiceInitializerThrewException, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:ex, initializerOrWrapper:serviceInitializer);
			}

			return result switch
			{
				// NOTE: Service Initializer returning null is a supported use case; it communicates that the framework should create the instance instead.
				null => new(CreateAsync(concreteType, concreteType, initialized, localServices)),
				Task task => new(await task.GetResult()),
				_ => new(result)
			};
		}

		private static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable
		#else
		System.Threading.Tasks.Task
		#endif
		<InitResult> CreateAsync(IServiceInitializerAsync serviceInitializerAsync, Type concreteType, Type initializerType, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(initialized.Contains(concreteType))
			{
				return new(InitFailReason.CircularDependencies);
			}

			object result;

			var interfaceTypes = initializerType.GetInterfaces();
			int parameterCount = 0;
			for(int interfaceIndex = interfaceTypes.Length - 1; interfaceIndex >= 0; interfaceIndex--)
			{
				var interfaceType = interfaceTypes[interfaceIndex];
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var typeDefinition = interfaceType.GetGenericTypeDefinition();
				if(!argumentCountsByIServiceInitializerTypeDefinition.TryGetValue(typeDefinition, out parameterCount))
				{
					continue;
				}

				initialized.Add(concreteType);

				var parameterTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
				var arguments = new object[parameterCount];
				int failedToGetArgumentAtIndex = await ServiceInjector.GetOrInitializeServices(parameterTypes, initialized, localServices, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					return InitResult.Failure(InitFailReason.MissingDependency, requestedType:concreteType, concreteType:concreteType, missingDependencyType:parameterTypes[failedToGetArgumentAtIndex], initializerOrWrapper:serviceInitializerAsync, localServices:localServices);
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif
					result = serviceInitializerAsync.InitTargetAsync(arguments);

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception exception)
				{
					return InitResult.Failure(InitFailReason.ServiceInitializerThrewException, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:exception, initializerOrWrapper:serviceInitializerAsync);
				}
				#endif

				if(serviceInitializerAsync is IAsyncDisposable asyncDisposable)
				{
					_ = asyncDisposable.DisposeAsync();
				}
				else if(serviceInitializerAsync is IDisposable disposable)
				{
					disposable.Dispose();
				}

				// NOTE: Service Initializer returning null is a supported use case; it communicates that the framework should create the instance instead.
				if(result is null)
				{
					// Create an instance of the object - but make sure to not try to use a service initializer, to avoid infinite loops.
					return new(CreateAsync(concreteType:concreteType, initializerType:concreteType, initialized:initialized, localServices:localServices));
				}

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via async service initializer {serviceInitializerAsync.GetType().Name} successfully.");
				#endif

				return new(result);
			}

			try
			{
				initialized.Add(concreteType);
				result = serviceInitializerAsync.InitTargetAsync(Array.Empty<object>());
			}
			catch(Exception ex)
			{
				return InitResult.Failure(InitFailReason.ServiceInitializerThrewException, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:ex, initializerOrWrapper:serviceInitializerAsync);
			}

			return result switch
			{
				// NOTE: Service Initializer returning null is a supported use case; it communicates that the framework should create the instance instead.
				null => new(CreateAsync(concreteType, concreteType, initialized, localServices)),
				Task task => new(await task.GetResult()),
				_ => new(result)
			};
		}

		private static object GetOrCreate(Type targetType, ReferenceType referenceType, LoadMethod loadMethod, [AllowNull] string path) => referenceType switch
		{
			//ReferenceType.DirectReference => throw new NotSupportedException(),
			ReferenceType.ResourcePath when loadMethod is LoadMethod.Load => LoadFromResources(path, targetType),
			ReferenceType.ResourcePath => InstantiateResource(path, targetType),
			ReferenceType.AddressableKey when loadMethod is LoadMethod.Load => LoadFromAddressables(path, targetType),
			ReferenceType.AddressableKey => InstantiateAddressable(path, targetType),
			ReferenceType.AddressableKey => throw new NotImplementedException(),
			ReferenceType.None when loadMethod is LoadMethod.FindFromScene => throw new NotImplementedException(),
			_ => CreateAsync(targetType)
		};

		private static object LoadResource(Type targetType, string resourcePath) => throw new NotImplementedException();
		private static object InstantiateResource(Type targetType, string resourcePath) => Instantiate(LoadResource(resourcePath, targetType));
		private static object Instantiate(object prefab) => throw new NotImplementedException();
		private static object LoadAddressable(Type targetType, string addressableKey) => throw new NotImplementedException();
		private static object InstantiateAddressable(Type targetType, string addressableKey) => Instantiate(LoadAddressable(resourcePath, targetType));

		private static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<InitResult>
		#else
		System.Threading.Tasks.Task<InitResult>
		#endif
		CreateUsingServiceInitializerAsync([DisallowNull] Type serviceInitializerType, [DisallowNull] Type instanceType, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(typeof(IServiceInitializer).IsAssignableFrom(serviceInitializerType))
			{
				if(initialized.Contains(instanceType))
				{
					#if DEV_MODE
					Debug.LogWarning($"initialized.Contains({TypeUtility.ToString(instanceType)})");
					#endif
					return InitResult.Failure(InitFailReason.CircularDependencies, concreteType:instanceType, requestedType:instanceType, localServices:localServices);
				}

				IServiceInitializer serviceInitializer;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

				// TODO: Support other methods of creation, like Resources.Load, so can assign dependencies via Inspector!
				serviceInitializer = Activator.CreateInstance(serviceInitializerType) as IServiceInitializer;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					return InitResult.Failure(InitFailReason.CreatingServiceInitializerFailed, concreteType:instanceType, requestedType:instanceType, localServices:localServices, exception:ex);
				}
				#endif

				return new(await CreateAsync(serviceInitializer:serviceInitializer, concreteType:instanceType, initializerType:serviceInitializerType, initialized:initialized, localServices:localServices));
			}

			if(typeof(IServiceInitializerAsync).IsAssignableFrom(serviceInitializerType))
			{
				if(initialized.Contains(instanceType))
				{
					#if DEV_MODE
					Debug.LogWarning($"initialized.Contains({TypeUtility.ToString(instanceType)})");
					#endif
					return InitResult.Failure(InitFailReason.CircularDependencies, concreteType:instanceType, requestedType:instanceType, localServices:localServices);
				}

				IServiceInitializerAsync serviceInitializerAsync;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					// TODO: Support other methods of creation, like Resources.Load, so can assign dependencies via Inspector!
					serviceInitializerAsync = Activator.CreateInstance(serviceInitializerType) as IServiceInitializerAsync;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					return InitResult.Failure(InitFailReason.CreatingServiceInitializerFailed, concreteType:instanceType, requestedType:instanceType, localServices:localServices, exception:ex);
				}
				#endif

				return new(await CreateAsync(serviceInitializerAsync:serviceInitializerAsync, concreteType:instanceType, initializerType:serviceInitializerType, initialized:initialized, localServices:localServices));
			}

			return InitResult.Failure(InitFailReason.CreatingServiceInitializerFailed, concreteType: instanceType, requestedType: instanceType, localServices: localServices, exception: new InvalidCastException($"Provided type {TypeUtility.ToString(serviceInitializerType)} was not assignable to {nameof(IServiceInitializer)} or {nameof(IServiceInitializerAsync)}."));
		}

		// Copy-paste of ServiceInitializer internal logic.
		// It could be that this is not needed, and can be deleted in the end.
		// Or it could be made internal and used by ServiceInjector + ServiceTag + Services to init services via the same method.
		// TODO: Move to ServiceUtility?
		internal static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<InitResult>
		#else
		System.Threading.Tasks.Task<InitResult>
		#endif
		CreateAsync([DisallowNull] ServiceInfo serviceInfo, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			switch(serviceInfo.serviceProviderType)
			{
				case ServiceProviderType.ServiceInitializer:
				case ServiceProviderType.ServiceInitializerAsync:
					return await CreateUsingServiceInitializerAsync(serviceInfo.serviceOrProviderType, serviceInfo.concreteType, initialized, localServices);
				case ServiceProviderType.Wrapper:
					return await CreateUsingWrapperAsync(serviceInfo.serviceOrProviderType, serviceInfo.concreteType, initialized, localServices);
				case ServiceProviderType.Initializer:
					return await CreateUsingInitializerAsync(serviceInfo.serviceOrProviderType, serviceInfo.concreteType, initialized, localServices);
				case ServiceProviderType.IValueProviderT:
				case ServiceProviderType.IValueProviderAsyncT:
				case ServiceProviderType.IValueByTypeProvider:
				case ServiceProviderType.IValueByTypeProviderAsync:
				case ServiceProviderType.IValueProvider:
				case ServiceProviderType.IValueProviderAsync:
					return await CreateUsingValueProviderAsync(serviceInfo.serviceOrProviderType, serviceInfo.concreteType, initialized, localServices);
			}
			object result;
			if(typeof(IServiceInitializer).IsAssignableFrom(initializerType))
			{
				if(initialized.Contains(concreteType))
				{
					#if DEV_MODE
					Debug.LogWarning($"initialized.Contains({TypeUtility.ToString(concreteType)})");
					#endif
					return InitResult.Failure(InitFailReason.CircularDependencies, concreteType:concreteType, requestedType:concreteType, localServices:localServices);
				}

				IServiceInitializer serviceInitializer;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

				// TODO: Support other methods of creation, like Resources.Load, so can assign dependencies via Inspector!
				serviceInitializer = Activator.CreateInstance(initializerType) as IServiceInitializer;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					return InitResult.Failure(InitFailReason.CreatingServiceInitializerFailed, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:ex);
				}
				#endif

				return new(await CreateAsync(serviceInitializer:serviceInitializer, concreteType:concreteType, initializerType:initializerType, initialized:initialized, localServices:localServices));
			}

			if(typeof(IServiceInitializerAsync).IsAssignableFrom(initializerType))
			{
				if(initialized.Contains(concreteType))
				{
					#if DEV_MODE
					Debug.LogWarning($"initialized.Contains({TypeUtility.ToString(concreteType)})");
					#endif
					return InitResult.Failure(InitFailReason.CircularDependencies, concreteType:concreteType, requestedType:concreteType, localServices:localServices);
				}

				IServiceInitializerAsync serviceInitializerAsync;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					// TODO: Support other methods of creation, like Resources.Load, so can assign dependencies via Inspector!
					serviceInitializerAsync = Activator.CreateInstance(initializerType) as IServiceInitializerAsync;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					return InitResult.Failure(InitFailReason.CreatingServiceInitializerFailed, concreteType:concreteType, requestedType:concreteType, localServices:localServices, exception:ex);
				}
				#endif

				return new(await CreateAsync(serviceInitializerAsync:serviceInitializerAsync, concreteType:concreteType, initializerType:initializerType, initialized:initialized, localServices:localServices));
			}

			if(serviceInfo.FindFromScene)
			{
				if(localServices.TryGet(null, concreteType, out result))
				{
					return result;
				}

				foreach(var definingType in serviceInfo.definingTypes)
				{
					if(localServices.TryGet(null, definingType, out result))
					{
						return result;
					}
				}

				if(typeof(Component).IsAssignableFrom(concreteType))
				{
					result = 
					#if UNITY_2023_1_OR_NEWER
					Object.FindAnyObjectByType(concreteType, FindObjectsInactive.Include);
					#else
					Object.FindObjectOfType(concreteType, true);
					#endif
				}
				else if(typeof(Component).IsAssignableFrom(serviceInfo.serviceOrProviderType))
				{
					result = 
					#if UNITY_2023_1_OR_NEWER
					Object.FindAnyObjectByType(serviceInfo.serviceOrProviderType, FindObjectsInactive.Include);
					#else
					Object.FindObjectOfType(serviceInfo.classWithAttribute, true);
					#endif
				}
				else if(concreteType.IsInterface)
				{
					result = Find.Any(concreteType, true);
				}
				else if(serviceInfo.serviceOrProviderType.IsInterface)
				{
					result = Find.Any(serviceInfo.serviceOrProviderType, true);
				}
				else
				{
					result = null;
				}

				if(result is not null)
				{
					if(!IsInstanceOf(serviceInfo, result)
						&& result is IInitializer initializerWithAttribute
						&& TargetIsAssignableOrConvertibleToType(initializerWithAttribute, serviceInfo))
					{
						result = await initializerWithAttribute.InitTargetAsync();
						if(IsInstanceOf(serviceInfo, result))
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {TypeUtility.ToString(result.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
							#endif
							return result;
						}
					}

					return result;

				}

				if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
				{
					throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ScriptableObjectWithFindFromScene, null, null, null, null, null, null, localServices);
				}

				#if UNITY_EDITOR
				if(!IsFirstSceneInBuildSettingsLoaded()) { return null; }
				#endif

				Debug.LogWarning($"Service Not Found: There is no '{concreteType.Name}' found in the active scene {SceneManager.GetActiveScene().name}, but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. Either add an instance to the scene or don't set {nameof(ServiceAttribute.FindFromScene)} true to have a new instance be created automatically.");
				return null;
			}

			if(serviceInfo.ResourcePath is string resourcePath)
			{
				return await CreateFromResourceAsync(concreteType, initialized, localServices, resourcePath);
			}

			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			if(serviceInfo.AddressableKey is string addressableKey)
			{
				return await InitializeAddressableAsset(addressableKey, serviceInfo, initialized, localServices);
			}
			#endif

			if(typeof(Component).IsAssignableFrom(concreteType))
			{
				if(!container)
				{
					CreateServicesContainer();
				}

				if(ShouldInitialize(true))
				{
					result = await AddComponent(serviceInfo, initialized, localServices);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif
				}
				else
				{
					result = container.AddComponent(concreteType);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(concreteType)} attached to '{container.name}' successfully.", container);
					#endif
				}

				return result;
			}

			if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
			{
				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created successfully.");
				#endif

				return ScriptableObject.CreateInstance(concreteType);
			}

			if(initialized.Contains(concreteType))
			{
				throw ServiceInitFailedException.Create(serviceInfo, InitFailReason.CircularDependencies, null, null, null, null, null, null, servicesInScene);
			}

			// TODO: Support value providers that is located using other methods like FindFromScene, Addressables, etc.
			if(!concreteType.IsAssignableFrom(initializerType) && typeof(IValueProvider).IsAssignableFrom(initializerType))
			{

			}

			var constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if(constructors.Length == 0)
			{
				constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			}

			IEnumerable<ConstructorInfo> constructorsByParameterCount = constructors.Length <= 1 ? constructors : constructors.OrderByDescending(c => c.GetParameters().Length);
			foreach(var constructor in constructorsByParameterCount)
			{
				var parameters = constructor.GetParameters();
				int parameterCount = parameters.Length;
				if(parameterCount == 0)
				{
					continue;
				}

				initialized.Add(concreteType);

				object[] arguments = new object[parameterCount];
				bool allArgumentsAvailable = true;

				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parameters[i].ParameterType;
					if(!TryGetOrInitializeService(parameterType, out arguments[i], initialized, localServices))
					{
						//LogMissingDependencyWarning(concreteType, parameterType);
						//LogMissingDependencyWarning(concreteType, parameterType, null, servicesInScene);
						ServiceInitFailedException.Create(serviceInfo, InitFailReason.MissingDependency, null, null, null, null, null, parameterType, servicesInScene);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					initialized.Remove(concreteType);
					continue;
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameters[parameterIndex].ParameterType;
					var argument = arguments[parameterIndex];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						#if DEV_MODE || DEBUG
						try
						{
						#endif

						await loadArgumentTask;

						#if DEV_MODE || DEBUG
						}
						catch(Exception exception)
						{
							if(TryFindContainedExceptionForService(exception, serviceInfo, out var exceptionForService))
							{
								// Intentionally using "throw exceptionForService" instead of just "throw" to remove bloat from the stack trace.
								throw exceptionForService;
							}

							var resultType = exception is CircularDependenciesException ? InitFailReason.CircularDependencies : InitFailReason.MissingDependency;
							throw CreateAggregateException(exception, ServiceInitFailedException.Create(serviceInfo, resultType, asset:null, missingDependencyType:parameterType, servicesInScene:servicesInScene));
						}
						#endif

						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				for(int i = 0; i < parameterCount; i++)
				{
					arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, localServices);
				}

				result = constructor.Invoke(arguments);

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via constructor {constructor} successfully.");
				#endif

				return result is Task task ? await task.GetResult() : result;
			}

			if(!Array.Exists(constructors, c => c.GetParameters().Length == 0))
			{
				throw MissingInitArgumentsException.ForService(concreteType, constructorsByParameterCount.FirstOrDefault()?.GetParameters().Select(p => p.ParameterType).ToArray(), servicesInScene);
			}

			result = Activator.CreateInstance(concreteType);

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {concreteType} created successfully via default constructor.");
			#endif

			return result;
		}

		// TODO: Split to
		// - Instantiate/Load
		// - Immediate/Async
		private static async Task<InitResult> CreateFromResourceAsync(Type concreteType, HashSet<Type> initialized, LocalServices localServices, string resourcePath)
		{
			InitResult initResult;
			Object asset;
			if(serviceInfo.LoadAsync)
			{
				ResourceRequest resourceRequest = Resources.LoadAsync<Object>(resourcePath);
					#if UNITY_2023_2_OR_NEWER
				await resourceRequest;
					#else
					while(!resourceRequest.isDone)
					{
						@await Task.Yield();
					}
					#endif

				asset = resourceRequest.asset;
			}
			else
			{
				asset = Resources.Load<Object>(resourcePath);
			}

				#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!asset)
			{
				throw ServiceInitFailedException.Create(serviceInfo, InitFailReason.MissingResource, null, null, null, null, null, null, servicesInScene);
			}
				#endif

			if(asset is GameObject gameObject)
			{
				if(serviceInfo.ShouldInstantiate(true))
				{
					result = await InstantiateFromAsset(gameObject, serviceInfo, initialized, localServices);

						#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, InitFailReason.MissingComponent, asset, null, null, null, null, null, servicesInScene);
					}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(concreteType)} instantiated from prefab at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
				}
				else
				{
					result = await GetServiceFromInstance(gameObject, serviceInfo);

						#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(concreteType)} was found on the resource at path 'Resources/{resourcePath}'.", asset);
						{
							initResult = null;
							return null;
						}
					}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(concreteType)} loaded from prefab at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
				}
			}
			else if(asset is ScriptableObject scriptableObject)
			{
				if(serviceInfo.ShouldInstantiate(false))
				{
					result = await InstantiateFromAsset(scriptableObject, serviceInfo, initialized, localServices);

						#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(concreteType)} was found on the clone created from the resource at path 'Resources/{resourcePath}'.", asset);
						{
							initResult = null;
							return null;
						}
					}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(concreteType)} instantiated from scriptable object at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
				}
				else
				{
					result = await GetServiceAsync(scriptableObject, serviceInfo);

						#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(concreteType)} was found on the resource at path 'Resources/{resourcePath}'.", asset);
						{
							initResult = null;
							return null;
						}
					}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(concreteType)} loaded from scriptable object at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
				}
			}
			else if(IsInstanceOf(serviceInfo, asset))
			{
				result = asset;

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(concreteType)} loaded from asset at path 'Resources/{resourcePath}' successfully.", asset);
					#endif
			}
			else
			{
				Debug.LogWarning($"Service Not Found: Resource at path 'Resources/{resourcePath}' could not be converted to type {serviceInfo.definingTypes.FirstOrDefault()?.Name}.", asset);
				{
					initResult = null;
					return null;
				}
			}

				#if DEBUG || INIT_ARGS_SAFE_MODE
			if(result is null)
			{
				Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(concreteType)} was found on the clone created from the resource at path 'Resources/{resourcePath}'.", asset);
				{
					initResult = null;
					return null;
				}
			}
				#endif

			initResult = result;
			return result;
		}

		private static async Task<object> CreateAsync(Type concreteType, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(typeof(Component).IsAssignableFrom(concreteType))
			{
				if(!container)
				{
					CreateServicesContainer();
				}

				if(ShouldInitialize(true))
				{
					result = await AddComponent(serviceInfo, initialized, localServices);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif
				}
				else
				{
					result = container.AddComponent(concreteType);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(concreteType)} attached to '{container.name}' successfully.", container);
					#endif
				}

				return result;
			}

			if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
			{
				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created successfully.");
				#endif

				return ScriptableObject.CreateInstance(concreteType);
			}

			if(initialized.Contains(concreteType))
			{
				throw ServiceInitFailedException.Create(serviceInfo, InitFailReason.CircularDependencies, null, null, null, null, null, null, servicesInScene);
			}

			// TODO: Support value providers that is located using other methods like FindFromScene, Addressables, etc.
			if(!concreteType.IsAssignableFrom(registererType) && typeof(IValueProvider).IsAssignableFrom(registererType))
			{

			}

			var constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if(constructors.Length == 0)
			{
				constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			}

			IEnumerable<ConstructorInfo> constructorsByParameterCount = constructors.Length <= 1 ? constructors : constructors.OrderByDescending(c => c.GetParameters().Length);
			foreach(var constructor in constructorsByParameterCount)
			{
				var parameters = constructor.GetParameters();
				int parameterCount = parameters.Length;
				if(parameterCount == 0)
				{
					continue;
				}

				initialized.Add(concreteType);

				object[] arguments = new object[parameterCount];
				bool allArgumentsAvailable = true;

				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parameters[i].ParameterType;
					if(!TryGetOrInitializeService(parameterType, out arguments[i], initialized, localServices))
					{
						//LogMissingDependencyWarning(concreteType, parameterType);
						//LogMissingDependencyWarning(concreteType, parameterType, null, servicesInScene);
						ServiceInitFailedException.Create(serviceInfo, InitFailReason.MissingDependency, null, null, null, null, null, parameterType, servicesInScene);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					initialized.Remove(concreteType);
					continue;
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameters[parameterIndex].ParameterType;
					var argument = arguments[parameterIndex];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						#if DEV_MODE || DEBUG
						try
						{
						#endif

						await loadArgumentTask;

						#if DEV_MODE || DEBUG
						}
						catch(Exception exception)
						{
							if(TryFindContainedExceptionForService(exception, serviceInfo, out var exceptionForService))
							{
								// Intentionally using "throw exceptionForService" instead of just "throw" to remove bloat from the stack trace.
								throw exceptionForService;
							}

							var resultType = exception is CircularDependenciesException ? InitFailReason.CircularDependencies : InitFailReason.MissingDependency;
							throw CreateAggregateException(exception, ServiceInitFailedException.Create(serviceInfo, resultType, asset:null, missingDependencyType:parameterType, servicesInScene:servicesInScene));
						}
						#endif

						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				for(int i = 0; i < parameterCount; i++)
				{
					arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, localServices);
				}

				result = constructor.Invoke(arguments);

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via constructor {constructor} successfully.");
				#endif

				return result is Task task ? await task.GetResult() : result;
			}

			if(!Array.Exists(constructors, c => c.GetParameters().Length == 0))
			{
				throw MissingInitArgumentsException.ForService(concreteType, constructorsByParameterCount.FirstOrDefault()?.GetParameters().Select(p => p.ParameterType).ToArray(), servicesInScene);
			}

			result = Activator.CreateInstance(concreteType);

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {concreteType} created successfully via default constructor.");
			#endif

			return result;
		}

		// Copy-paste of ServiceInitializer internal logic.
		// It could be that this is not needed, and can be deleted in the end.
		// Or it could be made internal and used by ServiceInjector + ServiceTag + Services to init services via the same method.
		// TODO: Move to ServiceUtility?
		internal static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<InitResult>
		#else
		System.Threading.Tasks.Task<InitResult>
		#endif
		CreateAsync([DisallowNull] Type concreteType, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(typeof(Component).IsAssignableFrom(concreteType))
			{
				if(!container)
				{
					CreateServicesContainer();
				}

				if(!InitializableUtility.CanSelfInitializeWithoutInitializer(concreteType))
				{
					result = await AddComponent(serviceInfo, initialized, localServices);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif
				}
				else
				{
					result = container.AddComponent(concreteType);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(concreteType)} to '{container.name}'.", container);
						return null;
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(concreteType)} attached to '{container.name}' successfully.", container);
					#endif
				}

				return result;
			}

			if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
			{
				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created successfully.");
				#endif

				return ScriptableObject.CreateInstance(concreteType);
			}

			if(initialized.Contains(concreteType))
			{
				throw ServiceInitFailedException.Create(serviceInfo, InitFailReason.CircularDependencies, null, null, null, null, null, null, servicesInScene);
			}

			// TODO: Support value providers that is located using other methods like FindFromScene, Addressables, etc.
			if(!concreteType.IsAssignableFrom(initializerType) && typeof(IValueProvider).IsAssignableFrom(initializerType))
			{

			}

			var constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if(constructors.Length == 0)
			{
				constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			}

			IEnumerable<ConstructorInfo> constructorsByParameterCount = constructors.Length <= 1 ? constructors : constructors.OrderByDescending(c => c.GetParameters().Length);
			foreach(var constructor in constructorsByParameterCount)
			{
				var parameters = constructor.GetParameters();
				int parameterCount = parameters.Length;
				if(parameterCount == 0)
				{
					continue;
				}

				initialized.Add(concreteType);

				object[] arguments = new object[parameterCount];
				bool allArgumentsAvailable = true;

				for(int i = 0; i < parameterCount; i++)
				{
					var parameterType = parameters[i].ParameterType;
					if(!TryGetOrInitializeService(parameterType, out arguments[i], initialized, localServices))
					{
						//LogMissingDependencyWarning(concreteType, parameterType);
						//LogMissingDependencyWarning(concreteType, parameterType, null, servicesInScene);
						ServiceInitFailedException.Create(serviceInfo, InitFailReason.MissingDependency, null, null, null, null, null, parameterType, servicesInScene);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					initialized.Remove(concreteType);
					continue;
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameters[parameterIndex].ParameterType;
					var argument = arguments[parameterIndex];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						#if DEV_MODE || DEBUG
						try
						{
						#endif

						await loadArgumentTask;

						#if DEV_MODE || DEBUG
						}
						catch(Exception exception)
						{
							if(TryFindContainedExceptionForService(exception, serviceInfo, out var exceptionForService))
							{
								// Intentionally using "throw exceptionForService" instead of just "throw" to remove bloat from the stack trace.
								throw exceptionForService;
							}

							var resultType = exception is CircularDependenciesException ? InitFailReason.CircularDependencies : InitFailReason.MissingDependency;
							throw CreateAggregateException(exception, ServiceInitFailedException.Create(serviceInfo, resultType, asset:null, missingDependencyType:parameterType, servicesInScene:servicesInScene));
						}
						#endif

						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				for(int i = 0; i < parameterCount; i++)
				{
					arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, localServices);
				}

				result = constructor.Invoke(arguments);

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via constructor {constructor} successfully.");
				#endif

				return result is Task task ? await task.GetResult() : result;
			}

			if(!Array.Exists(constructors, c => c.GetParameters().Length == 0))
			{
				throw MissingInitArgumentsException.ForService(concreteType, constructorsByParameterCount.FirstOrDefault()?.GetParameters().Select(p => p.ParameterType).ToArray(), servicesInScene);
			}

			result = Activator.CreateInstance(concreteType);

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {concreteType} created successfully via default constructor.");
			#endif

			return result;
		}
	}
}
*/