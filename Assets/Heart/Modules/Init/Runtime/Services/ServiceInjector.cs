//#define DEBUG_INIT_SERVICES
//#define DEBUG_DEFAULT_SERVICES
//#define DEBUG_CREATE_SERVICES
//#define DEBUG_LAZY_INIT
//#define DEBUG_INIT_TIME

#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.Init.Internal.InitializableUtility;
using Object = UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
using Sisus.Init.EditorOnly;
using Debug = UnityEngine.Debug;
#endif

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Class responsible for caching instances of all classes that have the <see cref="ServiceAttribute"/>,
	/// injecting dependencies for services that implement an <see cref="IInitializable{}"/>
	/// interface targeting only other services,
	/// and using <see cref="InitArgs.Set"/> to assign references to services ready to be retrieved
	/// for any other classes that implement an <see cref="IArgs{}"/> interface targeting only services.
	/// </summary>
	internal static class ServiceInjector
	{
		/// <summary>
		/// <see langword="true"/> if all shared services that are loaded synchronously during game initialization
		/// have been created and are ready to be used by clients; otherwise, <see langword="false"/>.
		/// <para>
		/// This only takes into consideration services that are initialized synchronously during game initialization.
		/// To determine if all asynchronously initialized services are also ready to be used,
		/// use <see cref="AsyncServicesAreReady"/> instead.
		/// </para>
		/// <para>
		/// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>.
		/// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
		/// Services that are registered manually using <see cref="Service.Set{TService}"/> are also not
		/// guaranteed to be loaded even if this is <see langword="true"/>.
		/// </para>
		/// </summary>
		public static bool ServicesAreReady { get; private set; }

		/// <summary>
		/// <see langword="true"/> if all shared services that are loaded asynchronously during game initialization
		/// have been created, initialized and are ready to be used by clients; otherwise, <see langword="false"/>.
		/// <para>
		/// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>.
		/// </para>
		/// <para>
		/// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
		/// Services that are registered manually using <see cref="Service.Set{TService}"/> are also not
		/// guaranteed to be loaded even if this is <see langword="true"/>.
		/// </para>
		/// </summary>
		public static bool AsyncServicesAreReady { get; private set; }

		/// <summary>
		/// Called when all services have been created,
		/// initialized and are ready to be used by clients.
		/// <para>
		/// This only takes into consideration services that are initialized synchronously and non-lazily
		/// during game initialization. To get a callback when all asynchronously initialized services are also
		/// ready to be used, use <see cref="AsyncServicesBecameReady"/> instead.
		/// </para>
		/// </summary>
		public static event Action ServicesBecameReady;

		/// <summary>
		/// Called when all services that are initialized asynchronously have been created,
		/// initialized and are ready to be used by clients.
		/// </summary>
		public static event Action AsyncServicesBecameReady;
		internal static Dictionary<Type, object> services = new();
		private static readonly Dictionary<Type, GlobalServiceInfo> uninitializedServices = new(); // Lazy and transient
		private static GameObject container;
		#if DEV_MODE || DEBUG
		private static readonly HashSet<GlobalServiceInfo> exceptionsLogged = new();
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Reset state when entering play mode in the editor to support Enter Play Mode Settings.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnEnterPlayMode()
		{
			ServicesAreReady = false;
			AsyncServicesAreReady = false;
			services.Clear();
			uninitializedServices.Clear();
			ThreadSafe.Application.ExitingPlayMode -= OnExitingPlayMode;
			ThreadSafe.Application.ExitingPlayMode += OnExitingPlayMode;

			static void OnExitingPlayMode()
			{
				ServicesAreReady = false;
				AsyncServicesAreReady = false;
				HashSet<object> handled = new();

				foreach(var serviceInfo in GetServiceDefinitions())
				{
					var concreteOrDefiningType = serviceInfo.ConcreteOrDefiningType;
					if(!services.TryGetValue(concreteOrDefiningType, out object instance)
					&& !services.TryGetValue(serviceInfo.definingTypes.FirstOrDefault() ?? typeof(void), out instance))
					{
						continue;
					}

					if(!handled.Add(instance)
					|| serviceInfo.FindFromScene
					|| instance is Component
					|| Find.WrapperOf(instance, out _))
					{
						continue;
					}

					var concreteType = instance.GetType();
					if(Find.typesToWrapperTypes.ContainsKey(concreteType))
					{
						continue;
					}

					if(instance is IUpdate update)
					{
						Updater.Unsubscribe(update);
					}

					if(instance is ILateUpdate lateUpdate)
					{
						Updater.Unsubscribe(lateUpdate);
					}

					if(instance is IFixedUpdate fixedUpdate)
					{
						Updater.Unsubscribe(fixedUpdate);
					}

					if(instance is IOnDisable onDisable)
					{
						try
						{
							onDisable.OnDisable();
						}
						catch(Exception e)
						{
							Debug.LogException(e);
						}
					}

					if(instance is IOnDestroy onDestroy)
					{
						try
						{
							onDestroy.OnDestroy();
						}
						catch(Exception e)
						{
							Debug.LogException(e);
						}
					}

					if(instance is IDisposable disposable)
					{
						try
						{
							disposable.Dispose();
						}
						catch(Exception e)
						{
							Debug.LogException(e);
						}
					}
					else if(instance is IAsyncDisposable asyncDisposable)
					{
						try
						{
							asyncDisposable.DisposeAsync();
						}
						catch(Exception e)
						{
							if(e is not TaskCanceledException)
							{
								Debug.LogException(e);
							}
						}
					}
				}

				services.Clear();
				uninitializedServices.Clear();
				container = null;
				exceptionsLogged.Clear();
			}
		}
		#endif

		/// <summary>
		/// Creates instances of all services,
		/// injects dependencies for servives that implement an <see cref="IInitializable{}"/>
		/// interface targeting only other services,
		/// and uses <see cref="InitArgs.Set"/> to assign references to services ready to be retrieved
		/// for any other classes that implement an <see cref="IArgs{}"/> interface targeting only services.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static async void CreateAndInjectServices()
		{
			#if DEV_MODE && DEBUG_INIT_TIME
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			#endif

#if UNITY_EDITOR
			Service.BatchEditingServices = true;
#endif
			
			CreateInstancesOfAllServices();

			ServicesAreReady = true;

			#if UNITY_EDITOR
			var scriptableObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();
			var uninitializedScriptableObjects = new Dictionary<Type, List<ScriptableObject>>(scriptableObjects.Length);
			foreach(var uninitializedScriptableObject in scriptableObjects)
			{
				var type = uninitializedScriptableObject.GetType();
				if(!uninitializedScriptableObjects.TryGetValue(type, out List<ScriptableObject> instances))
				{
					instances = new List<ScriptableObject>(1);
					uninitializedScriptableObjects.Add(type, instances);
				}

				instances.Add(uninitializedScriptableObject);
			}
			#endif

			InjectServiceDependenciesForTypesThatRequireOnlyThem(out List<Task> injectAsyncServices
			#if UNITY_EDITOR
			, uninitializedScriptableObjects
			#endif
			);

			#if UNITY_EDITOR
			InitializeAlreadyLoadedScriptableObjectsInTheEditor(uninitializedScriptableObjects);
			#endif

			ServicesBecameReady?.Invoke();
			ServicesBecameReady = null;

#if UNITY_EDITOR
			Service.BatchEditingServices = false;
#endif
			
			#if DEV_MODE && DEBUG_INIT_TIME
			Debug.Log($"Initialization of {services.Count} services took {timer.Elapsed.TotalSeconds} seconds.");
			#endif

			await Task.WhenAll(injectAsyncServices);

			#if DEV_MODE && DEBUG_INIT_TIME
			timer.Stop();
			Debug.Log($"Injection of {injectAsyncServices.Count} async services took {timer.Elapsed.TotalSeconds} seconds.");
			#endif

			AsyncServicesAreReady = true;
			AsyncServicesBecameReady?.Invoke();
			AsyncServicesBecameReady = null;

			// Make InactiveInitializer.OnAfterDeserialize continue execution
			Until.OnUnitySafeContext();
		}

		private static void CreateInstancesOfAllServices()
		{
			var globalServices = GetServiceDefinitions();
			int definitionCount = globalServices.Count;
			if(definitionCount == 0)
			{
				#if DEV_MODE
				Debug.Log("Won't inject any services because ServiceAttributeUtility.definingTypes is empty.");
				#endif
				services = null;
				return;
			}

			services = new Dictionary<Type, object>(definitionCount);

			// List of concrete service types that have already been initialized (instance created / retrieved)
			HashSet<Type> initialized = new(definitionCount);

			Dictionary<Type, ScopedServiceInfo> servicesInScene = new(0);
			List<IInitializer> initializersInScene = new(0);

			InitializeServices(globalServices, initialized, servicesInScene, initializersInScene);

			InjectCrossServiceDependencies(globalServices, initialized, servicesInScene, initializersInScene);

			#if UNITY_EDITOR
			_ = CreateServicesDebugger();
			#endif

			if(container)
			{
				container.SetActive(true);
			}

			HandleExecutingEventFunctionsForAll(initialized);

			static void HandleExecutingEventFunctionsForAll(HashSet<Type> initialized)
			{
				#if DEV_MODE
				var handled = new HashSet<object>();
				#endif

				foreach(var concreteType in initialized)
				{
					if(ServiceAttributeUtility.concreteTypes.TryGetValue(concreteType, out var serviceInfo) && serviceInfo.FindFromScene)
					{
						continue;
					}
					
					if(services.TryGetValue(concreteType, out object instance))
					{
						#if DEV_MODE
						Debug.Assert(handled.Add(instance));
						Debug.Assert(!concreteType.IsAbstract);
						Debug.Assert(concreteType == instance.GetType() || instance is Task);
						#endif

						_ = HandleExecutingEventFunctionsFor(instance);
					}
				}
			}
		}

		private static async Task HandleExecutingEventFunctionsFor(object instance)
        {
			if(instance is Task task)
			{
				instance = await task.GetResult();
			}

        	SubscribeToUpdateEvents(instance);
        	ExecuteAwake(instance);
        	ExecuteOnEnable(instance);
        	ExecuteStartAtEndOfFrame(instance);
        }

		private static List<IInitializer> FillIfEmpty([DisallowNull] List<IInitializer> initializersInScene)
		{
			if(initializersInScene.Count == 0)
			{
				Find.All(initializersInScene);
				if(initializersInScene.Count == 0)
				{
					initializersInScene.Add(new NullInitializer());
				}
			}
				
			return initializersInScene;
		}
		

		private static Dictionary<Type, ScopedServiceInfo> FillIfEmpty([DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene)
		{
			if(servicesInScene.Count > 0)
			{
				return servicesInScene;
			}

			var servicesComponents = 
			#if UNITY_2022_3_OR_NEWER
			Object.FindObjectsByType<Services>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			#else
			Object.FindObjectsOfType<Services>(true);
			#endif

			foreach(var servicesComponent in servicesComponents)
			{
				var toClients = servicesComponent.toClients;
				foreach(var definition in servicesComponent.providesServices)
				{
					if(definition.definingType?.Value is Type definingType && definition.service is Object service && service)
					{
						servicesInScene[definingType] = new(definingType, service, toClients);
					}
				}
			}

			var serviceTags =
			#if UNITY_2022_3_OR_NEWER
			Object.FindObjectsByType<ServiceTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			#else
			Object.FindObjectsOfType<ServiceTag>(true);
			#endif

			foreach(var serviceTag in serviceTags)
			{
				if(serviceTag.DefiningType is Type definingType && serviceTag.Service is Component service)
				{
					servicesInScene[definingType] = new(definingType, service, serviceTag.ToClients);
				}
			}

			// To avoid wasting resources try to rebuild the dictionary again and again, make sure it contains at least one entry
			if(servicesInScene.Count == 0)
			{
				servicesInScene.Add(typeof(void), new(typeof(void), null, Clients.Everywhere));
			}

			return servicesInScene;
		}

		#if UNITY_EDITOR
		private static async Task CreateServicesDebugger()
		{
			if(!Application.isPlaying)
			{
				return;
			}

			if(!container)
			{
				CreateServicesContainer();
			}

			var debugger = container.AddComponent<ServicesDebugger>();
			await debugger.SetServices(services.Values.Distinct());
		}
		#endif

		private static void CreateServicesContainer()
		{
			container = new GameObject("Services");
			container.SetActive(false);
			Object.DontDestroyOnLoad(container);
		}

		internal static Task<object> Register(GlobalServiceInfo serviceInfo) // TODO: Add overload accepting an array -> and support initializing them in optimal order automatically!
		{
			if (!serviceInfo.LazyInit /* && ServicesAreReady*/)
			{
				return LazyInit(serviceInfo, serviceInfo.definingTypes.FirstOrDefault() ?? serviceInfo.concreteType);
			}

			#if DEV_MODE && DEBUG_LAZY_INIT
			Debug.Log($"Will not initialize {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} yet because it LazyInit is True.");
			#endif

			if(serviceInfo.concreteType is Type concreteType)
			{
				uninitializedServices[concreteType] = serviceInfo;
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				uninitializedServices[definingType] = serviceInfo;
			}

			return Task.FromResult(default(object));
		}

		private static void InitializeServices(List<GlobalServiceInfo> globalServiceInfos, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			foreach(var serviceInfo in globalServiceInfos)
			{
				if(serviceInfo.LazyInit)
				{
					#if DEV_MODE && DEBUG_LAZY_INIT
					Debug.Log($"Will not initialize {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} yet because LazyInit is True.");
					#endif

					if(serviceInfo.concreteType is Type concreteType)
					{
						uninitializedServices[concreteType] = serviceInfo;
					}

					foreach(var definingType in serviceInfo.definingTypes)
					{
						uninitializedServices[definingType] = serviceInfo;
					}

					continue;
				}

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				var info = serviceInfo;
				#endif

				_ = GetOrInitializeService(serviceInfo, initialized, servicesInScene, initializersInScene, null)
					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					.ContinueWith(OnGetOrInitializeServiceFailed, TaskContinuationOptions.OnlyOnFaulted)
					#endif
					;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				void OnGetOrInitializeServiceFailed(Task task)
				{
					if(task.Exception is { } aggregateException)
					{
						aggregateException.LogToConsole();
					}
					else
					{
						Debug.LogError($"Initializing service {info.ConcreteOrDefiningType} failed for an unknown reason.");
					}
				}
				#endif
			}
		}

		/// <param name="requestedServiceType"> The type of the initialization argument being requested for the client. Could be abstract. </param>
		internal static async Task<object> LazyInit([DisallowNull] GlobalServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
		{
			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			if(requestedServiceType.IsGenericTypeDefinition)
			{
				Debug.LogError($"LazyInit called with {nameof(requestedServiceType)} {TypeUtility.ToString(requestedServiceType)} that was a generic type definition. This should not happen.");
				return null;
			}
			#endif

			if(GetConcreteAndClosedType(serviceInfo, requestedServiceType) is not Type concreteType)
			{
				#if DEV_MODE
				Debug.LogWarning($"LazyInit({TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} as {TypeUtility.ToString(requestedServiceType)}) called but could not determine {nameof(concreteType)} for service. Should this ever happen? Should the method be renamed to TryLazyInit?");
				#endif
				return null;
			}

			#if DEV_MODE
			Debug.Assert(!concreteType.IsAbstract, $"GetConcreteAndClosedType result {TypeUtility.ToString(concreteType)} was abstract.");
			Debug.Assert(!concreteType.IsGenericTypeDefinition, $"GetConcreteAndClosedType result {TypeUtility.ToString(concreteType)} was an generic type definition.");
			#endif

			// If service has already been initialized, no need to do anything.
			if(services.TryGetValue(concreteType, out object service))
			{
				return service;
			}

			#if DEV_MODE
			Debug.Assert(TryGetUninitializedServiceInfo(serviceInfo.ConcreteOrDefiningType, out _), concreteType);
			#endif

			bool isTransient = serviceInfo.IsTransient;
			if(!isTransient)
			{
				if(!uninitializedServices.Remove(serviceInfo.ConcreteOrDefiningType))
				{
					return null;
				}

				var definingTypes = serviceInfo.definingTypes;
				foreach(var definingType in definingTypes)
				{
					uninitializedServices.Remove(definingType);
				}
			}

			#if DEV_MODE && DEBUG_LAZY_INIT
			Debug.Log($"LazyInit({TypeUtility.ToString(concreteType)})");
			#endif

			var initialized = new HashSet<Type>(0);
			var servicesInScene = new Dictionary<Type, ScopedServiceInfo>(0);
			var initializersInScene = new List<IInitializer>(0);
			service = await GetOrInitializeService(serviceInfo, initialized, servicesInScene, initializersInScene, concreteType);

			#if DEV_MODE
			Debug.Assert(service is not Task);
			#endif

			#if UNITY_EDITOR
			if(container && container.TryGetComponent(out ServicesDebugger debugger))
			{
				_ = debugger.SetServices(services.Values.Distinct());
			}
			#endif

			await InjectCrossServiceDependencies(service, initialized, servicesInScene, initializersInScene);

			if(!serviceInfo.FindFromScene)
			{
				await HandleExecutingEventFunctionsFor(service);
			}

			return service;
		}

		[return: MaybeNull]
		private static Type GetConcreteAndClosedType([DisallowNull] GlobalServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
		{
			Type concreteType = serviceInfo.concreteType;

			if(concreteType is null)
			{
				if(!requestedServiceType.IsAbstract)
				{
					return requestedServiceType;
				}

				// Support ServiceAttribute being attached to an interface, with defining type being set to a concrete type.
				// E.g. interface ILogger<T> with attribute [Service(typeof(Logger<>))].
				concreteType = Array.Find(serviceInfo.definingTypes, t => !t.IsAbstract);
				if(concreteType is null)
				{
					return null;
				}
			}

			#if DEV_MODE
			Debug.Assert(!concreteType.IsAbstract, $"GetConcreteAndClosedType result {TypeUtility.ToString(concreteType)} was abstract.");
			#endif

			if(!concreteType.IsGenericTypeDefinition)
			{
				#if DEV_MODE
				if(concreteType.IsGenericType && concreteType.GetGenericArguments().Any(t => t.IsGenericParameter)) Debug.LogError($"GetConcreteAndClosedType result {TypeUtility.ToString(concreteType)} had a generic parameter.");
				#endif

				return concreteType;
			}

			if(!requestedServiceType.IsAbstract)
			{
				return requestedServiceType;
			}

			int requiredGenericArgumentCount = concreteType.GetGenericArguments().Length;
			for(var typeOrBaseType = requestedServiceType; typeOrBaseType != null; typeOrBaseType = typeOrBaseType.BaseType)
			{
				if(!typeOrBaseType.IsGenericType)
				{
					continue;
				}

				var genericArguments = typeOrBaseType.GetGenericArguments();
				if(genericArguments.Length != requiredGenericArgumentCount)
				{
					continue;
				}

				var closedConcreteType = concreteType.MakeGenericType(genericArguments);
				if(requestedServiceType.IsAssignableFrom(closedConcreteType))
				{
					return closedConcreteType;
				}
			}

			if(!requestedServiceType.IsInterface)
			{
				foreach(var interfaceType in requestedServiceType.GetInterfaces())
				{
					if(!interfaceType.IsGenericType)
					{
						continue;
					}

					var genericArguments = interfaceType.GetGenericArguments();
					if(genericArguments.Length != requiredGenericArgumentCount)
					{
						continue;
					}

					var closedConcreteType = concreteType.MakeGenericType(genericArguments);
					if(requestedServiceType.IsAssignableFrom(closedConcreteType))
					{
						return closedConcreteType;
					}
				}
			}

			throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.UnresolveableConcreteType);
		}

		private static async Task<object> GetOrInitializeService(GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene, [AllowNull] Type overrideConcreteType)
		{
			var concreteType = overrideConcreteType ?? serviceInfo.concreteType;
			if((concreteType ?? serviceInfo.definingTypes.FirstOrDefault()) is not Type concreteOrDefiningType)
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.UnresolveableConcreteType);
			}

			// If one class contains multiple Service attributes still create only one shared instance.
			if(services.TryGetValue(concreteOrDefiningType, out var existingInstance))
			{
				if(existingInstance is Task existingTask && !concreteOrDefiningType.IsInstanceOfType(existingInstance))
				{
					existingInstance = await existingTask.GetResult();
				}

				return existingInstance;
			}

			Task<object> task;
			try
			{
				task = InitializeService(serviceInfo, initialized, servicesInScene, initializersInScene, concreteType);
			}
			catch(ServiceInitFailedException)
			{
				throw;
			}
			catch(Exception e)
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ExceptionWasThrown, e);
			}

			if(concreteType is not null)
			{
				services[concreteType] = task;
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				services[definingType] = task;
			}

			object result = await task;

			if(result is Task chainedTask)
			{
				result = await chainedTask.GetResult();
			}

			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			if(result is null)
			{
				#if DEV_MODE
				Debug.LogError($"GetOrCreateInstance(concreteOrDefiningType:{concreteOrDefiningType.Name}, definingTypes:{string.Join(", ", serviceInfo.definingTypes.Select(t => t?.Name))}) returned instance was null.");
				#endif
				return null;
			}
			#endif

			#if DEV_MODE
			Debug.Assert(IsInstanceOf(serviceInfo, result), serviceInfo.ConcreteOrDefiningType.Name);
			#endif

			FinalizeServiceImmediate(serviceInfo, result);
			return result;
		}

		private static async Task<object> InitializeService([DisallowNull] GlobalServiceInfo serviceInfo, [DisallowNull] HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene, Type concreteType)
		{
			object result;
			if(typeof(IServiceInitializer).IsAssignableFrom(serviceInfo.classWithAttribute))
			{
				if(initialized.Contains(concreteType))
				{
					#if DEV_MODE
					Debug.LogWarning($"initialized.Contains({TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)})");
					#endif
					throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies);
				}

				Type initializerType = serviceInfo.classWithAttribute;
				var serviceInitializer = Activator.CreateInstance(initializerType) as IServiceInitializer;
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
					object[] arguments = new object[parameterTypes.Length + 1];
					int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, arguments, 1);
					if(failedToGetArgumentAtIndex != -1)
					{
						LogMissingDependencyWarning(initializerType, parameterTypes[failedToGetArgumentAtIndex]);
						continue;
					}

					for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
					{
						var parameterType = parameterTypes[parameterIndex];
						var argument = arguments[parameterIndex + 1];

						if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
						{
							await loadArgumentTask;
							argument = await loadArgumentTask.GetResult();
							arguments[parameterIndex + 1] = argument;
						}
					}

					try
					{
						result = serviceInitializer.InitTarget(arguments);
					}
					catch(Exception ex)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerThrewException, ex);
					}

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerReturnedNull);
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} created via service initializer {serviceInitializer.GetType().Name} successfully.");
					#endif

					return result;
				}

				if(parameterCount == 0)
				{
					try
					{
						result = serviceInitializer.InitTarget(Array.Empty<object>());
					}
					catch(Exception ex)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerThrewException, ex);
					}

					if(result != null)
					{
						initialized.Add(concreteType);
						return result is Task task ? await task.GetResult() : result;
					}
				}
			}

			if(typeof(IServiceInitializerAsync).IsAssignableFrom(serviceInfo.classWithAttribute))
			{
				if(initialized.Contains(concreteType))
				{
					return null;
				}

				Type initializerType = serviceInfo.classWithAttribute;
				var serviceInitializerAsync = Activator.CreateInstance(initializerType) as IServiceInitializerAsync;
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

					var parameterTypes = interfaceType.GetGenericArguments().Skip(1).ToArray(); // Use span?
					object[] arguments = new object[parameterCount];
					int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, arguments);
					if(failedToGetArgumentAtIndex != -1)
					{
						LogMissingDependencyWarning(initializerType, parameterTypes[failedToGetArgumentAtIndex]);
						continue;
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

					Task task = serviceInitializerAsync.InitTargetAsync(arguments);

					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(task is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerReturnedNull);
					}
					#endif

					try
					{
						result = await task.GetResult();
					}
					catch(Exception ex)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerThrewException, ex);
					}

					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerReturnedNull);
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} created via async service initializer {serviceInitializerAsync.GetType().Name} successfully.");
					#endif

					return result;
				}

				if(parameterCount == 0)
				{
					Task task = serviceInitializerAsync.InitTargetAsync(Array.Empty<object>());

					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(task is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerReturnedNull);
					}
					#endif

					result = await task.GetResult();

					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceInitializerReturnedNull);
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} created via async service initializer {serviceInitializerAsync.GetType().Name} successfully.");
					#endif

					return result;
				}

				Debug.LogWarning($"{TypeUtility.ToString(initializerType)} failed to initialize service {TypeUtility.ToString(concreteType)} with defining types {string.Join(", ", serviceInfo.definingTypes.Select(t => TypeUtility.ToString(t)))}.\n{nameof(IServiceInitializerAsync.InitTargetAsync)} should return an object of type Task<{TypeUtility.ToString(concreteType)}>.");
			}

			if(serviceInfo.FindFromScene)
			{
				if(typeof(Component).IsAssignableFrom(concreteType))
				{
					result = 
					#if UNITY_2023_1_OR_NEWER
					Object.FindAnyObjectByType(concreteType, FindObjectsInactive.Include);
					#else
					Object.FindObjectOfType(concreteType, true);
					#endif
				}
				else if(typeof(Component).IsAssignableFrom(serviceInfo.classWithAttribute))
				{
					result = 
					#if UNITY_2023_1_OR_NEWER
					Object.FindAnyObjectByType(serviceInfo.classWithAttribute, FindObjectsInactive.Include);
					#else
					Object.FindObjectOfType(serviceInfo.classWithAttribute, true);
					#endif
				}
				else if(concreteType.IsInterface)
				{
					result = Find.Any(concreteType, true);
				}
				else if(serviceInfo.classWithAttribute.IsInterface)
				{
					result = Find.Any(serviceInfo.classWithAttribute, true);
				}
				else
				{
					result = null;
				}

				if(result is not null)
				{
					if(IsInstanceOf(serviceInfo, result))
					{
						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(result.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
						#endif
						return result;
					}

					if(result is IInitializer initializerWithAttribute && TargetIsAssignableOrConvertibleToType(initializerWithAttribute, serviceInfo))
					{
						object value = await initializerWithAttribute.InitTargetAsync();
						if(IsInstanceOf(serviceInfo, value))
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {TypeUtility.ToString(value.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
							#endif
							return value;
						}
					}

					if(serviceInfo.LoadAsync && result is IValueProviderAsync valueProviderAsync)
					{
						object value = await valueProviderAsync.GetForAsync(valueProviderAsync as Component);
						if(IsInstanceOf(serviceInfo, value))
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {TypeUtility.ToString(value.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
							#endif
							return value;
						}
					}

					if(result is IValueProvider valueProvider)
					{
						if(valueProvider.TryGetFor(valueProvider as Component, out object value) && IsInstanceOf(serviceInfo, value))
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {TypeUtility.ToString(value.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
							#endif
							return value;
						}
					}

					Debug.LogWarning($"Failed to convert service instance from {TypeUtility.ToString(result.GetType())} to {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())}.", result as Object);
				}

				foreach(var initializer in FillIfEmpty(initializersInScene))
				{
					if(TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
					{
						result = await initializer.InitTargetAsync();

						// Support Initializer -> Initialized
						if(IsInstanceOf(serviceInfo, result))
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {TypeUtility.ToString(result.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
							#endif
							return result;
						}

						if(serviceInfo.LoadAsync && result is IValueProviderAsync valueProviderAsync)
						{
							object value = await valueProviderAsync.GetForAsync(initializer as Component);
							if(IsInstanceOf(serviceInfo, value))
							{
								#if DEV_MODE && DEBUG_CREATE_SERVICES
								Debug.Log($"Service {TypeUtility.ToString(value.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
								#endif
								return value;
							}
						}

						// Support WrapperInitializer -> Wrapper -> Wrapped Object
						if(result is IValueProvider valueProvider)
						{
							if(valueProvider.TryGetFor(initializer as Component, out object value) && IsInstanceOf(serviceInfo, value))
							{
								#if DEV_MODE && DEBUG_CREATE_SERVICES
								Debug.Log($"Service {TypeUtility.ToString(value.GetType())} of type {TypeUtility.ToString(serviceInfo.definingTypes.FirstOrDefault())} retrieved from scene successfully.", result as Object);
								#endif
								return value;
							}
						}
					}
				}

				if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
				{
					throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ScriptableObjectWithFindFromScene);
				}

				#if UNITY_EDITOR
				if(!IsFirstSceneInBuildSettingsLoaded()) { return null; }
				#endif

				Debug.LogWarning($"Service Not Found: There is no '{concreteType.Name}' found in the active scene {SceneManager.GetActiveScene().name}, but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. Either add an instance to the scene or don't set {nameof(ServiceAttribute.FindFromScene)} true to have a new instance be created automatically.");
				return null;
			}

			if(serviceInfo.ResourcePath is string resourcePath)
			{
				Object asset;
				if(serviceInfo.LoadAsync)
				{
  					ResourceRequest resourceRequest = Resources.LoadAsync<Object>(resourcePath);
					#if UNITY_2023_2_OR_NEWER
					await resourceRequest;
					#else
					while(!resourceRequest.isDone)
					{
						await Task.Yield();
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
					throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingResource);
				}
				#endif

				if(asset is GameObject gameObject)
				{
					if(serviceInfo.ShouldInstantiate(true))
					{
						result = await InstantiateFromAsset(gameObject, serviceInfo, initialized, servicesInScene, initializersInScene);

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset);
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} instantiated from prefab at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
					}
					else
					{
						result = await GetServiceFromInstance(gameObject, serviceInfo);
						
						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} was found on the resource at path 'Resources/{resourcePath}'.", asset);
							return null;
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} loaded from prefab at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
					}
				}
				else if(asset is ScriptableObject scriptableObject)
				{
					if(serviceInfo.ShouldInstantiate(false))
					{
						result = await InstantiateFromAsset(scriptableObject, serviceInfo, initialized, servicesInScene, initializersInScene);

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} was found on the clone created from the resource at path 'Resources/{resourcePath}'.", asset);
							return null;
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} instantiated from scriptable object at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
					}
					else
					{
						result = await GetServiceAsync(scriptableObject, serviceInfo);

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} was found on the resource at path 'Resources/{resourcePath}'.", asset);
							return null;
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} loaded from scriptable object at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
					}
				}
				else if(IsInstanceOf(serviceInfo, asset))
				{
					result = asset;

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} loaded from asset at path 'Resources/{resourcePath}' successfully.", asset);
					#endif
				}
				else
				{
					Debug.LogWarning($"Service Not Found: Resource at path 'Resources/{resourcePath}' could not be converted to type {serviceInfo.definingTypes.FirstOrDefault()?.Name}.", asset);
					return null;
				}

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(result is null)
				{
					Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} was found on the clone created from the resource at path 'Resources/{resourcePath}'.", asset);
					return null;
				}
				#endif

				return result;
			}

			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			if(serviceInfo.AddressableKey is string addressableKey)
			{
				return await InitializeAddressableAsset(addressableKey, serviceInfo, initialized, servicesInScene, initializersInScene);
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
					result = await AddComponent(serviceInfo, initialized, servicesInScene, initializersInScene);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null)
					{
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} to '{container.name}'.", container);
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
						Debug.LogWarning($"Service Initialization Failed: Failed to attach service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} to '{container.name}'.", container);
						return null;
					}
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} attached to '{container.name}' successfully.", container);
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
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies);
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
					if(!TryGetOrInitializeService(parameterType, out arguments[i], initialized, servicesInScene, initializersInScene))
					{
						LogMissingDependencyWarning(concreteType, parameterType);
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

							var failReason = exception is CircularDependenciesException ? ServiceInitFailReason.CircularDependencies : ServiceInitFailReason.MissingDependency;
							throw CreateAggregateException(exception, ServiceInitFailedException.Create(serviceInfo, failReason, asset:null, missingDependencyType:parameterType));
						}
						#endif

						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex] = argument;
					}
				}

				for(int i = 0; i < parameterCount; i++)
				{
					arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, servicesInScene, initializersInScene);
				}

				result = constructor.Invoke(arguments);

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} created via constructor {constructor} successfully.");
				#endif

				return result is Task task ? await task.GetResult() : result;
			}

			if(!Array.Exists(constructors, c => c.GetParameters().Length == 0))
			{
				throw MissingInitArgumentsException.ForService(concreteType, constructorsByParameterCount.FirstOrDefault()?.GetParameters().Select(p => p.ParameterType).ToArray());
			}

			result = Activator.CreateInstance(concreteType);

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {concreteType} created successfully via default constructor.");
			#endif

			return result;

			void LogMissingDependencyWarning(Type initializerType, Type dependencyType)
			{
				if(FillIfEmpty(servicesInScene).TryGetValue(dependencyType, out var serviceInfo))
				{
					if(!serviceInfo.service)
					{
						Debug.LogWarning($"{TypeUtility.ToString(initializerType)} needs service {TypeUtility.ToString(dependencyType)} to initialize {TypeUtility.ToString(concreteType)}, but reference to the service seems to be broken in the {nameof(Services)} component.");
					}
					else
					{
						Debug.LogWarning($"{TypeUtility.ToString(initializerType)} needs service {TypeUtility.ToString(dependencyType)} to initialize {TypeUtility.ToString(concreteType)}, but the service is only accessible to clients {serviceInfo.toClients}.", serviceInfo.service);
					}
				}
				else
				{
					Debug.LogWarning($"{TypeUtility.ToString(initializerType)} needs service {TypeUtility.ToString(dependencyType)} to initialize {TypeUtility.ToString(concreteType)}, but instance not found among {services.Count + servicesInScene.Count + uninitializedServices.Count} services.");
				}
			}
		}

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		private static async Task<Object> LoadAddressableAsset(string addressableKey, GlobalServiceInfo serviceInfo)
		{
			var asyncOperation = Addressables.LoadAssetAsync<Object>(addressableKey);
			Object asset;

			if(serviceInfo.LoadAsync)
			{
				asset = await asyncOperation.Task;
			}
			else
			{
				asset = asyncOperation.WaitForCompletion();
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!asset)
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingAddressable);
			}
			#endif

			return asset;
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		private static async Task<object> InitializeAddressableAsset(string addressableKey, GlobalServiceInfo serviceInfo, HashSet<Type> initialized, Dictionary<Type, ScopedServiceInfo> servicesInScene, List<IInitializer> initializersInScene)
		{
			var asset = await LoadAddressableAsset(addressableKey, serviceInfo);

			object result;
			if(asset is GameObject gameObject)
			{
				if(serviceInfo.ShouldInstantiate(true))
				{
					result = await InstantiateFromAsset(gameObject, serviceInfo, initialized, servicesInScene, initializersInScene);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} Instantiated from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}

				result = await GetServiceFromInstance(gameObject, serviceInfo);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset);
				#endif

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {concreteType.Name} loaded from addressable asset \"{addressableKey}\" successfully.", asset);
				#endif

				return result;
			}

			if(asset is ScriptableObject scriptableObject)
			{
				if(serviceInfo.ShouldInstantiate(false))
				{
					result = await InstantiateFromAsset(scriptableObject, serviceInfo, initialized, servicesInScene, initializersInScene);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} Instantiated from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}
				else
				{
					result = await GetServiceAsync(scriptableObject, serviceInfo);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} loaded from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!IsInstanceOf(serviceInfo, asset))
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.AssetNotConvertible, asset);
			}
			#endif

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {concreteType.Name} loaded from addressable asset \"{addressableKey}\" successfully.", asset);
			#endif

			return asset;
		}
		#endif

		private static Exception CreateAggregateException(Exception exiting, Exception addition)
		{
			if(exiting is AggregateException aggregateException)
			{
				var innerExceptions = aggregateException.InnerExceptions.ToList();
				innerExceptions.Add(addition);
				throw new AggregateException(innerExceptions);
			}

			return new AggregateException(exiting, addition);
		}

		private static void LogException(Exception exception)
		{
			if(exception is AggregateException aggregateException)
			{
				LogException(aggregateException);
				return;
			}

			// Avoid same exception being logged twice for the same service.
			#if DEV_MODE || DEBUG
			if(exception is not ServiceInitFailedException serviceInitFailedException || exceptionsLogged.Add(serviceInitFailedException.ServiceInfo))
			#endif
			{
				Debug.LogException(exception);
			}
		}

		private static void LogException(AggregateException aggregateException)
		{
			foreach(var innerException in aggregateException.InnerExceptions)
			{
				LogException(innerException);
			}
		}

		private static bool TryFindContainedExceptionForService(Exception exceptionToCheck, GlobalServiceInfo serviceInfo, out Exception exceptionForService)
		{
			if(exceptionToCheck is AggregateException aggregateException)
			{
				return TryFindContainedExceptionForService(aggregateException, serviceInfo, out exceptionForService);
			}

			if(exceptionToCheck is CircularDependenciesException circularDependenciesException)
			{
				return TryFindContainedExceptionForService(circularDependenciesException, serviceInfo, out exceptionForService);
			}

			if(exceptionToCheck.InnerException is { } innerException)
			{
				return TryFindContainedExceptionForService(innerException, serviceInfo, out exceptionForService);
			}

			exceptionForService = null;
			return false;
		}

		private static bool TryFindContainedExceptionForService(CircularDependenciesException exceptionToCheck, GlobalServiceInfo serviceInfo, out Exception exceptionForService)
		{
			if(exceptionToCheck.ServiceInfo == serviceInfo)
			{
				exceptionForService = exceptionToCheck;
				return true;
			}

			if(exceptionToCheck.InnerException is { } innerException)
			{
				return TryFindContainedExceptionForService(innerException, serviceInfo, out exceptionForService);
			}

			exceptionForService = null;
			return false;
		}

		private static bool TryFindContainedExceptionForService(AggregateException exceptionsToCheck, GlobalServiceInfo serviceInfo, out Exception exceptionForService)
		{
			foreach(var innerException in exceptionsToCheck.InnerExceptions)
			{
				if(TryFindContainedExceptionForService(innerException, serviceInfo, out exceptionForService))
				{
					return true;
				}
			}

			exceptionForService = null;
			return false;
		}

		internal static ParameterInfo[] GetConstructorParameters(Type concreteType)
		{
			var constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
			if(constructors.Length == 0)
			{
				constructors = concreteType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
			}

			return constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault()?.GetParameters() ?? Array.Empty<ParameterInfo>();
		}

		private static async Task<(object[] arguments, int failedToGetArgumentAtIndex)> GetOrInitializeServices([DisallowNull] Type[] serviceTypes, [DisallowNull] HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene, object client = null)
		{
			int argumentCount = serviceTypes.Length;
			object[] arguments = new object[argumentCount];

			for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
			{
				if(!TryGetOrInitializeService(serviceTypes[argumentIndex], out arguments[argumentIndex], initialized, servicesInScene, initializersInScene, client))
				{
					return (Array.Empty<object>(), argumentIndex);
				}
			}

			for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
			{
				var parameterType = serviceTypes[argumentIndex];
				var argument = arguments[argumentIndex];

				if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
				{
					await loadArgumentTask;
					argument = await loadArgumentTask.GetResult();
					arguments[argumentIndex] = argument;
				}
			}

			for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
			{
				arguments[argumentIndex] = await InjectCrossServiceDependencies(arguments[argumentIndex], initialized, servicesInScene, initializersInScene);
			}

			return (arguments, -1);
		}

		private static async Task<int> GetOrInitializeServices([DisallowNull] Type[] serviceTypes, [DisallowNull] HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene, object[] arguments, int firstServiceIndex = 0, object client = null)
		{
			int serviceCount = serviceTypes.Length;
			for(int i = 0; i < serviceCount; i++)
			{
				if(!TryGetOrInitializeService(serviceTypes[i], out arguments[firstServiceIndex + i], initialized, servicesInScene, initializersInScene, client))
				{
					return i;
				}
			}

			for(int parameterIndex = 0; parameterIndex < serviceCount; parameterIndex++)
			{
				var parameterType = serviceTypes[parameterIndex];
				var argument = arguments[parameterIndex];

				if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
				{
					await loadArgumentTask;
					argument = await loadArgumentTask.GetResult();
					arguments[parameterIndex] = argument;
				}
			}

			for(int i = 0; i < serviceCount; i++)
			{
				arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, servicesInScene, initializersInScene);
			}

			return -1;
		}

		private static bool TargetIsAssignableOrConvertibleToType(IInitializer initializer, GlobalServiceInfo serviceInfo)
		{
			if(serviceInfo.concreteType != null)
			{
				return initializer.TargetIsAssignableOrConvertibleToType(serviceInfo.concreteType);
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				if(initializer.TargetIsAssignableOrConvertibleToType(definingType))
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryGetOrInitializeService(Type definingType, out object service, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene, object client = null)
		{
			if(TryGetServiceFor(client, definingType, out service, initialized, servicesInScene, initializersInScene))
			{
				return true;
			}

			if(TryGetServiceInfo(definingType, out var serviceInfo))
			{
				service = GetOrInitializeService(serviceInfo, initialized, servicesInScene, initializersInScene, serviceInfo.concreteType is null && !definingType.IsAbstract ? definingType : null);
				return service is not null;
			}

			return false;
		}

		private static async
		#if UNITY_2023_1_OR_NEWER
		Awaitable<object>
		#else
		System.Threading.Tasks.Task<object>
		#endif
		InstantiateFromAsset([DisallowNull] GameObject gameObject, GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			if(TryGetServiceOrServiceProviderComponent(gameObject, serviceInfo, out Component component))
			{
				return InstantiateFromAsset(component, serviceInfo, initialized, servicesInScene, initializersInScene);
			}

			#if UNITY_6000_0_OR_NEWER
			if(serviceInfo.LoadAsync)
			{
				var instances = await Object.InstantiateAsync(gameObject);
				gameObject = instances[0];
			}
			else
			#endif
			{
				gameObject = Object.Instantiate(gameObject);
			}

			return await GetServiceFromInstance(gameObject, serviceInfo);
		}

		private static async Task<object> InstantiateFromAsset(Component serviceOrProvider, GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			bool serviceIsAssignableFromAsset = IsInstanceOf(serviceInfo, serviceOrProvider);

			var concreteType = serviceInfo.concreteType;
			if(concreteType is null)
			{
				if(serviceIsAssignableFromAsset)
				{
					concreteType = serviceOrProvider.GetType();
				}
				else if(serviceOrProvider is IValueProvider valueProvider && valueProvider.TryGetFor(serviceOrProvider, out object value) && IsInstanceOf(serviceInfo, value))
				{
					concreteType = value.GetType();
				}
				else if(serviceInfo.LoadAsync && serviceOrProvider is IValueProviderAsync valueProviderAsync)
				{
					value = await valueProviderAsync.GetForAsync(serviceOrProvider);
					concreteType = value?.GetType();
				}
				
				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(concreteType is null)
				{
					Debug.LogWarning($"Unable to determine concrete type of service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} on '{serviceOrProvider.name}'.", serviceOrProvider);
					return null;
				}
				#endif
			}

			if(!initialized.Add(concreteType))
			{
				if(services.TryGetValue(concreteType, out object result))
				{
					return await GetServiceFromInstance(result, serviceInfo);
				}

				return null;
			}
			
			if(serviceIsAssignableFromAsset ? !ShouldInitialize(serviceOrProvider) : serviceOrProvider is IInitializer)
			{
				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
				#endif

				if(serviceInfo.ShouldInstantiate(true))
				{
					#if UNITY_6000_0_OR_NEWER
					if(serviceInfo.LoadAsync)
					{
						Object[] instances = await Object.InstantiateAsync(serviceOrProvider);
						return await GetServiceFromInstance(instances[0], serviceInfo);
					}
					#endif

					return await GetServiceFromInstance(Object.Instantiate(serviceOrProvider), serviceInfo);
				}

				return await GetServiceFromInstance(serviceOrProvider, serviceInfo);
			}

			foreach(var parameterTypes in GetParameterTypesForAllInitMethods(concreteType))
			{
				int parameterCount = parameterTypes.Length;
				object[] arguments = new object[parameterCount + 1];
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], serviceOrProvider, servicesInScene);
					continue;
				}

				arguments[0] = serviceOrProvider;

				var instantiateGenericArgumentTypes = new Type[parameterCount + 1];
				Array.Copy(parameterTypes, 0, instantiateGenericArgumentTypes, 1, parameterCount);
				instantiateGenericArgumentTypes[0] = concreteType;

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex + 1];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex + 1] = argument;
					}
				}

				MethodInfo instantiateMethod;
				instantiateMethod =
					#if UNITY_6000_0_OR_NEWER
					serviceInfo.LoadAsync
					? typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.InstantiateAsync), BindingFlags.Static | BindingFlags.Public)
																.Select(member => (MethodInfo)member)
																.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1)
																.MakeGenericMethod(instantiateGenericArgumentTypes) :
					#endif
					typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.Instantiate), BindingFlags.Static | BindingFlags.Public)
																.Select(member => (MethodInfo)member)
																.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1 && method.GetParameters().Length == parameterCount + 1)
																.MakeGenericMethod(instantiateGenericArgumentTypes);

				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Injecting {parameterCount} dependencies to {concreteType.Name}.");
				#endif

				var instance = instantiateMethod.Invoke(null, arguments);
				return await GetServiceFromInstance(instance, serviceInfo);
			}

			#if UNITY_6000_0_OR_NEWER
			if(serviceInfo.LoadAsync)
			{
				Object[] instances = await Object.InstantiateAsync(serviceOrProvider);
				return await GetServiceFromInstance(instances[0], serviceInfo);
			}
			#endif

			return await GetServiceFromInstance(Object.Instantiate(serviceOrProvider), serviceInfo);
		}

		private static async Task<object> AddComponent(GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			var concreteType = serviceInfo.concreteType;

			if(!initialized.Add(concreteType))
			{
				return services.TryGetValue(concreteType, out object result) ? result : null;
			}
			
			if(!ShouldInitialize(concreteType))
			{
				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
				#endif

				return container.AddComponent(concreteType);
			}

			foreach(var parameterTypes in GetParameterTypesForAllInitMethods(concreteType))
			{
				int parameterCount = parameterTypes.Length;
				object[] arguments = new object[parameterCount + 1];
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], container, servicesInScene);
					continue;
				}

				if(!container)
				{
					CreateServicesContainer();
				}

				arguments[0] = container;

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex + 1];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex + 1] = argument;
					}
				}

				var genericArgumentTypes = new Type[parameterCount + 1];
				Array.Copy(parameterTypes, 0, genericArgumentTypes, 1, parameterCount);
				genericArgumentTypes[0] = concreteType;

				MethodInfo method;
				method = typeof(AddComponentExtensions).GetMember(nameof(AddComponentExtensions.AddComponent), BindingFlags.Static | BindingFlags.Public)
																.Select(member => (MethodInfo)member)
																.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1 && method.GetParameters().Length == parameterCount + 1)
																.MakeGenericMethod(genericArgumentTypes);

				#if DEV_MODE && DEBUG_CREATE_SERVICES
				Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} attached to '{container.name}' and initialized with {parameterCount} dependencies successfully.", container);
				#endif

				return method.Invoke(null, arguments);
			}

			return container.AddComponent(concreteType);
		}

		internal static IEnumerable<Type[]> GetParameterTypesForAllInitMethods(Type clientConcreteType)
		{
			var interfaceTypes = clientConcreteType.GetInterfaces();
			for(int i = interfaceTypes.Length - 1; i >= 0; i--)
			{
				var interfaceType = interfaceTypes[i];
				if(interfaceType.IsGenericType && argumentCountsByIArgsTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					yield return interfaceType.GetGenericArguments();
				}
			}

			var parameters = ServiceInjector.GetConstructorParameters(clientConcreteType);
			if(parameters.Length > 0)
			{
				yield return parameters.Select(p => p.ParameterType).ToArray();
			}
		}

		private static async Task<object> InstantiateFromAsset(ScriptableObject serviceOrProvider, GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			bool serviceIsAssignableFromAsset = IsInstanceOf(serviceInfo, serviceOrProvider);
			var concreteType = serviceInfo.concreteType;
			if(concreteType is null)
			{
				if(serviceIsAssignableFromAsset)
				{
					concreteType = serviceOrProvider.GetType();
				}
				else if(serviceOrProvider is IValueProvider valueProvider && valueProvider.Value is object value && IsInstanceOf(serviceInfo, value))
				{
					concreteType = value.GetType();
				}
				else
				{
					Debug.LogWarning($"Unable to determine concrete type of service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} on '{serviceOrProvider.name}'.", serviceOrProvider);
					return null;
				}
			}

			if(!initialized.Add(concreteType))
			{
				return services.TryGetValue(concreteType, out object result) ? result : null;
			}

			if(serviceIsAssignableFromAsset ? !ShouldInitialize(serviceOrProvider) : serviceOrProvider is IInitializer)
			{
				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
				#endif

				if(serviceInfo.ShouldInstantiate(false))
				{
					#if UNITY_6000_0_OR_NEWER
					if(serviceInfo.LoadAsync)
					{
						Object[] instances = await Object.InstantiateAsync(serviceOrProvider);
						return instances[0];
					}
					#endif

					return Object.Instantiate(serviceOrProvider);
				}

				return serviceOrProvider;
			}

			foreach(var parameterTypes in GetParameterTypesForAllInitMethods(concreteType))
			{
				int parameterCount = parameterTypes.Length;
				var arguments = new object[parameterCount + 1];
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], serviceOrProvider, servicesInScene);
					continue;
				}

				arguments[0] = serviceOrProvider;

				var instantiateGenericArgumentTypes = new Type[parameterCount + 1];
				Array.Copy(parameterTypes, 0, instantiateGenericArgumentTypes, 1, parameterCount);
				instantiateGenericArgumentTypes[0] = concreteType;

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex + 1];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex + 1] = argument;
					}
				}

				MethodInfo instantiateMethod =
					#if UNITY_6000_0_OR_NEWER
					serviceInfo.LoadAsync
					? typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.InstantiateAsync), BindingFlags.Static | BindingFlags.Public)
																.Select(member => (MethodInfo)member)
																.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1)
																.MakeGenericMethod(instantiateGenericArgumentTypes) :
					#endif
					typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.Instantiate), BindingFlags.Static | BindingFlags.Public)
																.Select(member => (MethodInfo)member)
																.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1 && method.GetParameters().Length == parameterCount + 1)
																.MakeGenericMethod(instantiateGenericArgumentTypes);

				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Injecting {parameterCount} dependencies to {concreteType.Name}.");
				#endif

				return instantiateMethod.Invoke(null, arguments);
			}

			#if UNITY_6000_0_OR_NEWER
			if(serviceInfo.LoadAsync)
			{
				Object[] instances = await Object.InstantiateAsync(serviceOrProvider);
				return instances[0];
			}
			#endif

			return Object.Instantiate(serviceOrProvider);
		}

		/// <summary>
		/// Tries to get a component from the game object that matches the service info.
		/// <para>
		/// Failing that, tries to get a component that can provide a value that matches the service info,
		/// such as a Wrapper or an Initializer.
		/// </para>
		/// </summary>
		private static bool TryGetServiceOrServiceProviderComponent([DisallowNull] GameObject gameObject, [DisallowNull] GlobalServiceInfo serviceInfo, [NotNullWhen(true), MaybeNullWhen(false)] out Component result)
		{
			var concreteType = serviceInfo.concreteType;
			if(concreteType is not null && Find.typesToComponentTypes.TryGetValue(concreteType, out var componentTypes))
			{
				for(int i = componentTypes.Length - 1; i >= 0; i--)
				{
					if(gameObject.TryGetComponent(componentTypes[i], out result))
					{
						return true;
					}
				}
			}
			else
			{
				foreach(var definingType in serviceInfo.definingTypes)
				{
					if(definingType != concreteType && Find.typesToComponentTypes.TryGetValue(definingType, out componentTypes))
					{
						for(int i = componentTypes.Length - 1; i >= 0; i--)
						{
							if(gameObject.TryGetComponent(componentTypes[i], out Component component)
								&& Array.TrueForAll(serviceInfo.definingTypes, t => t.IsInstanceOfType(component)))
							{
								result = component;
								return true;
							}
						}
					}
				}
			}

			if(serviceInfo.classWithAttribute != concreteType && Find.typesToComponentTypes.TryGetValue(serviceInfo.classWithAttribute, out componentTypes))
			{
				for(int i = componentTypes.Length - 1; i >= 0; i--)
				{
					if(gameObject.TryGetComponent(componentTypes[i], out result))
					{
						return true;
					}
				}
			}

			var valueProviders = gameObject.GetComponents<IValueProvider>();
			foreach(var valueProvider in valueProviders)
			{
				var value = valueProvider.Value;
				if(value != null && IsInstanceOf(serviceInfo, value))
				{
					if(value is Component component)
					{
						result = component;
						return true;
					}

					if(Find.WrapperOf(value, out IWrapper wrapper))
					{
						result = wrapper as Component;
						return result != null;
					}
				}
			}

			foreach(var valueProvider in valueProviders)
			{
				if(valueProvider is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
				{
					result = initializer as Component;
					return true;
				}
			}

			result = null;
			return false;
		}

		private static async Task<object> GetServiceFromInstance([DisallowNull] object instance, [DisallowNull] GlobalServiceInfo serviceInfo)
		{
			if(serviceInfo.IsInstanceOf(instance))
			{
				return instance;
			}

			if(instance is GameObject gameObject)
			{
				return GetServiceFromInstance(gameObject, serviceInfo);
			}

			if(instance is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
			{
				return await initializer.InitTargetAsync();
			}

			if(instance is IWrapper wrapper && wrapper.WrappedObject is object wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
			{
				return wrappedObject;
			}

			if(serviceInfo.LoadAsync && instance is IValueProviderAsync valueProviderAsync)
			{
				return await valueProviderAsync.GetForAsync(instance as Component);
			}

			if(instance is IValueProvider valueProvider && valueProvider.TryGetFor(instance as Component, out var value) && IsInstanceOf(serviceInfo, value))
			{
				return value;
			}

			return instance;
		}

		/// <summary>
		/// Tries to get a component from the game object that matches the service info.
		/// <para>
		/// Failing that, tries to get a component that can provide a value that matches the service info,
		/// such as a Wrapper or an Initializer.
		/// </para>
		/// </summary>
		private static async Task<object> GetServiceFromInstance([DisallowNull] GameObject gameObject, [DisallowNull] GlobalServiceInfo serviceInfo)
		{
			var concreteType = serviceInfo.concreteType;
			if(concreteType is not null && Find.In(gameObject, serviceInfo.concreteType, out object found))
			{
				return found;
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				if(definingType != concreteType && Find.In(gameObject, definingType, out found) && IsInstanceOf(serviceInfo, found))
				{
					return found;
				}
			}

			if(serviceInfo.classWithAttribute != concreteType && Find.In(gameObject, serviceInfo.classWithAttribute, out var provider))
			{
				if(provider is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
				{
					return await initializer.InitTargetAsync();
				}

				if(provider is IWrapper wrapper && wrapper.WrappedObject is object wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
				{
					return wrappedObject;
				}

				if(serviceInfo.LoadAsync && provider is IValueProviderAsync valueProviderAsync)
				{
					return await valueProviderAsync.GetForAsync(provider as Component);
				}

				if(provider is IValueProvider valueProvider && valueProvider.TryGetFor(provider as Component, out var value) && IsInstanceOf(serviceInfo, value))
				{
					return value;
				}
			}

			var valueProviders = gameObject.GetComponents<IValueProvider>();
			foreach(var valueProvider in valueProviders)
			{
				if(valueProvider is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
				{
					return await initializer.InitTargetAsync();
				}

				if(valueProvider is IWrapper wrapper && wrapper.WrappedObject is object wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
				{
					return wrappedObject;
				}
			}

			foreach(var valueProvider in valueProviders)
			{
				if(valueProvider.TryGetFor(valueProvider as Component, out var value) && IsInstanceOf(serviceInfo, value))
				{
					return value;
				}
			}

			return null;
		}

		/// <summary>
		/// Tries to get a component from the game object that matches the service info.
		/// <para>
		/// Failing that, tries to get a component that can provide a value that matches the service info,
		/// such as a Wrapper or an Initializer.
		/// </para>
		/// </summary>
		private static async Task<object> GetServiceAsync([DisallowNull] ScriptableObject scriptableObject, [DisallowNull] GlobalServiceInfo serviceInfo)
		{
			var concreteType = serviceInfo.concreteType;
			if(concreteType is not null && Find.In(scriptableObject, serviceInfo.concreteType, out object found))
			{
				return found;
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				if(definingType != concreteType && Find.In(scriptableObject, definingType, out found) && IsInstanceOf(serviceInfo, found))
				{
					return found;
				}
			}

			if(serviceInfo.classWithAttribute != concreteType && Find.In(scriptableObject, serviceInfo.classWithAttribute, out var provider))
			{
				if(provider is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
				{
					return await initializer.InitTargetAsync();
				}

				if(provider is IWrapper wrapper && wrapper.WrappedObject is object wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
				{
					return wrappedObject;
				}

				if(provider is IValueProvider valueProvider && valueProvider.TryGetFor(provider as Component, out var value) && IsInstanceOf(serviceInfo, value))
				{
					return value;
				}

				if(serviceInfo.LoadAsync && provider is IValueProviderAsync valueProviderAsync)
				{
					return await valueProviderAsync.GetForAsync(null);
				}
			}

			return null;
		}

		private static bool IsInstanceOf([DisallowNull] GlobalServiceInfo serviceInfo, [AllowNull] object instance)
		{
			if(instance is null)
			{
				return false;
			}

			if(serviceInfo.concreteType is Type concreteType)
			{
				if(concreteType.IsGenericTypeDefinition)
				{
					bool matchFound = false;
					for(var type = concreteType; type != null; type = type.BaseType)
					{
						if(type.IsGenericType && type.GetGenericTypeDefinition() == concreteType)
						{
							matchFound = true;
							break;
						}
					}

					if(!matchFound)
					{
						return false;
					}
				}
				else if(!concreteType.IsInstanceOfType(instance))
				{
					return false;
				}
			}

			foreach(var definingType in serviceInfo.definingTypes)
			{
				if(definingType.IsGenericTypeDefinition)
				{
					bool matchFound = false;
					if(definingType.IsInterface)
					{
						foreach(var interfaceType in instance.GetType().GetInterfaces())
						{
							if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == definingType)
							{
								matchFound = true;
								break;
							}
						}
					}
					else
					{
						for(var type = instance.GetType(); type != null; type = type.BaseType)
						{
							if(type.IsGenericType && type.GetGenericTypeDefinition() == definingType)
							{
								matchFound = true;
								break;
							}
						}
					}

					if(!matchFound)
					{
						return false;
					}
				}
				else if(!definingType.IsInstanceOfType(instance))
				{
					return false;
				}
			}

			return true;
		}

		private static bool TryGetServiceFor([AllowNull] object client, Type definingType, out object service, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			if(services.TryGetValue(definingType, out service))
			{
				return true;
			}

			if(!uninitializedServices.TryGetValue(definingType, out var serviceInfo)
				|| Array.IndexOf(serviceInfo.definingTypes, definingType) == -1)
			{
				// Also try to find scene from ServiceTag and Services components in the scene.
				if(FillIfEmpty(servicesInScene).TryGetValue(definingType, out var scopedServiceInfo)
				&& (scopedServiceInfo.toClients == Clients.Everywhere
				|| (Find.In(scopedServiceInfo.service, out Transform serviceTransform)
				&& Service.IsAccessibleTo(serviceTransform, scopedServiceInfo.toClients, client as Transform))))
				{
					service = scopedServiceInfo.service;
					return true;
				}

				if(!definingType.IsGenericType)
				{
					return false;
				}

				// Handle open generic types.
				// E.g. with attribute [Service(typeof(ILogger<>))] on class Logger<T>,
				// when ILogger<Client> is requested, should return new Logger<Client>.
				var definingTypeDefinition = definingType.GetGenericTypeDefinition();
				if(!uninitializedServices.TryGetValue(definingTypeDefinition, out serviceInfo))
				{
					return false;
				}
			}

			#if DEV_MODE && DEBUG_LAZY_INIT
			Debug.Log($"Initializing service {definingType.Name} with LazyInit=true because it is a dependency of another service.");
			#endif

			if(!serviceInfo.IsTransient)
			{
				uninitializedServices.Remove(serviceInfo.concreteType);

				foreach(var serviceInfoDefiningType in serviceInfo.definingTypes)
				{
					uninitializedServices.Remove(serviceInfoDefiningType);
				}
			}

			var initializeServiceTask = InitializeService(serviceInfo, initialized, servicesInScene, initializersInScene, GetConcreteAndClosedType(serviceInfo, definingType));
			if(initializeServiceTask.IsCompleted)
			{
				service = initializeServiceTask.Result;
				if(service is null)
				{
					return false;
				}

				FinalizeServiceImmediate(serviceInfo, service);
				return true;
			}

			service = FinalizeServiceAsync(serviceInfo, initializeServiceTask);
			return true;
		}

		private static async Task<object> FinalizeServiceAsync(GlobalServiceInfo serviceInfo, Task<object> initializeServiceTask)
		{
			var service = await initializeServiceTask;
			FinalizeServiceImmediate(serviceInfo, service);
			return service;
		}

		private static object FinalizeServiceImmediate(GlobalServiceInfo serviceInfo, object service)
		{
			if(!serviceInfo.IsTransient)
			{
				SetInstanceSync(serviceInfo, service);
			}

			if(ServicesAreReady && !serviceInfo.FindFromScene)
			{
				SubscribeToUpdateEvents(service);
				ExecuteAwake(service);
				ExecuteOnEnable(service);
				ExecuteStartAtEndOfFrame(service);
			}

			return service;
		}

		private static void SetInstanceSync(GlobalServiceInfo serviceInfo, object service)
		{
			#if DEV_MODE
			if(serviceInfo.IsTransient)
			{
				Debug.LogError(TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType));
				return;
			}
			#endif

			services[service.GetType()] = service;

			foreach(var definingType in serviceInfo.definingTypes)
			{
				services[definingType] = service;
				ServiceUtility.SetInstance(definingType, service);
			}
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Warnings about missing Services should be suppressed when entering Play Mode from a scene
		/// which is not the first enabled one in build settings.
		/// </summary>
		private static bool IsFirstSceneInBuildSettingsLoaded()
		{
			string firstSceneInBuildsPath = Array.Find(EditorBuildSettings.scenes, s => s.enabled)?.path ?? "";
			Scene firstSceneInBuilds = SceneManager.GetSceneByPath(firstSceneInBuildsPath);
			return firstSceneInBuilds.IsValid() && firstSceneInBuilds.isLoaded;
		}
		#endif

		private static void InjectCrossServiceDependencies(List<GlobalServiceInfo> globalServices, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			foreach(var globalService in globalServices)
			{
				var concreteOrDefiningType = globalService.ConcreteOrDefiningType;
				if(!uninitializedServices.ContainsKey(concreteOrDefiningType)
					&& services.TryGetValue(concreteOrDefiningType, out var client))
				{
					_ = InjectCrossServiceDependencies(client, initialized, servicesInScene, initializersInScene);
				}
			}

			foreach(var scopedService in FillIfEmpty(servicesInScene).Values)
			{
				if(scopedService.service is Object service && service)
				{
					_ = InjectCrossServiceDependencies(scopedService.service, initialized, servicesInScene, initializersInScene);
				}
			}
		}

		private static async Task<object> InjectCrossServiceDependencies(object client, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			if(client is Task clientTask)
			{
				client = await clientTask.GetResult();
			}
			
			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			if(client is null)
			{
				return null;
			}
			#endif

			var concreteType = client.GetType();
			bool isInitialized = !initialized.Add(concreteType);

			if(isInitialized)
			{
				return client;
			}

			if(CanSelfInitializeWithoutInitializer(client) || HasInitializer(client))
			{
				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
				#endif

				return client;
			}

			var interfaceTypes = concreteType.GetInterfaces();
			for(int i = interfaceTypes.Length - 1; i >= 0; i--)
			{
				var interfaceType = interfaceTypes[i];
				if(!interfaceType.IsGenericType || !argumentCountsByIInitializableTypeDefinition.ContainsKey(interfaceType.GetGenericTypeDefinition()))
				{
					continue;
				}

				var parameterTypes = interfaceType.GetGenericArguments();
				(object[] arguments, int failedToGetArgumentAtIndex) = await GetOrInitializeServices(parameterTypes, initialized, servicesInScene, initializersInScene, client);
				if(failedToGetArgumentAtIndex != -1)
				{
					throw MissingInitArgumentsException.ForService(concreteType, parameterTypes[failedToGetArgumentAtIndex]);
				}

				var initMethod = interfaceType.GetMethod(nameof(IInitializable<object>.Init), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				initMethod.Invoke(client, arguments);

				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Service {concreteType.Name} received {parameterTypes.Length} dependencies successfully.");
				#endif

				return client;
			}

			return client;
		}

		private static void LogMissingDependencyWarning([DisallowNull] Type clientType, [DisallowNull] Type dependencyType, [AllowNull] Object context, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene)
		{
			if(!FillIfEmpty(servicesInScene).TryGetValue(dependencyType, out var serviceInfo))
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + servicesInScene.Count + uninitializedServices.Count} services.", context);
				return;
			}

			if(serviceInfo.service)
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but the service is only accessible to clients {serviceInfo.toClients}.", context);
				return;
			}

			Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but reference to the service seems to be broken in the scene component.", context);
		}

		#if UNITY_6000_0_OR_NEWER
		private static async Task<object> InstantiateServiceAsync(Component prefab, GlobalServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene, [DisallowNull] List<IInitializer> initializersInScene)
		{
			var concreteType = serviceInfo.concreteType;
			if(concreteType is null)
			{
				if(!IsInstanceOf(serviceInfo, prefab))
				{
					Debug.LogWarning($"Unable to determine concrete type of service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} on '{prefab.name}'.", prefab);
					return null;
				}

				concreteType = prefab.GetType();
			}

			if(!initialized.Add(concreteType))
			{
				return null;
			}

			if(!ShouldInitialize(prefab))
			{
				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
				#endif

				Component[] instances = await Object.InstantiateAsync(prefab);
				return instances[0];
			}

			foreach(var parameterTypes in GetParameterTypesForAllInitMethods(concreteType))
			{
				int parameterCount = parameterTypes.Length;
				object[] arguments = new object[parameterCount];
				bool allArgumentsAvailable = true;

				for(int argumentIndex = 0; argumentIndex < parameterCount; argumentIndex++)
				{
					var parameterType = parameterTypes[argumentIndex];
					if(!TryGetOrInitializeService(parameterType, out object firstArgument, initialized, servicesInScene, initializersInScene, prefab))
					{
						LogMissingDependecyWarning(concreteType, parameterType, prefab, servicesInScene);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					continue;
				}

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = arguments[parameterIndex + 1];

					if(argument is Task loadArgumentTask && !parameterType.IsInstanceOfType(argument))
					{
						await loadArgumentTask;
						argument = await loadArgumentTask.GetResult();
						arguments[parameterIndex + 1] = argument;
					}
				}

				arguments[0] = prefab;

				for(int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
				{
					arguments[parameterIndex + 1] = await InjectCrossServiceDependencies(arguments[parameterIndex + 1], initialized, servicesInScene, initializersInScene);
				}

				Type[] instantiateGenericArgumentTypes = new Type[parameterCount + 1];
				Array.Copy(parameterTypes, 0, instantiateGenericArgumentTypes, 1, parameterCount);
				instantiateGenericArgumentTypes[0] = concreteType;

				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Injecting {parameterCount} dependencies to service {concreteType.Name}.");
				#endif

				if(serviceInfo.LoadAsync)
				{
					var instantiateAsyncMethod = typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.InstantiateAsync), BindingFlags.Static | BindingFlags.Public)
												.Select(member => (MethodInfo)member)
												.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1)
												.MakeGenericMethod(instantiateGenericArgumentTypes);

					AsyncInstantiateOperation asyncInstantiateOperation = (AsyncInstantiateOperation)instantiateAsyncMethod.Invoke(null, arguments);
					await asyncInstantiateOperation;
					Object[] instances = asyncInstantiateOperation.Result;
					return instances[0];
				}

				MethodInfo instantiateMethod;
				instantiateMethod = typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.Instantiate), BindingFlags.Static | BindingFlags.Public)
									.Select(member => (MethodInfo)member)
									.FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1 && method.GetParameters().Length == parameterCount + 1)
									.MakeGenericMethod(instantiateGenericArgumentTypes);

				return instantiateMethod.Invoke(null, arguments);
			}

			return null;
		}
		#endif

		static void LogMissingDependecyWarning(Type clientType, Type dependencyType, Object context, [DisallowNull] Dictionary<Type, ScopedServiceInfo> servicesInScene)
		{
			if(!FillIfEmpty(servicesInScene).TryGetValue(dependencyType, out var serviceInfo))
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + servicesInScene.Count + uninitializedServices.Count} services:\n{string.Join("\n", services.Keys.Select(t => TypeUtility.ToString(t)).Concat(uninitializedServices.Keys.Select(t => TypeUtility.ToString(t))).Concat(servicesInScene.Values.Select(i => TypeUtility.ToString(i.service?.GetType()))))}", context);
				return;
			}
			
			if(!serviceInfo.service)
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but reference to the service seems to be broken in the scene component.", context);
				return;
			}

			Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but the service is only accessible to clients {serviceInfo.toClients}.", context);
		}

		/// <summary>
		/// Should other services be injected to this service by the service injector during application startup or not?
		/// </summary>
		private static bool ShouldInitialize(object client)
		{
			if(CanSelfInitializeWithoutInitializer(client)
			#if UNITY_EDITOR
			&& client is not IInitializableEditorOnly { InitState: InitState.Uninitialized }
			#endif
			)
			{
				return false;
			}

			if(HasInitializer(client))
			{
				return false;
			}

			return true;
		}

		private static void InjectServiceDependenciesForTypesThatRequireOnlyThem(out List<Task> injectAsyncServices
		#if UNITY_EDITOR
		, Dictionary<Type, List<ScriptableObject>> uninitializedScriptableObjects
		#endif
		)
		{
			injectAsyncServices = new List<Task>(32);

			if(services is null)
			{
				return;
			}

			var setMethodsByServiceCount = GetInitArgsSetMethodsByServiceCount();
			foreach(Type interfaceType in argumentCountsByIXArgumentsTypeDefinition.Keys)
			{
				foreach(var clientType in TypeUtility.GetImplementingTypes(interfaceType, interfaceType.Assembly, false, 0))
				{
					if(clientType.IsAbstract)
					{
						continue;
					}

					var task = TrySetDefaultServices(clientType, setMethodsByServiceCount
					#if UNITY_EDITOR
					, uninitializedScriptableObjects
					#endif
					);

					if(!task.IsCompleted)
					{
						injectAsyncServices.Add(task);
					}
				}
			}
		}

		private static Dictionary<int, InitArgsSetMethodInvoker> GetInitArgsSetMethodsByServiceCount() => new(MAX_INIT_ARGUMENT_COUNT)
		{
			{ 1, new InitArgsSetMethodInvoker(a=>InitArgs.Set(typeof(object),a))},
			{ 2, new InitArgsSetMethodInvoker((a,b)=>InitArgs.Set(typeof(object),a,b))},
			{ 3, new InitArgsSetMethodInvoker((a,b,c)=>InitArgs.Set(typeof(object),a,b,c))},
			{ 4, new InitArgsSetMethodInvoker((a,b,c,d)=>InitArgs.Set(typeof(object), a,b,c,d))},
			{ 5, new InitArgsSetMethodInvoker((a,b,c,d,e)=>InitArgs.Set(typeof(object), a,b,c,d,e))},
			{ 6, new InitArgsSetMethodInvoker((a,b,c,d,e,f)=>InitArgs.Set(typeof(object), a,b,c,d,e,f))},
			{ 7, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g))},
			{ 8, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g,h)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g,h))},
			{ 9, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g,h,i)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g,h,i))},
			{ 10, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g,h,i,j)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g,h,i,j))},
			{ 11, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g,h,i,j,k)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g,h,i,j,k))},
			{ 12, new InitArgsSetMethodInvoker((a,b,c,d,e,f,g,h,i,j,k,l)=>InitArgs.Set(typeof(object), a,b,c,d,e,f,g,h,i,j,k,l))},
		};

		private static async Task TrySetDefaultServices(Type clientType, Dictionary<int, InitArgsSetMethodInvoker> setMethodsByServiceCount
		#if UNITY_EDITOR
		, Dictionary<Type, List<ScriptableObject>> uninitializedScriptableObjects
		#endif
		)
		{
			Type[] interfaceTypes = clientType.GetInterfaces();
			for(int interfaceIndex = interfaceTypes.Length - 1; interfaceIndex >= 0; interfaceIndex--)
			{
				var interfaceType = interfaceTypes[interfaceIndex];
				if(!interfaceType.IsGenericType || !argumentCountsByIArgsTypeDefinition.TryGetValue(interfaceType.GetGenericTypeDefinition(), out int serviceCount))
				{
					continue;
				}

				if(!setMethodsByServiceCount.TryGetValue(serviceCount, out var setMethod))
				{
					#if DEV_MODE
					Debug.LogWarning($"InitArgs.Set method not found for {serviceCount} services.");
					#endif
					continue;
				}

				var serviceTypes = interfaceType.GetGenericArguments();
				bool allServicesAvailable = true;
				for(int serviceIndex = 0; serviceIndex < serviceCount; serviceIndex++)
				{
					var serviceType = serviceTypes[serviceIndex];
					if(!services.TryGetValue(serviceType, out object service))
					{
						allServicesAvailable = false;
						break;
					}

					setMethod.SetService(serviceIndex, serviceType, service);
				}

				if(!allServicesAvailable)
				{
#if DEV_MODE && DEBUG_DEFAULT_SERVICES
					Debug.Log($"{serviceCount} default services for clients of type {clientType.FullName} not found: {string.Join(", ", serviceTypes.Select(t => TypeUtility.ToString(t)))}");
#endif

					continue;
				}

#if DEV_MODE && DEBUG_DEFAULT_SERVICES
				Debug.Log($"Registering {serviceCount} default services for clients of type {clientType.FullName}: {string.Join(", ", serviceTypes.Select(t => TypeUtility.ToString(t)))}");
#endif

				setMethod.SetClientType(clientType);
				await setMethod.Invoke();

				#if UNITY_EDITOR
				// In builds ScriptableObjects' Awake method gets called at runtime when a scene or prefab with a component referencing said ScriptableObject gets loaded.
				// Because of this we can rely on ServiceInjector to execute via the RuntimeInitializeOnLoadMethod attribute before the Awake methods of ScriptableObjects are executed.
				// In the editor however, ScriptableObject can already get loaded in edit mode, in which case Awake gets executed before service injection has taken place.
				// For this reason we need to manually initialize these ScriptableObjects.
				if(uninitializedScriptableObjects.TryGetValue(clientType, out var scriptableObjects))
				{
					for(int s = scriptableObjects.Count - 1; s >= 0; s--)
					{
						ScriptableObject scriptableObject = scriptableObjects[s];
						if(scriptableObject is IInitializableEditorOnly initializableEditorOnly
							&& initializableEditorOnly.Initializer is IInitializer initializer)
						{
							try
							{
								initializer.InitTarget();
							}
							catch(Exception ex)
							{
								Debug.LogError(ex);
							}

							scriptableObjects.RemoveAt(s);
							continue;
						}

						try
						{
							foreach((Type iinitializableTypeDefinition, int argumentCount) in argumentCountsByIInitializableTypeDefinition)
							{
								if(argumentCount != serviceCount)
								{
									continue;
								}

								var initMethod = iinitializableTypeDefinition.MakeGenericType(serviceTypes).GetMethod(nameof(IInitializable<object>.Init), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
								initMethod.Invoke(scriptableObject, setMethod.clientTypeAndServices.AsSpan().Slice(1).ToArray());
							}
						}
						catch(Exception ex)
						{
							Debug.LogError(ex);
						}
					}

					uninitializedScriptableObjects.Remove(clientType);
				}
				#endif
			}
		}

		#if UNITY_EDITOR
		// In builds ScriptableObjects' Awake method gets called at runtime when a scene or prefab with a component
		// referencing said ScriptableObject gets loaded.
		// Because of this we can rely on ServiceInjector to execute via the RuntimeInitializeOnLoadMethod attribute
		// before the Awake methods of ScriptableObjects are executed.
		// In the editor however, ScriptableObject can already get loaded in edit mode, in which case Awake gets
		// executed before service injection has taken place.
		// For this reason we need to manually initialize these ScriptableObjects.
		private static void InitializeAlreadyLoadedScriptableObjectsInTheEditor(Dictionary<Type, List<ScriptableObject>> uninitializedScriptableObjects)
		{
			foreach(var scriptableObjects in uninitializedScriptableObjects.Values)
			{
				foreach(var scriptableObject in scriptableObjects)
				{
					if(scriptableObject is IInitializableEditorOnly initializableEditorOnly
					&& initializableEditorOnly.Initializer is IInitializer initializer)
					{
						initializer.InitTarget();
					}
				}
			}
		}
		#endif

		private static void SubscribeToUpdateEvents(object service)
		{
			if(service is IUpdate update)
			{
				Updater.Subscribe(update);
			}

			if(service is ILateUpdate lateUpdate)
			{
				Updater.Subscribe(lateUpdate);
			}

			if(service is IFixedUpdate fixedUpdate)
			{
				Updater.Subscribe(fixedUpdate);
			}
		}

		private static void ExecuteAwake(object service)
		{
			if(service is IAwake awake)
			{
				try
				{
					awake.Awake();
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
			}
		}

		private static void ExecuteOnEnable(object service)
		{
			if(service is IOnEnable onEnable)
			{
				try
				{
					onEnable.OnEnable();
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
			}
		}

		private static void ExecuteStartAtEndOfFrame(object service)
		{
			if(service is IStart start)
			{
				Updater.InvokeAtEndOfFrame(start.Start);
			}
		}

		[return: MaybeNull]
		internal static Type GetClassWithServiceAttribute(Type definingType)
			=> ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out var serviceInfo)
				? serviceInfo.classWithAttribute
				: null;

		internal static bool TryGetServiceInfo(Type definingType, [MaybeNullWhen(false), NotNullWhen(true)] out GlobalServiceInfo serviceInfo) => ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out serviceInfo);

		private static List<GlobalServiceInfo> GetServiceDefinitions() => ServiceAttributeUtility.concreteTypes.Values.Concat(ServiceAttributeUtility.definingTypes.Values.Where(d => d.concreteType is null)).ToList();

		internal static bool CanProvideService<TService>() => services.ContainsKey(typeof(TService)) || uninitializedServices.ContainsKey(typeof(TService));

		internal static bool TryGetUninitializedServiceInfo(Type requestedType, out GlobalServiceInfo info)
		{
			if(uninitializedServices.TryGetValue(requestedType, out info))
			{
				return true;
			}

			if(!requestedType.IsGenericType)
			{
				return false;
			}

			var requestedTypeDefinition = requestedType.GetGenericTypeDefinition();
			if(uninitializedServices.TryGetValue(requestedTypeDefinition, out info))
			{
				return true;
			}

			return false;
		}

		private sealed class ScopedServiceInfo
		{
			public readonly Type definingType;
			public readonly Object service;
			public readonly Clients toClients;

			public ScopedServiceInfo(Type definingType, Object service, Clients toClients)
			{
				this.definingType = definingType;
				this.service = service;
				this.toClients = toClients;
			}
		}

		private sealed class InitArgsSetMethodInvoker
		{
			public readonly object[] clientTypeAndServices;
			private readonly Type[] serviceTypes;
			private readonly MethodInfo methodDefinition;

			public InitArgsSetMethodInvoker(Expression<Action<object>> expression) : this(GetGenericMethodDefinition(expression), 1, 2) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object>> expression) : this(GetGenericMethodDefinition(expression), 2, 3) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 3, 4) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 4, 5) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 5, 6) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 6, 7) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 7, 8) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 8, 9) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 9, 10) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 10, 11) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 11, 12) { }
			public InitArgsSetMethodInvoker(Expression<Action<object, object, object, object, object, object, object, object, object, object, object, object>> expression) : this(GetGenericMethodDefinition(expression), 12, 13) { }

			public InitArgsSetMethodInvoker(MethodInfo methodDefinition, int genericArgumentCount, int parameterCount)
			{
				this.methodDefinition = methodDefinition;
				serviceTypes = new Type[genericArgumentCount];
				clientTypeAndServices = new object[parameterCount];
			}

			private static MethodInfo GetGenericMethodDefinition(LambdaExpression expression) => ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();

			public void SetClientType(Type clientType) => clientTypeAndServices[0] = clientType;

			public void SetService(int index, Type serviceType, object service)
			{
				#if DEV_MODE && DEBUG
				Debug.Assert(index >= 0, index);
				Debug.Assert(index < serviceTypes.Length, $"{index} < {serviceTypes.Length}");
				Debug.Assert(index + 1 < clientTypeAndServices.Length, $"{index} + 1 < {clientTypeAndServices.Length}");
				Debug.Assert(serviceType is not null);
				Debug.Assert(service is not null);
				#endif

				serviceTypes[index] = serviceType;
				clientTypeAndServices[index + 1] = service;
			}

			public async Task Invoke()
			{
				bool needToAwait = false;
				var methodToInvoke = methodDefinition.MakeGenericMethod(serviceTypes);
				var loadArgumentTasks = Enumerable.Empty<Task>();
				int argumentCount = serviceTypes.Length;
				for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					var serviceType = serviceTypes[argumentIndex];
					var service = clientTypeAndServices[argumentIndex + 1];
					if(!serviceType.IsInstanceOfType(service) && service is Task loadServiceTask)
					{
						needToAwait = true;
						loadArgumentTasks = loadArgumentTasks.Append(loadServiceTask);
					}
				}

				object[] arguments;
				if(!needToAwait)
				{
					arguments = clientTypeAndServices;
				}
				else
				{
					arguments = (object[])clientTypeAndServices.Clone();

					#if DEBUG || INIT_ARGS_SAFE_MODE
					try
					{
					#endif

					await Task.WhenAll(loadArgumentTasks);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					}
					catch(Exception e)
					{
						Debug.LogWarning(e.ToString());
					}
					#endif
				}

				for(int serviceIndex = argumentCount; serviceIndex >= 1; serviceIndex--)
				{
					if(arguments[serviceIndex] is Task finishedTask)
					{
						arguments[serviceIndex] = await finishedTask.GetResult();

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(finishedTask.IsFaulted)
						{
							Debug.LogWarning($"Can not set default services for {TypeUtility.ToString((Type)arguments[0])} because initialization of dependency #{serviceIndex} (zero-based) failed.");
							return;
						}
						#endif

						#if DEV_MODE
						if(arguments[serviceIndex] is Task) Debug.LogWarning("Suspicious looking service: " + arguments[serviceIndex]);
						#endif
					}
				}

				#if DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

				methodToInvoke.Invoke(null, arguments);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception e)
				{
					Debug.LogError($"Failed to set default services for {TypeUtility.ToString((Type)arguments[0], '.')}.\nservices: {string.Join(", ", arguments.Skip(1).Select(obj => TypeUtility.ToString(obj?.GetType())))}\nloadArgumentTasks:{loadArgumentTasks.Count()}\n{e}");
				}
				#endif
			}
		}

		private sealed class NullInitializer : IInitializer
		{
			public Object Target { get => null; set { } }

			public object InitTarget() => this;

			public bool TargetIsAssignableOrConvertibleToType(Type type) => false;
		}
	}
}
#endif