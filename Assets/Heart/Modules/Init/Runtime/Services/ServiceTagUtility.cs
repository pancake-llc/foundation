using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Sisus.Init.Internal
{
	public static class ServiceTagUtility
	{
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
	}
}