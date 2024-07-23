using System;
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

		public GlobalServiceInfo([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		{
			this.classWithAttribute = classWithAttribute;
			this.attributes = attributes;
			concreteType = GetConcreteType(classWithAttribute, attributes);
			definingTypes = GetDefiningTypes(concreteType, attributes);
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
					if(attributes[i].definingType is Type definingType && !definingType.IsAssignableFrom(classWithAttribute))
					{
						string classWithAttributeName = TypeUtility.ToString(classWithAttribute);
						string definingTypeName = TypeUtility.ToString(definingType);
						Debug.LogAssertion($"Invalid {nameof(ServiceAttribute)} detected on {classWithAttributeName}. {classWithAttributeName} is not assignable from service defining type {definingTypeName}, and does not implement {nameof(IInitializer)}<{definingTypeName}> or {nameof(IWrapper)}<{definingTypeName}>. Unable to determine concrete type of the service.");
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
					if(!definingType.IsGenericTypeDefinition || instance.GetType() is not Type instanceType || !instanceType.IsGenericType || instanceType.GetGenericTypeDefinition() != definingType)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}