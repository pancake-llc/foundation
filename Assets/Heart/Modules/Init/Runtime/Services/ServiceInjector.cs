//#define DEBUG_INIT_SERVICES
//#define DEBUG_CREATE_SERVICES
//#define DEBUG_LAZY_INIT
//#define DEBUG_INIT_TIME
//#define DEBUG_TEAR_DOWN
#define DEBUG_LOAD_SCENE

#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sisus.Init.ValueProviders;
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
		private static readonly Dictionary<Type, ServiceInfo> uninitializedServices = new(); // Lazy and transient

		private static GameObject container;
		#if DEV_MODE || DEBUG
		private static readonly HashSet<ServiceInfo> exceptionsLogged = new();
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

			#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			#else
			UnityEngine.Application.quitting -= OnExitingApplicationOrPlayMode;
			UnityEngine.Application.quitting += OnExitingApplicationOrPlayMode;
			#endif

			#if UNITY_EDITOR
			static void OnPlayModeStateChanged(PlayModeStateChange state)
			{
				if(state is PlayModeStateChange.ExitingPlayMode)
				{
					ServicesAreReady = false;
					OnExitingApplicationOrPlayMode();
				}
			}
			#endif

			static void OnExitingApplicationOrPlayMode()
			{
				var services = ServiceInjector.services.ToArray();
				ServiceInjector.services.Clear();

				uninitializedServices.Clear();

				// TODO: Keep track of initialization order, and tear down in reverse order.
				foreach(var typeAndService in services)
				{
					var serviceType = typeAndService.Key;
					if(ServiceAttributeUtility.concreteTypes.TryGetValue(serviceType, out var serviceInfo))
					{
						HandleTearDown(typeAndService.Value, serviceType, serviceInfo);
					}
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void HandleTearDown(object service, Type serviceType, ServiceInfo serviceInfo)
			{
				while(service is Task task)
				{
					if(task.IsCompletedSuccessfully)
					{
						service = task.GetResult().Result;
					}
					else
					{
						#if DEV_MODE && DEBUG_TEAR_DOWN
						if(!task.IsFaulted)
						{
							Debug.Log($"service[{TypeUtility.ToString(serviceType)} was a {TypeUtility.ToString(service.GetType())} with Status:{task.Status} during {nameof(OnExitingApplicationOrPlayMode)}.");
						}
						#endif
							
						if(!task.IsCompleted)
						{
							task.ContinueWith(t => HandleTearDown(t.GetResult(), serviceType, serviceInfo), TaskContinuationOptions.OnlyOnRanToCompletion);
						}

						return;
					}
				}

				// Components can handle cleaning themselves up via OnDestroy
				// and should never be subscribed to Updater events.
				if(service is Component)
				{
					return;
				}

				if(service == NullExtensions.Null)
				{
					return;
				}

				// TODO: Don't just try to determine this again here, but cache this during initialization.
				// E.g. the Wrapper object could already be destroyed by this point.
				if(!ShouldInvokeUnityEvents(serviceInfo, service))
				{
					return;
				}

				if(service is Object unityObject)
				{
					#if UNITY_EDITOR
					if(AssetDatabase.Contains(unityObject))
					{
						return;
					}
					#endif

					Object.Destroy(unityObject);
					return;
				}

				if(service is IUpdate updateable)
				{
					Updater.Unsubscribe(updateable);
				}

				if(service is ILateUpdate lateUpdateable)
				{
					Updater.Unsubscribe(lateUpdateable);
				}

				if(service is IFixedUpdate fixedUpdateable)
				{
					Updater.Unsubscribe(fixedUpdateable);
				}

				ExecuteOnDisable(service);
				ExecuteOnDestroy(service);
				ExecuteDispose(service);
			}
		}
		#endif

		private static bool ShouldInvokeUnityEvents(ServiceInfo serviceInfo, object service)
		{
			if(service is Component || serviceInfo.FindFromScene || Find.typesToWrapperTypes.ContainsKey(service.GetType()) || Find.WrapperOf(service) is not null)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Creates instances of all services,
		/// injects dependencies for services that implement an <see cref="IInitializable{T}"/>
		/// interface targeting only other services,
		/// and uses <see cref="InitArgs.Set{T}"/> to assign references to services ready to be retrieved
		/// for any other classes that implement an <see cref="IArgs{T}"/> interface targeting only services.
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
			int asyncServiceCount = services.Values.Count(s => s is Task { IsCompleted: false });
			#endif

			while(services.Values.FirstOrDefault(s => s is Task { IsCompleted: false } ) is Task task)
			{
				await task;
			}

			#if DEV_MODE && DEBUG_INIT_TIME
			timer.Stop();
			Debug.Log($"Initialization of {asyncServiceCount} async services took {timer.Elapsed.TotalSeconds} seconds.");
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

			services = new(definitionCount);

			// List of concrete service types that have already been initialized (instance created / retrieved)
			HashSet<Type> initialized = new(definitionCount);

			var localServices = new LocalServices();

			InitializeServices(globalServices, initialized, localServices);

			InjectCrossServiceDependencies(globalServices, initialized, localServices);

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
					// TODO: add InitState to keep track of whether this has already been done or not
					// Or maybe bool shouldExecuteEventFunctions, which is only set to true for
					// objects created by ServiceInjector in the first place, and set to true
					// when ExecuteEventFunctions is called.

					if(ServiceAttributeUtility.concreteTypes.TryGetValue(concreteType, out var serviceInfo)
						&& serviceInfo.FindFromScene)
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
			container = new GameObject("Global Services");
			container.SetActive(false);
			Object.DontDestroyOnLoad(container);
		}

		internal static Task<object> Register(ServiceInfo serviceInfo) // TODO: Add overload accepting an array -> and support initializing them in optimal order automatically!
		{
			if (!serviceInfo.LazyInit)
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

		private static void InitializeServices(List<ServiceInfo> globalServiceInfos, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
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
				var failedServiceInfo = serviceInfo;
				#endif

				_ = GetOrInitializeService(serviceInfo, initialized, localServices, overrideConcreteType: null)
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				.ContinueWith(OnGetOrInitializeServiceFailed, TaskContinuationOptions.OnlyOnFaulted)
				#endif
				;

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				#if UNITY_6000_0_OR_NEWER
				[HideInCallstack]
				#endif
				void OnGetOrInitializeServiceFailed(Task task)
				{
					if(task.Exception is { } aggregateException)
					{
						if(aggregateException.InnerException is not InitArgsException initArgsException)
						{
							initArgsException = ServiceInitFailedException.Create(failedServiceInfo, ServiceInitFailReason.ExceptionWasThrown, aggregateException);
						}
						
						initArgsException.LogToConsole();
					}
					else
					{
						Debug.LogError($"Initializing service {failedServiceInfo.ConcreteOrDefiningType} failed for an unknown reason.");
					}
				}
				#endif
			}
		}

		/// <param name="requestedServiceType"> The type of the initialization argument being requested for the client. Could be abstract. </param>
		internal static async Task<object> LazyInit([DisallowNull] ServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
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
				#if DEV_MODE
				Debug.Assert(!serviceInfo.IsTransient, concreteType.Name);
				#endif
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
			Debug.Log($"LazyInit {TypeUtility.ToString(concreteType)} with IsTransient:{isTransient}");
			#endif

			var initialized = new HashSet<Type>(0);
			var localServices = new LocalServices();
			service = await GetOrInitializeService(serviceInfo, initialized, localServices, concreteType);

			#if DEV_MODE
			Debug.Assert(service is not Task);
			#endif

			#if UNITY_EDITOR
			if(container && container.TryGetComponent(out ServicesDebugger debugger))
			{
				_ = debugger.SetServices(services.Values.Distinct());
			}
			#endif

			await InjectCrossServiceDependencies(service, initialized, localServices);

			if(!serviceInfo.FindFromScene)
			{
				await HandleExecutingEventFunctionsFor(service);
			}

			return service;
		}

		[return: MaybeNull]
		private static Type GetConcreteAndClosedType([DisallowNull] ServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
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

		private static async Task<object> GetOrInitializeService(ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices, [AllowNull] Type overrideConcreteType)
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
				task = InitializeService(concreteType, serviceInfo.serviceProviderType, serviceInfo.loadMethod,  serviceInfo.referenceType, serviceInfo, initialized, localServices);
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
				Debug.LogError($"GetOrCreateInstance(concreteOrDefiningType:{concreteOrDefiningType.Name}, definingTypes:{TypeUtility.ToString(serviceInfo.definingTypes)}) returned instance was null.");
				#endif
				return null;
			}
			#endif

			#if DEV_MODE
			Debug.Assert(IsInstanceOf(serviceInfo, result) || serviceInfo.IsValueProvider, serviceInfo.ConcreteOrDefiningType.Name);
			#endif
			
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				throw new TaskCanceledException("Aborted initialization of async because no longer in Play Mode.");
			}
			#endif

			FinalizeServiceImmediate(serviceInfo, result);
			return result;
		}

		private static async Task<object> InitializeService(Type concreteType, ServiceProviderType serviceProviderType, LoadMethod loadMethod, ReferenceType referenceType, [DisallowNull] ServiceInfo serviceInfo, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			object result;

			switch(serviceProviderType)
			{
				case ServiceProviderType.ServiceInitializer:
					if(initialized.Contains(concreteType))
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies);
					}

					var serviceInitializer = (IServiceInitializer) await GetOrCreateServiceProvider(serviceInfo, initialized, localServices);
					var interfaceTypes = serviceInfo.serviceOrProviderType.GetInterfaces();
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
						int failedToGetArgumentAtIndex;
						
						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						try
						{
						#endif
						
							failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, localServices, arguments);
						
						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						}
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ExceptionWasThrown, ex);
						}
						#endif
						
						if(failedToGetArgumentAtIndex != -1)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingDependency, null, null, serviceInitializer, null, null, parameterTypes[failedToGetArgumentAtIndex], localServices);
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
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderThrewException, null, null, serviceInitializer, ex, null, null, localServices);
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

						if(result is not null)
						{
							#if DEV_MODE && DEBUG_CREATE_SERVICES
							Debug.Log($"Service {concreteType.Name} created via service initializer {serviceInitializer.GetType().Name} successfully.");
							#endif

							return result;
						}

						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						// InitTarget methods that accept arguments should never return null.
						if(parameterCount > 0)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, serviceInitializer, null, null, null, localServices);
						}
						#endif
					}

					if(parameterCount == 0)
					{
						try
						{
							result = serviceInitializer.InitTarget(Array.Empty<object>());
						}
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderThrewException, null, null, serviceInitializer, ex, null, null, localServices);
						}

						if(result is not null)
						{
							initialized.Add(concreteType);
							return result is Task task ? await task.GetResult() : result;
						}

						if(FilterForServiceProvider(serviceInfo.serviceOrProviderType, loadMethod, referenceType) != (LoadMethod.Default, ReferenceType.None))
						{
							loadMethod = LoadMethod.Default;
							referenceType = ReferenceType.None;
						}

						// InitTarget methods that accept zero arguments may return null, and it is not an error,
						// but just means that Init(args) should take care of creating the service...
						initialized.Remove(concreteType);
						return InitializeService(concreteType, ServiceProviderType.None, loadMethod, referenceType, serviceInfo, initialized, localServices);
					}

					break;
				case ServiceProviderType.ServiceInitializerAsync:
					if(initialized.Contains(concreteType))
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies);
					}

					var serviceInitializerAsync = (IServiceInitializerAsync) await GetOrCreateServiceProvider(serviceInfo, initialized, localServices);
					interfaceTypes = serviceInfo.serviceOrProviderType.GetInterfaces();
					parameterCount = 0;
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
						object[] arguments = new object[parameterCount];
						int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, localServices, arguments);
						if(failedToGetArgumentAtIndex != -1)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingDependency, null, null, serviceInitializerAsync, null, null, parameterTypes[failedToGetArgumentAtIndex], localServices);
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
						// InitTarget methods that accept arguments should never return null.
						if(task is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, serviceInitializerAsync, null, null, null, localServices);
						}
						#endif

						try
						{
							result = await task.GetResult();
						}
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderThrewException, null, null, serviceInitializerAsync, ex, null, null, localServices);
						}

						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						// InitTargetAsync should never return null.
						if(result is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, serviceInitializerAsync, null, null, null, localServices);
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {concreteType.Name} created via async service initializer {serviceInitializerAsync.GetType().Name} successfully.");
						#endif

						return result;
					}

					if(parameterCount == 0)
					{
						Task<object> task;
						try
						{
							task = serviceInitializerAsync.InitTargetAsync(Array.Empty<object>());
						}
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderThrewException, null, null, serviceInitializerAsync, ex, null, null, localServices);
						}

						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						if(task is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, serviceInitializerAsync, null, null, null, localServices);
						}
						#endif

						try
						{
							result = await task;
						}
						catch(Exception ex)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderThrewException, null, null, serviceInitializerAsync, ex, null, null, localServices);
						}

						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						// InitTargetAsync should never return null.
						if(result is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, serviceInitializerAsync, null, null, null, localServices);
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {concreteType.Name} created via async service initializer {serviceInitializerAsync.GetType().Name} successfully.");
						#endif

						return result;
					}

					break;
				case ServiceProviderType.IValueProvider:
				case ServiceProviderType.IValueProviderAsync:
				case ServiceProviderType.IValueProviderT:
				case ServiceProviderType.IValueProviderAsyncT:
				case ServiceProviderType.IValueByTypeProvider:
				case ServiceProviderType.IValueByTypeProviderAsync:
					if(initialized.Contains(concreteType))
					{
						throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies);
					}

					result = await GetOrCreateServiceProvider(serviceInfo, initialized, localServices);

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service provider {TypeUtility.ToString(result.GetType())} successfully registered with defining types: {TypeUtility.ToString(serviceInfo.definingTypes)}.", result as Object);
					#endif

					return result;
			}
			
			if(loadMethod is LoadMethod.FindFromScene)
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

				if(serviceInfo.SceneName is { Length: > 0 } sceneName && !SceneManager.GetSceneByName(sceneName).isLoaded)
				{
					await LoadDependenciesOfServicesInScene(sceneName, initialized, localServices);
					
					if(serviceInfo.LoadAsync)
					{
						#if DEV_MODE && DEBUG_LOAD_SCENE
						Debug.Log($"Loading scene '{sceneName}' asynchronously to initialize service {TypeUtility.ToString(concreteType)}...");
						#endif

						var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); 
						#if UNITY_6000_0_OR_NEWER
						await asyncOperation;
						#else
						while(!asyncOperation.isDone)
						{
							await Task.Yield();
						}
						#endif
					}
					else
					{
						#if DEV_MODE && DEBUG_LOAD_SCENE
						Debug.Log($"Loading scene '{sceneName}' to initialize service {TypeUtility.ToString(concreteType)}...");
						#endif

						SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
						var scene = SceneManager.GetSceneByName(sceneName);
						while(!scene.isLoaded)
						{
							await Task.Yield();
						}
					}
				}
				else if(serviceInfo.SceneBuildIndex is var sceneBuildIndex and not -1 && !SceneManager.GetSceneByBuildIndex(sceneBuildIndex).isLoaded)
				{
					await LoadDependenciesOfServicesInScene(sceneBuildIndex, initialized, localServices);
					
					if(serviceInfo.LoadAsync)
					{
						#if DEV_MODE && DEBUG_LOAD_SCENE
						Debug.Log($"Loading scene #{sceneBuildIndex} asynchronously to initialize service {TypeUtility.ToString(concreteType)}...");
						#endif

						var asyncOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
						#if UNITY_6000_0_OR_NEWER
						await asyncOperation;
						#else
						while(!asyncOperation.isDone)
						{
							await Task.Yield();
						}
						#endif
					}
					else
					{
						#if DEV_MODE && DEBUG_LOAD_SCENE
						Debug.Log($"Loading scene #{sceneBuildIndex} to initialize service {TypeUtility.ToString(concreteType)}...");
						#endif
						
						SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Additive);
						var scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
						while(!scene.isLoaded)
						{
							await Task.Yield();
						}
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
					Object.FindObjectOfType(serviceInfo.serviceOrProviderType, true);
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

			if(referenceType is ReferenceType.ResourcePath)
			{
				var resourcePath = serviceInfo.ResourcePath;
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
					throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingResource, null, null, null, null, null, null, localServices);
				}
				#endif

				if(asset is GameObject gameObject)
				{
					if(serviceInfo.ShouldInstantiate(true))
					{
						result = await InstantiateFromAsset(concreteType, gameObject, serviceInfo, initialized, localServices);

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset, null, null, null, null, null, localServices);
						}
						#endif

						#if DEV_MODE && DEBUG_CREATE_SERVICES
						Debug.Log($"Service {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} instantiated from prefab at path 'Resources/{resourcePath}' successfully.", asset);
						#endif
					}
					else
					{
						result = await GetServiceFromInstance(concreteType, gameObject, serviceInfo);

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
						result = await InstantiateFromAsset(concreteType, scriptableObject, serviceInfo, initialized, localServices);

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
						result = await GetServiceAsync(concreteType, scriptableObject, serviceInfo);

						#if DEBUG || INIT_ARGS_SAFE_MODE
						if(result is null)
						{
							Debug.LogWarning($"Service Not Found: No service of type {TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} was found on the resource at path 'Resources/{resourcePath}'.", asset);
							return null;
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
			if(referenceType is ReferenceType.AddressableKey)
			{
				return await InitializeAddressableAsset(concreteType, serviceInfo.AddressableKey, serviceInfo, initialized, localServices);
			}
			#endif

			if(typeof(Component).IsAssignableFrom(concreteType))
			{
				if(!container)
				{
					CreateServicesContainer();
				}

				if(ShouldInitialize(concreteType))
				{
					result = await AddComponent(serviceInfo, initialized, localServices);

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
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CircularDependencies, null, null, null, null, null, null, localServices);
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
						ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingDependency, null, null, null, null, null, parameterType, localServices);
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

					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					int maxAttempts = 3;
					#endif

					while(!parameterType.IsInstanceOfType(argument)
						#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
						&& --maxAttempts >= 0
						#endif
					)
					{
						if(argument is Task loadArgumentTask)
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
								throw CreateAggregateException(exception, ServiceInitFailedException.Create(serviceInfo, failReason, asset:null, missingDependencyType:parameterType, localServices:localServices));
							}
							#endif

							argument = await loadArgumentTask.GetResult();
							arguments[parameterIndex] = argument;
						}
						else if(ServiceAttributeUtility.definingTypes.TryGetValue(parameterType, out var argumentServiceInfo))
						{
							switch(argumentServiceInfo.serviceProviderType)
							{
								case ServiceProviderType.ServiceInitializer:
								case ServiceProviderType.ServiceInitializerAsync:
								case ServiceProviderType.IValueProviderT:
								case ServiceProviderType.IValueProviderAsyncT:
								case ServiceProviderType.IValueByTypeProvider:
								case ServiceProviderType.IValueByTypeProviderAsync:
								case ServiceProviderType.IValueProvider:
								case ServiceProviderType.IValueProviderAsync:
									if(ValueProviderUtility.TryGetValueProviderValue(argument, parameterType, out var providedValue))
									{
										argument = providedValue;
										arguments[parameterIndex] = argument;
									}
									else
									{
										throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingDependency, null, null, null, null, null, parameterType, localServices);
										//throw ServiceInitFailedException.Create(argumentServiceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, null, null, null, parameterType, localServices);
										//throw ServiceInitFailedException.Create(argumentServiceInfo, ServiceInitFailReason.ServiceProviderResultNotConvertible, null, null, null, null, concreteType:providedValue.GetType(), parameterType, localServices);
										//throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.ServiceProviderResultNull, null, null, null, null, null, parameterType, localServices);
									}
									break;
								default:
									throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingDependency, null, null, null, null, null, parameterType, localServices);
							}
						}
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

			if(!concreteType.IsValueType && !Array.Exists(constructors, c => c.GetParameters().Length == 0))
			{
				throw MissingInitArgumentsException.ForService(concreteType, constructorsByParameterCount.FirstOrDefault()?.GetParameters().Select(p => p.ParameterType).ToArray(), localServices);
			}

			result = Activator.CreateInstance(concreteType);

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {TypeUtility.ToString(concreteType)} created successfully via default constructor.");
			#endif

			return result;
		}

		private static async Task LoadDependenciesOfServicesInScene(string sceneName, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			var servicesInScene = GetServiceDefinitions().Where(s => s.SceneName == sceneName).ToArray();
			var dependencies = GetExternalDependenciesOfServices(servicesInScene, initialized, localServices);
			
			#if DEV_MODE && DEBUG_LOAD_SCENE
			Debug.Log($"Loading dependencies of services in scene '{sceneName}':\n{TypeUtility.ToString(dependencies, "\n")}");
			#endif
			
			var loadTasks = new List<Task>(0);

			foreach(var dependencyType in dependencies)
			{
				if(TryGetOrInitializeService(dependencyType, out object service, initialized, localServices) && service is Task task)
				{
					loadTasks.Add(task);
				}
			}
			
			await Task.WhenAll(loadTasks);
		}
		
		private static async Task LoadDependenciesOfServicesInScene(int sceneBuildIndex, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			var servicesInScene = GetServiceDefinitions().Where(s => s.SceneBuildIndex == sceneBuildIndex).ToArray();
			var dependencies = GetExternalDependenciesOfServices(servicesInScene, initialized, localServices);
			
			#if DEV_MODE && DEBUG_LOAD_SCENE
			Debug.Log($"Loading dependencies of services in scene with build index {sceneBuildIndex}:\n{TypeUtility.ToString(dependencies, "\n")}");
			#endif
			
			var loadTasks = new List<Task>(0);
			
			foreach(var dependencyType in dependencies)
			{
				if(TryGetOrInitializeService(dependencyType, out var service, initialized, localServices) && service is Task task)
				{
					loadTasks.Add(task);
				}
			}
			
			await Task.WhenAll(loadTasks);
		}
		
		/// <summary>
		/// Gets types of all dependencies that services have, excluding dependencies between the services themselves.
		/// </summary>
		private static HashSet<Type> GetExternalDependenciesOfServices(ServiceInfo[] services, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			HashSet<Type> dependencyTypes = new();

			foreach(var service in services)
			{
				GetAllDependencies(service.ConcreteOrDefiningType, dependencyTypes);
			}

			foreach(var service in services)
			{
				dependencyTypes.Remove(service.concreteType);
				foreach(var definingType in service.definingTypes)
				{
					dependencyTypes.Remove(definingType);
				}
			}

			foreach(var dependencyType in dependencyTypes)
			{
				TryGetOrInitializeService(dependencyType, out _, initialized, localServices);
			}

			return dependencyTypes;
		}

		internal async static Task<object> GetOrCreateServiceProvider(ServiceInfo serviceInfo, HashSet<Type> initialized, LocalServices localServices)
		{
			var type = serviceInfo.serviceOrProviderType;

			if(services.TryGetValue(type, out var serviceProvider))
			{
				return serviceProvider;
			}

			var (loadMethod, referenceType) = FilterForServiceProvider(type, serviceInfo.loadMethod, serviceInfo.referenceType);

			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			try
			{
			#endif

			serviceProvider = await InitializeService(type, ServiceProviderType.None, loadMethod, referenceType, serviceInfo, initialized, localServices);

			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			}
			catch(Exception ex)
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CreatingServiceProviderFailed, null, null, null, ex, null, null, localServices);
			}
			#endif

			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			if(serviceProvider is null)
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.CreatingServiceProviderFailed, null, null, null, null, null, null, localServices);
			}
			#endif

			return serviceProvider;
		}

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		private static async Task<Object> LoadAddressableAsset(string addressableKey, ServiceInfo serviceInfo)
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
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingAddressable, asset);
			}
			#endif

			return asset;
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		private static async Task<object> InitializeAddressableAsset(Type concreteType, string addressableKey, ServiceInfo serviceInfo, HashSet<Type> initialized, LocalServices localServices)
		{
			var asset = await LoadAddressableAsset(addressableKey, serviceInfo);

			object result;
			if(asset is GameObject gameObject)
			{
				if(serviceInfo.ShouldInstantiate(true))
				{
					result = await InstantiateFromAsset(concreteType, gameObject, serviceInfo, initialized, localServices);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset, null, null, null, null, null, localServices);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {concreteType.Name} Instantiated from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}

				result = await GetServiceFromInstance(concreteType, gameObject, serviceInfo);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset, null, null, null, null, null, localServices);
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
					result = await InstantiateFromAsset(concreteType, scriptableObject, serviceInfo, initialized, localServices);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset, null, null, null, null, null, localServices);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {serviceInfo.concreteType.Name} Instantiated from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}
				else
				{
					result = await GetServiceAsync(concreteType, scriptableObject, serviceInfo);

					#if DEBUG || INIT_ARGS_SAFE_MODE
					if(result is null) throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.MissingComponent, asset, null, null, null, null, null, localServices);
					#endif

					#if DEV_MODE && DEBUG_CREATE_SERVICES
					Debug.Log($"Service {serviceInfo.concreteType.Name} loaded from addressable asset \"{addressableKey}\" successfully.", asset);
					#endif

					return result;
				}
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(!IsInstanceOf(serviceInfo, asset))
			{
				throw ServiceInitFailedException.Create(serviceInfo, ServiceInitFailReason.AssetNotConvertible, asset, null, null, null, concreteType:asset.GetType(), null, localServices);
			}
			#endif

			#if DEV_MODE && DEBUG_CREATE_SERVICES
			Debug.Log($"Service {serviceInfo.concreteType.Name} loaded from addressable asset \"{addressableKey}\" successfully.", asset);
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

		private static bool TryFindContainedExceptionForService(Exception exceptionToCheck, ServiceInfo serviceInfo, out Exception exceptionForService)
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

		private static bool TryFindContainedExceptionForService(CircularDependenciesException exceptionToCheck, ServiceInfo serviceInfo, out Exception exceptionForService)
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

		private static bool TryFindContainedExceptionForService(AggregateException exceptionsToCheck, ServiceInfo serviceInfo, out Exception exceptionForService)
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

		internal static async Task<(object[] arguments, int failedToGetArgumentAtIndex)> GetOrInitializeServices([DisallowNull] Type[] serviceTypes, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices, object client = null)
		{
			int argumentCount = serviceTypes.Length;
			object[] arguments = new object[argumentCount];

			for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
			{
				if(!TryGetOrInitializeService(serviceTypes[argumentIndex], out arguments[argumentIndex], initialized, localServices, client))
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
				arguments[argumentIndex] = await InjectCrossServiceDependencies(arguments[argumentIndex], initialized, localServices);
			}

			return (arguments, -1);
		}

		internal static async Task<int> GetOrInitializeServices([DisallowNull] Type[] serviceTypes, [DisallowNull] HashSet<Type> initialized, [DisallowNull] LocalServices localServices, object[] arguments, int argumentsFirstServiceIndex = 0, object client = null)
		{
			int serviceCount = serviceTypes.Length;
			for(int i = 0; i < serviceCount; i++)
			{
				if(!TryGetOrInitializeService(serviceTypes[i], out arguments[argumentsFirstServiceIndex + i], initialized, localServices, client))
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
				arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, localServices);
			}

			return -1;
		}

		private static bool TargetIsAssignableOrConvertibleToType(IInitializer initializer, ServiceInfo serviceInfo)
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

		private static bool TryGetOrInitializeService(Type definingType, out object service, HashSet<Type> initialized, [DisallowNull] LocalServices localServices, object client = null)
		{
			if(TryGetServiceFor(client, definingType, out service, initialized, localServices))
			{
				return true;
			}

			if(TryGetServiceInfo(definingType, out var serviceInfo))
			{
				service = GetOrInitializeService(serviceInfo, initialized, localServices, serviceInfo.concreteType is null && !definingType.IsAbstract ? definingType : null);
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
		InstantiateFromAsset(Type concreteType, [DisallowNull] GameObject gameObject, ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(TryGetServiceOrServiceProviderComponent(concreteType, gameObject, serviceInfo, out Component component))
			{
				return InstantiateFromAsset(concreteType, component, serviceInfo, initialized, localServices);
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

			return await GetServiceFromInstance(concreteType, gameObject, serviceInfo);
		}

		private static async Task<object> InstantiateFromAsset(Type concreteType, Component serviceOrProvider, ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			bool serviceIsAssignableFromAsset = IsInstanceOf(serviceInfo, serviceOrProvider);

			concreteType ??= serviceInfo.concreteType;
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
					return await GetServiceFromInstance(concreteType, result, serviceInfo);
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
						return await GetServiceFromInstance(concreteType, instances[0], serviceInfo);
					}
					#endif

					return await GetServiceFromInstance(concreteType, Object.Instantiate(serviceOrProvider), serviceInfo);
				}

				return await GetServiceFromInstance(concreteType, serviceOrProvider, serviceInfo);
			}

			foreach(var parameterTypes in GetParameterTypesForAllInitMethods(concreteType))
			{
				int parameterCount = parameterTypes.Length;
				object[] arguments = new object[parameterCount + 1];
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, localServices, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], serviceOrProvider, localServices);
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
				return await GetServiceFromInstance(concreteType, instance, serviceInfo);
			}

			#if UNITY_6000_0_OR_NEWER
			if(serviceInfo.LoadAsync)
			{
				Object[] instances = await Object.InstantiateAsync(serviceOrProvider);
				return await GetServiceFromInstance(concreteType, instances[0], serviceInfo);
			}
			#endif

			return await GetServiceFromInstance(concreteType, Object.Instantiate(serviceOrProvider), serviceInfo);
		}

		private static async Task<object> AddComponent(ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
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
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, localServices, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], container, localServices);
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

		internal static IEnumerable<Type[]> GetParameterTypesForAllInitMethods([DisallowNull] Type clientConcreteType)
		{
			#if DEV_MODE
			Debug.Assert(clientConcreteType is not null);
			#endif

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

		private static async Task<object> InstantiateFromAsset(Type concreteType, ScriptableObject serviceOrProvider, ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			concreteType ??= serviceInfo.concreteType;
			bool serviceIsAssignableFromAsset = concreteType?.IsInstanceOfType(serviceOrProvider) ?? IsInstanceOf(serviceInfo, serviceOrProvider);
			
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
				int failedToGetArgumentAtIndex = await GetOrInitializeServices(parameterTypes, initialized, localServices, arguments, 1);
				if(failedToGetArgumentAtIndex != -1)
				{
					LogMissingDependencyWarning(concreteType, parameterTypes[failedToGetArgumentAtIndex], serviceOrProvider, localServices);
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
		private static bool TryGetServiceOrServiceProviderComponent(Type concreteType, [DisallowNull] GameObject gameObject, [DisallowNull] ServiceInfo serviceInfo, [NotNullWhen(true), MaybeNullWhen(false)] out Component result)
		{
			concreteType ??= serviceInfo.concreteType;
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

			if(serviceInfo.serviceOrProviderType != concreteType && Find.typesToComponentTypes.TryGetValue(serviceInfo.serviceOrProviderType, out componentTypes))
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
						return result;
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

		private static async Task<object> GetServiceFromInstance([AllowNull] Type concreteType, [DisallowNull] object instance, [DisallowNull] ServiceInfo serviceInfo)
		{
			if(concreteType is not null && concreteType.IsInstanceOfType(instance))
			{
				return instance;
			}
			
			if(serviceInfo.IsInstanceOf(instance))
			{
				return instance;
			}

			if(instance is GameObject gameObject)
			{
				return GetServiceFromInstance(concreteType, gameObject, serviceInfo);
			}

			if(instance is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
			{
				return await initializer.InitTargetAsync();
			}

			if(instance is IWrapper wrapper && wrapper.WrappedObject is { } wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
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
		private static async Task<object> GetServiceFromInstance([AllowNull] Type concreteType, [DisallowNull] GameObject gameObject, [DisallowNull] ServiceInfo serviceInfo)
		{
			concreteType ??= serviceInfo.concreteType;
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

			if(serviceInfo.serviceOrProviderType != concreteType && Find.In(gameObject, serviceInfo.serviceOrProviderType, out var provider))
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
		private static async Task<object> GetServiceAsync([AllowNull] Type concreteType, [DisallowNull] ScriptableObject scriptableObject, [DisallowNull] ServiceInfo serviceInfo)
		{
			concreteType ??= serviceInfo.concreteType;
			if(concreteType is not null && Find.In(scriptableObject, concreteType, out object found))
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

			if(serviceInfo.serviceOrProviderType != concreteType && Find.In(scriptableObject, serviceInfo.serviceOrProviderType, out var provider))
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

		private static bool IsInstanceOf([DisallowNull] ServiceInfo serviceInfo, [AllowNull] object instance)
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

		private static bool TryGetServiceFor([AllowNull] object client, Type definingType, out object service, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(services.TryGetValue(definingType, out service))
			{
				return true;
			}

			if(!uninitializedServices.TryGetValue(definingType, out var serviceInfo)
				|| Array.IndexOf(serviceInfo.definingTypes, definingType) == -1)
			{
				// Also try to find scene from ServiceTag and Services components in the scene.
				if(localServices.TryGetInfo(definingType, out var localServiceInfo)
				&& (localServiceInfo.toClients == Clients.Everywhere
				//|| (Find.In(localServiceInfo.serviceOrProvider, out Transform serviceOrProviderTransform)
				|| (localServiceInfo.serviceOrProvider is Component serviceOrProviderComponent
				&& serviceOrProviderComponent
				&& Service.IsAccessibleTo(client as Transform, serviceOrProviderComponent.transform, localServiceInfo.toClients))))
				{
					// TODO: Need to initialize first !!!!
					// Should this be handled in LocalServices?

					service = localServiceInfo.serviceOrProvider;
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

			var initializeServiceTask = InitializeService(GetConcreteAndClosedType(serviceInfo, definingType), serviceInfo.serviceProviderType, serviceInfo.loadMethod, serviceInfo.referenceType, serviceInfo, initialized, localServices);
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

		private static async Task<object> FinalizeServiceAsync(ServiceInfo serviceInfo, Task<object> initializeServiceTask)
		{
			var service = await initializeServiceTask;
			
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				throw new TaskCanceledException("Aborted initialization of async service because no longer in Play Mode.");
			}
			#endif
			
			FinalizeServiceImmediate(serviceInfo, service);
			return service;
		}

		private static object FinalizeServiceImmediate(ServiceInfo serviceInfo, object service)
		{
			if(!serviceInfo.IsTransient)
			{
				if(serviceInfo.IsValueProvider)
				{
					#if DEV_MODE
					if(!ValueProviderUtility.IsValueProvider(service))
					{
						Debug.LogError("ServiceInfo.IsValueProvider was true but IsTransientServiceProvider was false." +
							$"Concrete type: {TypeUtility.ToString(serviceInfo.concreteType)}\n" +
							$"Defining types: {TypeUtility.ToString(serviceInfo.definingTypes)}");
					}
					#endif

					AddServiceProviderSync(serviceInfo, service);
				}
				else if(IsInstanceOf(serviceInfo, service))
				{
					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(!IsInstanceOf(serviceInfo, service))
					{
						Debug.LogError($"Instance {TypeUtility.ToString(service.GetType())} is not assignable to all the configured defining types of the service, and is not a value provider." +
						$"Concrete type: {TypeUtility.ToString(serviceInfo.concreteType)}\n" +
						$"Defining types: {TypeUtility.ToString(serviceInfo.definingTypes)}");
					}
					#endif

					SetInstanceSync(serviceInfo, service);
				}
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

		private static void SetInstanceSync(ServiceInfo serviceInfo, object service)
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
				ServiceUtility.Set(definingType, service);
			}
		}

		private static void AddServiceProviderSync(ServiceInfo serviceInfo, object serviceProvider)
		{
			#if DEV_MODE
			if(serviceInfo.IsTransient)
			{
				Debug.LogError(TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType));
				return;
			}
			#endif

			services[serviceInfo.concreteType] = serviceProvider;

			if(!container)
			{
				CreateServicesContainer();
			}

			Component registerer = container.transform;

			foreach(var definingType in serviceInfo.definingTypes)
			{
				services[definingType] = serviceProvider;
				ServiceUtility.AddFor(serviceProvider, serviceInfo.serviceProviderType, definingType, Clients.Everywhere, registerer);
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

		private static void InjectCrossServiceDependencies(List<ServiceInfo> globalServices, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			foreach(var globalService in globalServices)
			{
				var concreteOrDefiningType = globalService.ConcreteOrDefiningType;
				if(!uninitializedServices.ContainsKey(concreteOrDefiningType)
					&& services.TryGetValue(concreteOrDefiningType, out var client))
				{
					_ = InjectCrossServiceDependencies(client, initialized, localServices);
				}
			}

			foreach(var localService in localServices.All())
			{
				#if UNITY_EDITOR || INIT_ARGS_SAFE_MODE
				if(!localService)
				{
					continue;
				}
				#endif

				_ = InjectCrossServiceDependencies(localService, initialized, localServices);
			}
		}

		private static async Task<object> InjectCrossServiceDependencies(object client, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
		{
			if(client is Task clientTask)
			{
				client = await clientTask.GetResult();
				
				#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					throw new TaskCanceledException("Aborted injection of cross-service dependencies because no longer in Play Mode.");
				}
				#endif
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
				(object[] arguments, int failedToGetArgumentAtIndex) = await GetOrInitializeServices(parameterTypes, initialized, localServices, client);
				if(failedToGetArgumentAtIndex != -1)
				{
					#if DEBUG
					if(ShouldSelfGuardAgainstNull(client))
					{
						throw MissingInitArgumentsException.ForService(concreteType, parameterTypes[failedToGetArgumentAtIndex], localServices);
					}
					#endif
					
					continue;
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
		
		private static void LogMissingDependencyWarning([DisallowNull] Type clientType, [DisallowNull] Type dependencyType, [AllowNull] Object context, [DisallowNull] LocalServices localServices)
		{
			if(!localServices.TryGetInfo(dependencyType, out var serviceInfo))
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + uninitializedServices.Count} global and {localServices.GetCountSlow()} local services.", context);
				return;
			}

			if(serviceInfo.serviceOrProvider)
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but the service is only accessible to clients {serviceInfo.toClients}.", context);
				return;
			}

			Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but reference to the service seems to be broken in the scene component.", context);
		}

		#if UNITY_6000_0_OR_NEWER
		private static async Task<object> InstantiateServiceAsync(Component prefab, ServiceInfo serviceInfo, HashSet<Type> initialized, [DisallowNull] LocalServices localServices)
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
					if(!TryGetOrInitializeService(parameterType, out arguments[argumentIndex], initialized, localServices, prefab))
					{
						LogMissingDependecyWarning(concreteType, parameterType, prefab, localServices);
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
					arguments[parameterIndex + 1] = await InjectCrossServiceDependencies(arguments[parameterIndex + 1], initialized, localServices);
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

		static void LogMissingDependecyWarning(Type clientType, Type dependencyType, Object context, [DisallowNull] LocalServices localServices)
		{
			if(!localServices.TryGetInfo(dependencyType, out var serviceInfo))
			{
				Debug.LogError($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + uninitializedServices.Count} global and {localServices.GetCountSlow()} local services:\n{TypeUtility.ToString(services.Keys.Concat(uninitializedServices.Keys).Concat(uninitializedServices.Keys).Concat(localServices.All().Select(o => o?.GetType())), "\n")}", context);
				return;
			}
			
			if(!serviceInfo.serviceOrProvider)
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

		/// <summary>
		/// Should other services be injected to this service by the service injector during application startup or not?
		/// </summary>
		private static bool ShouldInitialize(Type clientType) => !CanSelfInitializeWithoutInitializer(clientType);

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
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					awake.Awake();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
				#endif
			}
		}

		private static void ExecuteOnEnable(object service)
		{
			if(service is IOnEnable onEnable)
			{
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					onEnable.OnEnable();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
				#endif
			}
		}
		
		private static void ExecuteOnDisable(object service)
		{
			if(service is IOnDisable onDisable)
			{
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					onDisable.OnDisable();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
				#endif
			}
		}
		
		private static void ExecuteOnDestroy(object service)
		{
			if(service is IOnDestroy onDestroy)
			{
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					onDestroy.OnDestroy();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
				#endif
			}
		}
		
		private static void ExecuteDispose(object service)
		{
			if(service is IDisposable disposable)
			{
				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				try
				{
				#endif

					disposable.Dispose();

				#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
				}
				catch(Exception ex)
				{
					Debug.LogError(ex);
				}
				#endif
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
				? serviceInfo.serviceOrProviderType
				: null;

		internal static bool TryGetServiceInfo(Type definingType, [MaybeNullWhen(false), NotNullWhen(true)] out ServiceInfo serviceInfo) => ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out serviceInfo);

		private static List<ServiceInfo> GetServiceDefinitions() => ServiceAttributeUtility.concreteTypes.Values.Concat(ServiceAttributeUtility.definingTypes.Values.Where(d => d.concreteType is null)).ToList();

		internal static bool CanProvideService<TService>() => services.ContainsKey(typeof(TService)) || uninitializedServices.ContainsKey(typeof(TService));

		internal static bool TryGetUninitializedServiceInfo(Type requestedType, out ServiceInfo info)
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

		private static (LoadMethod, ReferenceType) FilterForServiceProvider(Type serviceProviderType, LoadMethod loadMethod, ReferenceType referenceType)
			=> typeof(Object).IsAssignableFrom(serviceProviderType) ? (loadMethod, referenceType) : (LoadMethod.Default, ReferenceType.None);
	}
}
#endif