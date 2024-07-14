using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static Sisus.NullExtensions;

namespace Sisus.Init.Internal
{
	public static class ServiceTagUtility
	{
		private static readonly HashSet<Type> currentDefiningTypes = new();
		private static readonly HashSet<Type> definingTypeOptions = new();
		private static readonly List<ServiceTag> serviceTags = new();

		public static
		#if DEV_MODE
		IEnumerable<ServiceTag>
		#else
		List<ServiceTag>
		#endif
			GetServiceTags(Component serviceOrServiceProvider)
		{
			serviceTags.Clear();
			serviceOrServiceProvider.GetComponents(serviceTags);

			for(int i = serviceTags.Count - 1; i >= 0; i--)
			{
				if(serviceTags[i].Service != serviceOrServiceProvider)
				{
					serviceTags.RemoveAt(i);
				}
			}

			return serviceTags;
		}

		public static bool HasServiceTag(object service) => service is Component component && HasServiceTag(component);

		public static bool HasServiceTag(Component service)
		{
			serviceTags.Clear();
			service.GetComponents(serviceTags);

			foreach(var tag in serviceTags)
			{
				if(tag.Service == service)
				{
					serviceTags.Clear();
					return true;
				}
			}

			serviceTags.Clear();
			return false;
		}

		public static bool IsValidDefiningTypeFor([DisallowNull] Type definingType, [DisallowNull] Component service)
		{
			if(definingType.IsInstanceOfType(service))
			{
				return true;
			}

			if(service is not IValueProvider)
			{
				return false;
			}

			return GetAllDefiningTypeOptions(service).Contains(definingType);
		}

		public static IEnumerable<Type> GetAllDefiningTypeOptions(Component component)
		{
			definingTypeOptions.Clear();

			GetAllDefiningTypeOptions(component.GetType(), definingTypeOptions);

			// We want to display the types and interfaces of the target of wrappers and initializers as well.
			if(component is IValueProvider valueProvider && valueProvider. Value is object providedValue)
			{
				GetAllDefiningTypeOptions(providedValue.GetType(), definingTypeOptions);
			}

			return definingTypeOptions;
		}

		private static void GetAllDefiningTypeOptions(Type type, HashSet<Type> definingTypeOptions)
		{
			definingTypeOptions.Add(type);

			foreach(var t in type.GetInterfaces())
			{
				definingTypeOptions.Add(t);
			}

			if(type.IsValueType)
			{
				return;
			}

			for(var t = type.BaseType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				definingTypeOptions.Add(t);
			}
		}

		internal static IEnumerable<Type> GetServiceDefiningTypes(object serviceOrServiceProvider)
		{
			Profiler.BeginSample("GetServiceDefiningTypes");

			currentDefiningTypes.Clear();

			Transform client = Find.In<Transform>(serviceOrServiceProvider);
			AddServiceDefiningTypes(client, serviceOrServiceProvider, currentDefiningTypes);

			if(serviceOrServiceProvider is IValueProvider valueProvider)
			{
				AddServiceProviderValueDefiningTypes(client, valueProvider, currentDefiningTypes);
			}

			Profiler.EndSample();

			return currentDefiningTypes;

			static void AddServiceDefiningTypes([AllowNull] Component client, [DisallowNull] object service, [DisallowNull] HashSet<Type> currentDefiningTypes)
			{
				Type concreteType = service.GetType();
				foreach(var definingType in ServiceAttributeUtility.GetDefiningTypes(concreteType))
				{
					currentDefiningTypes.Add(definingType);
				}

				for(var typeOrBaseType = concreteType; !TypeUtility.IsNullOrBaseType(typeOrBaseType); typeOrBaseType = typeOrBaseType.BaseType)
				{
					if(!currentDefiningTypes.Contains(typeOrBaseType) && ServiceUtility.IsServiceOrServiceProviderFor(client, typeOrBaseType, service))
					{
						currentDefiningTypes.Add(typeOrBaseType);
					}
				}

				var interfaces = concreteType.GetInterfaces();
				foreach(var interfaceType in interfaces)
				{
					if(!currentDefiningTypes.Contains(interfaceType) && ServiceUtility.IsServiceOrServiceProviderFor(client, interfaceType, service))
					{
						currentDefiningTypes.Add(interfaceType);
					}
				}
			}

			static void AddServiceProviderValueDefiningTypes([AllowNull] Component client, [DisallowNull] IValueProvider serviceProvider, [DisallowNull] HashSet<Type> currentDefiningTypes)
			{
				Type concreteType;
				var interfaces = serviceProvider.GetType().GetInterfaces();
				HashSet<Type> providedValueTypes;
				object providedValue = serviceProvider.Value;
				bool hasValue = providedValue != Null;
				if(hasValue)
				{
					concreteType = providedValue.GetType();
					providedValueTypes = interfaces.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValueProvider<>)).Select(t => t.GetGenericArguments()[0]).Append(concreteType).ToHashSet();
				}
				else
				{
					providedValueTypes = interfaces.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValueProvider<>)).Select(t => t.GetGenericArguments()[0]).ToHashSet();
				}

				foreach(var providedValueType in providedValueTypes)
				{
					foreach(var definingType in ServiceAttributeUtility.GetDefiningTypes(providedValueType))
					{
						currentDefiningTypes.Add(definingType);
					}
				}

				if(!hasValue)
				{
					return;
				}

				foreach(var providedValueType in providedValueTypes)
				{
					for(var typeOrBaseType = providedValueType; !TypeUtility.IsNullOrBaseType(typeOrBaseType); typeOrBaseType = typeOrBaseType.BaseType)
					{
						if(!currentDefiningTypes.Contains(typeOrBaseType) && ServiceUtility.IsServiceOrServiceProviderFor(client, typeOrBaseType, providedValue))
						{
							currentDefiningTypes.Add(typeOrBaseType);
						}
					}

					interfaces = providedValueType.GetInterfaces();
					foreach(var interfaceType in interfaces)
					{
						if(!currentDefiningTypes.Contains(interfaceType) && ServiceUtility.IsServiceOrServiceProviderFor(client, interfaceType, providedValue))
						{
							currentDefiningTypes.Add(interfaceType);
						}
					}
				}
			}
		}
	}
}