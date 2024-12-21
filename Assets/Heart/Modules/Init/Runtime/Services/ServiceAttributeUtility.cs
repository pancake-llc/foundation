using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sisus.Init.Internal
{
	internal static class ServiceAttributeUtility
	{
		private static readonly ServiceInfo serviceProviderInfo = new(typeof(ServiceProvider), new []{ new ServiceAttribute(typeof(IServiceProvider)), new ServiceAttribute(typeof(System.IServiceProvider))}, typeof(ServiceProvider), new []{ typeof(IServiceProvider), typeof(System.IServiceProvider)});
		private static readonly ServiceInfo updaterInfo = new(typeof(Updater), new []{ new ServiceAttribute(typeof(ICoroutineRunner))}, typeof(Updater), new []{ typeof(ICoroutineRunner)});

		internal static readonly Dictionary<Type, ServiceInfo> concreteTypes = new()
		{
			{ typeof(ServiceProvider), serviceProviderInfo },
			{ typeof(Updater), updaterInfo }
		};

		internal static readonly Dictionary<Type, ServiceInfo> definingTypes = new()
		{
			{ typeof(IServiceProvider), serviceProviderInfo },
			{ typeof(System.IServiceProvider), serviceProviderInfo },
			{ typeof(ICoroutineRunner), updaterInfo }
		};

		static ServiceAttributeUtility()
		{
			var typesWithAttribute = TypeUtility.GetTypesWithAttribute<ServiceAttribute>(typeof(ServiceAttribute).Assembly, false, 128);
			int count = typesWithAttribute.Count;

			concreteTypes.EnsureCapacity(count);
			definingTypes.EnsureCapacity(count);

			foreach(var typeWithAttribute in typesWithAttribute)
			{
				var attributes = typeWithAttribute.GetCustomAttributes<ServiceAttribute>().ToArray();
				foreach(var info in ServiceInfo.From(typeWithAttribute, attributes))
				{
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
		}

		/// <summary>
		/// Gets the defining types configured for <paramref name="concreteType"/> using <see cref="ServiceAttribute"/>.
		/// </summary>
		/// <param name="concreteType"> Concrete type of object that may or may not be a service. </param>
		/// <returns> An array containing one or more defining types, if <paramref name="concreteType"/> has the <see cref="ServiceAttribute"/>; otherwise, an empty array. </returns>
		[return: NotNull]
		public static Type[] GetDefiningTypes([DisallowNull] Type concreteType) => concreteTypes.TryGetValue(concreteType, out var serviceInfo) ? serviceInfo.definingTypes : Type.EmptyTypes;
	}
}