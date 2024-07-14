//#define DEBUG_INIT_SERVICES
//#define DEBUG_DEFAULT_SERVICES
//#define DEBUG_CREATE_SERVICES
//#define DEBUG_LAZY_INIT

#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Sisus.Init.Internal.InitializerUtility;
using Object = UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
using Sisus.Init.EditorOnly;
using Debug = UnityEngine.Debug;
using TypeCollection = UnityEditor.TypeCache.TypeCollection;
#else
using TypeCollection = System.Collections.Generic.IEnumerable<System.Type>;
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
		/// have been created, initialized and are ready to be used by clients; otherwise, <see langword="false"/>.
        /// <para>
        /// This only takes into consideration services that are initialized synchronously during game initialization.
		/// To determine if all asynchronously initialized services are also ready to be used,
		/// use <see cref="AsyncServicesAreReady"/> instead.
        /// </para>
        /// <para>
        /// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>.
        /// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
        /// Services that are registered manually using <see cref="Service.SetInstance"/> are also not
        /// guaranteed to be loaded even if this is <see langword="true"/>.
        /// </para>
        /// </summary>
        public static bool ServicesAreReady { get; private set; }

        /// <summary>
		/// <see langword="true"/> if all shared services that are loaded asynchronously during game initialization
		/// have been created, initialized and are ready to be used by clients; otherwise, <see langword="false"/>.
        /// </para>
		/// <para>
        /// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>.
		/// </para>
		/// <para>
        /// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
        /// Services that are registered manually using <see cref="Service.SetInstance"/> are also not
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
        /// Called when all services that are initialized asynchrously have been created,
        /// initialized and are ready to be used by clients.
        /// </summary>
        public static event Action AsyncServicesBecameReady;
        internal static Dictionary<Type, object> services = new Dictionary<Type, object>();
        internal static readonly Dictionary<Type, ServiceInfo> uninitializedServices = new Dictionary<Type, ServiceInfo>();
        private static GameObject container;

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

			static void OnExitingPlayMode() => ServicesAreReady = false;
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
            #if DEV_MODE
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
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

            #if DEV_MODE
            Debug.Log($"Initialization of {services.Count} service took {timer.Elapsed.TotalSeconds} seconds.");
            #endif

            await Task.WhenAll(injectAsyncServices);

            #if DEV_MODE
            timer.Stop();
            Debug.Log($"Injection of {injectAsyncServices.Count} async service took {timer.Elapsed.TotalSeconds} seconds.");
            #endif

            AsyncServicesAreReady = true;
            AsyncServicesBecameReady?.Invoke();
            AsyncServicesBecameReady = null;
        }

        private static void CreateInstancesOfAllServices()
		{
			var serviceInfos = GetServiceDefinitions();
			int definitionCount = serviceInfos.Count;
			if(definitionCount == 0)
			{
				services = null;
				return;
			}

			services = new Dictionary<Type, object>(definitionCount);

			// List of concrete service types that have already been initialized (instance created / retrieved)
			HashSet<Type> initialized = new HashSet<Type>();

			Dictionary<Type, ScopedServiceInfo> scopedServiceInfos = new(0);

			CreateServices(serviceInfos, initialized, scopedServiceInfos);

			InjectCrossServiceDependencies(serviceInfos, initialized, scopedServiceInfos);

            #if UNITY_EDITOR
			_ = CreateServicesDebugger();
            #endif

			if(container != null)
			{
				container.SetActive(true);
			}

			HandleExecutingEventFunctions(initialized);

			static void HandleExecutingEventFunctions(HashSet<Type> concreteServiceTypes)
			{
				foreach(var concreteType in concreteServiceTypes)
				{
					if(services.TryGetValue(concreteType, out object instance))
					{
						SubscribeToUpdateEvents(instance);
						ExecuteAwake(instance);
						ExecuteOnEnable(instance);
						ExecuteStartAtEndOfFrame(instance);
					}
				}
			}
		}

        private static Dictionary<Type, ScopedServiceInfo> FillIfEmpty(Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
		{
            if(scopedServiceInfos.Count > 0)
			{
                return scopedServiceInfos;
			}

            var servicesComponents = 
            #if UNITY_2022_2_OR_NEWER
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
					    scopedServiceInfos[definingType] = new(definingType, service, toClients);
                    }
				}
			}

			var serviceTags =
            #if UNITY_2022_2_OR_NEWER
			Object.FindObjectsByType<ServiceTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            #else
            Object.FindObjectsOfType<ServiceTag>(true);
            #endif
            
			foreach(var serviceTag in serviceTags)
			{
                if(serviceTag.DefiningType is Type definingType && serviceTag.Service is Component service)
                {
				    scopedServiceInfos[definingType] = new(definingType, service, serviceTag.ToClients);
                }
			}

            // To avoid wasting resources try to rebuild the dictionary again and again, make sure it contains at least one entry
            if(scopedServiceInfos.Count == 0)
			{
                scopedServiceInfos.Add(typeof(void), new(typeof(void), null, Clients.Everywhere));
			}

			return scopedServiceInfos;
		}

        #if UNITY_EDITOR
		private static async Task CreateServicesDebugger()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            if(container == null)
            {
                CreateServicesContainer();
            }

            var debugger = container.AddComponent<ServicesDebugger>();
            await debugger.SetServices(services.Values.Distinct());
        }
        #endif

        private static void ForEachService(Action<object> action)
		{
            foreach(var service in services.Values.Distinct())
			{
                if(service is Task task)
				{
                    task.ContinueWith(InvokeOnResult);
                }
                else
                {
                    action(service);
                }
			}

            void InvokeOnResult(Task task) => action(task.GetResult());
		}

		private static void CreateServicesContainer()
        {
            container = new GameObject("Services");
            container.SetActive(false);
            container.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(container);
        }

        private static void CreateServices(List<ServiceInfo> serviceInfos, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            foreach(var serviceInfo in serviceInfos)
			{
				if(serviceInfo.LazyInit || serviceInfo.ConcreteOrDefiningType.IsGenericTypeDefinition)
				{
                    #if DEV_MODE && DEBUG_LAZY_INIT
                    Debug.Log($"Will not initialize {(serviceInfo.concreteType ?? serviceInfo.definingTypes[0]).FullName} yet because it has LazyInit set to true");
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

				_ = GetOrCreateInstance(serviceInfo, initialized, scopedServiceInfos, null);
			}
		}

        /// <param name="requestedServiceType"> The type of the initialization argument being requested for the client. Could be abstract. </param>
        /// <returns></returns>
        internal static async Task LazyInit([DisallowNull] ServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
        {
            #if DEV_MODE || DEBUG || SAFE_MODE
            if(requestedServiceType.IsGenericTypeDefinition)
			{
                Debug.LogError($"LazyInit called with {nameof(requestedServiceType)} {TypeUtility.ToString(requestedServiceType)} that was a generic type definition. This should not happen.");
                return;
			}
            #endif

            if(GetConcreteAndClosedType(serviceInfo, requestedServiceType) is not Type concreteType)
			{
                #if DEV_MODE
                Debug.LogWarning($"LazyInit({TypeUtility.ToString(serviceInfo.ConcreteOrDefiningType)} as {TypeUtility.ToString(requestedServiceType)}) called but could not determine {nameof(concreteType)} for service. Should this ever happen? Should the method be renamed to TryLazyInit?");
                #endif
                return;
			}

            // If service has already been initialized, no need to do anything.
            if(services.TryGetValue(concreteType, out object service))
            {
                return;
            }

            if(serviceInfo.ConcreteOrDefiningType is Type concreteOrDefiningType)
            {
                if(concreteOrDefiningType.IsGenericTypeDefinition ?
                  !uninitializedServices.ContainsKey(concreteOrDefiningType)
                : !uninitializedServices.Remove(concreteOrDefiningType))
			    {
                    return;
			    }
            }

            var definingTypes = serviceInfo.definingTypes;
            foreach(var definingType in definingTypes)
            {
                uninitializedServices.Remove(definingType);
            }

            #if DEV_MODE && DEBUG_LAZY_INIT
            Debug.Log($"LazyInit({(serviceInfo.concreteType ?? serviceInfo.definingTypes[0]).FullName})");
            #endif

            var initialized = new HashSet<Type>();
            service = await GetOrCreateInstance(serviceInfo, initialized, null, concreteType);

            #if UNITY_EDITOR
            if(container != null && container.TryGetComponent(out ServicesDebugger debugger))
            {
                _ = debugger.SetServices(services.Values.Distinct());
            }
            #endif

            await InjectCrossServiceDependencies(service, initialized, null, definingTypes);

            [return: MaybeNull]
			static Type GetConcreteAndClosedType([DisallowNull] ServiceInfo serviceInfo, [DisallowNull] Type requestedServiceType)
			{
				Type concreteType = serviceInfo.concreteType;
				if(concreteType is null)
				{
					if(!requestedServiceType.IsAbstract)
					{
						return requestedServiceType;
					}

					concreteType = Array.Find(serviceInfo.definingTypes, t => !t.IsAbstract);
                    if(concreteType is null)
					{
                        return null;
					}
				}

				if(!concreteType.IsGenericTypeDefinition)
				{
					return concreteType;
				}

				if(!requestedServiceType.IsAbstract
				&& requestedServiceType.GetGenericTypeDefinition().IsAssignableFrom(concreteType))
				{
					return requestedServiceType;
				}
				
                if(requestedServiceType.IsGenericType)
				{
					var requestedServiceTypeGenericArguments = requestedServiceType.GetGenericArguments();
					if(requestedServiceTypeGenericArguments.Length == concreteType.GetGenericArguments().Length)
					{
						concreteType = concreteType.MakeGenericType(requestedServiceTypeGenericArguments);
						if(requestedServiceType.IsAssignableFrom(concreteType))
						{
                            return concreteType;
						}
					}
				}

				return Array.Find(serviceInfo.definingTypes, t => !t.IsAbstract);
			}
        }

        private static async Task<object> GetOrCreateInstance(ServiceInfo serviceInfo, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos, Type overrideConcreteType)
        {
            var concreteType = overrideConcreteType ?? serviceInfo.concreteType;
            if((concreteType ?? serviceInfo.definingTypes.FirstOrDefault()) is not Type concreteOrDefiningType)
			{
                return null;
			}

            // If one class contains multiple Service attributes still create only one shared instance.
            if(services.TryGetValue(concreteOrDefiningType, out var existingInstance))
            {
                if(existingInstance is Task existingTask)
				{
                    existingInstance = await existingTask.GetResult();
				}

                return existingInstance;
            }

            Task<object> task;
            try
			{
				task = LoadInstance(serviceInfo, initialized, scopedServiceInfos);
			}
            catch(MissingMethodException e)
			{
                Debug.LogWarning($"Failed to initialize service {concreteOrDefiningType.Name} with defining types {string.Join(", ", serviceInfo.definingTypes.Select(t => t?.Name))}.\nThis can happen when constructor arguments contain circular references (for example: object A requires object B, but object B also requires object A, so neither object can be constructed).\n{e}");
                return null;
			}
			catch(Exception e)
			{
				Debug.LogWarning($"Failed to initialize service {concreteOrDefiningType.Name} with defining types {string.Join(", ", serviceInfo.definingTypes.Select(t => t?.Name))}.\n{e}");
                return null;
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

            if(result is null)
            {
                #if DEV_MODE
                Debug.LogWarning($"GetOrCreateInstance(concreteOrDefiningType:{concreteOrDefiningType.Name}, definingTypes:{string.Join(", ", serviceInfo.definingTypes.Select(t => t?.Name))}) returned instance was null.");
                #endif
                return null;
            }

            SetInstanceSync(serviceInfo, result);
            return result;
        }

        private static async Task<object> LoadInstance(ServiceInfo serviceInfo, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            object result;
            Type concreteType = serviceInfo.concreteType;
            if(typeof(IServiceInitializer).IsAssignableFrom(serviceInfo.classWithAttribute))
			{
                if(initialized.Contains(concreteType))
			    {
                    return null;
			    }

                Type initializerType = serviceInfo.classWithAttribute;
                var serviceInitializer = Activator.CreateInstance(initializerType) as IServiceInitializer;
                var interfaceTypes = initializerType.GetInterfaces();
                int argumentCount = 0;
                for(int interfaceIndex = interfaceTypes.Length - 1; interfaceIndex >= 0; interfaceIndex--)
				{
					var interfaceType = interfaceTypes[interfaceIndex];
					if(!interfaceType.IsGenericType)
					{
						continue;
					}

					var typeDefinition = interfaceType.GetGenericTypeDefinition();
					if(!argumentCountsByIServiceInitializerTypeDefinition.TryGetValue(typeDefinition, out argumentCount))
					{
						continue;
					}

					initialized.Add(concreteType);

					var parameterTypes = interfaceType.GetGenericArguments().Skip(1).ToArray();
					object[] arguments = new object[argumentCount];
					bool allArgumentsAvailable = true;

					for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
					{
						var parameterType = parameterTypes[argumentIndex];
						if(!TryGetOrCreateService(parameterType, out arguments[argumentIndex], initialized, scopedServiceInfos))
						{
							LogMissingDependencyWarning(initializerType, parameterType);
							allArgumentsAvailable = false;
							break;
						}
					}

					if(!allArgumentsAvailable)
					{
						initialized.Remove(concreteType);
						break;
					}

					var loadArgumentTasks = Enumerable.Empty<Task>();
					for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
					{
						var argumentType = parameterTypes[argumentIndex];
						var argument = arguments[argumentIndex];

						if(!argumentType.IsInstanceOfType(argument) && argument is Task loadArgumentTask)
						{
							loadArgumentTasks.Append(loadArgumentTask);
						}
					}

					await Task.WhenAll(loadArgumentTasks);

					for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
					{
						arguments[argumentIndex] = await InjectCrossServiceDependencies(arguments[argumentIndex], initialized, scopedServiceInfos);
					}

					result = serviceInitializer.InitTarget(arguments);
					if(result != null)
					{
						if(result is Task task)
						{
							result = await task.GetResult();
							if(result != null)
							{
								#if DEBUG_CREATE_SERVICES
								Debug.Log($"Service {concreteType.Name} created via service initializer {serviceInitializer.GetType().Name} successfully.");
								#endif

								return result;
							}
						}
						else
						{
							#if DEBUG_CREATE_SERVICES
							Debug.Log($"Service {concreteType.Name} created via service initializer {serviceInitializer.GetType().Name} successfully.");
							#endif

							return result;
						}
					}

                    initialized.Remove(concreteType);
					break;
				}

                if(argumentCount == 0)
                {
				    result = serviceInitializer.InitTarget(Array.Empty<object>());
                    if(result != null)
				    {
                        initialized.Add(concreteType);
                        return result is Task task ? await task.GetResult() : result;
				    }
                }
			}

            if(serviceInfo.FindFromScene)
            {
                object instance;
                if(typeof(Component).IsAssignableFrom(concreteType))
                {
                    instance = 
                    #if UNITY_2023_1_OR_NEWER
                    Object.FindAnyObjectByType(concreteType, FindObjectsInactive.Include);
                    #else
                    Object.FindObjectOfType(concreteType, true);
                    #endif
                }
                else if(typeof(Component).IsAssignableFrom(serviceInfo.classWithAttribute))
                {
                    instance = 
                    #if UNITY_2023_1_OR_NEWER
                    Object.FindAnyObjectByType(serviceInfo.classWithAttribute, FindObjectsInactive.Include);
                    #else
                    Object.FindObjectOfType(serviceInfo.classWithAttribute, true);
                    #endif
                }
                else
				{
                    instance = Find.Any(concreteType, true);
				}

                if(instance != null)
                {
                    #if DEBUG_CREATE_SERVICES
                    Debug.Log($"Service {concreteType.Name} retrieved from scene successfully.", instance as Object);
                    #endif

                    if(IsInstanceOf(serviceInfo, instance))
					{
                        return instance;
					}

                    if(instance is IInitializer initializerWithAttribute)
					{
                        instance = initializerWithAttribute.InitTarget();
                        if(IsInstanceOf(serviceInfo, instance))
					    {
                            return instance;
					    }
                    }

                    if(instance is IValueProvider valueProvider && valueProvider.Value is object value)
                    {
                        #if DEBUG || INIT_ARGS_SAFE_MODE
                        if(!IsInstanceOf(serviceInfo, value))
					    {
                            throw new Exception($"Failed to convert service instance from {value.GetType().Name} to {serviceInfo.definingTypes.FirstOrDefault()?.Name}.");
					    }
                        #endif

                        return value;
                    }
                }

                var allInitializerInLoadedScenes = Find.All<IInitializer>();
                foreach(var initializer in allInitializerInLoadedScenes)
                {
                    if(TargetIsAssignableOrConvertibleToType(initializer, serviceInfo) && initializer.InitTarget() is object initializedTarget)
                    {
                        #if DEBUG_CREATE_SERVICES
                        Debug.Log($"Service {initializedTarget.GetType().Name} retrieved from scene successfully.", instance as Object);
                        #endif

                        // Support Initializer -> Initialized
                        if(IsInstanceOf(serviceInfo, instance))
					    {
                            return initializedTarget;
						}

                        // Support WrapperInitializer -> Wrapper -> Wrapped Object
                        if(initializedTarget is IValueProvider valueProvider)
					    {
                            return valueProvider.Value;
                        }
                    }
                }

                foreach(var servicesComponent in Find.All<Services>())
				{
					if(!servicesComponent.AreAvailableToAnyClient())
					{
						continue;
					}

					foreach(var serviceComponent in servicesComponent.providesServices)
					{
						if(serviceInfo.HasDefiningType(serviceComponent.definingType.Value) && TryGetService(serviceComponent.service, serviceInfo, out result))
						{
							return result;
						}
					}
				}

				if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
                {
                    Debug.LogWarning($"Invalid Service Definition: Service '{concreteType.Name}' is a ScriptableObject type but has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. ScriptableObjects can not exist in scenes and as such can't be retrived using this method.");
                    return null;
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
  					ResourceRequest resourceRequest = Resources.LoadAsync(resourcePath, typeof(Object));
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
                    asset = Resources.Load(resourcePath, typeof(Object));
				}

                if(asset == null)
                {
                    Debug.LogWarning($"Service Not Found: There is no '{concreteType.Name}' found at resource path 'Resources/{resourcePath}', but the service class has the {nameof(ServiceAttribute)} {nameof(ServiceAttribute.ResourcePath)} set to '{resourcePath}'. Either make sure an asset exists in the project at the expected path or don't specify a {nameof(ServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
                    return null;
                }

                // If loaded asset is a prefab, instantiate a clone from it
                if(asset is GameObject gameObject && !gameObject.scene.IsValid() && TryGetServiceComponent(gameObject, serviceInfo, out Component component))
                {
                    result = await InstantiateServiceAsync(component, initialized, scopedServiceInfos);
                }
                else if(!TryGetService(asset, serviceInfo, out result))
                {
                    Debug.LogWarning($"Service Not Found: Resource at path 'Resources/{resourcePath}' could not be converted to type {serviceInfo.definingTypes.FirstOrDefault()?.Name}.");
                    return null;
                }

                if(result is IWrapper wrapper && Array.Exists(serviceInfo.definingTypes, t => !t.IsInstanceOfType(result)))
				{
                    result = wrapper.WrappedObject;
				}

                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {concreteType.Name} loaded from resources successfully.", asset);
                #endif

                return result;
            }

            #if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
            if(serviceInfo.AddressableKey is string addressableKey)
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

                if(asset == null)
                {
                    Debug.LogWarning($"Service Not Found: There is no '{concreteType.Name}' found in the Addressable registry under the address {addressableKey}, but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.AddressableKey)} set to '{addressableKey}'. Either make sure an instance with the address exists in the project or don't specify a {nameof(ServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
                    return null;
                }

                // If loaded asset is a prefab, instantiate a clone from it
                if(asset is GameObject gameObject && !gameObject.scene.IsValid() && TryGetServiceComponent(gameObject, serviceInfo, out Component component))
                {
                    result = await InstantiateServiceAsync(component, initialized, scopedServiceInfos);
                }
                else if(!TryGetService(asset, serviceInfo, out result))
                {
                    Debug.LogWarning($"Service Not Found: Addressable in the Addressable registry under the address {addressableKey} could not be converted to type {serviceInfo.definingTypes.FirstOrDefault()?.Name}.");
                    return null;
                }

                if(result is IWrapper wrapper && Array.Exists(serviceInfo.definingTypes, t => !t.IsInstanceOfType(result)))
				{
                    result = wrapper.WrappedObject;
				}

                #if DEBUG_CREATE_SERVICES
                if(result != null) { Debug.Log($"Service {concreteType.Name} loaded using Addressables successfully.", asset); }
                #endif

                return result;
                
            }
            #endif

            if(typeof(Component).IsAssignableFrom(concreteType))
            {
                if(container == null)
                {
                    CreateServicesContainer();
                }

                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {concreteType.Name} attached to {container.name}.", container);
                #endif

                return container.AddComponent(concreteType);
            }

            if(typeof(ScriptableObject).IsAssignableFrom(concreteType))
            {
                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {concreteType.Name} created successfully.");
                #endif

                return ScriptableObject.CreateInstance(concreteType);
            }

            if(initialized.Contains(concreteType))
			{
                return null;
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
                int argumentCount = parameters.Length;
                if(argumentCount == 0)
				{
                    continue;
				}

                initialized.Add(concreteType);
                
                object[] arguments = new object[argumentCount];
                bool allArgumentsAvailable = true;

                for(int i = 0; i < argumentCount; i++)
				{
                    var parameterType = parameters[i].ParameterType;
                    if(!TryGetOrCreateService(parameterType, out arguments[i], initialized, scopedServiceInfos))
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

                var loadArgumentTasks = Enumerable.Empty<Task>();
                for(int i = 0; i < argumentCount; i++)
				{
                    var parameterType = parameters[i].ParameterType;
                    var argument = arguments[i];

                    if(!parameterType.IsInstanceOfType(argument) && argument is Task loadArgumentTask)
					{
                        loadArgumentTasks.Append(loadArgumentTask);
					}
                }

                await Task.WhenAll(loadArgumentTasks);

                for(int i = 0; i < argumentCount; i++)
				{
                    arguments[i] = await InjectCrossServiceDependencies(arguments[i], initialized, scopedServiceInfos);
                }

                result = constructor.Invoke(arguments);

                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {concreteType.Name} created via constructor {constructor} successfully.");
                #endif

				return result is Task task ? await task.GetResult() : result;
			}

            if(Array.Exists(constructors, c => c.GetParameters().Length == 0))
            {
                result = Activator.CreateInstance(concreteType);
            }
            else
			{
                result = null;
			}

            #if DEBUG || DEBUG_CREATE_SERVICES
            if(result == null)
            {
                Debug.Log($"Failed to create instance of Service {concreteType}. This could happen if some of the types it depends on are not services, are at least services that are accessible to every client and at this particular time.");
            }
            #endif
            #if DEBUG_CREATE_SERVICES
            else
            {
                Debug.Log($"Service {concreteType} created successfully via default constructor.");
            }
            #endif

            return result;

            void LogMissingDependencyWarning(Type initializerType, Type dependencyType)
            {
                if(FillIfEmpty(scopedServiceInfos).TryGetValue(dependencyType, out var serviceInfo))
				{
                    if(serviceInfo.service == null)
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
                    Debug.LogWarning($"{TypeUtility.ToString(initializerType)} needs service {TypeUtility.ToString(dependencyType)} to initialize {TypeUtility.ToString(concreteType)}, but instance not found among {services.Count + scopedServiceInfos.Count + uninitializedServices.Count} services:\n{string.Join("\n", services.Keys.Select(t => TypeUtility.ToString(t)).Concat(uninitializedServices.Keys.Select(t => TypeUtility.ToString(t))).Concat(scopedServiceInfos.Values.Select(i => TypeUtility.ToString(i.service?.GetType()))))}");
                }
            }
        }

        private static bool TargetIsAssignableOrConvertibleToType(IInitializer initializer, ServiceInfo serviceInfo)
		{
            foreach(var definingType in serviceInfo.definingTypes)
			{
                if(initializer.TargetIsAssignableOrConvertibleToType(definingType))
				{
                    return true;
				}
			}

            return false;
		}

        private static bool TryGetOrCreateService(Type definingType, out object service, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos, object client = null)
        {
            if(TryGetServiceFor(client, definingType, out service, initialized, scopedServiceInfos))
			{
                return true;
			}

            if(TryGetServiceInfo(definingType, out var serviceInfo))
			{
                service = GetOrCreateInstance(serviceInfo, initialized, scopedServiceInfos, serviceInfo.concreteType is null && !definingType.IsAbstract ? definingType : null);
                return service != null;
			}

            return false;
        }

        private static bool TryGetService([DisallowNull] Object unityObject, [DisallowNull] ServiceInfo serviceInfo, out object result)
        {
            if(unityObject is GameObject gameObject)
            {
                return TryGetService(gameObject, serviceInfo, out result);
            }

            if(IsInstanceOf(serviceInfo, unityObject))
            {
                result = unityObject;
                return true;
            }

            if(unityObject is IWrapper wrapper && wrapper.WrappedObject is object wrappedObject && IsInstanceOf(serviceInfo, wrappedObject))
            {
                result = wrappedObject;
                return true;
            }

            if(unityObject is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
            {
                result = initializer.InitTarget();
                return result != null;
            }

            result = null;
            return false;
        }

        private static bool TryGetService([DisallowNull] GameObject gameObject, ServiceInfo serviceInfo, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
        {
            if(Find.In(gameObject, serviceInfo.ConcreteOrDefiningType, out result))
			{
                return true;
			}

            if(typeof(Component).IsAssignableFrom(serviceInfo.classWithAttribute) && gameObject.TryGetComponent(serviceInfo.classWithAttribute, out var componentWithAttribute))
            {
                if(IsInstanceOf(serviceInfo, componentWithAttribute))
                {
                    result = componentWithAttribute;
                    return true;
                }

                if(componentWithAttribute is IValueProvider valueProvider && valueProvider.Value is object value && IsInstanceOf(serviceInfo, value))
                {
                    result = value;
                    return true;
                }

                if(componentWithAttribute is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
                {
                    result = initializer.InitTarget();
                    if(IsInstanceOf(serviceInfo, result))
					{
                        return true;
					}

                    valueProvider = result as IValueProvider;
                    if(valueProvider != null)
					{
                        result = valueProvider.Value;

                        if(result is Component resultComponent && resultComponent.gameObject == gameObject)
						{
                            result = Object.Instantiate(componentWithAttribute);
						}
                    }

                    return result is not null;
                }

                if(componentWithAttribute is IValueByTypeProvider valueByTypeProvider)
                {
                    var method = typeof(IValueByTypeProvider).GetMethod(nameof(IValueByTypeProvider.TryGetFor), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    var args = new object[] { componentWithAttribute, null };
                    if((bool)method.Invoke(valueByTypeProvider, args))
					{
                        result = args[1];

                        if(result is Component resultComponent && resultComponent.gameObject == gameObject)
						{
                            result = Object.Instantiate(componentWithAttribute);
						}

                        return true;
					}
                }

                if(componentWithAttribute is IValueProviderAsync valueProviderAsync)
                {
                    var method = typeof(IValueProviderAsync).GetMethod(nameof(IValueProviderAsync.GetForAsync), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					var args = new object[] { componentWithAttribute };
                    var task =
					#if !UNITY_2023_1_OR_NEWER
					(System.Threading.Tasks.Task<object>)method.Invoke(valueProviderAsync, args);
					result = task;
					return !task.IsFaulted;
					#else
					method.Invoke(valueProviderAsync, args);
					result = task;
					return true;
					#endif
                }

                if(componentWithAttribute is IValueByTypeProviderAsync valueByTypeProviderAsync)
                {
                    var method = typeof(IValueByTypeProviderAsync).GetMethod(nameof(IValueByTypeProviderAsync.GetForAsync), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    var task = method.Invoke(valueByTypeProviderAsync, new object[] { componentWithAttribute });
                    result = task;
                    return true;
                }

                result = null;
                return false;
            }

            var valueProviders = gameObject.GetComponents<IValueProvider>();
            foreach(var valueProvider in valueProviders)
            {
                var value = valueProvider.Value;
                if(value != null && IsInstanceOf(serviceInfo, value))
                {
                    result = value;
                    return true;
                }
            }

            foreach(var valueProvider in valueProviders)
            {
                if(valueProvider is IInitializer initializer && TargetIsAssignableOrConvertibleToType(initializer, serviceInfo))
                {
                    result = initializer.InitTarget();
                    return result != null;
                }
            }

            result = null;
            return false;
        }

        private static bool TryGetServiceComponent([DisallowNull] GameObject gameObject, ServiceInfo serviceInfo, [NotNullWhen(true), MaybeNullWhen(false)] out Component result)
        {
            if(Find.typesToFindableTypes.TryGetValue(serviceInfo.ConcreteOrDefiningType, out var findableTypes))
            {
                for(int i = findableTypes.Length - 1; i >= 0; i--)
                {
                    Type findableType = findableTypes[i];
                    if(typeof(Component).IsAssignableFrom(findableType) && gameObject.TryGetComponent(findableType, out result))
                    {
                        return true;
                    }
                }
            }

            if(Find.typesToFindableTypes.TryGetValue(serviceInfo.classWithAttribute, out findableTypes))
            {
                for(int i = findableTypes.Length - 1; i >= 0; i--)
                {
                    Type findableType = findableTypes[i];
                    if(typeof(Component).IsAssignableFrom(findableType) && gameObject.TryGetComponent(findableType, out result))
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

        private static bool IsInstanceOf(ServiceInfo serviceInfo, object instance)
		{
            if(serviceInfo.concreteType != null)
			{
                return serviceInfo.concreteType.IsInstanceOfType(instance);
			}

            foreach(var definingType in serviceInfo.definingTypes)
			{
                if(!definingType.IsInstanceOfType(instance))
                {
                    return false;
                }
			}

            return true;
		}

        private static bool TryGetServiceFor([AllowNull] object client, Type definingType, out object service, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            if(services.TryGetValue(definingType, out service))
            {
                return true;
            }

            if(!uninitializedServices.TryGetValue(definingType, out var definition)
                || !ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out var serviceInfo))
            {
                // Also try to find scene from ServiceTag and Services components in the scene.
				if(FillIfEmpty(scopedServiceInfos).TryGetValue(definingType, out var scopedServiceInfo)
                && (scopedServiceInfo.toClients == Clients.Everywhere
			    || (Find.In(scopedServiceInfo.service, out Transform serviceTransform)
				&& Service.IsAccessibleTo(serviceTransform, scopedServiceInfo.toClients, client as Transform))))
				{
					service = scopedServiceInfo.service;
					return true;
				}

				return false;
            }

            #if DEV_MODE && DEBUG_LAZY_INIT
            Debug.Log($"Initializing service {definingType.Name} with LazyInit=true because it is a dependency of another service.");
            #endif

            uninitializedServices.Remove(serviceInfo.concreteType);
			foreach(var singleDefiningType in serviceInfo.definingTypes)
			{
                uninitializedServices.Remove(singleDefiningType);
			}

            service = LoadInstance(serviceInfo, initialized, scopedServiceInfos);
            if(service is null)
            {
                return false;
            }

            if(service is Task task)
			{
                services[serviceInfo.concreteType] = task;
                foreach(var singleDefiningType in serviceInfo.definingTypes)
				{
                    services[singleDefiningType] = task;
				}
                
                _ = SetInstanceAsync(serviceInfo, task);
                return true;
			}

            SetInstanceSync(serviceInfo, service);
            return service != null;
        }

        private static async Task SetInstanceAsync(ServiceInfo serviceInfo, Task loadServiceTask)
		{
			var service = await loadServiceTask.GetResult();

			SetInstanceSync(serviceInfo, service);

            SubscribeToUpdateEvents(service);
            ExecuteAwake(service);
            ExecuteOnEnable(service);
            ExecuteStartAtEndOfFrame(service);
		}

		private static void SetInstanceSync(ServiceInfo serviceInfo, object service) => SetInstanceSync(serviceInfo.definingTypes, service);

        private static void SetInstanceSync(Type[] definingTypes, object service)
		{
			services[service.GetType()] = service;

            foreach(var definingType in definingTypes)
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
		/// <returns></returns>
		private static bool IsFirstSceneInBuildSettingsLoaded()
        {
            string firstSceneInBuildsPath = Array.Find(EditorBuildSettings.scenes, s => s.enabled)?.path ?? "";
            Scene firstSceneInBuilds = SceneManager.GetSceneByPath(firstSceneInBuildsPath);
            return firstSceneInBuilds.IsValid() && firstSceneInBuilds.isLoaded;
        }
        #endif

        private static void InjectCrossServiceDependencies(List<ServiceInfo> serviceInfos, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            foreach(var serviceInfo in serviceInfos)
            {
                var concreteOrDefiningType = serviceInfo.ConcreteOrDefiningType;
                if(!uninitializedServices.ContainsKey(concreteOrDefiningType)
                    && services.TryGetValue(concreteOrDefiningType, out var client))
                {
                    _ = InjectCrossServiceDependencies(client, initialized, scopedServiceInfos, serviceInfo.definingTypes);
                }
            }

            foreach(var scopedServiceInfo in FillIfEmpty(scopedServiceInfos).Values)
            {
                if(scopedServiceInfo.service is Object service && service)
                {
                    _ = InjectCrossServiceDependencies(scopedServiceInfo.service, initialized, scopedServiceInfos, scopedServiceInfo.definingType);
                }
            }
        }

        private static async Task<object> InjectCrossServiceDependencies(object client, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos, params Type[] definingTypes)
        {
            if(client is Task clientTask)
			{
                client = await clientTask.GetResult();
			}

            var concreteType = client.GetType();

            if(definingTypes is not null)
            {
                foreach(var definingType in definingTypes)
			    {
                    initialized.Add(definingType);
			    }
            }

            if(!initialized.Add(concreteType))
            {
                return client;
            }

            if(CanSelfInitialize(client))
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
				if(!interfaceType.IsGenericType || !argumentCountsByIInitializableTypeDefinition.TryGetValue(interfaceType.GetGenericTypeDefinition(), out int argumentCount))
				{
					continue;
				}

				var parameterTypes = interfaceType.GetGenericArguments();
				object[] arguments = new object[argumentCount];
				bool allArgumentsAvailable = true;

				for(int argumentIndex = 0; i < argumentCount; i++)
				{
					var parameterType = parameterTypes[argumentIndex];
					if(!TryGetServiceFor(client, parameterType, out arguments[argumentIndex], initialized, scopedServiceInfos))
					{
						LogMissingDependencyWarning(parameterType);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					continue;
				}

				var loadArgumentTasks = Enumerable.Empty<Task>();
				for(int argumentIndex = 0; i < argumentCount; i++)
				{
					var argumentType = parameterTypes[argumentIndex + 1];
					var argument = arguments[argumentIndex];

					if(!argumentType.IsInstanceOfType(argument) && argument is Task loadArgumentTask)
					{
						loadArgumentTasks.Append(loadArgumentTask);
					}
				}

				await Task.WhenAll(loadArgumentTasks);

				for(int argumentIndex = 0; i < argumentCount; i++)
				{
					arguments[argumentIndex] = await InjectCrossServiceDependencies(arguments[argumentIndex], initialized, scopedServiceInfos);
				}

				var initMethod = interfaceType.GetMethod(nameof(IInitializable<object>.Init), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				initMethod.Invoke(client, arguments);

				#if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Service {concreteType.Name} received {argumentCount} dependencies successfully.");
				#endif

				return client;
			}

			return client;

            void LogMissingDependencyWarning(Type dependencyType)
            {
                if(FillIfEmpty(scopedServiceInfos).TryGetValue(dependencyType, out var serviceInfo))
				{
                    if(serviceInfo.service == null)
					{
                        Debug.LogWarning($"Service {TypeUtility.ToString(concreteType)} requires argument {TypeUtility.ToString(dependencyType)} but reference to the service seems to be broken in the scene component.", client as Component);
					}
                    else
					{
                        Debug.LogWarning($"Service {TypeUtility.ToString(concreteType)} requires argument {TypeUtility.ToString(dependencyType)} but the service is only accessible to clients {serviceInfo.toClients}.", client as Component);
					}
				}
                else
                {
                    Debug.LogWarning($"Service {TypeUtility.ToString(concreteType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + scopedServiceInfos.Count + uninitializedServices.Count} services:\n{string.Join("\n", services.Keys.Select(t => TypeUtility.ToString(t)).Concat(uninitializedServices.Keys.Select(t => TypeUtility.ToString(t))).Concat(scopedServiceInfos.Values.Select(i => TypeUtility.ToString(i.service?.GetType()))))}", client as Component);
                }
            }
        }

        private static async Task<object> InstantiateServiceAsync(Component prefab, HashSet<Type> initialized, [AllowNull] Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            var concreteType = prefab.GetType();
            if(!initialized.Add(concreteType))
            {
                return null;
            }

            if(CanSelfInitialize(prefab))
			{
                #if DEV_MODE && DEBUG_INIT_SERVICES
                Debug.Log($"Will not inject dependencies to service {concreteType.Name} because it should be able to acquire them independently.");
                #endif

                return Object.Instantiate(prefab);
			}

            var interfaceTypes = concreteType.GetInterfaces();
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];
                if(!interfaceType.IsGenericType || !argumentCountsByIArgsTypeDefinition.TryGetValue(interfaceType.GetGenericTypeDefinition(), out int parameterCount))
				{
					continue;
				}

				var parameterTypes = interfaceType.GetGenericArguments();
				object[] instantiateArguments = new object[parameterCount];
				bool allArgumentsAvailable = true;

				for(int argumentIndex = 0; i < parameterCount; i++)
				{
					var parameterType = parameterTypes[argumentIndex];
                    if(!TryGetOrCreateService(parameterType, out object firstArgument, initialized, scopedServiceInfos, prefab))
					{
                        LogMissingDependecyWarning(concreteType, parameterType, prefab, scopedServiceInfos);
						allArgumentsAvailable = false;
						break;
					}
				}

				if(!allArgumentsAvailable)
				{
					continue;
				}

				var loadArgumentTasks = Enumerable.Empty<Task>();
				for(int parameterIndex = 0; i < parameterCount; i++)
				{
					var parameterType = parameterTypes[parameterIndex];
					var argument = instantiateArguments[parameterIndex + 1];

					if(!parameterType.IsInstanceOfType(argument) && argument is Task loadArgumentTask)
					{
						loadArgumentTasks.Append(loadArgumentTask);
					}
				}

				await Task.WhenAll(loadArgumentTasks);

				for(int parameterIndex = 0; i < parameterCount; i++)
				{
					instantiateArguments[parameterIndex + 1] = await InjectCrossServiceDependencies(instantiateArguments[parameterIndex + 1], initialized, scopedServiceInfos);
				}

                Type[] instantiateGenericArgumentTypes = new Type[parameterCount + 1];
                Array.Copy(parameterTypes, 0, instantiateGenericArgumentTypes, 1, parameterCount);
                instantiateGenericArgumentTypes[0] = concreteType;

                MethodInfo instantiateMethod = typeof(InstantiateExtensions).GetMember(nameof(InstantiateExtensions.Instantiate), BindingFlags.Static | BindingFlags.Public)
                                                                .Select(member => (MethodInfo)member)
                                                                .FirstOrDefault(method => method.GetGenericArguments().Length == parameterCount + 1 && method.GetParameters().Length == parameterCount + 1)
                                                                .MakeGenericMethod(instantiateGenericArgumentTypes);

                instantiateArguments[0] = prefab;

                #if DEV_MODE && DEBUG_INIT_SERVICES
				Debug.Log($"Injecting {parameterCount} dependencies to {concreteType.Name}.");
				#endif

                return instantiateMethod.Invoke(null, instantiateArguments);
            }

            return null;
        }

        static void LogMissingDependecyWarning(Type clientType, Type dependencyType, Object context, Dictionary<Type, ScopedServiceInfo> scopedServiceInfos)
        {
            if(FillIfEmpty(scopedServiceInfos).TryGetValue(dependencyType, out var serviceInfo))
			{
                if(serviceInfo.service == null)
				{
                    Debug.LogWarning($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but reference to the service seems to be broken in the scene component.", context);
				}
                else
				{
                    Debug.LogWarning($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but the service is only accessible to clients {serviceInfo.toClients}.", context);
				}
			}
            else
            {
                Debug.LogWarning($"Service {TypeUtility.ToString(clientType)} requires argument {TypeUtility.ToString(dependencyType)} but instance not found among {services.Count + scopedServiceInfos.Count + uninitializedServices.Count} services:\n{string.Join("\n", services.Keys.Select(t => TypeUtility.ToString(t)).Concat(uninitializedServices.Keys.Select(t => TypeUtility.ToString(t))).Concat(scopedServiceInfos.Values.Select(i => TypeUtility.ToString(i.service?.GetType()))))}", context);
            }
        }

        /// <summary>
        /// Does the client have an initializer or a base class that can handle automatically initializing itself with all services?
        /// </summary>
        private static bool CanSelfInitialize([DisallowNull] object client) => CanSelfInitializeWithoutInitializer(client) || HasInitializer(client);
        private static bool CanSelfInitialize([DisallowNull] Component client) => CanSelfInitializeWithoutInitializer(client) || HasInitializer(client);

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
                foreach(var clientType in TypeUtility.GetImplementingTypes(interfaceType, interfaceType.Assembly, typeof(bool).Assembly))
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

        private static Dictionary<int, InitArgsSetMethodInvoker> GetInitArgsSetMethodsByServiceCount()
        {
            Dictionary<int, InitArgsSetMethodInvoker> result = new(MAX_INIT_ARGUMENT_COUNT);

            foreach(MethodInfo method in typeof(InitArgs).GetMember(nameof(InitArgs.Set), MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic))
            {
                var genericArguments = method.GetGenericArguments();
                var parameters = method.GetParameters();
                int genericArgumentCount = genericArguments.Length;
                int parameterCount = parameters.Length;
                if(genericArgumentCount == parameterCount - 1) // in addition to the service types, there is clientType
                {
                    result.Add(genericArgumentCount, new InitArgsSetMethodInvoker(method, genericArgumentCount, parameterCount));
                }
            }

            #if DEV_MODE
            Debug.Assert(result.Count == MAX_INIT_ARGUMENT_COUNT, result.Count);
            #endif

            return result;
        }

        private static async Task TrySetDefaultServices(Type clientType, Dictionary<int, InitArgsSetMethodInvoker> setMethodsByServiceCount
        #if UNITY_EDITOR
        , Dictionary<Type, List<ScriptableObject>> uninitializedScriptableObjects
        #endif
        )
        {
            Type[] interfaceTypes = clientType.GetInterfaces();
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
			{
				var interfaceType = interfaceTypes[i];
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

                setMethod.SetClientType(clientType);

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
                    Debug.Log($"{serviceCount} default services for clients of type {clientType.FullName} not found: {string.Join(", ", serviceTypes.Select(t => t.Name))}");
                    #endif

					continue;
				}

                #if DEV_MODE && DEBUG_DEFAULT_SERVICES
                Debug.Log($"Registering {serviceCount} default services for clients of type {clientType.FullName}: {string.Join(", ", serviceTypes.Select(t => t.Name))}");
                #endif

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
			if(service is IAwake onEnable)
			{
				onEnable.Awake();
			}
		}

		private static void ExecuteOnEnable(object service)
		{
			if(service is IOnEnable onEnable)
			{
				onEnable.OnEnable();
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

		[return: MaybeNull]
        internal static bool TryGetServiceInfo(Type definingType, out ServiceInfo serviceInfo) => ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out serviceInfo);

        private static List<ServiceInfo> GetServiceDefinitions() => ServiceAttributeUtility.concreteTypes.Values.Concat(ServiceAttributeUtility.definingTypes.Values.Where(d => d.concreteType is null)).ToList();

		public static bool CanProvideService<TService>() => services.ContainsKey(typeof(TService)) || uninitializedServices.ContainsKey(typeof(TService));

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
            private readonly MethodInfo method;

            public InitArgsSetMethodInvoker(MethodInfo method, int genericArgumentCount, int parameterCount)
            {
                this.method = method;
                serviceTypes = new Type[genericArgumentCount];
                clientTypeAndServices = new object[parameterCount];
            }

			public void SetClientType(Type clientType) => clientTypeAndServices[0] = clientType;

			public void SetService(int index, Type serviceType, object service)
            {
                #if DEV_MODE && DEBUG
                Debug.Assert(index >= 0);
                Debug.Assert(index < serviceTypes.Length);
                Debug.Assert(index + 1 < clientTypeAndServices.Length);
                Debug.Assert(serviceType is not null);
                Debug.Assert(service is not null);
                #endif

                serviceTypes[index] = serviceType;
                clientTypeAndServices[index + 1] = service;
            }

            public async Task Invoke()
            {
                bool isAsync = false;
                var loadArgumentTasks = Enumerable.Empty<Task>();
                int argumentCount = serviceTypes.Length;
				for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
					var serviceType = serviceTypes[argumentIndex];
					var service = clientTypeAndServices[argumentIndex + 1];
					if(!serviceType.IsInstanceOfType(service) && service is Task loadServiceTask)
					{
                        isAsync = true;
						loadArgumentTasks.Append(loadServiceTask);
					}
				}

                object[] arguments;
                if(!isAsync)
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

                for(int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
				{
                    if(arguments[argumentIndex] is Task finishedTask)
                    {
                        arguments[argumentIndex] = await finishedTask.GetResult();
                    }
                }

                method.MakeGenericMethod(serviceTypes).Invoke(null, arguments);
            }
		}
    }
}
#endif