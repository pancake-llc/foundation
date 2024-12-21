//#define DEBUG_INIT_TIME
//#define DEBUG_INIT_SERVICES
//#define DEBUG_CREATE_SERVICES

#if !INIT_ARGS_DISABLE_EDITOR_SERVICE_INJECTION && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.EditorOnly;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
#endif

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Class responsible for caching instances of all classes that have the <see cref="EditorServiceAttribute"/>,
	/// injecting dependencies for services that implement an <see cref="IInitializable{}"/>
	/// interface targeting only other services,
	/// and using <see cref="InitArgs.Set"/> to assign references to services ready to be retrieved
	/// for any other classes that implement an <see cref="IArgs{}"/> interface targeting only services.
	/// </summary>
	[InitializeOnLoad]
	internal static class EditorServiceInjector
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

		static EditorServiceInjector() => CreateAndInjectServices();

		/// <summary>
		/// Gets a value indicating whether <typeparamref name="T"/> is an editor service type.
		/// <para>
		/// Service types are non-abstract classes that have the <see cref="EditorServiceAttribute"/>.
		/// </para>
		/// </summary>
		/// <param name="type"> Type to test. </param>
		/// <returns> <see langword="true"/> if <typeparamref name="T"/> is a concrete service type; otherwise, <see langword="false"/>. </returns>
		public static bool IsServiceDefiningType([DisallowNull] Type type)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			return !type.IsValueType && typeof(Service<>).MakeGenericType(type).GetProperty(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is not null;
			#else
			return !type.IsValueType && typeof(Service<>).MakeGenericType(type).GetField(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is not null;
			#endif
		}

		/// <summary>
		/// Creates instances of all services,
		/// injects dependencies for servives that implement an <see cref="IInitializable{}"/>
		/// interface targeting only other services,
		/// and uses <see cref="InitArgs.Set"/> to assign references to services ready to be retrieved
		/// for any other classes that implement an <see cref="IArgs{}"/> interface targeting only services.
		/// </summary>
		private static void CreateAndInjectServices()
		{
			if(ServicesAreReady || Application.isPlaying)
			{
				return;
			}

			#if DEV_MODE
			UnityEngine.Profiling.Profiler.BeginSample("EditorServiceInjector.CreateAndInjectServices");
			#if DEBUG_INIT_TIME
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			#endif
			#endif

			InjectServiceDependenciesForTypesThatRequireOnlyThem(CreateInstancesOfAllServices());

			ServicesAreReady = true;
			OnServicesBecameReady?.Invoke();
			OnServicesBecameReady = null;

			// Make InactiveInitializer.OnAfterDeserialize continue execution
			Until.OnUnitySafeContext();

			ThreadSafe.Application.ExitingPlayMode -= OnExitingPlayMode;
			ThreadSafe.Application.ExitingPlayMode += OnExitingPlayMode;

			#if DEV_MODE
			UnityEngine.Profiling.Profiler.EndSample();
			#if DEBUG_INIT_TIME
			timer.Stop();
			Debug.Log($"Injection of editor services took {timer.Elapsed.TotalSeconds} seconds.");
			#endif
			#endif

			static void OnExitingPlayMode() => ServicesAreReady = false;
		}

		[return: MaybeNull]
		private static Dictionary<Type, object> CreateInstancesOfAllServices()
		{
			ThreadSafe.Application.IsPlaying = false;

			var serviceInfos = EditorServiceAttributeUtility.concreteTypes;
			var serviceTypes = serviceInfos.Keys;
			int count = serviceTypes.Count;
			if(count == 0)
			{
				return null;
			}

			GameObject servicesContainer = null;

			var services = new Dictionary<Type, object>(count);

			foreach(var serviceInfo in serviceInfos)
			{
				foreach(var attribute in serviceInfo.Value)
				{
					try
					{
						CreateService(services, serviceInfo.Key, attribute, ref servicesContainer);
					}
					catch(Exception e)
					{
						Debug.LogWarning($"Failed to initialize service {serviceInfo.Key.Name} with defining type {attribute.definingType.Name}.\n{e}");
					}
				}
			}

			foreach(var classWithAttribute in serviceTypes)
			{
				InjectCrossServiceDependencies(services, classWithAttribute);
			}

			if(servicesContainer)
			{
				servicesContainer.SetActive(true);
			}

			HandleBroadcastingUnityEvents(services);

			return services;
		}

		private static void CreateService(Dictionary<Type, object> services, Type classWithAttribute, EditorServiceAttribute serviceAttribute, ref GameObject containerGameObject)
		{
			var definingType = serviceAttribute.definingType;
			if(definingType is null)
			{
				definingType = classWithAttribute;
			}

			var instance = GetInstance(classWithAttribute, definingType, serviceAttribute, ref containerGameObject);
			if(instance is null)
			{
				return;
			}

			InjectServiceInstanceToRegistry(definingType, instance);
			services[definingType] = instance;
		}

		private static object GetInstance(Type classWithAttribute, Type definingType, EditorServiceAttribute serviceAttribute, ref GameObject containerGameObject)
		{
			if(serviceAttribute.FindAssetByType)
			{
				if(!Find.Asset(classWithAttribute, out object instance))
				{
					Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found in the asset database, but the service class has the {nameof(EditorServiceAttribute)} with {nameof(EditorServiceAttribute.FindAssetByType)} set to true. Either create an asset in the project view or don't set {nameof(EditorServiceAttribute.FindAssetByType)} true to have a new instance be created automatically.");
					return null;
				}

				#if DEBUG_CREATE_SERVICES
				Debug.Log($"Service {classWithAttribute.Name} retrieved from asset database successfully.", instance as Object);
				#endif

				return instance;
			}

			if(serviceAttribute.ResourcePath is string resourcePath)
			{
				if(!Find.Resource(classWithAttribute, resourcePath, out object instance))
				{
					Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found at resource path 'Resources/{resourcePath}', but the service class has the {nameof(EditorServiceAttribute)} {nameof(EditorServiceAttribute.ResourcePath)} set to '{resourcePath}'. Either make sure an asset exists in the project at the expected path or don't specify a {nameof(EditorServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
					return null;
				}

				#if DEBUG_CREATE_SERVICES
				Debug.Log($"Service {classWithAttribute.Name} loaded from resources successfully.", instance as Object);
				#endif

				return instance;
			}

			if(serviceAttribute.EditorDefaultResourcesPath is string editorDefaultResourcesPath)
			{
				Object obj = EditorGUIUtility.Load(editorDefaultResourcesPath);
				if(obj == null)
				{
					Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found at 'Editor Default Resources/{editorDefaultResourcesPath}', but the service class has the {nameof(EditorServiceAttribute)} {nameof(EditorServiceAttribute.EditorDefaultResourcesPath)} set to '{editorDefaultResourcesPath}'. Either make sure an asset exists in the project at the expected path or don't specify a {nameof(EditorServiceAttribute.EditorDefaultResourcesPath)} to have a new instance be created automatically.");
					return null;
				}

				if(classWithAttribute.IsInstanceOfType(obj))
				{
					#if DEBUG_CREATE_SERVICES
					Debug.Log($"Service {classWithAttribute.Name} loaded from resources successfully.", obj);
					#endif

					return obj;
				}

				if(!(obj is IWrapper wrapper) || !(wrapper.WrappedObject is object instance) || !classWithAttribute.IsInstanceOfType(instance))
				{
					Debug.LogWarning($"Service Not Found: Editor default resource 'Editor Default Resources/{editorDefaultResourcesPath}' did not match type of class {classWithAttribute.Name} which has the {nameof(EditorServiceAttribute)}.", obj);
					return null;
				}

				#if DEBUG_CREATE_SERVICES
				Debug.Log($"Service {classWithAttribute.Name} loaded from editor default resources successfully.", obj);
				#endif

				return instance;
			}

			#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
			if(serviceAttribute.AddressableKey is string addressableKey)
			{
				if(!Find.Addressable(classWithAttribute, addressableKey, out object instance))
				{
					Debug.LogWarning($"Service Not Found: There is no '{classWithAttribute.Name}' found in the Addressable registry under the address {addressableKey}, but the service class has the {nameof(EditorServiceAttribute)} with {nameof(EditorServiceAttribute.AddressableKey)} set to '{addressableKey}'. Either make sure an asset with the address exists in the project or don't specify a {nameof(EditorServiceAttribute.ResourcePath)} to have a new instance be created automatically.");
					return null;
				}

				#if DEBUG_CREATE_SERVICES
				Debug.Log($"Service {classWithAttribute.Name} loaded using Addressables successfully.", instance as Object);
				#endif

				return instance;
			}
			#endif

			#if DEBUG_CREATE_SERVICES
			Debug.Log($"Service {classWithAttribute.Name} created successfully.");
			#endif

			if(typeof(Component).IsAssignableFrom(classWithAttribute))
			{
				return containerGameObject.AddComponent(classWithAttribute);
			}

			if(typeof(ScriptableObject).IsAssignableFrom(classWithAttribute))
			{
				return ScriptableObject.CreateInstance(classWithAttribute);
			}

			return Activator.CreateInstance(classWithAttribute);
		}

		private static void InjectServiceInstanceToRegistry(Type definingType, object instance)
		{
			if(!definingType.IsInstanceOfType(instance))
			{
				if(definingType.IsInterface)
				{
					Debug.LogWarning($"Invalid Service Definition: Service '{instance.GetType().Name}' has the EditorServiceAttribute with defining interface type {definingType.Name} but it does not implement {definingType.Name}.");
					return;
				}

				Debug.LogWarning($"Invalid Service Definition: Service '{instance.GetType().Name}' has the EditorServiceAttribute with defining type {definingType.Name} but {definingType.Name} is not a derived type.");
				return;
			}

			ServiceUtility.Set(definingType, instance);
		}

		private static void InjectServiceDependenciesForTypesThatRequireOnlyThem([AllowNull] Dictionary<Type, object> services)
		{
			if(services == null)
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

				TrySetOneServiceDependency(services, clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[1], setMethodArgumentTypes, setMethodArguments);
			}

			setMethodArgumentTypes = new Type[] { typeof(Type), null, null };
			setMethodArguments = new object[3];
			foreach(var clientType in GetImplementingTypes<ITwoArguments>())
			{
				if(clientType.IsAbstract)
				{
					continue;
				}

				TrySetTwoServiceDependencies(services, clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[2], setMethodArgumentTypes, setMethodArguments);
			}

			setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null };
			setMethodArguments = new object[4];
			foreach(var clientType in GetImplementingTypes<IThreeArguments>())
			{
				if(clientType.IsAbstract)
				{
					continue;
				}

				TrySetThreeServiceDependencies(services, clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[3], setMethodArgumentTypes, setMethodArguments);
			}

			setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null, null };
			setMethodArguments = new object[5];
			foreach(var clientType in GetImplementingTypes<IFourArguments>())
			{
				if(clientType.IsAbstract)
				{
					continue;
				}

				TryInjectFourServiceDependencies(services, clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[4], setMethodArgumentTypes, setMethodArguments);
			}

			setMethodArgumentTypes = new Type[] { typeof(Type), null, null, null, null, null };
			setMethodArguments = new object[6];
			foreach(var clientType in GetImplementingTypes<IFiveArguments>())
			{
				if(clientType.IsAbstract)
				{
					continue;
				}

				TryInjectFiveServiceDependencies(services, clientType, clientType.GetInterfaces(), setMethodsByArgumentCount[5], setMethodArgumentTypes, setMethodArguments);
			}
		}

		private static Dictionary<int, MethodInfo> GetInitArgsSetMethods()
		{
			const int MAX_INIT_ARGUMENT_COUNT = 12;

			Dictionary<int, MethodInfo> setMethodsByArgumentCount = new Dictionary<int, MethodInfo>(MAX_INIT_ARGUMENT_COUNT);

			foreach(MethodInfo method in typeof(InitArgs).GetMember(nameof(InitArgs.Set), MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic))
			{
				var arguments = method.GetGenericArguments();
				var parameters = method.GetParameters();
				int argumentCount = arguments.Length;
				if(argumentCount == parameters.Length - 1) // in addition to the init argument types, there is clientType
				{
					setMethodsByArgumentCount.Add(argumentCount, method);
				}
			}

			#if DEV_MODE
			Debug.Assert(setMethodsByArgumentCount.Count == MAX_INIT_ARGUMENT_COUNT);
			#endif

			return setMethodsByArgumentCount;
		}

		private static void InjectCrossServiceDependencies(Dictionary<Type, object> services, Type clientType)
		{
			var interfaceTypes = clientType.GetInterfaces();
			for(int i = interfaceTypes.Length - 1; i >= 0; i--)
			{
				var interfaceType = interfaceTypes[i];

				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var typeDefinition = interfaceType.GetGenericTypeDefinition();
				if(typeDefinition == typeof(IInitializable<>))
				{
					if(services.TryGetValue(interfaceType.GetGenericArguments()[0], out object argument))
					{
						if(services.TryGetValue(clientType, out object client))
						{
							interfaceType.GetMethod("Init").Invoke(client, new object[] { argument });
						}
					}
					#if DEBUG_INIT_SERVICES
					else { Debug.Log($"Service {clientType.Name} requires argument {interfaceType.GetGenericArguments()[0].Name} but instance not found among {services.Count} services..."); }
					#endif
					return;
				}

				if(typeDefinition == typeof(IInitializable<,>))
				{
					var argumentTypes = interfaceType.GetGenericArguments();
					if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument))
					{
						if(services.TryGetValue(clientType, out object client))
						{
							interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument });
						}
					}
					#if DEBUG_INIT_SERVICES
					else { Debug.Log($"Service {clientType.Name} requires 2 arguments but instances not found among {services.Count} services..."); }
					#endif
					return;
				}

				if(typeDefinition == typeof(IInitializable<,,>))
				{
					var argumentTypes = interfaceType.GetGenericArguments();
					if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
						&& services.TryGetValue(argumentTypes[2], out object thirdArgument))
					{
						if(services.TryGetValue(clientType, out object client))
						{
							interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument });
						}
					}
					#if DEBUG_INIT_SERVICES
					else { Debug.Log($"Service {clientType.Name} requires 3 arguments but instances not found among {services.Count} services..."); }
					#endif
					return;
				}

				if(typeDefinition == typeof(IInitializable<,,,>))
				{
					var argumentTypes = interfaceType.GetGenericArguments();
					if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
						&& services.TryGetValue(argumentTypes[2], out object thirdArgument) && services.TryGetValue(argumentTypes[3], out object fourthArgument))
					{
						if(services.TryGetValue(clientType, out object client))
						{
							interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument });
						}
					}
					#if DEBUG_INIT_SERVICES
					else { Debug.Log($"Service {clientType.Name} requires 4 arguments but instances not found among {services.Count} services..."); }
					#endif
					return;
				}

				if(typeDefinition == typeof(IInitializable<,,,,>))
				{
					var argumentTypes = interfaceType.GetGenericArguments();
					if(services.TryGetValue(argumentTypes[0], out object firstArgument) && services.TryGetValue(argumentTypes[1], out object secondArgument)
						&& services.TryGetValue(argumentTypes[2], out object thirdArgument) && services.TryGetValue(argumentTypes[3], out object fourthArgument)
							&& services.TryGetValue(argumentTypes[4], out object fifthArgument))
					{
						if(services.TryGetValue(clientType, out object client))
						{
							interfaceType.GetMethod("Init").Invoke(client, new object[] { firstArgument, secondArgument, thirdArgument, fourthArgument, fifthArgument });
						}
					}
					#if DEBUG_INIT_SERVICES
					else { Debug.Log($"Service {clientType.Name} requires 5 arguments but instances not found among {services.Count} services..."); }
					#endif
				}
			}
		}

		private static void TrySetOneServiceDependency(Dictionary<Type, object> services, Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
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

		private static void TrySetTwoServiceDependencies(Dictionary<Type, object> services, Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
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
						Debug.Log($"Providing 2 services for client {clientType.Name}: {firstArgument.GetType().Name}, {secondArgument.GetType().Name}.");
						#endif

						setMethod.Invoke(null, setMethodArguments);
					}
					return;
				}
			}
		}

		private static void TrySetThreeServiceDependencies(Dictionary<Type, object> services, Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
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

		private static void TryInjectFourServiceDependencies(Dictionary<Type, object> services, Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
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

		private static void TryInjectFiveServiceDependencies(Dictionary<Type, object> services, Type clientType, Type[] interfaceTypes, MethodInfo setMethod, Type[] setMethodArgumentTypes, object[] setMethodArguments)
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

		private static void HandleBroadcastingUnityEvents(Dictionary<Type, object> services)
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

		//private static TypeCache.TypeCollection GetServiceTypes() => TypeCache.GetTypesWithAttribute<EditorServiceAttribute>();
		private static TypeCache.TypeCollection GetImplementingTypes<TInterface>() where TInterface : class => TypeCache.GetTypesDerivedFrom<TInterface>();
	}
}
#endif