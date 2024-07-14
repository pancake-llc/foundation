using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sisus.Init.Internal
{
	internal static class ServiceAttributeUtility
	{
        internal static readonly Dictionary<Type, ServiceInfo> concreteTypes;
        internal static readonly Dictionary<Type, ServiceInfo> definingTypes;

        static ServiceAttributeUtility()
        {
            #if UNITY_EDITOR
            var typesWithAttribute = UnityEditor.TypeCache.GetTypesWithAttribute<ServiceAttribute>();
            int count = typesWithAttribute.Count;
            #else
			var typesWithAttribute = TypeUtility.GetTypesWithAttribute<ServiceAttribute>(typeof(ServiceAttribute).Assembly, null);
            int count = typesWithAttribute.Count();
			#endif

            concreteTypes = new Dictionary<Type, ServiceInfo>(count);
            definingTypes = new Dictionary<Type, ServiceInfo>(count);

			#if UNITY_EDITOR
			foreach(var typeWithAttribute in typesWithAttribute)
			{
                var attributes = typeWithAttribute.GetCustomAttributes<ServiceAttribute>().ToArray();
			#else
			foreach((var typeWithAttribute, var attributes) in typesWithAttribute)
			{
			#endif
                var info = BuildInfo(typeWithAttribute, attributes);
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

			[return: NotNull]
            static ServiceInfo BuildInfo([DisallowNull] Type classWithAttribute, [DisallowNull] ServiceAttribute[] attributes)
		    {
                var concreteType = GetConcreteType(classWithAttribute);
			    var definingTypesOfService = new Type[attributes.Length];
			    for(int i = attributes.Length - 1; i >= 0; i--)
			    {
                    definingTypesOfService[i] = attributes[i].definingType ?? concreteType ?? classWithAttribute;
			    }

                return new ServiceInfo(classWithAttribute, concreteType, definingTypesOfService, attributes);

                [return: MaybeNull]
				static Type GetConcreteType([DisallowNull] Type classWithAttribute)
		        {
                    bool isInitializer = typeof(IInitializer).IsAssignableFrom(classWithAttribute);
                    bool isWrapper = typeof(IWrapper).IsAssignableFrom(classWithAttribute);
                    if(isInitializer || isWrapper)
			        {
                        var match = isInitializer ? typeof(IInitializer<>) : typeof(IWrapper<>);
                        var interfaceTypes = classWithAttribute.GetInterfaces();
                        for(int i = interfaceTypes.Length - 1; i >= 0; i--)
				        {
					        var interfaceType = interfaceTypes[i];
					        if(!interfaceType.IsGenericType)
					        {
						        continue;
					        }

					        var genericType = interfaceType.GetGenericTypeDefinition();
					        if(genericType != match)
					        {
						        continue;
					        }

					        var clientType = interfaceType.GetGenericArguments()[0];

                            if(TypeUtility.IsBaseType(clientType))
						    {
                                return null;
						    }

                            if(clientType.IsAbstract)
                            {
						        return GetSingleDerivedConcreteTypeOrNull(clientType);
                            }

                            if(clientType.IsGenericType && typeof(Task).IsAssignableFrom(clientType))
					        {
                                var taskReturnType = clientType.GetGenericArguments()[0];
								return !taskReturnType.IsAbstract ? taskReturnType : GetSingleDerivedConcreteTypeOrNull(taskReturnType);
					        }
					
					        return clientType;
				        }
			        }

                    if(TypeUtility.IsBaseType(classWithAttribute))
				    {
                        return null;
				    }

				    return !classWithAttribute.IsAbstract ? classWithAttribute : GetSingleDerivedConcreteTypeOrNull(classWithAttribute);
			    }

			    [return: MaybeNull]
				static Type GetSingleDerivedConcreteTypeOrNull([DisallowNull] Type baseOrInterfaceType)
                {
                    Type result = null;
			        #if UNITY_EDITOR
			        foreach(Type derivedType in UnityEditor.TypeCache.GetTypesDerivedFrom(baseOrInterfaceType))
                    {
			        #else
                    foreach(Type derivedType in TypeUtility.GetAllTypesThreadSafe(baseOrInterfaceType.Assembly, null))
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