using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.Collections.Concurrent;
using UnityEditor;
#endif

namespace Sisus.Init
{
	/// <summary>
	/// Class that can act as a container for a service instances that are accessible by a limited number of clients.
	/// </summary>
	/// <typeparam name="TService">
	/// The defining type of the service class, which is the type specified in its <see cref="ServiceAttribute"/>,
	/// or - if no other type has been explicitly specified - the exact type of the service class.
	/// <para>
	/// This type must be an interface that the service implements, a base type that the service derives from,
	/// or the exact type of the service.
	/// </para>
	/// </typeparam>
	/// <seealso cref="Service.GetFor{TService}(Component)"/>
	/// <seealso cref="Service.TryGetFor{TService}(Component, out TService)"/>
	/// <seealso cref="Service.AddFor{TService}(Clients, TService)"/>
	/// <seealso cref="Service.RemoveFrom{TService}(Clients, TService)"/>
	internal static class ScopedService<TService>
	{
		#if UNITY_EDITOR
		// In the editor services can be registered in OnValidate and OnAfterDeserialize,
		// so we need to use thread safe collections. In builds services are only registered
		// in OnEnable, so we can just use a simple list.
		// In the editor we also use separate caches for edit and play modes to avoid issues with
		// Add / Remove not being called in pairs when entering / exiting play mode etc.
		private static readonly ConcurrentDictionary<Instance, byte> instancesInEditMode = new();
		private static readonly ConcurrentDictionary<Instance, byte> instancesInPlayMode = new();

		public static ICollection<Instance> Instances => (EditorOnly.ThreadSafe.Application.IsPlaying ? instancesInPlayMode : instancesInEditMode).Keys;
		#else
		public static readonly List<Instance> Instances = new List<Instance>();
		#endif

		#if UNITY_EDITOR
		static ScopedService()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

			static void OnPlayModeStateChanged(PlayModeStateChange state)
			{
				if(state == PlayModeStateChange.ExitingPlayMode)
				{
					instancesInPlayMode.Clear();
				}
			}
		}
		#endif

		internal static bool IsServiceFor([DisallowNull] Component client, [DisallowNull] TService test)
		{
			#if DEV_MODE
			Debug.Assert(client is not null, "ScopedService<T>.IsServiceFor called with null client.");
			Debug.Assert(test is not null, "ScopedService<T>.IsServiceFor called with null object to test.");
			#endif

			foreach(var instance in Instances)
			{
				#if UNITY_EDITOR
				if(!instance.serviceProvider)
				{
					continue;
				}
				#endif

				if(ReferenceEquals(test, instance.service))
				{
					return Service.IsAccessibleTo(instance, client.transform);
				}
			}

			return false;
		}

		public static bool IsService(TService test)
		{
			#if DEV_MODE
			Debug.Assert(test is not null, "ScopedService<T>.IsService called with null object to test.");
			#endif

			foreach(var instance in Instances)
			{
				#if UNITY_EDITOR
				if(!instance.serviceProvider)
				{
					continue;
				}
				#endif

				if(ReferenceEquals(test, instance.service))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets a value indicating whether a service of type <see paramref="TService"/> exists
		/// with accessibility level <see cref="Clients.Everywhere"/>.
		/// </summary>
		public static bool ExistsForAllClients()
		{
			foreach(var instance in Instances)
			{
				#if UNITY_EDITOR
				if(!instance.serviceProvider)
				{
					continue;
				}
				#endif

				if(instance is { clients: Clients.Everywhere, service: not null })
				{
					return true;
				}
			}

			return false;
		}

		#if UNITY_EDITOR
		public static bool Add([DisallowNull] TService service, Clients clients, [DisallowNull] Component container)
		{
			Debug.Assert(service != null, "ScopedService<TService>.Add()");
			var instances = EditorOnly.ThreadSafe.Application.IsPlaying ? instancesInPlayMode : instancesInEditMode;
			return instances.TryAdd(new Instance(service, clients, container), default);
		}
		#else
		public static bool Add([DisallowNull] TService service, Clients clients, [DisallowNull] Component container)
		{
			Instances.Add(new Instance(service, clients, container));
			return true;
		}
		#endif

		#if UNITY_EDITOR
		internal static bool AddInternal([AllowNull] TService service, Clients clients, [DisallowNull] Component container)
		{
			var instances = EditorOnly.ThreadSafe.Application.IsPlaying ? instancesInPlayMode : instancesInEditMode;
			return instances.TryAdd(new Instance(service, clients, container), default);
		}
		#endif

		public static bool Remove([DisallowNull] TService service, [DisallowNull] Component container)
		{
			#if UNITY_EDITOR
			var instances = UnityEditor.EditorApplication.isPlaying ? instancesInPlayMode : instancesInEditMode;
			foreach(var instance in instances.Keys)
			{
				if(ReferenceEquals(instance.service, service) && ReferenceEquals(instance.serviceProvider, container))
				{
					return instances.TryRemove(instance, out _);
				}
			}
			#else
			for(int i = Instances.Count - 1; i >= 0; i--)
			{
				var instance = Instances[i];
				if(ReferenceEquals(instance.service, service) && ReferenceEquals(instance.serviceProvider, container))
				{
					Instances.RemoveAt(i);
					return true;
				}
			}
			#endif

			return false;
		}

		public static bool RemoveFrom([DisallowNull] Component serviceProvider, out TService service)
		{
			#if UNITY_EDITOR
			var instances = UnityEditor.EditorApplication.isPlaying ? instancesInPlayMode : instancesInEditMode;
			foreach(var instance in instances.Keys)
			{
				if(ReferenceEquals(instance.serviceProvider, serviceProvider))
				{
					instances.TryRemove(instance, out _);
					service = instance.service;
					return true;
				}
			}
			#else
			for(int i = Instances.Count - 1; i >= 0; i--)
			{
				var instance = Instances[i];
				if(ReferenceEquals(instance.serviceProvider, serviceProvider))
				{
					Instances.RemoveAt(i);
					service = instance.service;
					return true;
				}
			}
			#endif

			service = default;
			return false;
		}

		#if (ENABLE_BURST_AOT || ENABLE_IL2CPP) && !INIT_ARGS_DISABLE_AUTOMATIC_AOT_SUPPORT
		private static void EnsureAOTPlatformSupport() => ServiceUtility.EnsureAOTPlatformSupportForService<TService>();
		#endif

		internal readonly struct Instance : IEquatable<Instance>
		{
			/// <summary>
			/// The service instance
			/// </summary>
			public readonly TService service;

			/// <summary>
			/// Specifies which client objects can receive services from the service.
			/// </summary>
			public readonly Clients clients;

			/// <summary>
			/// ServiceTag, ServicesComponent, Initializer etc.
			/// </summary>
			public readonly Component serviceProvider;

			public readonly Transform Transform => serviceProvider ? serviceProvider.transform : null;
			public readonly Scene Scene => serviceProvider ? serviceProvider.gameObject.scene : default;

			public Instance([DisallowNull] TService service, Clients clients, [DisallowNull] Component serviceProvider)
			{
				this.service = service;
				this.clients = clients;
				this.serviceProvider = serviceProvider;
			}

			public override bool Equals(object obj) => obj is Instance instance && Equals(instance);
			
			public bool Equals(Instance other) => 
				EqualityComparer<TService>.Default.Equals(service, other.service) &&
				clients == other.clients &&
				ReferenceEquals(serviceProvider, other.serviceProvider);

			public override int GetHashCode()
			{
				int hashCode = -618816118;
				hashCode = hashCode * -1521134295 + EqualityComparer<TService>.Default.GetHashCode(service);
				hashCode = hashCode * -1521134295 + clients.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<Component>.Default.GetHashCode(serviceProvider);
				return hashCode;
			}
		}
	}
}