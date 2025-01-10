using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Information about a shared and globally accessible service registered using the <see cref="ServiceAttribute"/>.
	/// </summary>
	internal sealed class ServiceInfo
	{
		private static readonly Dictionary<Type, ServiceProviderType> genericInterfaceToServiceProviderType = new()
		{
			{ typeof(IServiceInitializer<>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializer<,,,,,,,,,,,,>), ServiceProviderType.ServiceInitializer },
			{ typeof(IServiceInitializerAsync<>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IServiceInitializerAsync<,,,,,,,,,,,,>), ServiceProviderType.ServiceInitializerAsync },
			{ typeof(IInitializer<>), ServiceProviderType.Initializer },
			{ typeof(IWrapper<>), ServiceProviderType.Wrapper },
			{ typeof(IValueProvider<>), ServiceProviderType.IValueProviderT },
			{ typeof(IValueProviderAsync<>), ServiceProviderType.IValueProviderAsyncT },
		};

		/// <summary>
		/// If <see cref="serviceProviderType"/> is <see cref="ServiceProviderType.None"/>,
		/// this is the concrete type of the service; otherwise, this is the type of the service provider.
		/// </summary>
		[NotNull] public readonly Type serviceOrProviderType;

		/// <summary>
		/// Specifies which clients have access to the service.
		/// </summary>
		public Clients clients => Clients.Everywhere;

		/// <summary>
		/// Can in theory be null in rare instances, if the <see cref="ServiceAttribute"/> was attached to an initializer
		/// like a <see cref="CustomInitializer"/> where the generic type for the initialized object is abstract.
		/// <para>
		/// Can also be a generic type definition. E.g. type Logger{T} registered using [Service(typeof(ILogger{}))].
		/// </para>
		/// </summary>
		[MaybeNull] public readonly Type concreteType;

		/// <summary>
		/// An array containing of the defining types of the service.
		/// </summary>
		[NotNull] public readonly Type[] definingTypes;

		public readonly LoadMethod loadMethod;
		public readonly ReferenceType referenceType;
		private readonly string loadData;

		/// <summary>
		/// Returns <see cref="concreteType"/> if it's not <see langword="null"/>,
		/// otherwise returns first element from <see cref="definingTypes"/>.
		/// <para>
		/// Can be an abstract or a generic type definition.
		/// </para>
		/// </summary>
		[NotNull] public Type ConcreteOrDefiningType => concreteType ?? definingTypes.FirstOrDefault();

		public bool IsTransient { get; }

		public readonly ServiceProviderType serviceProviderType;

		public bool IsValueProvider
			=> serviceProviderType is ServiceProviderType.IValueProvider
				or ServiceProviderType.IValueProviderT
				or ServiceProviderType.IValueProviderAsyncT
				or ServiceProviderType.IValueByTypeProvider
				or ServiceProviderType.IValueByTypeProviderAsync
				or ServiceProviderType.IValueProvider
				or ServiceProviderType.IValueProviderAsync;

		public readonly bool LazyInit;
		public readonly bool LoadAsync;
		public bool FindFromScene => loadMethod == LoadMethod.FindFromScene;
		public string SceneName => referenceType == ReferenceType.SceneName ? loadData : null;
		public int SceneBuildIndex => referenceType == ReferenceType.SceneBuildIndex && int.TryParse(loadData, out int buildIndex) ? buildIndex : -1;
		public string ResourcePath => referenceType is ReferenceType.ResourcePath ? loadData : null;

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		public string AddressableKey => referenceType is ReferenceType.AddressableKey ? loadData : null;
		#endif

		public bool ShouldInstantiate(bool isPrefab)
		{
			if(loadMethod is LoadMethod.Instantiate)
			{
				return true;
			}

			if(loadMethod is LoadMethod.Load)
			{
				return false;
			}

			return isPrefab;
		}

		public static IEnumerable<ServiceInfo> From([DisallowNull] Type typeWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			if (!typeWithAttribute.IsAbstract)
			{
				yield return new ServiceInfo(typeWithAttribute, attributes);
				yield break;
			}

			var registeredDefiningTypes = new Dictionary<Type, bool>(8);
			var concreteToDefiningTypes = new Dictionary<Type, HashSet<Type>>(8);

			foreach(var attribute in attributes)
			{
				foreach(var serviceType in attribute.definingTypes)
				{
					// Support
					// [Service(typeof(Logger<>))]
					// public interface ILogger<T> { }
					// registering the concrete class Logger<> as ILogger<>
					if(!serviceType.IsAbstract)
					{
						yield return new(typeWithAttribute, attributes, concreteType: serviceType, definingTypes: new[] { typeWithAttribute });
						registeredDefiningTypes[serviceType] = false;
						continue;
					}

					// Support
					// [Service(typeof(ISingleton<>))]
					// public interface ISingleton<T> { }
					// registering all types that implement ISingleton<TService> and ISingleton<TService>
					if(serviceType.IsGenericTypeDefinition)
					{
						foreach(var derivedType in TypeUtility.GetOpenGenericTypeDerivedTypes(serviceType, publicOnly: false, concreteOnly: true, 8))
						{
							AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, derivedType);
						}

						continue;
					}

					// Support
					// [Service(typeof(Logger))]
					// public interface ILogger { }
					// registering all Logger<T> as ILogger<TService>
					foreach(var derivedType in TypeUtility.GetDerivedTypes(typeWithAttribute, typeWithAttribute.Assembly, publicOnly: false, 8))
					{
						if(!derivedType.IsAbstract)
						{
							AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, derivedType);
						}
					}
				}
			}

			if (attributes.Length != 1 || attributes[0].definingTypes.Length > 0)
			{
				goto RegisterAll;
			}

			if(typeWithAttribute.IsGenericTypeDefinition)
			{
				// Support
				// [Service]
				// public interface ISingleton<TSingleton> { }
				// registering all implementing types as services, with their concrete type, all base types and all the interfaces types
				// as the defining types of the service, which fulfill these requirements:
				// 1. None other of the types that derive from Manager may also derive from or implement that same type.
				// 2. The type can't be a common built-in type, such as System.Object, UnityEngine.Object or IEnumerable.
				foreach(var derivedType in TypeUtility.GetOpenGenericTypeDerivedTypes(typeWithAttribute, publicOnly:false, concreteOnly:true, 8))
				{
					AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, derivedType);
				}

				goto RegisterAll;
			}

			// Support:
			// [Service]
			// public abstract class Manager { }
			// registering all derived types as services, with their concrete type, all base types and all the interfaces types
			// as the defining types of the service, which fulfill these requirements:
			// 1. None other of the types that derive from Manager may also derive from or implement that same type.
			// 2. The type can't be a common built-in type, such as System.Object, UnityEngine.Object or IEnumerable.
			foreach(var serviceType in TypeUtility.GetDerivedTypes(typeWithAttribute, typeWithAttribute.Assembly, publicOnly:false, 8))
			{
				if(!serviceType.IsAbstract)
				{
					AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, serviceType);
				}
			}

			RegisterAll:

			foreach(var (concreteType, definingTypes) in concreteToDefiningTypes)
			{
				yield return new ServiceInfo(typeWithAttribute, attributes, concreteType, definingTypes.ToArray());
			}

			static void AddConcreteDerivedAndImplementedTypes(Dictionary<Type, HashSet<Type>> concreteToDefiningTypes, Dictionary<Type, bool> registeredDefiningTypes, Type serviceType)
			{
				// Registering service serviceType as it's concrete type, and as its defines types the following:
				// its own type, all the types it derives from, and all the interface types it implements, which fulfill these requirements:
				// 1. None other of the types that derive from serviceType may also derive from/implement that same type.
				// 2. The type can't be a common built-in type, such as System.Object, UnityEngine.Object or IEnumerable.

				Add(concreteToDefiningTypes, registeredDefiningTypes, concreteType:serviceType, definingType:serviceType);

				foreach(var baseType in serviceType.GetBaseTypes())
				{
					Add(concreteToDefiningTypes, registeredDefiningTypes, concreteType:serviceType, definingType:baseType);
				}

				foreach(var interfaceType in serviceType.GetInterfaces())
				{
					const int SystemLength = 6;
					if(interfaceType.Namespace is string @namespace
					&& @namespace.StartsWith("System")
					&& (@namespace.Length == SystemLength || @namespace[SystemLength] is '.'))
					{
						continue;
					}

					Add(concreteToDefiningTypes, registeredDefiningTypes, serviceType, interfaceType);
				}
			}

			static void Add(Dictionary<Type, HashSet<Type>> concreteToDefiningTypes, Dictionary<Type, bool> registeredDefiningTypes, Type concreteType, Type definingType)
			{
				if(registeredDefiningTypes.TryGetValue(definingType, out var isFirst))
				{
					if(isFirst)
					{
						foreach(var removeFrom in concreteToDefiningTypes.Values)
						{
							removeFrom.Remove(definingType);
						}

						registeredDefiningTypes[definingType] = false;
					}

					return;
				}

				registeredDefiningTypes.Add(definingType, true);

				if(!concreteToDefiningTypes.TryGetValue(concreteType, out var addTo))
				{
					addTo = new();
					concreteToDefiningTypes.Add(concreteType, addTo);
				}

				addTo.Add(definingType);
			}
		}

		private static void SetupFromAttributes([DisallowNull] ServiceAttribute[] attributes, out bool lazyInit, out LoadMethod loadMethod, out ReferenceType referenceType, out string loadData)
		{
			lazyInit = false;
			loadMethod = LoadMethod.Default;
			referenceType = ReferenceType.None;
			loadData = "";

			foreach(var attribute in attributes)
			{
				lazyInit |= attribute.LazyInit;
				
				if(attribute.loadMethod is not LoadMethod.Default)
				{
					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(loadMethod != LoadMethod.Default && loadMethod != attribute.loadMethod) Debug.LogWarning($"Replacing loadMethod {loadMethod} with {attribute.loadMethod} for [Service] target with defining types {string.Join(", ", attributes.SelectMany(a => a.definingTypes))}.");
					#endif
					
					loadMethod = attribute.loadMethod;
				}
				
				if(attribute.referenceType is not ReferenceType.None)
				{
					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(referenceType != ReferenceType.None && referenceType != attribute.referenceType) Debug.LogWarning($"Replacing referenceType {referenceType} with {attribute.referenceType} for [Service] target with defining types {string.Join(", ", attributes.SelectMany(a => a.definingTypes))}.");
					#endif

					referenceType = attribute.referenceType;
				}

				if(attribute.loadData is { Length: > 0 })
				{
					#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
					if(loadData is { Length: > 0 } && !string.Equals(loadData, attribute.loadData)) Debug.LogWarning($"Replacing loadData \"{loadData}\" with \"{attribute.loadData}\" for [Service] target with defining types {string.Join(", ", attributes.SelectMany(a => a.definingTypes))}.");
					#endif

					loadData = attribute.loadData;
				}
			}
		}

		private ServiceInfo([DisallowNull] Type concreteClassWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			serviceOrProviderType = concreteClassWithAttribute;
			concreteType = GetConcreteType(concreteClassWithAttribute, attributes, out serviceProviderType);
			LoadAsync = serviceProviderType is ServiceProviderType.ServiceInitializerAsync or ServiceProviderType.IValueProviderAsyncT or ServiceProviderType.IValueByTypeProviderAsync;

			definingTypes = GetDefiningTypes(concreteType, attributes);

			SetupFromAttributes(attributes, out LazyInit, out loadMethod, out referenceType, out loadData);
			
			if(ConcreteOrDefiningType?.IsGenericTypeDefinition ?? false)
			{
				// client type is required for constructing an instance, so service must be initialized lazily
				LazyInit = true;
				IsTransient = true;
			}

			[return: MaybeNull]
			static Type GetConcreteType([DisallowNull] Type concreteClassWithAttribute, [DisallowNull] ServiceAttribute[] attributes, out ServiceProviderType serviceProviderType)
			{
				if(TryGetConcreteTypeFromImplementedInterfaces(concreteClassWithAttribute, genericInterfaceToServiceProviderType, attributes, out Type concreteType, out serviceProviderType))
				{
					return concreteType;
				}

				if(AllDefiningTypesAreAssignableFrom(attributes, concreteClassWithAttribute))
				{
					return concreteClassWithAttribute;
				}

				if(typeof(IValueByTypeProvider).IsAssignableFrom(concreteClassWithAttribute))
				{
					serviceProviderType = ServiceProviderType.IValueByTypeProvider;
				}
				else if(typeof(IValueByTypeProviderAsync).IsAssignableFrom(concreteClassWithAttribute))
				{
					serviceProviderType = ServiceProviderType.IValueByTypeProviderAsync;
				}
				else if(typeof(IValueProvider).IsAssignableFrom(concreteClassWithAttribute))
				{
					serviceProviderType = ServiceProviderType.IValueProvider;
				}
				else if(typeof(IValueProviderAsync).IsAssignableFrom(concreteClassWithAttribute))
				{
					serviceProviderType = ServiceProviderType.IValueProviderAsync;
				}
				else
				{
					serviceProviderType = ServiceProviderType.None;
				}

				#if DEBUG || INIT_ARGS_SAFE_MODE
				for(int i = attributes.Length - 1; i >= 0; i--)
				{
					foreach(var definingType in attributes[i].definingTypes)
					{
						if(!ServiceUtility.IsValidDefiningTypeFor(definingType, concreteClassWithAttribute))
						{
							string classWithAttributeName = TypeUtility.ToString(concreteClassWithAttribute);
							string definingTypeName = TypeUtility.ToString(definingType);
							Debug.LogAssertion($"Invalid {nameof(ServiceAttribute)} detected on {classWithAttributeName}. {classWithAttributeName} is not assignable to service defining type {definingTypeName}, and does not implement {nameof(IInitializer)}<{definingTypeName}> or {nameof(IWrapper)}<{definingTypeName}>, IServiceInitializer<{definingTypeName}> or IValueProvider<{definingTypeName}>. Unable to determine concrete type of the service.");
						}
					}
				}
				#endif

				return attributes.SelectMany(attribute => attribute.definingTypes).SingleOrDefaultNoException(definingType => definingType is { IsAbstract: false });
			}

			static bool TryGetConcreteTypeFromImplementedInterfaces([DisallowNull] Type concreteClassWithAttribute, Dictionary<Type, ServiceProviderType> genericInterfaceToValueProviderType, [DisallowNull] ServiceAttribute[] attributes, [NotNullWhen(true)][MaybeNullWhen(false)] out Type concreteType, out ServiceProviderType serviceProviderType)
			{
				var implementedInterfaces = concreteClassWithAttribute.GetInterfaces();
				for(int i = implementedInterfaces.Length - 1; i >= 0; i--)
				{
					var interfaceType = implementedInterfaces[i];
					if(!interfaceType.IsGenericType)
					{
						continue;
					}

					var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
					if(!genericInterfaceToValueProviderType.TryGetValue(genericTypeDefinition, out var valueProviderType))
					{
						continue;
					}

					concreteType = interfaceType.GetGenericArguments()[0];
					if(concreteType.IsAbstract || TypeUtility.IsBaseType(concreteType) || !AllDefiningTypesAreAssignableFrom(attributes, concreteType))
					{
						continue;
					}

					serviceProviderType = valueProviderType;
					return true;
				}

				if(AllDefiningTypesAreAssignableFrom(attributes, concreteClassWithAttribute))
				{
					serviceProviderType = ServiceProviderType.None;
					concreteType = null;
					return false;
				}

				#if DEBUG || INIT_ARGS_SAFE_MODE
				for(int i = attributes.Length - 1; i >= 0; i--)
				{
					foreach(var definingType in attributes[i].definingTypes)
					{
						if(!ServiceUtility.IsValidDefiningTypeFor(definingType, concreteClassWithAttribute))
						{
							string classWithAttributeName = TypeUtility.ToString(concreteClassWithAttribute);
							string definingTypeName = TypeUtility.ToString(definingType);
							Debug.LogAssertion($"Invalid {nameof(ServiceAttribute)} detected on {classWithAttributeName}. {classWithAttributeName} is not assignable to service defining type {definingTypeName}, and does not implement {nameof(IInitializer)}<{definingTypeName}> or {nameof(IWrapper)}<{definingTypeName}>, IServiceInitializer<{definingTypeName}> or IValueProvider<{definingTypeName}>. Unable to determine concrete type of the service.");
						}
					}
				}
				#endif

				serviceProviderType = ServiceProviderType.None;
				concreteType = null;
				return false;
			}
		}

		internal ServiceInfo([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes, Type concreteType, Type[] definingTypes)
		{
			this.serviceOrProviderType = classWithAttribute;
			this.concreteType = concreteType;
			this.definingTypes = definingTypes;

			SetupFromAttributes(attributes, out LazyInit, out loadMethod, out referenceType, out loadData);

			if(ConcreteOrDefiningType?.IsGenericTypeDefinition ?? false)
			{
				// client type is required for constructing an instance, so service must be initialized lazily
				LazyInit = true;
				IsTransient = true;
			}
		}

		private static bool AllDefiningTypesAreAssignableFrom(ServiceAttribute[] attributes, Type type)
		{
			foreach(var attribute in attributes)
			{
				foreach(var definingType in attribute.definingTypes)
				{
					if(!IsAssignableFrom(definingType, type))
					{
						return false;
					}
				}
			}

			return true;

			static bool IsAssignableFrom(Type definingType, Type from)
			{
				if(!definingType.IsGenericTypeDefinition)
				{
					if(definingType.IsAssignableFrom(from))
					{
						return true;
					}

					return false;
				}

				if(definingType == from)
				{
					return true;
				}

				if(!definingType.IsInterface)
				{
					for(var fromBase = from.BaseType; fromBase is not null; fromBase = fromBase.BaseType)
					{
						if(!fromBase.IsGenericType)
						{
							continue;
						}

						var fromBaseDefinition = fromBase.IsGenericTypeDefinition ? fromBase : fromBase.GetGenericTypeDefinition();
						if(definingType == fromBaseDefinition)
						{
							return true;
						}
					}
				}

				foreach(var fromInterface in from.GetInterfaces())
				{
					if(!fromInterface.IsGenericType)
					{
						continue;
					}

					var fromInterfaceDefinition = fromInterface.IsGenericTypeDefinition ? fromInterface : fromInterface.GetGenericTypeDefinition();
					if(definingType == fromInterfaceDefinition)
					{
						return true;
					}
				}

				return false;
			}
		}

		[return: MaybeNull]
		private static Type GetSingleDerivedConcreteTypeOrNull([DisallowNull] Type baseOrInterfaceType)
		{
			Type result = null;

			#if UNITY_EDITOR
			foreach(Type derivedType in UnityEditor.TypeCache.GetTypesDerivedFrom(baseOrInterfaceType))
			{
			#else
			foreach(Type derivedType in TypeUtility.GetAllTypesThreadSafe(baseOrInterfaceType.Assembly, false))
			{
				if(!baseOrInterfaceType.IsAssignableFrom(derivedType))
				{
					continue;
				}
			#endif

				if(derivedType.IsAbstract)
				{
					continue;
				}

				if(result != null)
				{
					return null;
				}

				result = derivedType;
			}

			return result;
		}

		static Type[] GetDefiningTypes(Type concreteType, ServiceAttribute[] attributes)
		{
			if(attributes.Length == 1)
			{
				return attributes[0].definingTypes is { Length: > 0 } definingTypes ? definingTypes : new[] { concreteType };
			}
			else
			{
				var definingTypes = attributes.SelectMany(attribute => attribute.definingTypes).ToArray();
				return definingTypes.Length > 0 ? definingTypes : new[] { concreteType };
			}
		}

		public bool HasDefiningType(Type type) => Array.IndexOf(definingTypes, type) != -1;

		public bool IsInstanceOf([AllowNull] object instance)
		{
			if(concreteType is not null && !concreteType.IsInstanceOfType(instance))
			{
				if(!concreteType.IsGenericTypeDefinition || instance.GetType() is not Type instanceType || !instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != concreteType)
				{
					return false;
				}
			}

			foreach(var definingType in definingTypes)
			{
				if(!definingType.IsInstanceOfType(instance))
				{
					// Needed here?
					if(!definingType.IsGenericTypeDefinition || instance.GetType() is not { IsGenericType: true } instanceType || instanceType.GetGenericTypeDefinition() != definingType)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}