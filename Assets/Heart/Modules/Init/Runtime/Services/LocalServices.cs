using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Contains information about all local services that exist in the scene currently.
	/// <remarks>
	/// Useful when registering services, to enable services to depend on any other services,
	/// regardless of where and how they are being registered (ServiceAttribute, Service Tag, Services component).
	/// </remarks>
	/// </summary>
	internal sealed class LocalServices
	{
		private bool initialized;
		private Dictionary<Type, LocalServiceInfo> infosByDefiningTypes;
		private Services[] servicesComponents;
		private ServiceTag[] serviceTags;
		private Initializer[] initializers;

		public int GetCountSlow() => All().Count();

		/// <summary>
		/// Gets all services from Service Tags and Services components in the loaded scenes.
		/// <para>
		/// Initializers are not included in the results.
		/// </para>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Object> All()
		{
			Init();

			foreach(var servicesComponent in servicesComponents)
			{
				foreach(var serviceInfo in servicesComponent.providesServices)
				{
					yield return serviceInfo.service;
				}
			}

			foreach(var serviceTag in serviceTags)
			{
				yield return serviceTag.Service;
			}
		}

		public bool TryGetInfo(Type definingType, out LocalServiceInfo serviceInfo)
		{
			if(!initialized)
			{
				Init();
			}

			return infosByDefiningTypes.TryGetValue(definingType, out serviceInfo);
		}

		public bool TryGet(Component client, Type definingType, out object service)
		{
			if(!TryGetInfo(definingType, out var serviceInfo))
			{
				service = null;
				return false;
			}

			if(!Service.IsAccessibleTo(client, serviceInfo.registerer, serviceInfo.toClients))
			{
				service = null;
				return false;
			}

			if(serviceInfo.registerer is IInitializer initializer)
			{
				service = initializer.InitTargetAsync();
				return true;
			}

			service = serviceInfo.serviceOrProvider;
			return true;
		}

		void Init()
		{
			initialized = true;
			servicesComponents = FindAll<Services>();
			serviceTags = FindAll<ServiceTag>();
			initializers = FindAll<Initializer>();
			infosByDefiningTypes = new Dictionary<Type, LocalServiceInfo>((servicesComponents.Length + serviceTags.Length + initializers.Length) * 2);

			foreach(var serviceTag in serviceTags)
			{
				if(serviceTag.DefiningType is Type type && serviceTag.Service is Object service && service)
				{
					infosByDefiningTypes[type] = new( service, serviceTag.ToClients, serviceTag);
				}
			}

			foreach(var servicesComponent in servicesComponents)
			{
				var toClients = servicesComponent.toClients;
				foreach(var definition in servicesComponent.providesServices)
				{
					if(definition.definingType.Value is Type type && definition.service is Object service && service)
					{
						infosByDefiningTypes[type] = new( service, toClients, servicesComponent);
					}
				}
			}

			foreach(var initializer in initializers)
			{
				if (!InitializerUtility.TryGetClientType(initializer.GetType(), out var definingType))
				{
					continue;
				}

				if(infosByDefiningTypes.ContainsKey(definingType))
				{
					continue;
				}

				if(ServiceAttributeUtility.definingTypes.ContainsKey(definingType))
				{
					infosByDefiningTypes[definingType] = new(initializer, Clients.Everywhere, initializer);
					continue;
				}

				if(initializer.GetTarget() is Component component
				&& !component.gameObject.scene.IsValid()
				&& ServiceTagUtility.HasServiceTag(component))
				{
					infosByDefiningTypes[definingType] = new(initializer, Clients.Everywhere, initializer);
				}
			}

			static T[] FindAll<T>() where T : Component
			{
				#if UNITY_6000_0_OR_NEWER
				return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
				#else
				return Object.FindObjectsOfType<T>(false);
				#endif
			}
		}
	}
}