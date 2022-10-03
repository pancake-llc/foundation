using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
#endif

namespace Pancake.Init.Internal
{
	using static ConversionExtensions;
	using static NullExtensions;
	using Object = UnityEngine.Object;

	public partial class Services
	{
		private static readonly ConcurrentDictionary<Type, List<ProvidedServiceInfo>> infosByDefiningType = new ConcurrentDictionary<Type, List<ProvidedServiceInfo>>();

		#if UNITY_EDITOR
		// Separate caches for edit and play modes to avoid issues with Register / Deregister not being called in pairs
		// when entering / exiting play mode etc.
		private static readonly ConcurrentDictionary<Type, List<ProvidedServiceInfo>> infosByDefiningTypeInEditMode = new ConcurrentDictionary<Type, List<ProvidedServiceInfo>>();
		#endif

		internal static ConcurrentDictionary<Type, List<ProvidedServiceInfo>> InfosByDefiningType
		{
			get
			{
				#if UNITY_EDITOR
				if(!EditorOnly.ThreadSafe.Application.IsPlaying)
				{
					return infosByDefiningTypeInEditMode;
				}
				#endif

				return infosByDefiningType;
			}
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Reset state when entering play mode in the editor to support Enter Play Mode Settings.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnEnterPlayMode()
		{
			infosByDefiningType.Clear();
			infosByDefiningTypeInEditMode.Clear();
		}
		#endif

		internal static bool IsService([NotNull] Type definingType, [NotNull] object instance)
		{
			if(!InfosByDefiningType.TryGetValue(definingType, out var list))
			{
				return false;
			}

			foreach(var item in list)
			{
				if(ReferenceEquals(item.service, instance))
				{
					return true;
				}
			}

			return false;
		}

		#if UNITY_EDITOR
		internal static bool IsService([CanBeNull] object instance)
		{
			if(instance is null)
			{
				return false;
			}

			var type = instance.GetType();
			for(var typeOrBaseType = type; !TypeUtility.IsNullOrBaseType(typeOrBaseType); typeOrBaseType = typeOrBaseType.BaseType)
			{
				if(IsService(typeOrBaseType, instance))
				{
					return true;
				}
			}

			foreach(var interfaceType in type.GetInterfaces())
			{
				if(IsService(interfaceType, instance))
				{
					return true;
				}
			}

			return false;
		}
		#endif

		#if UNITY_EDITOR
		internal static bool TryGetServiceToClientVisibility(Component service, Type definingType, out Clients toClients)
		{
			if(InfosByDefiningType.TryGetValue(definingType, out var infos))
			{
				foreach(var info in infos)
				{
					if(info.serviceProvider == null)
					{
						continue;
					}

					if(info.service as Object != service)
					{
						continue;
					}

					toClients = info.toClients;
					return true;
				}
			}

			toClients = default;
			return false;
		}
		#endif

		internal static bool TryGetForAnyClient<TService>(out TService service)
		{
			if(InfosByDefiningType.TryGetValue(typeof(TService), out var infos))
			{
				ProvidedServiceInfo nearest = default;

				foreach(var info in infos)
				{
					#if UNITY_EDITOR
					if(info.serviceProvider == null)
					{
						continue;
					}
					#endif

					if(info.toClients != Clients.Everywhere)
					{
						continue;
					}

					#if DEBUG && !INIT_ARGS_DISABLE_WARNINGS
					if(nearest.service != Null)
					{
						Debug.LogWarning($"AmbiguousMatchWarning: All clients have access to both services \"{nearest.serviceProvider.gameObject.name}\"/{TypeUtility.ToString(nearest.service.GetType())} and \"{info.serviceProvider.gameObject.name}\"/{TypeUtility.ToString(info.service.GetType())} via {nameof(Services)}s and unable to determine which one should be prioritized.");
					}
					#endif

					nearest = info;

					#if !DEBUG
                    break;
					#endif
				}

				if(nearest.service != Null && TryConvert<TService>(nearest.service, out var asService))
				{
					service = asService;
					return true;
				}
			}

			if(typeof(TService).IsValueType)
			{
				service = default;
				return false;
			}

			#if UNITY_EDITOR && !INIT_ARGS_DISABLE_SERVICE_INJECTION
			// Usually we can rely on static constructor of Service<T> to handle lazily initializing the service when necessary.
			// However with Enter Play Mode Options enabled this might not work, so we should handle that here instead.
			if(ServiceInjector.unitializedServicesByDefiningType.TryGetValue(typeof(TService), out var definition))
			{
				ServiceInjector.LazyInit(definition.classWithAttribute, definition.attribute);
			}
			#endif

			service = Service<TService>.Instance;

			return !(service is null);
		}

		internal static bool TryGetFor<TService>([NotNull] GameObject gameObject, out TService service)
		{
			Debug.Assert(gameObject != null);

			if(InfosByDefiningType.TryGetValue(typeof(TService), out var infos))
			{
				ProvidedServiceInfo? nearest = default;

				foreach(var info in infos)
				{
					#if UNITY_EDITOR
					if(info.serviceProvider == null)
					{
						continue;
					}
					#endif

					if(!AreAvailableToClient(info.serviceProvider.transform, info.toClients, gameObject.transform))
					{
						continue;
					}

					nearest = GetNearest(gameObject, info, nearest);
				}

				if(nearest.HasValue && nearest.Value.service is object obj && TryConvert<TService>(obj, out var asService))
				{
					service = asService;
					return true;
				}
			}

			if(typeof(TService).IsValueType)
			{
				service = default;
				return false;
			}

			var globalService = Service<TService>.Instance;
			if(globalService != Null)
			{
				service = globalService;
				return true;
			}

			#if UNITY_EDITOR
			if(ServiceInjector.GetClassWithServiceAttribute(typeof(TService)) is Type classWithServiceAttribute)
			{
				foreach(ServiceAttribute attribute in classWithServiceAttribute.GetCustomAttributes(typeof(ServiceAttribute), true))
				{
					if(attribute.FindFromScene)
					{
						service = Find.Any<TService>();
						return service != Null;
					}
				}
			}
			#endif

			service = default;
			return false;
		}

		private static bool AreAvailableToClient(Transform serviceProvider, Clients availability, [NotNull] Transform client)
		{
			Debug.Assert(client != null);
			Debug.Assert(serviceProvider != null);

			switch(availability)
			{
				case Clients.InGameObject:
					return client == serviceProvider;
				case Clients.InChildren:
					var injectorTransform = client;
					for(var parent = client.transform; parent != null; parent = parent.parent)
					{
						if(parent == injectorTransform)
						{
							return true;
						}
					}
					return false;
				case Clients.InParents:
					var clientTransform = client;
					for(var parent = serviceProvider; parent != null; parent = parent.parent)
					{
						if(parent == clientTransform)
						{
							return true;
						}
					}
					return false;
				case Clients.InHierarchyRootChildren:
					return serviceProvider.root == client.root;
				case Clients.InScene:
					return client.gameObject.scene == serviceProvider.gameObject.scene;
				case Clients.InAllScenes:
				case Clients.Everywhere:
					return true;
				default:
					Debug.LogError($"Unrecognized {nameof(Clients)} value: {availability}.", serviceProvider);
					return false;
			}
		}

		internal static void Register([NotNull] Component serviceProvider, IEnumerable<ServiceDefinition> providesServices, Clients toClients)
		{
			Debug.Assert(serviceProvider != null);

			#if UNITY_EDITOR
			var infosByDefiningType = EditorOnly.ThreadSafe.Application.IsPlaying ? Services.infosByDefiningType : infosByDefiningTypeInEditMode;
			#endif

			foreach(var serviceDefinition in providesServices)
			{
				var service = serviceDefinition.service;
				if(service == null)
				{
					Debug.LogWarning($"{serviceProvider.GetType().Name} component has a missing service reference.", serviceProvider);
					continue;
				}

				var definingType = serviceDefinition.definingType.Value;
				if(definingType is null)
				{
					Debug.LogWarning($"{serviceProvider.GetType().Name} component service {service.GetType().Name} is missing its defining type.", serviceProvider);
					continue;
				}

				if(!infosByDefiningType.TryGetValue(definingType, out var infos))
				{
					infos = new List<ProvidedServiceInfo>();
					if(!infosByDefiningType.TryAdd(definingType, infos))
					{
						infosByDefiningType.TryGetValue(definingType, out infos);
					}
				}
				#if UNITY_EDITOR
				else if(infos.Any(i => i.service as Object == service && i.serviceProvider == serviceProvider))
				{
					continue;
				}
				#endif

				if(definingType.IsAssignableFrom(service.GetType()))
				{
					infos.Add(new ProvidedServiceInfo(service, serviceProvider, toClients));
				}
				else if(service is IValueProvider valueProvider)
				{
					if(valueProvider.Value is object providedValue)
					{
						if(definingType.IsAssignableFrom(providedValue.GetType()))
						{
							infos.Add(new ProvidedServiceInfo(providedValue, serviceProvider, toClients));
						}
					}
					else if(valueProvider is IInitializer initializer)
					{
						object initialized = initializer.InitTarget();
						if(definingType.IsAssignableFrom(initialized.GetType()))
						{
							infos.Add(new ProvidedServiceInfo(initialized, serviceProvider, toClients));
						}
					}
				}

				#if SET_GLOBAL_INSTANCE_FROM_DYNAMIC_SERVICES
                if(toClients == Clients.Everywhere && !ServiceUtility.IsServiceDefiningType(definingType))
			    {
				    // Worth the performance cost? Uses reflection, so it could affect performance when spawning entities at a high pace.
                    // However it feels pretty unlikely that this would happen with services that are accessible everywhere.
                    // Another possible issue is thread safety. It's possible that an object tries to access the Service class from
                    // a background thread at the same time that the service instance is set.
				    ServiceUtility.SetInstance(definingType, service);
			    }
				#endif
			}
		}

		internal static void Deregister([NotNull] Object serviceProvider)
		{
			#if UNITY_EDITOR
			var infosByDefiningType = EditorOnly.ThreadSafe.Application.IsPlaying ? Services.infosByDefiningType : infosByDefiningTypeInEditMode;
			#endif

			foreach(var instance in infosByDefiningType)
			{
				var infos = instance.Value;
				for(int i = infos.Count - 1; i >= 0; i--)
				{
					if(infos[i].serviceProvider != serviceProvider)
					{
						continue;
					}

					#if SET_GLOBAL_INSTANCE_FROM_DYNAMIC_SERVICES
					if(infos[i].toClients == Clients.Everywhere && ServiceUtility.IsService(instance.Key, infos[i].service))
					{
						ServiceUtility.SetInstance(instance.Key, null);
					}
					#endif

					infos.RemoveAt(i);
				}
			}
		}

		private static ProvidedServiceInfo GetNearest([NotNull] GameObject client, ProvidedServiceInfo firstOption, ProvidedServiceInfo? secondOptionOrNull)
		{
			if(!secondOptionOrNull.HasValue)
			{
				return firstOption;
			}

			var firstService = firstOption.service;
			var secondOption = secondOptionOrNull.Value;
			var secondService = secondOption.service;

			if(firstService is null)
			{
				return secondOption;
			}

			if(secondService is null)
			{
				return firstOption;
			}

			if(ReferenceEquals(firstService, secondService))
			{
				return firstOption;
			}

			var scene = client.gameObject.scene;

			if(firstOption.serviceProvider.gameObject.scene != scene)
			{
				#if DEBUG
				if(secondOption.serviceProvider.gameObject.scene != scene)
				{
					Debug.LogWarning($"AmbiguousMatchWarning: Client on GameObject \"{client.name}\" has access to both services {TypeUtility.ToString(firstOption.service.GetType())} and {TypeUtility.ToString(secondOption.service.GetType())} via {nameof(Services)}s and unable to determine which one should be prioritized.", client);
				}
				#endif

				return secondOption;
			}

			if(secondOption.serviceProvider.gameObject.scene != scene)
			{
				return firstOption;
			}

			var firstTransform = firstOption.serviceProvider.gameObject.transform;
			var secondTransform = secondOption.serviceProvider.gameObject.transform;

			for(var parent = client.transform; parent != null; parent = parent.parent)
			{
				if(parent == firstTransform)
				{
					if(parent == secondTransform)
					{
						break;
					}

					return firstOption;
				}

				if(parent == secondTransform)
				{
					return secondOption;
				}
			}

			#if DEBUG
			Debug.LogWarning($"AmbiguousMatchWarning: Client on GameObject \"{client.name}\" has access to both services \"{firstOption.serviceProvider.gameObject.name}\"/{TypeUtility.ToString(firstOption.service.GetType())} and \"{secondOption.serviceProvider.gameObject.name}\"/{TypeUtility.ToString(secondOption.service.GetType())} via {nameof(Services)} and unable to determine which one should be prioritized.", client);
			#endif

			return firstOption;
		}

		internal readonly struct ProvidedServiceInfo
		{
			public readonly object service;
			public readonly Component serviceProvider;
			public readonly Clients toClients;

			public ProvidedServiceInfo(object service, Component serviceProvider, Clients toClients)
			{
				this.service = service;
				this.serviceProvider = serviceProvider;
				this.toClients = toClients;
			}
		}
	}
}