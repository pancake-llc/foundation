//#define DEBUG_SORTING
//#define DEBUG_SORTING_DETAILED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class responsible for reapplying script execution orders for all initializers so that
	/// all dependencies are initialized before the dependent objects.
	/// </summary>
	internal sealed class InitializerExecutionOrderApplier
	{
		private readonly HashSet<Type> initializerInterfaceTypes = new()
		{
			typeof(IInitializer<,>),
			typeof(IInitializer<,,>),
			typeof(IInitializer<,,,>),
			typeof(IInitializer<,,,,>),
			typeof(IInitializer<,,,,,>),
			typeof(IInitializer<,,,,,,>)
		};

		private readonly Dictionary<Type, Type> clientTypes;
		private readonly Dictionary<Type, Type[]> genericArgumentTypes;
		private readonly Dictionary<Type, Type[]> initAfter;
		private readonly Dictionary<Type, HashSet<Type>> initBefore;
		private readonly HashSet<Type> typesWithDefaultExecutionOrderAttribute;
		private readonly HashSet<Type> serviceTypes;
		private readonly Dictionary<Type, int> currentExecutionOrders;
		private readonly Dictionary<Type, MonoScript> scriptAssets;
		private readonly Dictionary<Type, SortingState> sortingStates;
		private readonly Type[] initializerTypes;
		private readonly int initializerCount;

		public InitializerExecutionOrderApplier()
		{
			clientTypes = new Dictionary<Type, Type>(128);
			genericArgumentTypes = new Dictionary<Type, Type[]>(128);
			
			var typesWithInitAfterAttribute = TypeCache.GetTypesWithAttribute<InitAfterAttribute>();
			initAfter = new Dictionary<Type, Type[]>(typesWithInitAfterAttribute.Count);
			initBefore = new Dictionary<Type, HashSet<Type>>(typesWithInitAfterAttribute.Count);

			foreach(var type in typesWithInitAfterAttribute)
			{
				var afterTypes = type.GetCustomAttribute<InitAfterAttribute>().types;

				initAfter.Add(type, afterTypes);

				foreach(var afterType in afterTypes)
				{
					if(!initBefore.TryGetValue(afterType, out var beforeTypes))
					{
						beforeTypes = new HashSet<Type>();
						initBefore[afterType] = beforeTypes;
					}
					else if(beforeTypes.Contains(type))
					{
						continue;
					}

					beforeTypes.Add(type);
				}
			}

			typesWithDefaultExecutionOrderAttribute = new HashSet<Type>(TypeCache.GetTypesWithAttribute<DefaultExecutionOrder>());
			serviceTypes = new HashSet<Type>(GetAllTypesWithServiceAttributeAndServiceDefiningTypesThreadSafe());

			const int DefaultOrder = ExecutionOrder.Initializer + (int)Order.Default;
			const int ServiceOrder = ExecutionOrder.ServiceInitializer;
			initializerTypes = TypeCache.GetTypesDerivedFrom<IInitializer>()
				.Where(t => !t.IsAbstract)
				.OrderBy(t => typesWithDefaultExecutionOrderAttribute.Contains(t)
					&& t.GetCustomAttributes<DefaultExecutionOrder>().FirstOrDefault() is DefaultExecutionOrder defaultExecutionOrder
					? defaultExecutionOrder.order
					: serviceTypes.Contains(GetClientType(t))
					? ServiceOrder
					: DefaultOrder)
				.ThenBy(t => t.Assembly.GetName().Name)
				.ThenBy(t => t.Namespace ?? "")
				.ThenBy(t => t.FullName)
				.ToArray();

			initializerCount = initializerTypes.Length;
			sortingStates = new Dictionary<Type, SortingState>(initializerCount);
			currentExecutionOrders = new Dictionary<Type, int>(initializerCount);
			scriptAssets = new Dictionary<Type, MonoScript>(initializerCount);
			
			static IEnumerable<Type> GetAllTypesWithServiceAttributeAndServiceDefiningTypesThreadSafe()
			{
				foreach(var type in TypeCache.GetTypesWithAttribute<ServiceAttribute>())
				{
					yield return type;

					foreach(var attribute in type.GetCustomAttributes<ServiceAttribute>())
					{
						foreach(var definingType in attribute.definingTypes)
						{
							yield return definingType;
						}
					}
				}
			}
		}

		public void UpdateExecutionOrderOfAllInitializers()
		{
			var initializersInOptimalOrder = GetAllInitializerSortedByInitOrder();
			if(initializerCount == 0)
			{
				return;
			}

			int i = initializerCount - 1;
			Type firstInitializer = initializersInOptimalOrder[i];
			while(typesWithDefaultExecutionOrderAttribute.Contains(firstInitializer))
			{
				i--;
				if(i < 0)
				{
					return;
				}

				firstInitializer = initializersInOptimalOrder[i];
			}

			int currentInitializerOrder = GetScriptExecutionOrder(firstInitializer);
			const int FirstMinOrder = ExecutionOrder.Initializer + (int)Order.VeryEarly;
			const int FirstMaxOrder = ExecutionOrder.Initializer + (int)Order.VeryLate;
			const int FirstDefaultOrder = ExecutionOrder.Initializer + (int)Order.Late;
			if(currentInitializerOrder < FirstMinOrder || currentInitializerOrder > FirstMaxOrder)
			{
				#if DEV_MODE && DEBUG_SET_ORDER
				Debug.Log($"Changing {firstInitializer.Name} order from {currentInitializerOrder} to {FirstDefaultOrder} [FIRST]");
				#endif

				SetScriptExecutionOrder(firstInitializer, FirstDefaultOrder);
				currentInitializerOrder = FirstDefaultOrder;
			}
			#if DEV_MODE && DEBUG_SET_ORDER
			else Debug.Log($"NOT Changing {firstInitializer.Name} order from {currentInitializerOrder} [FIRST]");
			#endif

			if(i == 0)
			{
				return;
			}

			i--;
			int previousInitializerOrder;
			int nextInitializerOrder = GetScriptExecutionOrder(initializersInOptimalOrder[i]);
			const int STEP_SIZE = 5;
			const int ASSEMBLY_STEP_SIZE = 100;
			Assembly previousTypeAssembly = firstInitializer.Assembly;

			for(; i >= 1; i--)
			{
				Type initializerType = initializersInOptimalOrder[i];
				Assembly initializerAssembly = initializerType.Assembly;
				previousInitializerOrder = currentInitializerOrder;
				currentInitializerOrder = nextInitializerOrder;
				Type nextInitializerType = initializersInOptimalOrder[i - 1];
				nextInitializerOrder = GetScriptExecutionOrder(nextInitializerType);

				if(typesWithDefaultExecutionOrderAttribute.Contains(initializerType))
				{
					#if DEV_MODE && DEBUG_SET_ORDER
					Debug.Log($"NOT Changing {initializerType.Name} order from {currentInitializerOrder} because it has the DefaultExecutionOrderAttribute. [{i}/{initializerCount}]");
					#endif

					continue;
				}

				bool assemblyHasChanged = initializerAssembly != previousTypeAssembly;
				int maxDistanceToPrevious = assemblyHasChanged ? ASSEMBLY_STEP_SIZE : STEP_SIZE + STEP_SIZE;
				int stepSize = assemblyHasChanged ? ASSEMBLY_STEP_SIZE : STEP_SIZE;
				previousTypeAssembly = initializerAssembly;

				bool isCloseEnoughToPreviousOrder =	currentInitializerOrder < previousInitializerOrder
													&& previousInitializerOrder - currentInitializerOrder <= maxDistanceToPrevious;

				if(isCloseEnoughToPreviousOrder && currentInitializerOrder > nextInitializerOrder)
				{
					#if DEV_MODE && DEBUG_SET_ORDER
					Debug.Log($"NOT Changing {initializerType.Name} order from {currentInitializerOrder} because current order fits between previous order {previousInitializerOrder} and next order {nextInitializerOrder}. [{i}/{initializerCount}]");
					#endif

					continue;
				}

				int differenceBetweenPreviousAndNext = previousInitializerOrder - nextInitializerOrder;

				if(isCloseEnoughToPreviousOrder
					&& (differenceBetweenPreviousAndNext <= 1 || differenceBetweenPreviousAndNext > STEP_SIZE + STEP_SIZE))
				{
					#if DEV_MODE && DEBUG_SET_ORDER
					Debug.Log($"NOT Changing {initializerType.Name} order from {currentInitializerOrder} because current order less than {STEP_SIZE} from previous order {previousInitializerOrder} (ignoring next order {nextInitializerOrder}). [{i}/{initializerCount}]");
					#endif

					continue;
				}

				if(differenceBetweenPreviousAndNext > 1 && differenceBetweenPreviousAndNext <= STEP_SIZE + STEP_SIZE)
				{
					#if DEV_MODE && DEBUG_SET_ORDER
					Debug.Log($"Changing {initializerType.Name} order from {currentInitializerOrder} to {Mathf.Clamp((previousInitializerOrder + nextInitializerOrder) / 2, nextInitializerOrder + 1, previousInitializerOrder - 1)} (midway point) with previousInitializerOrder={previousInitializerOrder} and nextInitializerOrder={nextInitializerOrder} and diff={differenceBetweenPreviousAndNext}. [{i}/{initializerCount}]");
					#endif

					currentInitializerOrder = Mathf.Clamp((previousInitializerOrder + nextInitializerOrder) / 2, nextInitializerOrder + 1, previousInitializerOrder - 1);
				}
				else
				{
					#if DEV_MODE && DEBUG_SET_ORDER
					Debug.Log($"Changing {initializerType.Name} order from {currentInitializerOrder} to {previousInitializerOrder - STEP_SIZE} (step) with previousInitializerOrder={previousInitializerOrder} and nextInitializerOrder={nextInitializerOrder} and diff={differenceBetweenPreviousAndNext}. [{i}/{initializerCount}]");
					#endif

					currentInitializerOrder = previousInitializerOrder - stepSize;
				}

				SetScriptExecutionOrder(initializerType, currentInitializerOrder);
			}

			Type lastInitializer = initializersInOptimalOrder[0];
			previousInitializerOrder = currentInitializerOrder;
			currentInitializerOrder = nextInitializerOrder;

			if(typesWithDefaultExecutionOrderAttribute.Contains(lastInitializer))
			{
				#if DEV_MODE && DEBUG_SET_ORDER
				Debug.Log($"NOT Changing {lastInitializer.Name} order from {currentInitializerOrder} because it has the DefaultExecutionOrderAttribute. [LAST]");
				#endif

				return;
			}

			if(currentInitializerOrder < previousInitializerOrder
					&& previousInitializerOrder - currentInitializerOrder <= STEP_SIZE + STEP_SIZE)
			{
				#if DEV_MODE && DEBUG_SET_ORDER
				Debug.Log($"NOT Changing {lastInitializer.Name} order from {currentInitializerOrder} because current order {currentInitializerOrder} fits between previous order {previousInitializerOrder}. [LAST]");
				#endif

				return;
			}

			#if DEV_MODE && DEBUG_SET_ORDER
			Debug.Log($"Changing {lastInitializer.Name} order from {currentInitializerOrder} to {previousInitializerOrder - STEP_SIZE} (step) with previousInitializerOrder={previousInitializerOrder}. [LAST]");
			#endif

			SetScriptExecutionOrder(lastInitializer, previousInitializerOrder - STEP_SIZE);
		}

		private List<Type> GetAllInitializerSortedByInitOrder()
		{
			var results = new List<Type>(initializerCount);

			for(int i = 0; i < initializerCount; i++)
			{
				AddTypeAndItsDependenciesSorted(initializerTypes[i], results);
			}

			return results;
		}

		private int GetScriptExecutionOrder(Type classType)
		{
			if(!currentExecutionOrders.TryGetValue(classType, out int result))
			{
				result = GetScriptAsset(classType) is MonoScript script ? MonoImporter.GetExecutionOrder(script) : 0;
				currentExecutionOrders.Add(classType, result);
			}

			return result;
		}

		private MonoScript GetScriptAsset(Type classType)
		{
			if(!scriptAssets.TryGetValue(classType, out MonoScript result))
			{
				result = Find.Script(classType);
				scriptAssets.Add(classType, result);
			}

			return result;
		}

		private void SetScriptExecutionOrder(Type classType, int executionOrder)
		{
			if(GetScriptAsset(classType) is MonoScript script)
			{
				MonoImporter.SetExecutionOrder(script, executionOrder);
			}
		}

		private Type[] GetInitParameterTypes(Type initializerType)
		{
			if(genericArgumentTypes.TryGetValue(initializerType, out Type[] results))
			{
				return results;
			}

			return GetAndCacheClientAndParameterTypes(initializerType).argumentTypes;
		}

		private (Type clientType, Type[] argumentTypes) GetAndCacheClientAndParameterTypes(Type initializerType)
		{
			var interfaceTypes = initializerType.GetInterfaces();
			for(int i = 0, count = interfaceTypes.Length; i < count; i++)
			{
				var interfaceType = interfaceTypes[i];
				if(!interfaceType.IsGenericType || !initializerInterfaceTypes.Contains(interfaceType.GetGenericTypeDefinition()))
				{
					continue;
				}

				var genericTypes = interfaceType.GetGenericArguments();

				var clientType = genericTypes[0];
				clientTypes[initializerType] = clientType;

				var initArgumentTypes = genericTypes.Skip(1).ToArray();

				// Unpack tuple types, as their contents tell us more about dependencies than the tuples themselves.
				if(initArgumentTypes.Length == 1)
				{
					Type onlyParameterType = initArgumentTypes[0];
					if(onlyParameterType.IsGenericType)
					{
						var onlyParameterTypeDefinition = onlyParameterType.GetGenericTypeDefinition();
						if(onlyParameterTypeDefinition == typeof(ValueTuple<>) || onlyParameterTypeDefinition == typeof(Tuple<>))
						{
							initArgumentTypes = onlyParameterType.GetGenericArguments();
						}
					}
				}

				genericArgumentTypes[initializerType] = initArgumentTypes;

				return (clientType, initArgumentTypes);
			}

			var noTypes = Array.Empty<Type>();
			clientTypes[initializerType] = typeof(object);
			genericArgumentTypes[initializerType] = noTypes;
			return (typeof(object), noTypes);
		}

		private Type GetClientType(Type initializerType)
		{
			if(clientTypes.TryGetValue(initializerType, out Type result))
			{
				return result;
			}

			return GetAndCacheClientAndParameterTypes(initializerType).clientType;
		}

		/// <summary>
		/// Determines whether <paramref name="initializerA"/>'s client depends on
		/// <paramref name="initializerB"/>'s client.
		/// <para>
		/// If true then <paramref name="initializerB"/> should be initialized before
		/// <paramref name="initializerA"/>.
		/// </para>
		/// </summary>
		private bool Requires(Type initializerA, Type initializerB, Type initializerBClient)
		{
			if(initializerA == initializerB)
			{
				return false;
			}

			// A requires B if one of the Init arguments of A's target is B's target.
			foreach(var initializerAParameter in GetInitParameterTypes(initializerA))
			{
				// if b initializes one of the Init arguments of a, then b should run before a.
					
				if(TypeUtility.IsBaseType(initializerAParameter))
				{
					continue;
				}

				if(initializerAParameter.IsAssignableFrom(initializerBClient))
				{
					return true;
				}

				// A's client could require IValueProvider<B> instead of B directly.
				if(typeof(IValueProvider<>).MakeGenericType(initializerAParameter).IsAssignableFrom(initializerBClient))
				{
					return true;
				}
			}

			return false;
		}

		private void AddTypeAndItsDependenciesSorted(Type initializerType, List<Type> addToList)
		{
			if(!sortingStates.TryGetValue(initializerType, out var sortingState))
			{
				#if DEV_MODE && DEBUG_SORTING_DETAILED
				Debug.Log($"Adding {initializerType.Name}, but first adding all its dependencies...");
				#endif

				sortingStates.Add(initializerType, SortingState.NowAddingDependencies);

				foreach(var dependencyType in GetDependencies(initializerType))
				{
					#if DEV_MODE && DEBUG_SORTING_DETAILED
					Debug.Assert(dependencyType != initializerType);
					Debug.Log($"Adding {initializerType.Name} dependency {dependencyType.Name}...");
					#endif

					AddTypeAndItsDependenciesSorted(dependencyType, addToList);
				}

				sortingStates[initializerType] = SortingState.AlreadyAdded;

				if(!addToList.Contains(initializerType))
				{
					addToList.Add(initializerType);
				}

				return;
			}

			if(sortingState != SortingState.NowAddingDependencies)
			{
				#if DEV_MODE && DEBUG_SORTING_DETAILED
				Debug.Log($"{initializerType.Name} has already been added.");
				#endif
				return;
			}

			#if DEV_MODE && DEBUG_SORTING_DETAILED
			Debug.Log($"Cyclic dependency detected while adding {initializerType.Name}...");
			#endif

			#if (DEV_MODE && DEBUG_SORTING) || INIT_ARGS_WARN_ABOUT_ALL_CYCLIC_DEPENDENCIES
			bool indirectCyclicDependency = true;
			#endif

			foreach(var dependencyType in GetDependencies(initializerType))
			{
				if(dependencyType == initializerType)
				{
					continue;
				}

				foreach(var dependencyOfDependency in GetDependencies(dependencyType))
				{
					if(dependencyOfDependency != initializerType)
					{
						continue;
					}

					#if (DEV_MODE && DEBUG_SORTING) || INIT_ARGS_WARN_ABOUT_ALL_CYCLIC_DEPENDENCIES
					indirectCyclicDependency = false;
					#endif

					Type clientType = GetClientType(initializerType);
					Type dependencyClientType = GetClientType(dependencyType);

					var clientConcreteTypeOptions = GetConcreteTypeOptions(clientType).ToArray();
					var dependencyClientConcreteTypeOptions = GetConcreteTypeOptions(dependencyClientType).ToArray();

					bool clientMaybeComponent = clientConcreteTypeOptions.Any(t => typeof(Component).IsAssignableFrom(t));
					bool clientForSureComponent = clientMaybeComponent && clientConcreteTypeOptions.All(t => typeof(Component).IsAssignableFrom(t));

					bool dependencyClientMaybeComponent = dependencyClientConcreteTypeOptions.Any(t => typeof(Component).IsAssignableFrom(t));
					bool dependencyClientForSureComponent = dependencyClientConcreteTypeOptions.Any(t => typeof(Component).IsAssignableFrom(t));

					bool clientMaybeCreatableWithoutDependency = clientMaybeComponent || clientConcreteTypeOptions.Any(t => t.GetConstructors().Any(c => !c.GetParameters().Select(p => p.ParameterType).Contains(dependencyClientType)));
					bool clientForSureCreatableWithoutDependency = clientMaybeCreatableWithoutDependency && (clientForSureComponent || clientConcreteTypeOptions.All(t => t.GetConstructors().Any(c => !c.GetParameters().Select(p => p.ParameterType).Contains(dependencyClientType))));

					bool dependencyMaybeCreatableWithoutClient = dependencyClientMaybeComponent || dependencyClientConcreteTypeOptions.Any(t => t.GetConstructors().Any(c => !c.GetParameters().Select(p => p.ParameterType).Contains(clientType)));
					bool dependencyForSureCreatableWithoutClient = dependencyMaybeCreatableWithoutClient && (dependencyClientForSureComponent || dependencyClientConcreteTypeOptions.Any(t => t.GetConstructors().Any(c => !c.GetParameters().Select(p => p.ParameterType).Contains(clientType))));

					if(!clientMaybeCreatableWithoutDependency && !dependencyMaybeCreatableWithoutClient)
					{
						Debug.LogWarning($"{initializerType.Name} has a cyclic dependency with {dependencyType.Name}: {clientType.Name} depends on {dependencyClientType.Name}, and {dependencyClientType.Name} depends on {clientType.Name}. Because of this the system is unable to determine which initializer should be executed first.\nYou can use the {nameof(InitAfterAttribute)} to specify which Initializer should be executed first.");
						continue;
					}

					if(!dependencyMaybeCreatableWithoutClient || clientForSureCreatableWithoutDependency)
					{
						#if DEV_MODE && DEBUG_SORTING
						Debug.Log($"Adding {initializerType.Name} before {dependencyType.Name} in initialization order because {dependencyType.Name} client maybe can not be initialized without {initializerType.Name} client existing.");
						#endif

						addToList.Remove(dependencyType);
						addToList.Remove(initializerType);
						addToList.Add(initializerType);
						addToList.Add(dependencyType);
						sortingStates[initializerType] = SortingState.AlreadyAdded;
						sortingStates[dependencyType] = SortingState.AlreadyAdded;
						continue;
					}

					if(!clientMaybeCreatableWithoutDependency || dependencyForSureCreatableWithoutClient)
					{
						#if DEV_MODE && DEBUG_SORTING
						Debug.Log($"Adding {dependencyType.Name} before {initializerType.Name} in initialization order because {initializerType.Name} client maybe can not be initialized without {dependencyType.Name} client existing.");
						#endif

						addToList.Remove(dependencyType);
						addToList.Remove(initializerType);
						addToList.Add(dependencyType);
						addToList.Add(initializerType);
						sortingStates[initializerType] = SortingState.AlreadyAdded;
						sortingStates[dependencyType] = SortingState.AlreadyAdded;
					}
					#if (DEV_MODE && DEBUG_SORTING) || INIT_ARGS_WARN_ABOUT_ALL_CYCLIC_DEPENDENCIES
					else Debug.Log($"{initializerType.Name} has a cyclic dependency with {dependencyType.Name}, but it seems potentially resolvable.\nYou can use the {nameof(InitAfterAttribute)} to configure which Initializer should be executed first.");
					#endif
				}
			}

			#if (DEV_MODE && DEBUG_SORTING) || INIT_ARGS_WARN_ABOUT_ALL_CYCLIC_DEPENDENCIES
			if(indirectCyclicDependency)
			{
				Debug.LogWarning($"{initializerType.Name} has an indirect cyclic dependency. Not sure if it's resolvable or not.\nYou can use the {nameof(InitAfterAttribute)} to configure which Initializers should be executed first.");
			}
			#endif
		}

		/// <summary>
		/// Gets a list of types of initializers that the initializer of type <paramref name="initializerType"/> depends.
		/// </summary>
		/// <param name="initializerType"> The dependent initializer. </param>
		/// <returns> Collection of initializer types. </returns>
		private IEnumerable<Type> GetDependencies(Type initializerType)
		{
			bool hasUserDefinedDependencies = initAfter.TryGetValue(initializerType, out Type[] userDefinedDependencyTypes);
			bool isUserDefinedDependency = initBefore.TryGetValue(initializerType, out HashSet<Type> userDefinedDependencyOf);

			for(int i = 0; i < initializerCount; i++)
			{
				var potentialDependency = initializerTypes[i];
				var potentialDependencyClient = GetClientType(potentialDependency);

				if(Requires(initializerType, potentialDependency, potentialDependencyClient))
				{
					if(isUserDefinedDependency && userDefinedDependencyOf.Contains(potentialDependency))
					{
						continue;
					}

					yield return potentialDependency;
					continue;
				}

				if(!hasUserDefinedDependencies)
				{
					continue;
				}

				if(Array.IndexOf(userDefinedDependencyTypes, potentialDependency) != -1)
				{
					yield return potentialDependency;
					continue;
				}

				if(!potentialDependencyClient.IsAbstract)
				{
					if(Array.IndexOf(userDefinedDependencyTypes, potentialDependencyClient) != -1)
					{
						yield return potentialDependency;
					}

					continue;
				}

				foreach(Type potentialDependencyConcreteClient in TypeCache.GetTypesDerivedFrom(potentialDependencyClient))
				{
					if(Array.IndexOf(userDefinedDependencyTypes, potentialDependencyConcreteClient) != -1)
					{
						yield return potentialDependency;
					}
				}
			}
		}

		private IEnumerable<Type> GetConcreteTypeOptions(Type type)
		{
			var result = TypeCache.GetTypesDerivedFrom(type).Where(t => !t.IsAbstract);
			if(!type.IsAbstract)
			{
				result = result.Append(type);
			}

			return result;
		}

		private enum SortingState
		{
			NowAddingDependencies,
			AlreadyAdded
		}
    }
}