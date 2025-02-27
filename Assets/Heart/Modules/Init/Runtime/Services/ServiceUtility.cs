using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sisus.Init.Internal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using static Sisus.NullExtensions;

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Init
{
	/// <summary>
	/// Utility class responsible for providing information about <see cref="ServiceAttribute">services</see>.
	/// </summary>
	public static class ServiceUtility
	{
		static readonly Dictionary<Type, GlobalServiceGetter> globalServiceGetters = new();
		static readonly Dictionary<Type, GlobalServiceSetter> globalServiceSetters = new();
		static readonly Dictionary<Type, GlobalServiceSilentSetter> globalServiceSilentSetters = new();
		static readonly Dictionary<ScopedServiceAdder.Key, ScopedServiceAdder> scopedServiceAdders = new();
		static readonly Dictionary<Type, ScopedServiceGetter> scopedServiceGetters = new();
		static readonly Dictionary<Type, IsServiceOrServiceProviderForResolver> isServiceOrServiceProviderForResolvers = new();
		static readonly Dictionary<Type, IsServiceOrServiceProviderResolver> isServiceOrServiceProviderResolvers = new();
		static readonly Dictionary<ScopedServiceRemover.Key, ScopedServiceRemover> scopedServiceRemovers = new();
		static readonly Dictionary<Type, ScopedServicesFromProviderRemover> scopedServicesFromProviderRemovers = new();
		static readonly Dictionary<Type, ServiceClientsGetter> serviceClientsGetters = new();

		/// <summary>
		/// <see langword="true"/> if all shared services that are loaded synchronously during game initialization
		/// have been created, initialized and are ready to be used by clients; otherwise, <see langword="false"/>.
		/// <para>
		/// This only takes into consideration services that are initialized synchronously during game initialization.
		/// To determine if all asynchronously initialized services are also ready to be used,
		/// use <see cref="AsyncServicesAreReady"/> instead.
		/// </para>
		/// <para>
		/// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>
		/// (<see cref="EditorServiceAttribute"/> in Edit Mode).
		/// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
		/// Services that are registered manually using <see cref="Service.Set{TService}"/> are also not
		/// guaranteed to be loaded even if this is <see langword="true"/>.
		/// </para>
		/// </summary>
		public static bool ServicesAreReady
		{
			get
			{
				#if INIT_ARGS_DISABLE_SERVICE_INJECTION
				return true;
				#else

				#if UNITY_EDITOR
				if(!EditorOnly.ThreadSafe.Application.IsPlaying)
				{
					return EditorServiceInjector.ServicesAreReady;
				}
				#endif


				return ServiceInjector.ServicesAreReady;
				#endif
			}
		}

		/// <summary>
		/// <see langword="true"/> if all shared services that are loaded asynchronously during game initialization
		/// have been created, initialized and are ready to be used by clients; otherwise, <see langword="false"/>.
		/// <para>
		/// This only takes into consideration services defined using the <see cref="ServiceAttribute"/>.
		/// </para>
		/// <para>
		/// Services set up in scenes and prefabs using <see cref="ServiceTag"/> and <see cref="Services"/>
		/// components are not guaranteed to be yet loaded even if this is <see langword="true"/>.
		/// Services that are registered manually using <see cref="Service.Set{TService}"/> are also not
		/// guaranteed to be loaded even if this is <see langword="true"/>.
		/// </para>
		/// </summary>
		public static bool AsyncServicesAreReady
		{
			get
			{
				#if INIT_ARGS_DISABLE_SERVICE_INJECTION
				return true;
				#else

				#if UNITY_EDITOR
				if(!EditorOnly.ThreadSafe.Application.IsPlaying)
				{
					return EditorServiceInjector.ServicesAreReady;
				}
				#endif

				return ServiceInjector.ServicesAreReady;
				#endif
			}
		}

		/// <summary>
		/// Event that is broadcast when all <see cref="ServiceAttribute">services</see> have been created,
		/// initialized and are ready to be used by clients.
		/// </summary>
		public static event Action ServicesBecameReady
		{
			add
			{
				#if INIT_ARGS_DISABLE_SERVICE_INJECTION
				value?.Invoke();
				#else
				if(ServiceInjector.ServicesAreReady)
				{
					value?.Invoke();
				}

				ServiceInjector.ServicesBecameReady += value;
				#endif
			}

			remove
			{
				#if !INIT_ARGS_DISABLE_SERVICE_INJECTION
				ServiceInjector.ServicesBecameReady -= value;
				#endif
			}
		}

		/// <summary>
		/// Gets the shared service instance of the given <paramref name="definingType"/>.
		/// <para>
		/// The returned object's class will match the provided <paramref name="definingType"/>,
		/// derive from it or implement an interface of the type.
		/// </para>
		/// </summary>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"> Thrown if no globally accessbible service of type  <paramref name="definingType"/> is found. </exception>
		public static object Get([DisallowNull] Type definingType)
		{
			if(!globalServiceGetters.TryGetValue(definingType, out var getter))
			{
				getter = new GlobalServiceGetter(definingType);
				globalServiceGetters.Add(definingType, getter);
			}

			return getter.TryGet(out var service) ? service : throw new NullReferenceException($"No globally accessible Service of type {definingType.Name} was found.");
		}

		/// <summary>
		/// Gets the shared service instance of the given <paramref name="serviceType"/>.
		/// <para>
		/// The returned object's class will match the provided <paramref name="serviceType"/>,
		/// derive from it or implement an interface of the type.
		/// </para>
		/// </summary>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"> Thrown if no service of type <typeparamref name="TService"/> is found that is globally accessible to any client. </exception>
		public static bool TryGet([DisallowNull] Type definingType, out object service)
		{
			if(!globalServiceGetters.TryGetValue(definingType, out var getter))
			{
				getter = new GlobalServiceGetter(definingType);
				globalServiceGetters.Add(definingType, getter);
			}

			return getter.TryGet(out service);
		}

		/// <summary>
		/// Gets the shared service instance of the given <paramref name="definingType"/>.
		/// <para>
		/// The returned object's class will match the provided <paramref name="definingType"/>,
		/// derive from it or implement an interface of the type.
		/// </para>
		/// <para>
		/// If no such service has been registered then <see langword="null"/> is returned.
		/// </para>
		/// </summary>
		/// <exception cref="NullReferenceException"> Thrown if no service of type <typeparamref name="TService"/> is found that is accessible to the <paramref name="client"/>. </exception>
		/// <param name="client"> The client that needs the service. </param>
		/// <param name="definingType"> The defining type of the service. </param>
		/// <param name="context"> The context from which the request is being made. </param>
		/// <returns> Shared instance of the service of the given type. </returns>
		public static object Get(object client, [DisallowNull] Type definingType, Context context = Context.MainThread)
		{
			if(!context.IsUnitySafeContext())
			{
				return Get(definingType);
			}

			if(!scopedServiceGetters.TryGetValue(definingType, out var getter))
			{
				getter = new ScopedServiceGetter(definingType);
				scopedServiceGetters.Add(definingType, getter);
			}

			return getter.TryGetFor(client, out var service) ? service : throw new NullReferenceException($"No service of type {definingType.Name} was found that was accessible to client {(client is null ? "null" : client.GetType().Name)} during context {context}.");
		}

		/// <summary>
		/// Gets the shared service instance of the given <paramref name="serviceType"/>.
		/// <para>
		/// The returned object's class will match the provided <paramref name="serviceType"/>,
		/// derive from it or implement an interface of the type.
		/// </para>
		/// </summary>
		/// <exception cref="NullReferenceException"> Thrown if no service of type <typeparamref name="TService"/> is found that is accessible to the <paramref name="client"/>. </exception>
		/// <param name="client"> The client that needs the service. </param>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <param name="service">
		/// When this method returns, contains service of type <paramref name="definingType"/>
		/// if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="context"> The context from which the request is being made. </param>
		/// <returns> Shared instance of the service of the given type. </returns>
		public static bool TryGetFor(object client, [DisallowNull] Type definingType, out object service, Context context = Context.MainThread)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = tryGetForMarker.Auto();
			#endif

			if(!context.IsUnitySafeContext())
			{
				return TryGet(definingType, out service);
			}

			if(!scopedServiceGetters.TryGetValue(definingType, out var getter))
			{
				getter = new ScopedServiceGetter(definingType);
				scopedServiceGetters.Add(definingType, getter);
			}

			return getter.TryGetFor(client, out service);
		}

		public static bool IsServiceOrServiceProviderFor([DisallowNull] Component client, [DisallowNull] Type definingType, [DisallowNull] object test)
		{
			if(!isServiceOrServiceProviderForResolvers.TryGetValue(definingType, out var resolver))
			{
				resolver = new IsServiceOrServiceProviderForResolver(definingType);
				isServiceOrServiceProviderForResolvers.Add(definingType, resolver);
			}

			return resolver.IsServiceOrServiceProviderFor(client, test);
		}

		public static bool IsServiceOrServiceProvider([DisallowNull] Type definingType, [DisallowNull] object test)
		{
			if(!isServiceOrServiceProviderResolvers.TryGetValue(definingType, out var resolver))
			{
				resolver = new IsServiceOrServiceProviderResolver(definingType);
				isServiceOrServiceProviderResolvers.Add(definingType, resolver);
			}
			
			return resolver.IsServiceOrServiceProvider(test);
		}

		internal static bool IsValidDefiningTypeFor(Type definingType, Type concreteType)
		{
			if(definingType.IsAssignableFrom(concreteType))
			{
				return true;
			}

			if(!concreteType.IsGenericTypeDefinition || !definingType.IsGenericTypeDefinition)
			{
				return false;
			}

			foreach(var interfaceType in concreteType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == definingType)
				{
					return true;
				}
			}

			for(var type = concreteType; type is not null; type = type.BaseType)
			{
				if(type.IsGenericType && type.GetGenericTypeDefinition() == definingType)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether service of type <typeparamref name="TService"/>
		/// is available for the <paramref name="client"/>.
		/// <para>
		/// The service can be located from <see cref="Services"/> components in the active scenes,
		/// or failing that, from the globally shared <see cref="Service{TService}.Instance"/>.
		/// </para>
		/// <para>
		/// This method can only be called from the main thread.
		/// </para>
		/// </summary>
		/// <param name="client"> The client that needs the service. </param>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <returns>
		/// <see langword="true"/> if service of the given type exists for the client; otherwise, <see langword="false"/>.
		/// </returns>
		public static bool ExistsFor([DisallowNull] object client, [DisallowNull] Type definingType) => TryGetFor(client, definingType, out _);

		public static bool Exists([DisallowNull] Type definingType) => TryGet(definingType, out _);

		public static bool TryGetClients([DisallowNull] Component serviceOrServiceProvider, [DisallowNull] Type definingType, out Clients clients)
		{
			if(!definingType.IsInstanceOfType(serviceOrServiceProvider))
			{
				foreach(var serviceTag in serviceOrServiceProvider.gameObject.GetComponentsNonAlloc<ServiceTag>())
				{
					if(serviceTag == serviceOrServiceProvider)
					{
						clients = serviceTag.ToClients;
						return true;
					}
				}

				foreach(var services in Find.All<Services>())
				{
					if(services == serviceOrServiceProvider)
					{
						clients = services.toClients;
						return true;
					}
				}

				clients = Clients.Everywhere;
				return true;
			}

			if(!serviceClientsGetters.TryGetValue(definingType, out var getter))
			{
				getter = new ServiceClientsGetter(definingType);
				serviceClientsGetters.Add(definingType, getter);
			}

			return getter.TryGetFor(null, out clients);
		}
		
		public static bool TryGetClients([DisallowNull] Component serviceOrServiceProvider, out Clients clients)
		{
			foreach(var serviceTag in serviceOrServiceProvider.gameObject.GetComponentsNonAlloc<ServiceTag>())
			{
				if(serviceTag == serviceOrServiceProvider)
				{
					clients = serviceTag.ToClients;
					return true;
				}
			}

			foreach(var services in Find.All<Services>())
			{
				if(services == serviceOrServiceProvider)
				{
					clients = services.toClients;
					return true;
				}
			}

			if(ServiceAttributeUtility.concreteTypes.TryGetValue(serviceOrServiceProvider.GetType(), out var serviceInfo))
			{
				clients = serviceInfo.clients;
				return true;
			}
			
			foreach(var someServiceInfo in ServiceAttributeUtility.definingTypes.Values)
			{
				if(someServiceInfo.serviceOrProviderType.IsInstanceOfType(serviceOrServiceProvider))
				{
					clients = someServiceInfo.clients;
					return true;
				}
			}

			clients = Clients.Everywhere;
			return false;
		}

		/// <summary>
		/// Sets the <see cref="Service{}.Instance">service instance</see> of the provided
		/// <paramref name="definingType">type</paramref> that is shared across clients
		/// to the given value.
		/// <para>
		/// If the provided instance is not equal to the old <see cref="Service{}.Instance"/>
		/// then the <see cref="Service{}.InstanceChanged"/> event will be raised.
		/// </para>
		/// </summary>
		/// <param name="instance"> The new instance of the service. </param>
		#if UNITY_2022_3_OR_NEWER
		[HideInCallstack]
		#endif
		public static void SetInstanceSilently([DisallowNull] Type definingType, [AllowNull] object instance)
		{
			Debug.Assert(definingType is not null);

			if(instance is not null && !definingType.IsInstanceOfType(instance))
			{
				Object context = instance as Object;

				if(definingType.IsInterface)
				{
					Debug.LogAssertion($"Invalid Service Definition: Class of the registered instance '{TypeUtility.ToString(instance.GetType())}' does not implement the defining interface type of the service '{definingType.Name}'.", context);
				}
				else
				{
					Debug.LogAssertion($"Invalid Service Definition: Class of the registered instance '{TypeUtility.ToString(instance.GetType())}' does not derive from the defining type of the service '{definingType.Name}'.", context);
				}

				return;
			}

			if(!globalServiceSilentSetters.TryGetValue(definingType, out var serviceSetter))
			{
				serviceSetter = new GlobalServiceSilentSetter(definingType);
				globalServiceSilentSetters.Add(definingType, serviceSetter);
			}

			serviceSetter.SetInstanceSilently(instance);
		}

		/// <summary>
		/// Sets the <see cref="Service{}.Instance">service instance</see> of the provided
		/// <paramref name="definingType">type</paramref> that is shared across clients
		/// to the given value.
		/// <para>
		/// If the provided instance is not equal to the old <see cref="Service{}.Instance"/>
		/// then the <see cref="Service{}.InstanceChanged"/> event will be raised.
		/// </para>
		/// </summary>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <param name="instance"> The new instance of the service. </param>
		#if UNITY_2022_3_OR_NEWER
		[HideInCallstack]
		#endif
		public static void Set([DisallowNull] Type definingType, [AllowNull] object instance)
		{
			#if DEV_MODE || DEBUG || INIT_ARGS_SAFE_MODE
			Debug.Assert(definingType is not null);

			if(instance is not null && !definingType.IsInstanceOfType(instance))
			{
				if(definingType.IsInterface)
				{
					Debug.LogAssertion($"Invalid Service Definition: Class of the registered instance '{TypeUtility.ToString(instance.GetType())}' does not implement the defining interface type of the service '{TypeUtility.ToString(definingType)}'.");
				}
				else
				{
					Debug.LogAssertion($"Invalid Service Definition: Class of the registered instance '{TypeUtility.ToString(instance.GetType())}' does not derive from the defining type of the service '{TypeUtility.ToString(definingType)}'.");
				}

				return;
			}
			#endif

			if(!globalServiceSetters.TryGetValue(definingType, out var serviceSetter))
			{
				serviceSetter = new GlobalServiceSetter(definingType);
				globalServiceSetters.Add(definingType, serviceSetter);
			}

			serviceSetter.SetInstance(instance);
		}

		/// <summary>
		/// Sets the <see cref="Service{T}.Instance">service instance</see> of the provided
		/// <paramref name="definingType">type</paramref> that is shared across clients
		/// to the given value.
		/// <para>
		/// If the provided instance is not equal to the old <see cref="Service{T}.Instance"/>
		/// then the <see cref="ServiceChanged{T}.listeners"/> event will be raised.
		/// </para>
		/// </summary>
		/// <param name="serviceOrProvider"> The service instance to add. </param>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <param name="clients">
		/// Specifies which client objects can receive the service instance in their Init function
		/// during their initialization.
		/// </param>
		/// <param name="registerer">
		/// Component that is registering the service. This can also be the service itself, if it is a component.
		/// <para>
		/// This same argument should be passed when <see cref="RemoveFromClients">removing the instance</see>.
		/// </para>
		/// </param>
		public static void AddFor([AllowNull] object serviceOrProvider, [DisallowNull] Type definingType, Clients clients, [DisallowNull] Component registerer)
		{
			#if DEV_MODE
			Debug.Assert(definingType is not null, serviceOrProvider?.GetType().Name ?? "defining type null", serviceOrProvider as Object);
			Debug.Assert(serviceOrProvider != Null, definingType?.Name ?? "service null", serviceOrProvider as Object);
			#endif

			ServiceProviderType serviceProviderType;
			if(definingType.IsInstanceOfType(serviceOrProvider))
			{
				serviceProviderType = ServiceProviderType.None;
			}
			else if(!TryExtractServiceOrProvider(serviceOrProvider, definingType, registerer, out serviceOrProvider, out serviceProviderType))
			{
				LogInvalidServiceDefinitionError(serviceOrProvider.GetType(), definingType, registerer);
				return;
			}

			AddFor(serviceOrProvider, serviceProviderType, definingType, clients, registerer);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void AddFor([AllowNull] object serviceOrProvider, ServiceProviderType serviceProviderType, [DisallowNull] Type definingType, Clients clients, [DisallowNull] Component registerer)
		{
			#if DEV_MODE
			Debug.Assert(definingType is not null, serviceOrProvider?.GetType().Name ?? "defining type null", serviceOrProvider as Object);
			Debug.Assert(serviceOrProvider != Null, definingType?.Name ?? "service null", serviceOrProvider as Object);
			#endif

			var key = new ScopedServiceAdder.Key(definingType, serviceProviderType);
			if(!scopedServiceAdders.TryGetValue(key, out var adder))
			{
				adder = new ScopedServiceAdder(definingType, serviceProviderType);
				scopedServiceAdders.Add(key, adder);
			}

			adder.AddFor(serviceOrProvider, clients, registerer);
		}

		/// <summary>
		/// Sets the <see cref="Service{}.Instance">service instance</see> of the provided
		/// <paramref name="definingType">type</paramref> that is shared across clients
		/// to the given value.
		/// <para>
		/// If the provided instance is not equal to the old <see cref="Service{}.Instance"/>
		/// then the <see cref="Service{}.InstanceChanged"/> event will be raised.
		/// </para>
		/// </summary>
		/// <param name="serviceOrProvider"> The service instance to remove. </param>
		/// <param name="definingType">
		/// The defining type of the service; the class or interface type that uniquely defines
		/// the service and can be used to retrieve an instance of it.
		/// <para>
		/// This must be an interface that the service implement, a base type that the service derives from,
		/// or the exact type of the service.
		/// </para>
		/// </param>
		/// <param name="clients"> The availability of the service being removed.
		/// </param>
		/// <param name="registerer">
		/// Component that registered the service. This can also be the service itself, if it is a component.
		/// </param>
		public static void RemoveFrom([AllowNull] object serviceOrProvider, [DisallowNull] Type definingType, Clients clients, [DisallowNull] Component registerer)
		{
			#if DEV_MODE
			Debug.Assert(definingType is not null, serviceOrProvider?.GetType().Name ?? "defining type null", serviceOrProvider as Object);
			Debug.Assert(serviceOrProvider != Null, definingType?.Name ?? "service null", serviceOrProvider as Object);
			#endif

			ServiceProviderType serviceProviderType;
			if(definingType.IsInstanceOfType(serviceOrProvider))
			{
				serviceProviderType = ServiceProviderType.None;
			}
			else if(!TryExtractServiceOrProvider(serviceOrProvider, definingType, registerer, out serviceOrProvider, out serviceProviderType))
			{
				LogInvalidServiceDefinitionError(serviceOrProvider.GetType(), definingType, registerer);
				return;
			}

			RemoveFrom(serviceOrProvider, serviceProviderType, definingType, clients, registerer);
		}
		
		internal static void RemoveFrom([AllowNull] object serviceOrProvider, ServiceProviderType serviceProviderType, [DisallowNull] Type definingType, Clients clients, [DisallowNull] Component registerer)
		{
			#if DEV_MODE
			Debug.Assert(definingType is not null, serviceOrProvider?.GetType().Name ?? "defining type null", serviceOrProvider as Object);
			Debug.Assert(serviceOrProvider != Null, definingType?.Name ?? "service null", serviceOrProvider as Object);
			#endif
			
			var key = new ScopedServiceRemover.Key(definingType, serviceProviderType);
			if(!scopedServiceRemovers.TryGetValue(key, out var remover))
			{
				remover = new ScopedServiceRemover(definingType, serviceProviderType);
				scopedServiceRemovers.Add(key, remover);
			}
			
			remover.RemoveFrom(serviceOrProvider, clients, registerer);
		}

		/// <summary>
		/// Gets a value indicating whether or not <typeparamref name="T"/> is the defining type of a service.
		/// </summary>
		/// <typeparam name="T"> Type to test. </typeparam>
		/// <returns>
		/// <see langword="true"/> if <typeparamref name="T"/> is the defining type of a service;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public static bool IsServiceDefiningType<T>()
		{
			#if UNITY_EDITOR && !INIT_ARGS_DISABLE_SERVICE_INJECTION
			return (!typeof(T).IsValueType && Service<T>.Instance != Null)
					|| ScopedService<T>.ExistsForAllClients()
					|| ServiceAttributeUtility.definingTypes.ContainsKey(typeof(T))
					|| (!ServicesAreReady && EditorServiceAttributeUtility.definingTypes.ContainsKey(typeof(T)));
			#else
			return (!typeof(T).IsValueType && Service<T>.Instance is not null) || ScopedService<T>.ExistsForAllClients();
			#endif
		}

		/// <summary>
		/// Gets a value indicating whether the provided <paramref name="type"/> is the defining type of a service.
		/// <para>
		/// By default, the defining type of a class that has the <see cref="ServiceAttribute"/> is the type of the class itself,
		/// however it is possible provide a different defining type, which can be any type as long as it is assignable from the
		/// type of the class with the attribute.
		/// </para>
		/// </summary>
		/// <param name="type"> Type to test. </param>
		/// <returns> <see langword="true"/> if type is the defining type of a service; otherwise, <see langword="false"/>. </returns>
		public static bool IsServiceDefiningType([DisallowNull] Type type)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = isServiceDefiningTypeMarker.Auto();
			#endif

			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			#endif

			if(ServiceAttributeUtility.definingTypes.ContainsKey(type))
			{
				return true;
			}

			if(type.IsValueType)
			{
				return false;
			}

			#if DEBUG || INIT_ARGS_SAFE_MODE
			return typeof(Service<>).MakeGenericType(type).GetProperty(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) != null;
			#else
			return typeof(Service<>).MakeGenericType(type).GetField(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) != null;
			#endif
		}

		/// <summary>
		/// A method that can be referenced with particular service types, to ensure that
		/// instance of said service can be registered using reflection on ahead-of-time compiled
		/// platforms without errors occurring.
		/// <para>
		/// Note that this method never needs to be actually called; simply referencing it in your code is enough.
		/// </para>
		/// <para>
		/// For more information see <see href="https://docs.unity3d.com/Manual/ScriptingRestrictions.html"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="TService"> The defining type of a service that should be supported on AOT platforms. </typeparam>
		#if !ENABLE_BURST_AOT && !ENABLE_IL2CPP
		[System.Diagnostics.Conditional("FALSE")]
		#endif
		public static void EnsureAOTPlatformSupportForService<TService>()
		{
			#if (ENABLE_BURST_AOT || ENABLE_IL2CPP) && !INIT_ARGS_DISABLE_AUTOMATIC_AOT_SUPPORT
			if(Application.isEditor || !Application.isEditor)
			{
				return;
			}

			_ = Service<TService>.Instance;
			_ = new ServiceProvider<TService>(default(IValueProvider)).TryGetFor(null, out _);
			_ = ScopedService<TService>.Instances;
			Service.Set<TService>(default);
			Service.SetSilently<TService>(default);
			ServiceChanged<TService>.listeners += (_, _, _) => { };
			Service.AddFor(Clients.Everywhere, new GameObject().GetComponent<TService>(), new GameObject().GetComponent<Transform>());
			Service.RemoveFrom(Clients.Everywhere, new GameObject().GetComponent<TService>(), new GameObject().GetComponent<Transform>());
			Service.TryGetClients<TService>(default, out _);
			#endif
		}

		public static void RemoveAllServicesProvidedBy(Services serviceProvider)
		{
			foreach(var providedService in serviceProvider.providesServices)
			{
				if(providedService.definingType.Value is not { } definingType)
				{
					continue;
				}

				if(!scopedServicesFromProviderRemovers.TryGetValue(definingType, out var remover))
				{
					remover = new ScopedServicesFromProviderRemover(definingType);
					scopedServicesFromProviderRemovers.Add(definingType, remover);
				}

				remover.RemoveFrom(serviceProvider.toClients, serviceProvider);
			}
		}
		
		private static bool TryExtractServiceOrProvider(object serviceProvider, Type definingType, Component registerer, out object result, out ServiceProviderType serviceProviderType)
		{
			if(serviceProvider is IInitializer initializer)
			{
				#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					result = initializer.Target;

					#if DEV_MODE && DEBUG_ENABLED
					if(result is null)
					{
						Debug.Log("Can't register {TypeUtility.ToString(service.GetType())} as {TypeUtility.ToString(definingType)}, but won't log an error, because it is an initializer, and this is edit mode.");
						serviceProviderType = ServiceProviderType.Initializer;
						return false;
					}
					#endif

					serviceProviderType = ServiceProviderType.Initializer;
					return true;
				}
				#endif

				var initialized = initializer.InitTarget();
				if(!definingType.IsInstanceOfType(initialized))
				{
					LogInvalidServiceDefinitionError(serviceProvider.GetType(), definingType, registerer);
					result = null;
					serviceProviderType = ServiceProviderType.None;
					return false;
				}

				result = initialized;
				serviceProviderType = ServiceProviderType.None;
				return true;
			}

			if(serviceProvider is IWrapper wrapper)
			{
				#if UNITY_EDITOR
				if(!Application.isPlaying)
				{
					result = wrapper.WrappedObject;

					#if DEV_MODE && DEBUG_ENABLED
					if(result is null)
					{
						Debug.Log("Can't register {TypeUtility.ToString(service.GetType())} as {TypeUtility.ToString(definingType)}, but won't log an error, because the wrapper has an initializer, and this is edit mode.");
						serviceProviderType = ServiceProviderType.Wrapper;
						return false;
					}
					#endif
					
					serviceProviderType = ServiceProviderType.None;
					if(!definingType.IsInstanceOfType(result))
					{
						LogInvalidServiceDefinitionError(serviceProvider.GetType(), definingType, registerer);
						result = null;
						serviceProviderType = ServiceProviderType.None;
						return false;
					}

					serviceProviderType = ServiceProviderType.None;
					return true;
				}
				else
				#endif
				if(wrapper is Object unityObject
					&& InitializerUtility.TryGetInitializer(unityObject, out initializer)
					&& initializer.InitTarget() is { } initialized)
				{
					result = initialized;
					serviceProviderType = ServiceProviderType.None;
					return true;
				}

				LogInvalidServiceDefinitionError(serviceProvider.GetType(), definingType, registerer);
				result = null;
				serviceProviderType = ServiceProviderType.None;
				return false;
			}

			if(serviceProvider is IValueProvider)
			{
				if(typeof(IValueProvider<>).MakeGenericType(definingType) is { } valueProviderTType
					&& valueProviderTType.IsInstanceOfType(serviceProvider))
				{
					result = serviceProvider;
					serviceProviderType = ServiceProviderType.IValueProviderT;
					return true;
				}

				serviceProviderType = ServiceProviderType.IValueProvider;
			}
			else
			{
				serviceProviderType = ServiceProviderType.None;
			}

			if(serviceProvider is IValueProviderAsync)
			{
				if(typeof(IValueProviderAsync<>).MakeGenericType(definingType) is { } valueProviderAsyncTType
				&& valueProviderAsyncTType.IsInstanceOfType(serviceProvider))
				{
					result = serviceProvider;
					serviceProviderType = ServiceProviderType.IValueProviderAsyncT;
					return true;
				}

				if(serviceProviderType == ServiceProviderType.None)
				{
					serviceProviderType = ServiceProviderType.IValueProviderAsync;
				}
			}

			if(serviceProvider is IValueByTypeProvider)
			{
				result = serviceProvider;
				serviceProviderType = ServiceProviderType.IValueByTypeProvider;
				return true;
			}

			if(serviceProvider is IValueByTypeProviderAsync)
			{
				result = serviceProvider;
				serviceProviderType = ServiceProviderType.IValueByTypeProviderAsync;
				return true;
			}

			if(serviceProviderType != ServiceProviderType.None)
			{
				result = serviceProvider;
				return true;
			}

			result = null;
			return false;
		}
		
		private static void LogInvalidServiceDefinitionError(Type concreteType, Type definingType, Component container)
		{
			#if UNITY_EDITOR
			// Don't log warnings if the container is a selected asset, as that probably means it's still being actively configured.
			if(Array.IndexOf(UnityEditor.Selection.gameObjects, container.gameObject) != -1 && container.gameObject.IsAsset(true))
			{
				return;
			}
			#endif

			if(definingType.IsInterface)
			{
				Debug.LogWarning($"Invalid Service Definition: {concreteType.Name} has been configured as a service with the defining type {definingType.Name}, but {concreteType.Name} does not implement {definingType.Name}.");
				return;
			}

			Debug.LogWarning($"Invalid Service Definition: {concreteType.Name} has been configured as a service with the defining type {definingType.Name}, but {concreteType.Name} does not derive from {definingType.Name}.");
		}

		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker tryGetForMarker = new(ProfilerCategory.Gui, "ServiceUtility.TryGetFor");
		private static readonly ProfilerMarker isServiceDefiningTypeMarker = new(ProfilerCategory.Gui, "ServiceUtility.IsServiceDefiningType");
		#endif
	}
}