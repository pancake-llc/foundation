//#define INIT_ARGS_DISABLE_SERVICE_CONSTRUCTOR_SUPPORT

//#define DEBUG_INIT_SERVICES
//#define DEBUG_CREATE_SERVICES
//#define DEBUG_LAZY_INIT

#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using ServiceDefinitions = System.Collections.Generic.IEnumerable<(System.Type classWithAttribute, Pancake.Init.ServiceAttribute attribute)>;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using TypeCollection = UnityEditor.TypeCache.TypeCollection;
#else
using TypeCollection = System.Collections.Generic.IEnumerable<System.Type>;
#endif

namespace Pancake.Init.Internal
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
        /// <see langword="true"/> if all services have been created,
        /// initialized and are ready to be used by clients.
        /// </summary>
        public static bool ServicesAreReady { get; private set; }

        /// <summary>
        /// Called when all services have been created,
        /// initialized and are ready to be used by clients.
        /// </summary>
        public static event Action OnServicesBecameReady;

        internal static Dictionary<Type, object> services = new Dictionary<Type, object>();
        internal static readonly Dictionary<Type, (Type classWithAttribute, ServiceAttribute attribute)> unitializedServicesByDefiningType = new Dictionary<Type, (Type, ServiceAttribute)>();

        /// <summary>
        /// Gets a value indicating whether or not <typeparamref name="T"/> is the defining type of a service.
        /// <para>
        /// By default the defining type of a class that has the <see cref="ServiceAttribute"/> is the type of the class itself,
        /// however it is possible provide a different defining type, which can be any type as long as it is assignable from the
        /// type of the class with the attribute.
        /// </para>
        /// </summary>
        /// <typeparam name="T"> Type to test. </typeparam>
        /// <returns> <see langword="true"/> if <typeparamref name="T"/> is the defining type of a service; otherwise, <see langword="false"/>. </returns>
        public static bool IsServiceDefiningType<T>()
        {
            if(typeof(T).IsValueType)
            {
                return false;
            }

            if(Service<T>.Instance != null)
            {
                return true;
            }

            bool servicesAreReady = ServicesAreReady;

            #if UNITY_EDITOR
            if(servicesAreReady && !EditorOnly.ThreadSafe.Application.IsPlaying)
            {
                servicesAreReady = false;
            }
            #endif

            if(servicesAreReady)
            {
                return false;
            }

            foreach(var service in GetServiceDefinitions())
            {
                if(service.attribute.definingType is Type definingType)
                {
                    if(definingType == typeof(T))
				    {
                        return true;
				    }
                }
                else if(typeof(IInitializer).IsAssignableFrom(service.classWithAttribute))
			    {
                    var interfaceTypes = service.classWithAttribute.GetInterfaces();
                    for(int i = interfaceTypes.Length - 1; i >= 0; i--)
                    {
                        var interfaceType = interfaceTypes[i];
                        if(interfaceType.IsGenericType
                            && interfaceType.GetGenericTypeDefinition() == typeof(IInitializer<>)
                            && interfaceType.GetGenericArguments()[0] == typeof(T))
                        {
                            return true;
					    }
				    }
                }
                else if(service.classWithAttribute == typeof(T))
				{
                    return true;
				}
            }

            return false;
        }

		/// <summary>
		/// Gets a value indicating whether or not <typeparamref name="T"/> is a concrete class
		/// that contains the <see cref="ServiceAttribute"/>.
		/// </summary>
		/// <typeparam name="T"> Type to test. </typeparam>
		/// <returns> <see langword="true"/> if <typeparamref name="T"/> is a concrete service type; otherwise, <see langword="false"/>. </returns>
		public static bool HasServiceAttribute<T>()
		{
            if(typeof(T).IsValueType || typeof(T).IsAbstract)
			{
                return false;
			}

            foreach(var service in GetServiceDefinitions())
            {
                if(typeof(T) == service.classWithAttribute)
                {
                    return true;
                }
            }

            return false;
		}

        /// <summary>
        /// Gets a value indicating whether or not the provided <paramref name="type"/> is the defining type of a service.
        /// <para>
        /// By default the defining type of a class that has the <see cref="ServiceAttribute"/> is the type of the class itself,
        /// however it is possible provide a different defining type, which can be any type as long as it is assignable from the
        /// type of the class with the attribute.
        /// </para>
        /// </summary>
        /// <param name="type"> Type to test. </param>
        /// <returns> <see langword="true"/> if type is the defining type of a service; otherwise, <see langword="false"/>. </returns>
        public static bool IsDefiningTypeOfAnyServiceAttribute([JetBrains.Annotations.NotNull] Type type)
        {
            #if DEBUG || INIT_ARGS_SAFE_MODE
            if(type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            #endif

            if(type.IsValueType)
            {
                return false;
            }

            bool servicesAreReady = ServicesAreReady;

            #if UNITY_EDITOR
            if(servicesAreReady && !EditorOnly.ThreadSafe.Application.IsPlaying)
            {
                servicesAreReady = false;
            }
            #endif

            if(!servicesAreReady)
            {
                foreach(var service in GetServiceDefinitions())
                {
                    var definingType = service.attribute.definingType;
                    if(definingType is null)
					{
                        if(type == GetServiceConcreteType(service.classWithAttribute))
						{
                            return true;
						}
                    }
                    else if(type == definingType && GetServiceConcreteType(service.classWithAttribute) != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            #if DEBUG
            return !(typeof(Service<>).MakeGenericType(type).GetProperty(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is null);
            #else
            return !(typeof(Service<>).MakeGenericType(type).GetField(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is null);
            #endif
        }


        /// <summary>
        /// Gets a value indicating whether or not the provided <paramref name="type"/> class has the <see cref="ServiceAttribute"/>.
        /// </para>
        /// </summary>
        /// <param name="type"> Type to test. </param>
        /// <returns> <see langword="true"/> if <typeparamref name="T"/> is a concrete service type; otherwise, <see langword="false"/>. </returns>
        public static bool HasServiceAttribute([JetBrains.Annotations.NotNull] Type type)
        {
            #if DEBUG || INIT_ARGS_SAFE_MODE
            if(type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            #endif

            if(type.IsValueType || type.IsAbstract)
            {
                return false;
            }

            foreach(var service in GetServiceDefinitions())
            {
                if(type == service.classWithAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Reset state when entering play mode in the editor to support Enter Play Mode Settings.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnEnterPlayMode()
		{
            ServicesAreReady = false;
            services.Clear();
            unitializedServicesByDefiningType.Clear();
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
        private static void CreateAndInjectServices()
        {
            CreateInstancesOfAllServices();
            InjectServiceDependenciesForTypesThatRequireOnlyThem();

            ServicesAreReady = true;
            OnServicesBecameReady?.Invoke();
            OnServicesBecameReady = null;

            #if UNITY_EDITOR
            void OnExitingPlayMode()
            {
                ServicesAreReady = false;
            }

            EditorOnly.ThreadSafe.Application.ExitingPlayMode -= OnExitingPlayMode;
            EditorOnly.ThreadSafe.Application.ExitingPlayMode += OnExitingPlayMode;
            #endif
        }

        private static void CreateInstancesOfAllServices()
        {
            var serviceDefinitions = GetServiceDefinitions();

            int count = serviceDefinitions.Count();

            if(count == 0)
            {
                services = null;
                return;
            }

            GameObject container = null;

            services = new Dictionary<Type, object>(count);
            HashSet<Type> initialized = new HashSet<Type>();
            CreateServices(serviceDefinitions, ref container, initialized);
            InjectCrossServiceDependencies(serviceDefinitions, ref container, initialized);

            #if UNITY_EDITOR
            CreateServicesDebugger(ref container);
            #endif

            if(container != null)
            {
                container.SetActive(true);
            }

            HandleBroadcastingUnityEvents();
        }

        #if UNITY_EDITOR
        private static void CreateServicesDebugger(ref GameObject container)
        {
            if(!Application.isPlaying)
            {
                return;
            }

            if(container == null)
            {
                container = CreateServicesContainer();
            }

            var debugger = container.AddComponent<ServicesDebugger>();
            debugger.SetServices(services.Values.Distinct().ToArray());
        }
        #endif

        private static GameObject CreateServicesContainer()
        {
            var container = new GameObject("Services");
            container.SetActive(false);
            container.hideFlags = HideFlags.DontSave;
            Object.DontDestroyOnLoad(container);
            return container;
        }

        private static void CreateServices(ServiceDefinitions serviceDefinitions, ref GameObject container, HashSet<Type> initialized)
        {
            foreach(var serviceDefinition in serviceDefinitions)
			{
				var classWithAttribute = serviceDefinition.classWithAttribute;
				if(classWithAttribute.IsAbstract || classWithAttribute.IsGenericTypeDefinition)
				{
					continue;
				}

				var serviceAttribute = serviceDefinition.attribute;
				if(serviceAttribute.LazyInit)
				{
                    #if DEV_MODE && DEBUG_LAZY_INIT
                    Debug.Log($"Will not initialize {classWithAttribute.FullName} yet because it has LazyInit set to true");
                    #endif

					var definingType = serviceAttribute.definingType is null ? classWithAttribute : serviceAttribute.definingType;
					unitializedServicesByDefiningType[definingType] = (classWithAttribute, serviceAttribute);
					continue;
				}

				try
				{
					GetOrCreateInstance(classWithAttribute, serviceAttribute, ref container, initialized);
				}
                catch(MissingMethodException e)
				{
                    Debug.LogWarning($"Failed to initialize service {classWithAttribute.Name} with defining type {(serviceDefinition.attribute is null ? "n/a" : serviceDefinition.attribute.definingType is null ? "null" : serviceDefinition.attribute.definingType.Name)}.\nThis can happen when constructor arguments contain circular references (for example: object A requires object B, but object B also requires object A, so neither object can be constructed).\n{e}");
				}
				catch(Exception e)
				{
					Debug.LogWarning($"Failed to initialize service {classWithAttribute.Name} with defining type {(serviceDefinition.attribute is null ? "n/a" : serviceDefinition.attribute.definingType is null ? "null" : serviceDefinition.attribute.definingType.Name)}.\n{e}");
				}
			}
		}

        internal static void LazyInit(Type classWithAttribute, ServiceAttribute serviceAttribute)
        {
            Type definingType = serviceAttribute.definingType is null ? classWithAttribute : serviceAttribute.definingType;
            if(services.TryGetValue(definingType, out object service))
            {
                return;
            }

            if(!unitializedServicesByDefiningType.Remove(definingType))
            {
                return;
            }

            #if UNITY_EDITOR
            GameObject container = Find.GameObjectWith<ServicesDebugger>(true);
            #else
            GameObject container = null;
            #endif

            #if DEV_MODE && DEBUG_LAZY_INIT
            Debug.Log($"LazyInit({definingType.Name} => {classWithAttribute.FullName})");
            #endif

            var initialized = new HashSet<Type>();
            GetOrCreateInstance(classWithAttribute, serviceAttribute, ref container, initialized);
            InjectCrossServiceDependencies(classWithAttribute, initialized, ref container);

            #if UNITY_EDITOR
            if(container != null && container.TryGetComponent(out ServicesDebugger debugger))
            {
                debugger.SetServices(services.Values.ToArray());
            }
            #endif
        }

        private static void GetOrCreateInstance(Type classWithAttribute, ServiceAttribute serviceAttribute, ref GameObject container, HashSet<Type> initialized)
        {
            var definingType = serviceAttribute.definingType;
            if(definingType is null)
            {
                definingType = classWithAttribute;
            }

            // If one class contains multiple Service attributes still create only one shared instance.
            if(services.TryGetValue(classWithAttribute, out var existingInstance))
            {
                ServiceUtility.SetInstance(definingType, existingInstance);
                services[definingType] = existingInstance;
                return;
            }

            var instance = LoadInstance(classWithAttribute, definingType, serviceAttribute, ref container, initialized);
            if(instance is null)
            {
                #if DEV_MODE
                Debug.LogWarning($"GetInstance({classWithAttribute.Name}. {definingType.Name}) returned instance was null.");
                #endif
                return;
            }

            ServiceUtility.SetInstance(definingType, instance);
            services[definingType] = instance;
            services[classWithAttribute] = instance;
        }

        private static object LoadInstance([JetBrains.Annotations.NotNull] Type classWithAttribute, [JetBrains.Annotations.NotNull] Type definingType, [JetBrains.Annotations.NotNull] ServiceAttribute serviceAttribute, ref GameObject container, HashSet<Type> initialized)
        {
            if(serviceAttribute.FindFromScene)
            {
                Object instance = typeof(Component).IsAssignableFrom(classWithAttribute) ? Object.FindObjectOfType(classWithAttribute) : null;

                if(instance != null)
                {
                    #if DEBUG_CREATE_SERVICES
                    Debug.Log($"Service {classWithAttribute.Name} retrieved from scene successfully.", instance);
                    #endif

                    if(definingType.IsAssignableFrom(classWithAttribute))
					{
                        return instance;
                    }

                    if(instance is IValueProvider valueProvider)
                    {
                        if(valueProvider.Value is object value)
						{
                            return value;
						}

                        if(valueProvider is IInitializer initializerWithAttribute)
						{
                            return initializerWithAttribute.InitTarget();
                        }
                    }
                }

                var initializer = Find.All<IInitializer>().Where(i => i.TargetIsAssignableOrConvertibleToType(classWithAttribute)).FirstOrDefault();
                // NOTE: Intentionally not using Null but null.
                // InitTarget can still return the previously initialized target even if the initializer has been destroyed.
                if(initializer != null && initializer.InitTarget() is object initializedTarget)
                {
                    #if DEBUG_CREATE_SERVICES
                    Debug.Log($"Service {classWithAttribute.Name} retrieved from scene successfully.", instance);
                    #endif

                    var initializedType = initializedTarget.GetType();
                    if(definingType.IsAssignableFrom(initializedType))
					{
                        return initializedTarget;
					}

                    if(initializedTarget is IValueProvider valueProvider)
					{
                        return valueProvider.Value;
                    }
                }

                foreach(var services in Find.All<Services>())
				{
					if(!services.AreAvailableToAnyClient())
					{
						continue;
					}

					foreach(var service in services.providesServices)
					{
						if(service.definingType.Value == definingType && TryGetService(service.service, classWithAttribute, definingType, out var result))
						{
							return result;
						}
					}
				}

				if(typeof(ScriptableObject).IsAssignableFrom(classWithAttribute))
                {
                    Debug.LogWarning($"Invalid Service Definition: Service '{classWithAttribute.Name}' is a ScriptableObject type but has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. ScriptableObjects can not exist in scenes and as such can't be retrived using this method.");
                    return null;
                }

                #if UNITY_EDITOR
                if(!IsFirstSceneInBuildSettingsLoaded()) { return null; }
                #endif


                Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found in the active scene {SceneManager.GetActiveScene().name}, but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.FindFromScene)} set to true. Either add an instance to the scene or don't set {nameof(ServiceAttribute.FindFromScene)} true to have a new instance be created automatically.");
                return null;
            }

            if(serviceAttribute.ResourcePath is string resourcePath)
            {
                var asset = Resources.Load(resourcePath, typeof(Object));
                if(asset == null)
                {
                    Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found at resource path 'Resources/{resourcePath}', but the service class has the {nameof(ServiceAttribute)} {nameof(ServiceAttribute.ResourcePath)} set to '{resourcePath}'. Either make sure an asset exists in the project at the expected path or don't specify a {nameof(ServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
                    return null;
                }

                if(TryGetService(asset, classWithAttribute, definingType, out object result))
                {
                    #if DEBUG_CREATE_SERVICES
                    Debug.Log($"Service {classWithAttribute.Name} loaded from resources successfully.", asset);
                    #endif

                    return result;
                }

                Debug.LogWarning($"Service Not Found: Resource at path 'Resources/{resourcePath}' could not be converted to type {definingType.Name}.");
                return null;
            }

            #if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
            if(serviceAttribute.AddressableKey is string addressableKey)
            {
                var asyncOperation = Addressables.LoadAssetAsync<Object>(addressableKey);
                var asset = asyncOperation.WaitForCompletion();
                if(asset == null)
                {
                    Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found in the Addressable registry under the address {addressableKey}, but the service class has the {nameof(ServiceAttribute)} with {nameof(ServiceAttribute.AddressableKey)} set to '{addressableKey}'. Either make sure an instance with the address exists in the project or don't specify a {nameof(ServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
                    return null;
                }

                if(TryGetService(asset, classWithAttribute, definingType, out object result))
                {
                    #if DEBUG_CREATE_SERVICES
                    Debug.Log($"Service {classWithAttribute.Name} loaded using Addressables successfully.", asset);
                    Debug.Assert(asset != null, addressableKey, asset);
                    Debug.Assert(result != null, addressableKey, asset);
                    #endif

                    return result;
                }

                Debug.LogWarning($"Service Not Found: Addressable in the Addressable registry under the address {addressableKey} could not be converted to type {definingType.Name}.");
                return null;
            }
            #endif

            if(typeof(Component).IsAssignableFrom(classWithAttribute))
            {
                if(container == null)
                {
                    container = CreateServicesContainer();
                }

                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {classWithAttribute.Name} added to Services container.", container);
                #endif

                return container.AddComponent(classWithAttribute);
            }

            if(typeof(ScriptableObject).IsAssignableFrom(classWithAttribute))
            {
                #if DEBUG_CREATE_SERVICES
                Debug.Log($"Service {classWithAttribute.Name} created successfully.");
                #endif

                return ScriptableObject.CreateInstance(classWithAttribute);
            }

            #if !INIT_ARGS_DISABLE_SERVICE_CONSTRUCTOR_SUPPORT
            if(initialized.Contains(classWithAttribute))
			{
                return null;
			}

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var constructors = classWithAttribute.GetConstructors(flags);
            IEnumerable<ConstructorInfo> constructorsByParameterCount = constructors.Length <= 1 ? constructors as IEnumerable<ConstructorInfo> : constructors.OrderByDescending(c => c.GetParameters().Length);
            foreach(var constructor in constructorsByParameterCount)
            {
                var parameters = constructor.GetParameters();

                switch(parameters.Length)
				{
                    case 1:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out object argument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(argument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { argument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                    case 2:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out object firstArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[1], out object secondArgument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { firstArgument, secondArgument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                    case 3:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out firstArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[1], out secondArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[2], out object thirdArgument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { firstArgument, secondArgument, thirdArgument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                    case 4:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out firstArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[1], out secondArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[2], out thirdArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[3], out object fourthArgument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                    case 5:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out firstArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[1], out secondArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[2], out thirdArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[3], out fourthArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[4], out object fifthArgument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(fifthArgument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                    case 6:
                        initialized.Add(classWithAttribute);
                        if(TryGetOrCreateService(parameters[0], out firstArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[1], out secondArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[2], out thirdArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[3], out fourthArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[4], out fifthArgument, ref container, initialized)
                        && TryGetOrCreateService(parameters[5], out object sixthArgument, ref container, initialized))
                        {
                            InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(fifthArgument.GetType(), initialized, ref container);
                            InjectCrossServiceDependencies(sixthArgument.GetType(), initialized, ref container);
                            return constructor.Invoke(new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument });
                        }
                        initialized.Remove(classWithAttribute);
                        break;
                }
			}
            #endif

            #if DEBUG_CREATE_SERVICES
            Debug.Log($"Service {classWithAttribute} created successfully via default constructor.");
            #endif

            return Activator.CreateInstance(classWithAttribute);
        }

        #if !INIT_ARGS_DISABLE_SERVICE_CONSTRUCTOR_SUPPORT
        private static bool TryGetOrCreateService(ParameterInfo parameter, out object service, ref GameObject container, HashSet<Type> initialized)
        {
            var parameterType = parameter.ParameterType;
            if(TryGetService(parameterType, out service, ref container, initialized))
			{
                return true;
			}

            foreach(var serviceDefinition in GetServiceDefinitions())
            {
                if(parameterType == serviceDefinition.classWithAttribute || parameterType == serviceDefinition.attribute.definingType)
                {
                    GetOrCreateInstance(serviceDefinition.classWithAttribute, serviceDefinition.attribute, ref container, initialized);
                    return services.TryGetValue(parameterType, out service);
                }
            }

            return false;
        }
        #endif

        private static bool TryGetService([JetBrains.Annotations.NotNull] GameObject gameObject, [JetBrains.Annotations.NotNull] Type classWithAttribute, [JetBrains.Annotations.NotNull] Type definingType, out object instance)
        {
            if(typeof(Component).IsAssignableFrom(classWithAttribute) && gameObject.TryGetComponent(classWithAttribute, out var component))
            {
                if(definingType.IsAssignableFrom(component.GetType()))
                {
                    instance = component;
                    return true;
                }

                if(component is IValueProvider valueProvider && valueProvider.Value is object value && definingType.IsAssignableFrom(value.GetType()))
                {
                    instance = value;
                    return true;
                }

                if(component is IInitializer initializer && initializer.TargetIsAssignableOrConvertibleToType(definingType))
                {
                    instance = initializer.InitTarget();

                    var initializedType = instance.GetType();
                    if(definingType.IsAssignableFrom(initializedType))
					{
                        return true;
					}

                    valueProvider = instance as IValueProvider;
                    if(valueProvider != null)
					{
                        instance = valueProvider.Value;
                    }

                    return instance != null;
                }

                instance = null;
                return false;
            }

            var valueProviders = gameObject.GetComponents<IValueProvider>();
            foreach(var valueProvider in valueProviders)
            {
                var value = valueProvider.Value;
                if(value != null && definingType.IsAssignableFrom(value.GetType()))
                {
                    instance = value;
                    return true;
                }
            }

            foreach(var valueProvider in valueProviders)
            {
                if(valueProvider is IInitializer initializer && initializer.TargetIsAssignableOrConvertibleToType(definingType))
                {
                    instance = initializer.InitTarget();
                    return instance != null;
                }
            }

            instance = null;
            return false;
        }

        private static bool TryGetService([JetBrains.Annotations.NotNull] Object unityObject, [JetBrains.Annotations.NotNull] Type classWithAttribute, [JetBrains.Annotations.NotNull] Type definingType, out object instance)
        {
            if(unityObject is GameObject gameObject)
            {
                return TryGetService(gameObject, classWithAttribute, definingType, out instance);
            }

            if(definingType.IsAssignableFrom(unityObject.GetType()))
            {
                instance = unityObject;
                return true;
            }

            if(unityObject is IWrapper wrapper)
            {
                var wrapped = wrapper.WrappedObject;
                if(wrapped != null && definingType.IsAssignableFrom(wrapped.GetType()))
                {
                    instance = wrapped;
                    return true;
                }
            }

            if(unityObject is IInitializer initializer && initializer.TargetIsAssignableOrConvertibleToType(definingType))
            {
                instance = initializer.InitTarget();
                return instance != null;
            }

            instance = null;
            return false;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Warnings about missing Services should be suppressed when entering Play Mode from a scene
        /// which is not the first enabled one in build settings.
        /// </summary>
        /// <returns></returns>
        private static bool IsFirstSceneInBuildSettingsLoaded()
        {
            string firstSceneInBuildsPath = UnityEditor.EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled)?.path ?? "";
            Scene firstSceneInBuilds = SceneManager.GetSceneByPath(firstSceneInBuildsPath);
            return firstSceneInBuilds.IsValid() && firstSceneInBuilds.isLoaded;
        }
        #endif

        private static void InjectCrossServiceDependencies(ServiceDefinitions serviceDefinitions, ref GameObject container, HashSet<Type> initialized)
        {
            foreach(var serviceDefinition in serviceDefinitions)
            {
                var classWithAttribute = serviceDefinition.classWithAttribute;
                if(classWithAttribute.IsAbstract || classWithAttribute.IsGenericTypeDefinition)
                {
                    continue;
                }

                var definingType = serviceDefinition.attribute.definingType;
                if(!unitializedServicesByDefiningType.ContainsKey(definingType is null ? classWithAttribute : definingType))
                {
                    InjectCrossServiceDependencies(classWithAttribute, initialized, ref container);
                }
            }
        }

        private static void InjectServiceDependenciesForTypesThatRequireOnlyThem()
        {
            if(services is null)
            {
                return;
            }

            var setMethodsByArgumentCount = GetInitArgsSetMethods();

            var setMethodArgumentTypes = new Type[] { typeof(Type), null };
            var setMethodArguments = new object[2];
            foreach(var clientType in GetImplementingTypes<IOneArgument>())
            {
                if(clientType.IsAbstract)
                {
                    continue;
                }
                TrySetOneServiceDependency(clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[1], setMethodArgumentTypes, setMethodArguments);
            }
            setMethodArgumentTypes = new Type[] { typeof(Type), null, null };
            setMethodArguments = new object[3];
            foreach(var clientType in GetImplementingTypes<ITwoArguments>())
            {
                if(clientType.IsAbstract)
                {
                    continue;
                }
                TrySetTwoServiceDependencies(clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[2], setMethodArgumentTypes, setMethodArguments);
            }
            setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null };
            setMethodArguments = new object[4];
            foreach(var clientType in GetImplementingTypes<IThreeArguments>())
            {
                if(clientType.IsAbstract)
                {
                    continue;
                }
                TrySetThreeServiceDependencies(clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[3], setMethodArgumentTypes, setMethodArguments);
            }
            setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null, null };
            setMethodArguments = new object[5];
            foreach(var clientType in GetImplementingTypes<IFourArguments>())
            {
                if(clientType.IsAbstract)
                {
                    continue;
                }
                TryInjectFourServiceDependencies(clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[4], setMethodArgumentTypes, setMethodArguments);
            }
            setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null, null, null };
            setMethodArguments = new object[6];
            foreach(var clientType in GetImplementingTypes<IFiveArguments>())
            {
                if(clientType.IsAbstract)
                {
                    continue;
                }
                TryInjectFiveServiceDependencies(clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[5], setMethodArgumentTypes, setMethodArguments);
            }
        }

        private static Dictionary<int, MethodInfo> GetInitArgsSetMethods()
        {
            Dictionary<int, MethodInfo> setMethodsByArgumentCount = new Dictionary<int, MethodInfo>(5);

            foreach(MethodInfo method in typeof(InitArgs).GetMember(nameof(InitArgs.Set), MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic))
            {
                var genericArguments = method.GetGenericArguments();
                var parameters = method.GetParameters();
                int genericArgumentCount = genericArguments.Length;
                if(genericArgumentCount == parameters.Length - 1)
                {
                    setMethodsByArgumentCount.Add(genericArgumentCount, method);
                }
            }

            Debug.Assert(setMethodsByArgumentCount.Count == 6);

            return setMethodsByArgumentCount;
        }

        private static void InjectCrossServiceDependencies(Type classWithAttribute, HashSet<Type> initialized, ref GameObject container)
        {
            var concreteType = GetServiceConcreteType(classWithAttribute);
            if(!initialized.Add(concreteType))
            {
                return;
            }

            var interfaceTypes = concreteType.GetInterfaces();
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];

                if(!interfaceType.IsGenericType)
                {
                    continue;
                }

                var typeDefinition = interfaceType.GetGenericTypeDefinition();

                if(typeDefinition == typeof(IInitializable<,,,,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var firstArgumentType = argumentTypes[0];
                    var secondArgumentType = argumentTypes[1];
                    var thirdArgumentType = argumentTypes[2];
                    var fourthArgumentType = argumentTypes[3];
                    var fifthArgumentType = argumentTypes[4];
                    var sixthArgumentType = argumentTypes[5];
                    if(TryGetService(firstArgumentType, out object firstArgument, ref container, initialized)
                    && TryGetService(secondArgumentType, out object secondArgument, ref container, initialized)
                    && TryGetService(thirdArgumentType, out object thirdArgument, ref container, initialized)
                    && TryGetService(fourthArgumentType, out object fourthArgument, ref container, initialized)
                    && TryGetService(fifthArgumentType, out object fifthArgument, ref container, initialized)
                    && TryGetService(sixthArgumentType, out object sixthArgument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(fifthArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(sixthArgument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument, sixthArgument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires 6 arguments but instances not found among {services.Count} services..."); }
                    #endif
                }

                if(typeDefinition == typeof(IInitializable<,,,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var firstArgumentType = argumentTypes[0];
                    var secondArgumentType = argumentTypes[1];
                    var thirdArgumentType = argumentTypes[2];
                    var fourthArgumentType = argumentTypes[3];
                    var fifthArgumentType = argumentTypes[4];
                    if(TryGetService(firstArgumentType, out object firstArgument, ref container, initialized)
                    && TryGetService(secondArgumentType, out object secondArgument, ref container, initialized)
                    && TryGetService(thirdArgumentType, out object thirdArgument, ref container, initialized)
                    && TryGetService(fourthArgumentType, out object fourthArgument, ref container, initialized)
                    && TryGetService(fifthArgumentType, out object fifthArgument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(fifthArgument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires 5 arguments but instances not found among {services.Count} services..."); }
                    #endif
                }

                if(typeDefinition == typeof(IInitializable<,,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var firstArgumentType = argumentTypes[0];
                    var secondArgumentType = argumentTypes[1];
                    var thirdArgumentType = argumentTypes[2];
                    var fourthArgumentType = argumentTypes[3];
                    if(TryGetService(firstArgumentType, out object firstArgument, ref container, initialized)
                    && TryGetService(secondArgumentType, out object secondArgument, ref container, initialized)
                    && TryGetService(thirdArgumentType, out object thirdArgument, ref container, initialized)
                    && TryGetService(fourthArgumentType, out object fourthArgument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(fourthArgument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires 4 arguments but instances not found among {services.Count} services..."); }
                    #endif
                }

                if(typeDefinition == typeof(IInitializable<,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var firstArgumentType = argumentTypes[0];
                    var secondArgumentType = argumentTypes[1];
                    var thirdArgumentType = argumentTypes[2];
                    if(TryGetService(firstArgumentType, out object firstArgument, ref container, initialized)
                    && TryGetService(secondArgumentType, out object secondArgument, ref container, initialized)
                    && TryGetService(thirdArgumentType, out object thirdArgument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(thirdArgument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires 3 arguments but instances not found among {services.Count} services..."); }
                    #endif
                }

                if(typeDefinition == typeof(IInitializable<,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var firstArgumentType = argumentTypes[0];
                    var secondArgumentType = argumentTypes[1];
                    if(TryGetService(firstArgumentType, out object firstArgument, ref container, initialized)
                    && TryGetService(secondArgumentType, out object secondArgument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(firstArgument.GetType(), initialized, ref container);
                        InjectCrossServiceDependencies(secondArgument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires 2 arguments but instances not found among {services.Count} services..."); }
                    #endif
                }

                if(typeDefinition == typeof(IInitializable<>))
                {
                    var argumentType = interfaceType.GetGenericArguments()[0];
                    if(TryGetService(argumentType, out object argument, ref container, initialized))
                    {
                        InjectCrossServiceDependencies(argument.GetType(), initialized, ref container);

                        if(services.TryGetValue(concreteType, out object client))
                        {
                            interfaceType.GetMethod("Init").Invoke(client, new object[] { argument });
                            return;
                        }
                    }
                    #if DEBUG_INIT_SERVICES
                    else { Debug.Log($"Service {concreteType.Name} requires argument {interfaceType.GetGenericArguments()[0].Name} but instance not found among {services.Count} services..."); }
                    #endif
                }
            }
        }

        private static bool TryGetService(Type definingType, out object service, ref GameObject container, HashSet<Type> initialized)
        {
            if(services.TryGetValue(definingType, out service))
            {
                return true;
            }

            if(!unitializedServicesByDefiningType.TryGetValue(definingType, out var definition))
            {
                return false;
            }

            #if DEV_MODE && DEBUG_LAZY_INIT
            Debug.Log($"Initializing service {definingType.Name} with LazyInit=true because it is a dependency of another service.");
            #endif

            unitializedServicesByDefiningType.Remove(definingType);
            service = LoadInstance(definition.classWithAttribute, definingType, definition.attribute, ref container, initialized);

            if(service is null)
            {
                return false;
            }

            ServiceUtility.SetInstance(definingType, service);
            services[definingType] = service;
            services[GetServiceConcreteType(definition.classWithAttribute)] = service;

            return service != null;
        }

        private static void TrySetOneServiceDependency(Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
        {
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];

                if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IArgs<>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    var argumentType = argumentTypes[0];
                    if(services.TryGetValue(argumentType, out object argument))
                    {
                        setMethodArgumentTypes[1] = argumentType;
                        setMethodArguments[0] = clientType;
                        setMethodArguments[1] = argument;

                        setMethod = setMethod.MakeGenericMethod(argumentTypes);

                        #if DEV_MODE && DEBUG_INIT_SERVICES
                        Debug.Log($"Providing 1 service for client {clientType.Name}: {argument.GetType().Name}.");
                        #endif

                        setMethod.Invoke(null, setMethodArguments);
                    }
                    return;
                }
            }
        }

        private static void TrySetTwoServiceDependencies(Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
        {
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];
                if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument))
                    {
                        setMethodArgumentTypes[1] = argumentTypes[0];
                        setMethodArgumentTypes[2] = argumentTypes[1];
                        setMethodArguments[0] = clientType;
                        setMethodArguments[1] = firstArgument;
                        setMethodArguments[2] = secondArgument;

                        setMethod = setMethod.MakeGenericMethod(argumentTypes);

                        #if DEV_MODE && DEBUG_INIT_SERVICES
                        Debug.Log($"Providing 2 services for client {clientType.Name}: {firstArgument.GetType().Name}, {secondArgument.GetType().Name}. via method " + setMethod);
                        #endif

                        setMethod.Invoke(null, setMethodArguments);
                    }                    
                    return;
                }
            }
        }

        private static void TrySetThreeServiceDependencies(Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
        {
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];
                if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
                        && services.TryGetValue(argumentTypes[2], out object thirdArgument))
                    {
                        setMethodArgumentTypes[1] = argumentTypes[0];
                        setMethodArgumentTypes[2] = argumentTypes[1];
                        setMethodArgumentTypes[3] = argumentTypes[2];
                        setMethodArguments[0] = clientType;
                        setMethodArguments[1] = firstArgument;
                        setMethodArguments[2] = secondArgument;
                        setMethodArguments[3] = thirdArgument;

                        setMethod = setMethod.MakeGenericMethod(argumentTypes);

                        #if DEV_MODE && DEBUG_INIT_SERVICES
                        Debug.Log($"Providing 3 services for client {clientType.Name}: {firstArgument.GetType().Name}, {secondArgument.GetType().Name}, {thirdArgument.GetType().Name}.");
                        #endif

                        setMethod.Invoke(null, setMethodArguments);
                    }
                    return;
                }
            }
        }

        private static void TryInjectFourServiceDependencies(Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
        {
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];
                if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
                        && services.TryGetValue(argumentTypes[2], out object thirdArgument) && services.TryGetValue(argumentTypes[3], out object fourthArgument))
                    {
                        setMethodArgumentTypes[1] = argumentTypes[0];
                        setMethodArgumentTypes[2] = argumentTypes[1];
                        setMethodArgumentTypes[3] = argumentTypes[2];
                        setMethodArgumentTypes[4] = argumentTypes[3];
                        setMethodArguments[0] = clientType;
                        setMethodArguments[1] = firstArgument;
                        setMethodArguments[2] = secondArgument;
                        setMethodArguments[3] = thirdArgument;
                        setMethodArguments[4] = fourthArgument;

                        setMethod = setMethod.MakeGenericMethod(argumentTypes);

                        #if DEV_MODE && DEBUG_INIT_SERVICES
                        Debug.Log($"Providing 4 services for client {clientType.Name}: {firstArgument.GetType().Name}, {secondArgument.GetType().Name}, {thirdArgument.GetType().Name}, {fourthArgument.GetType().Name}.");
                        #endif

                        setMethod.Invoke(null, setMethodArguments);
                    }
                    return;
                }
            }
        }

        private static void TryInjectFiveServiceDependencies(Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
        {
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                var interfaceType = interfaceTypes[i];
                if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IArgs<,,,,>))
                {
                    var argumentTypes = interfaceType.GetGenericArguments();
                    if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
                        && services.TryGetValue(argumentTypes[2], out object thirdArgument) && services.TryGetValue(argumentTypes[3], out object fourthArgument)
                         && services.TryGetValue(argumentTypes[4], out object fifthArgument))
                    {
                        setMethodArgumentTypes[1] = argumentTypes[0];
                        setMethodArgumentTypes[2] = argumentTypes[1];
                        setMethodArgumentTypes[3] = argumentTypes[2];
                        setMethodArgumentTypes[4] = argumentTypes[3];
                        setMethodArgumentTypes[5] = argumentTypes[4];
                        setMethodArguments[0] = clientType;
                        setMethodArguments[1] = firstArgument;
                        setMethodArguments[2] = secondArgument;
                        setMethodArguments[3] = thirdArgument;
                        setMethodArguments[4] = fourthArgument;
                        setMethodArguments[5] = fifthArgument;

                        setMethod = setMethod.MakeGenericMethod(argumentTypes);

                        #if DEV_MODE && DEBUG_INIT_SERVICES
                        Debug.Log($"Providing 5 services for client {clientType.Name}: {firstArgument.GetType().Name}, {secondArgument.GetType().Name}, {thirdArgument.GetType().Name}, {fourthArgument.GetType().Name}, {fifthArgument.GetType().Name}.");
                        #endif

                        setMethod.Invoke(null, setMethodArguments);
                    }
                }
            }
        }

        private static void HandleBroadcastingUnityEvents()
        {
            foreach(var service in services.Values)
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

            foreach(var service in services.Values)
            {
                if(service is IAwake onEnable)
                {
                    onEnable.Awake();
                }
            }

            foreach(var service in services.Values)
            {
                if(service is IOnEnable onEnable)
                {
                    onEnable.OnEnable();
                }
            }

            foreach(var service in services.Values)
            {
                if(service is IStart start)
                {
                    start.Start();
                }
            }
        }

        [CanBeNull]
        internal static Type GetClassWithServiceAttribute(Type definingType)
        {
            foreach(var service in GetServiceDefinitions())
            {
                if(service.attribute.definingType is Type type)
				{
                    if(type == definingType)
					{
                        return service.classWithAttribute;
					}

                    continue;
				}

                if(definingType == service.classWithAttribute)
                {
                    return definingType;
                }
            }

            return null;
        }

        private static ServiceDefinitions GetServiceDefinitions()
        {
            return TypeUtility.GetTypesWithAttribute<ServiceAttribute>();
        }

        private static TypeCollection GetImplementingTypes<TInterface>() where TInterface : class
        {
            #if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesDerivedFrom<TInterface>();
            #else
            return TypeUtility.GetImplementingTypes<TInterface>(typeof(object).Assembly, typeof(InitArgs).Assembly);
            #endif
        }

        private static Type GetServiceConcreteType(Type classWithAttribute) => ServiceUtility.GetServiceConcreteType(classWithAttribute);
    }
}
#endif