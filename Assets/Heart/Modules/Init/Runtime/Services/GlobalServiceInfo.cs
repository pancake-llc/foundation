using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Sisus.Init.Internal;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Information about a shared and globally accessible service registered using the <see cref="ServiceAttribute"/>.
	/// </summary>
	internal sealed class GlobalServiceInfo
	{
		[NotNull] public readonly Type classWithAttribute;

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

		[NotNull] public readonly ServiceAttribute[] attributes;

		/// <summary>
		/// Returns <see cref="concreteType"/> if it's not <see langword="null"/>,
		/// otherwise returns first element from <see cref="definingTypes"/>.
		/// <para>
		/// Can be an abstract or a generic type definition.
		/// </para>
		/// </summary>
		[NotNull] public Type ConcreteOrDefiningType => concreteType ?? definingTypes.FirstOrDefault();

		public bool IsTransient => ConcreteOrDefiningType?.IsGenericTypeDefinition ?? false;

		public bool LazyInit
		{
			get
			{
				foreach(var attribute in attributes)
				{
					if(attribute.LazyInit)
					{
						return true;
					}
				}
				
				// if service type is generic type definition, then client is needed
				// to determine the generic arguments to use.
				return ConcreteOrDefiningType?.IsGenericTypeDefinition ?? false;
			}
		}

		public bool LoadAsync
		{
			get
			{
				if(typeof(IServiceInitializerAsync).IsAssignableFrom(classWithAttribute))
				{
					return true;
				}
				
				foreach(var attribute in attributes)
				{
					if(attribute.LoadAsync)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool FindFromScene
		{
			get
			{
				foreach(var attribute in attributes)
				{
					if(attribute.FindFromScene)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool ShouldInstantiate(bool isPrefab)
		{
			foreach(var attribute in attributes)
			{
				if(attribute.Instantiate is bool result)
				{
					return result;
				}
			}

			return isPrefab;
		}

		public string ResourcePath
		{
			get
			{

				foreach(var attribute in attributes)
				{
					if(attribute.ResourcePath is string resourcePath)
					{
						return resourcePath;
					}
				}

				return null;
			}
		}

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		public string AddressableKey
		{
			get
			{

				foreach(var attribute in attributes)
				{
					if(attribute.AddressableKey is string addressableKey)
					{
						return addressableKey;
					}
				}

				return null;
			}
		}
		#endif

		public static IEnumerable<GlobalServiceInfo> From([DisallowNull] Type typeWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			if (!typeWithAttribute.IsAbstract)
			{
				yield return new GlobalServiceInfo(typeWithAttribute, attributes);
				yield break;
			}

			var registeredDefiningTypes = new Dictionary<Type, bool>(8);
			var concreteToDefiningTypes = new Dictionary<Type, HashSet<Type>>(8);

			foreach(var attribute in attributes)
			{
				if(attribute.definingType is not Type serviceType)
				{
					continue;
				}

				// Support
				// [Service(typeof(Logger<>))]
				// public interface ILogger<T> { }
				// registering the concrete class Logger<> as ILogger<>
				if(!serviceType.IsAbstract)
				{
					yield return new(typeWithAttribute, attributes, concreteType:serviceType, definingTypes:new[] { typeWithAttribute });
					registeredDefiningTypes[serviceType] = false;
					continue;
				}

				// Support
				// [Service(typeof(ISingleton<>))]
				// public interface ISingleton<T> { }
				// registering all types that implement ISingleton<TService> and ISingleton<TService>
				if(serviceType.IsGenericTypeDefinition)
				{
					foreach(var derivedType in TypeUtility.GetOpenGenericTypeDerivedTypes(serviceType, publicOnly:false, concreteOnly:true, 8))
					{
						AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, derivedType);
					}

					continue;
				}

				// Support
				// [Service(typeof(Logger))]
				// public interface ILogger { }
				// registering all Logger<T> as ILogger<TService>
				foreach(var derivedType in TypeUtility.GetDerivedTypes(typeWithAttribute, typeWithAttribute.Assembly, publicOnly:false, 8))
				{
					if(!derivedType.IsAbstract)
					{
						AddConcreteDerivedAndImplementedTypes(concreteToDefiningTypes, registeredDefiningTypes, derivedType);
					}
				}
			}

			if (attributes.Length != 1 || attributes[0].definingType is not null)
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
				yield return new GlobalServiceInfo(typeWithAttribute, attributes, concreteType, definingTypes.ToArray());
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

		private GlobalServiceInfo([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			this.classWithAttribute = classWithAttribute;
			this.attributes = attributes;
			concreteType = GetConcreteType(classWithAttribute, attributes);
			definingTypes = GetDefiningTypes(concreteType, attributes);
		}

		internal GlobalServiceInfo([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes, Type concreteType, Type[] definingTypes)
		{
			this.classWithAttribute = classWithAttribute;
			this.attributes = attributes;
			this.concreteType = concreteType;
			this.definingTypes = definingTypes;
		}

		[return: MaybeNull]
		private static Type GetConcreteType([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			if(typeof(IServiceInitializerAsync).IsAssignableFrom(classWithAttribute))
			{
				if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IServiceInitializerAsync<>), attributes, out Type concreteType))
				{
					return concreteType;
				}
			}
			else if(typeof(IServiceInitializer).IsAssignableFrom(classWithAttribute))
			{
				if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IServiceInitializer<>), attributes, out Type concreteType))
				{
					return concreteType;
				}
			}
			else if(typeof(IInitializer).IsAssignableFrom(classWithAttribute))
			{
				if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IInitializer<>), attributes, out Type concreteType))
				{
					return concreteType;
				}
			}
			else if(typeof(IWrapper).IsAssignableFrom(classWithAttribute))
			{
				if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IWrapper<>), attributes, out Type concreteType))
				{
					return concreteType;
				}
			}
			else if(!AllDefiningTypesAreAssignableFrom(attributes, classWithAttribute))
			{
				if(typeof(IValueProvider).IsAssignableFrom(classWithAttribute))
				{
					if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IValueProvider<>), attributes, out Type concreteType))
					{
						return concreteType;
					}
				}

				if(typeof(IValueProviderAsync).IsAssignableFrom(classWithAttribute))
				{
					if(TryGetConcreteTypeFromInterface(classWithAttribute, typeof(IValueProviderAsync<>), attributes, out Type concreteType))
					{
						return concreteType;
					}
				}

				#if DEBUG || INIT_ARGS_SAFE_MODE
				for(int i = attributes.Length - 1; i >= 0; i--)
				{
					if(attributes[i].definingType is Type definingType
						&& !ServiceUtility.IsValidDefiningTypeFor(definingType, classWithAttribute))
					{
						string classWithAttributeName = TypeUtility.ToString(classWithAttribute);
						string definingTypeName = TypeUtility.ToString(definingType);
						Debug.LogAssertion($"Invalid {nameof(ServiceAttribute)} detected on {classWithAttributeName}. {classWithAttributeName} is not assignable to service defining type {definingTypeName}, and does not implement {nameof(IInitializer)}<{definingTypeName}> or {nameof(IWrapper)}<{definingTypeName}>, IServiceInitializer<{definingTypeName}> or IValueProvider<{definingTypeName}>. Unable to determine concrete type of the service.");
					}
				}
				#endif
			}

			if(classWithAttribute.IsAbstract)
			{
				// Support "inverse" configuration:
				// [Service(typeof(ConcreteType))]
				// interface IInterfaceType { }
				for(int i = attributes.Length - 1; i >= 0; i--)
				{
					if(attributes[i].definingType is Type definingType && !definingType.IsAbstract && !classWithAttribute.IsAssignableFrom(definingType))
					{
						return definingType;
					}
				}

				if(GetSingleDerivedConcreteTypeOrNull(classWithAttribute) is Type singleDerivedConcreteType)
				{
					return singleDerivedConcreteType;
				}

				Debug.LogAssertion($"Invalid {nameof(ServiceAttribute)} detected on {TypeUtility.ToString(classWithAttribute)}. Class with the attribute is abstract, and no defining type has been specified. Unable to determine concrete type of the service.");

				return classWithAttribute;
			}

			return classWithAttribute;
		}

		private static bool TryGetConcreteTypeFromInterface([DisallowNull] Type classWithAttribute, [DisallowNull] Type targetInterface, [DisallowNull] ServiceAttribute[] attributes, [NotNullWhen(true)][MaybeNullWhen(false)] out Type result)
		{
			var implementedInterfaces = classWithAttribute.GetInterfaces();
			for(int i = implementedInterfaces.Length - 1; i >= 0; i--)
			{
				var interfaceType = implementedInterfaces[i];
				if(!interfaceType.IsGenericType)
				{
					continue;
				}

				var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
				if(genericTypeDefinition != targetInterface)
				{
					continue;
				}

				result = interfaceType.GetGenericArguments()[0];

				if(result.IsGenericType && typeof(Task).IsAssignableFrom(result))
				{
					result = result.GetGenericArguments()[0];
				}

				if(result.IsAbstract || TypeUtility.IsBaseType(result) || !AllDefiningTypesAreAssignableFrom(attributes, result))
				{
					continue;
				}

				return true;
			}

			result = null;
			return false;
		}

		private static bool AllDefiningTypesAreAssignableFrom(ServiceAttribute[] attributes, Type type)
		{
			foreach(var attribute in attributes)
			{
				if(attribute.definingType is Type definingType && !definingType.IsAssignableFrom(type))
				{
					return false;
				}
			}

			return true;
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
			var results = attributes.Select(attribute => attribute.definingType).Where(definingType => definingType is not null).ToArray();
			if(results.Length == 0 && concreteType is not null)
			{
				results = new[] { concreteType };
			}

			return results;
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