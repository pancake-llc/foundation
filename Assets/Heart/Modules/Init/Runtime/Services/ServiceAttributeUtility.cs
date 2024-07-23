using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sisus.Init.Internal
{
	internal static class ServiceAttributeUtility
	{
		internal static readonly Dictionary<Type, GlobalServiceInfo> concreteTypes;
		internal static readonly Dictionary<Type, GlobalServiceInfo> definingTypes;

		static ServiceAttributeUtility()
		{
			var typesWithAttribute = TypeUtility.GetTypesWithAttribute<ServiceAttribute>(typeof(ServiceAttribute).Assembly, false, 128);
			int count = typesWithAttribute.Count;

			concreteTypes = new(count);
			definingTypes = new(count);

			foreach(var typeWithAttribute in typesWithAttribute)
			{
				var attributes = typeWithAttribute.GetCustomAttributes<ServiceAttribute>().ToArray();
				var info = new GlobalServiceInfo(typeWithAttribute, attributes);
				if(info.concreteType is null)
				{
					#if DEV_MODE
					UnityEngine.Debug.Log(typeWithAttribute.Name + " concrete type is null.");
					#endif
					continue;
				}

				concreteTypes[info.concreteType] = info;
				foreach(var definingType in info.definingTypes)
				{
					definingTypes[definingType] = info;
				}
			}
		}

		[return: NotNull]
		public static bool IsServiceDefiningType([DisallowNull] Type type) => concreteTypes.TryGetValue(type, out var serviceInfo) && Array.IndexOf(serviceInfo.definingTypes, type) != -1;

		/// <summary>
		/// Gets the defining types configured for <paramref name="concreteType"/> using <see cref="ServiceAttribute"/>.
		/// </summary>
		/// <param name="concreteType"> Concrete type of an object that may or may not be a service. </param>
		/// <returns> An array containing one or more defining types, if <paramref name="concreteType"/> has the <see cref="ServiceAttribute"/>; otherwise, an empty array. </returns>
		[return: NotNull]
		public static Type[] GetDefiningTypes([DisallowNull] Type concreteType) => concreteTypes.TryGetValue(concreteType, out var serviceInfo) ? serviceInfo.definingTypes : Type.EmptyTypes;
	}
}