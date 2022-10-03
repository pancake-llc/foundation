//#define DEBUG_INIT_SERVICES
#define DEBUG_CREATE_SERVICES

using System;
using System.Reflection;
using UnityEngine;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using UnityEngine.Scripting;
using System.Linq;

namespace Pancake.Init
{
    /// <summary>
    /// Utility class responsible for providing information about <see cref="ServiceAttribute">services</see>.
    /// </summary>
    public static class ServiceUtility
    {
        private static readonly MethodInfo serviceGetForAnyMethodDefinition;
        private static readonly MethodInfo serviceGetForClientMethodDefinition;
        private static readonly MethodInfo serviceExistsForClientMethodDefinition;
        private static readonly MethodInfo setInstanceMethodDefinition;

        static ServiceUtility()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            var objectParameterType = new Type[] { typeof(object) };
            serviceGetForAnyMethodDefinition = typeof(Service).GetMethod(nameof(Service.Get), BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
            serviceGetForClientMethodDefinition = typeof(Service).GetMethod(nameof(Service.Get), flags, null, objectParameterType, null);
            serviceExistsForClientMethodDefinition = typeof(Service).GetMethod(nameof(Service.Exists), flags, null, objectParameterType, null);
            setInstanceMethodDefinition = typeof(Service).GetMethod(nameof(Service.SetInstance), flags);
        }

        #if UNITY_EDITOR
        private static bool isPlaying = true;
        #endif

        /// <summary>
        /// <see langword="true"/> if all <see cref="ServiceAttribute">services</see>
        /// have been created, initialized and are ready to be used by clients.
        /// </summary>
        public static bool ServicesAreReady
        {
            get
            {
                #if INIT_ARGS_DISABLE_SERVICE_INJECTION
                return true;
                #else
                
                #if UNITY_EDITOR
                if(!isPlaying)
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
        public static event Action OnServicesBecameReady
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

                ServiceInjector.OnServicesBecameReady += value;
                #endif
            }

            remove
            {
                #if !INIT_ARGS_DISABLE_SERVICE_INJECTION
                ServiceInjector.OnServicesBecameReady -= value;
                #endif
            }
        }

        /// <summary>
        /// Gets the shared service instance of the given <paramref name="serviceType"/>.
        /// <para>
        /// The returned object's class will match the provided <paramref name="serviceType"/>,
        /// derive from it or implement an interface of the type.
        /// </para>
        /// </summary>
        /// <exception cref="NullReferenceException"> Thrown if no service of type <typeparamref name="TService"/> is found that is globally accessible to any client. </exception>
        [Preserve]
		public static object GetService([NotNull] Type serviceType) => serviceGetForAnyMethodDefinition.MakeGenericMethod(serviceType).Invoke(null, null);

        /// <summary>
        /// Gets the shared service instance of the given <paramref name="serviceType"/>.
        /// <para>
        /// The returned object's class will match the provided <paramref name="serviceType"/>,
        /// derive from it or implement an interface of the type.
        /// </para>
        /// <para>
        /// If no such service has been registered then <see langword="null"/> is returned.
        /// </para>
        /// </summary>
        /// <exception cref="NullReferenceException"> Thrown if no service of type <typeparamref name="TService"/> is found that is accessible to the <paramref name="client"/>. </exception>
        [Preserve]
        public static object GetService(object client, [NotNull] Type serviceType)
        {
            return serviceGetForClientMethodDefinition.MakeGenericMethod(serviceType).Invoke(null, new object[] { client });
        }

        /// <summary>
        /// Determines whether or not service of type <typeparamref name="TService"/>
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
        /// <param name="client"> The defining type of the service. </param>
        /// <returns>
        /// <see langword="true"/> if service of the given type exists for the client; otherwise, <see langword="false"/>.
        /// </returns>
        [Preserve]
        public static bool ServiceExists([NotNull] object client, [NotNull] Type serviceType)
        {
            if(serviceType.IsValueType)
            {
                return false;
            }

            return (bool)serviceExistsForClientMethodDefinition.MakeGenericMethod(serviceType).Invoke(null, new object[] { client });
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
        [Preserve]
        public static void SetInstance([NotNull] Type definingType, [CanBeNull] object instance)
        {
            Debug.Assert(definingType != null);

            if(instance != null && !definingType.IsAssignableFrom(instance.GetType()))
            {
                if(definingType.IsInterface)
                {
                    Debug.LogWarning($"Invalid Service Definition: Service '{instance.GetType().Name}' has the {nameof(ServiceAttribute)} with defining interface type {definingType.Name} but it does not implement {definingType.Name}.");
                }
                else
                {
                    Debug.LogWarning($"Invalid Service Definition: Service '{instance.GetType().Name}' has the {nameof(ServiceAttribute)} with defining type {definingType.Name} but {definingType.Name} is not a derived type.");
                }
                return;
            }

            var setInstanceMethod = setInstanceMethodDefinition.MakeGenericMethod(definingType);
            setInstanceMethod.Invoke(null, new object[] { instance });
        }

        /// <summary>
        /// Gets a value indicating whether or not <typeparamref name="T"/> is a service type.
        /// <para>
        /// Service types are non-abstract classes that have the <see cref="ServiceAttribute"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T"> Type to test. </typeparam>
        /// <returns> <see langword="true"/> if <typeparamref name="T"/> is a concrete service type; otherwise, <see langword="false"/>. </returns>
        public static bool IsServiceDefiningType<T>()
        {
            #if UNITY_EDITOR && !INIT_ARGS_DISABLE_SERVICE_INJECTION
            return ServiceInjector.IsServiceDefiningType<T>() || (!ServicesAreReady && EditorServiceInjector.IsServiceDefiningType<T>());
            #else
            return !typeof(T).IsValueType && Service<T>.Instance != null;
            #endif
        }

        /// <summary>
        /// Gets a value indicating whether or not the provided <paramref name="type"/> is the defining type of a service.
        /// <para>
        /// By default the defining type of a class that has the <see cref="ServiceAttribute"/> is the type of the class itself,
        /// however it is possible provide a different defining type, which can be any type as long as it is assignable from the
        /// type of the class with the attribute.
        /// </para>
        /// </summary>
        /// <param name="type"> Type to test. </param>
        /// <returns> <see langword="true"/> if type is the defining type of a service; otherwise, <see langword="false"/>. </returns>
        public static bool IsDefiningTypeOfAnyServiceAttribute([NotNull] Type type)
        {
            #if DEBUG
            if(type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            #endif

            if(type.IsValueType)
            {
                return false;
            }

            #if UNITY_EDITOR && !INIT_ARGS_DISABLE_SERVICE_INJECTION
            return ServiceInjector.IsDefiningTypeOfAnyServiceAttribute(type);
            #elif DEBUG
            return !(typeof(Service<>).MakeGenericType(type).GetProperty(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is null);
            #else
            return !(typeof(Service<>).MakeGenericType(type).GetField(nameof(Service<object>.Instance), BindingFlags.Static | BindingFlags.Public).GetValue(null) is null);
            #endif
        }

        /// <summary>
        /// Gets a value indicating whether or not the provided <paramref name="instance"/> is a service of the given type.
        /// </summary>
        /// <param name="definingType"> The defining type of the service. </param>
        /// <param name="instance"> The instance to test. </param>
        /// <returns> <see langword="true"/> if <paramref name="instance"/> is a service of type <paramref name="definingType"/>; otherwise, <see langword="false"/>. </returns>
        public static bool IsService([NotNull] Type definingType, [CanBeNull] object instance)
        {
            #if DEBUG
            if(definingType is null)
            {
                throw new ArgumentNullException(nameof(definingType));
            }
            #endif

            if(definingType.IsValueType || instance is null)
            {
                return false;
            }

            var serviceClassType = typeof(Service<>).MakeGenericType(definingType);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            #if DEBUG
            object service = serviceClassType.GetProperty(nameof(Service<object>.Instance), flags).GetValue(null);
            #else
            object service = serviceClassType.GetField(nameof(Service<object>.Instance), flags).GetValue(null);
            #endif

            
            if(ReferenceEquals(service, instance))
			{
                return true;
			}

            return Services.IsService(definingType, instance);
        }

        [CanBeNull]
        public static Type GetServiceConcreteType([NotNull] Type classWithAttribute)
		{
            if(typeof(IInitializer).IsAssignableFrom(classWithAttribute) || typeof(IWrapper).IsAssignableFrom(classWithAttribute))
			{
                var interfaceTypes = classWithAttribute.GetInterfaces();
                for(int i = interfaceTypes.Length - 1; i >= 0; i--)
                {
                    var interfaceType = interfaceTypes[i];
                    if(!interfaceType.IsGenericType)
					{
                        continue;
					}

                    var genericType = interfaceType.GetGenericTypeDefinition();
                    if(genericType == typeof(IInitializer<>) || genericType == typeof(IWrapper<>))
                    {
                        var clientType = interfaceType.GetGenericArguments()[0];
						return clientType.IsAbstract || TypeUtility.IsBaseType(clientType)
							? TypeUtility.GetDerivedTypes(clientType, clientType.Assembly, null).SingleOrDefault(t => !t.IsAbstract)
							: clientType;
					}
				}
            }

            if(!classWithAttribute.IsAbstract)
			{
                return classWithAttribute;
			}

            return TypeUtility.GetDerivedTypes(classWithAttribute, classWithAttribute.Assembly, null).SingleOrDefault();
		}

        #if UNITY_EDITOR
        /// <summary>
        /// Gets a value indicating whether or not the provided <paramref name="instance"/> is a service of any type.
        /// </summary>
        /// <param name="instance"> The instance to test. </param>
        /// <returns> <see langword="true"/> if <paramref name="instance"/> is a service; otherwise, <see langword="false"/>. </returns>
        internal static bool IsService([CanBeNull] object instance)
        {
            if(instance is null)
			{
                return false;
			}

            if(instance.GetType().GetCustomAttributes<ServiceAttribute>().Any())
			{
                return true;
			}

            return Services.IsService(instance);
        }
        #endif
    }
}