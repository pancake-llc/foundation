using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Contains a collections of utility methods related to <see cref="Type"/>.
	/// </summary>
	internal static class TypeUtility
	{
		private static readonly HashSet<Type> baseTypes = new()
		{
			typeof(object),
			typeof(Object),
			typeof(Component),
			typeof(Behaviour),
			typeof(MonoBehaviour),
			typeof(ScriptableObject),
			typeof(StateMachineBehaviour),
			#if UNITY_EDITOR
			typeof(UnityEditor.EditorWindow),
			#endif
			#if UNITY_UGUI
			typeof(UnityEngine.EventSystems.UIBehaviour)
			#endif
		};

		private static readonly Dictionary<char, Dictionary<Type, string>> toStringCache = new(64)
		{
			{ '\0', new Dictionary<Type, string>(4096) {
				{ typeof(Serialization._Integer), "Integer" }, { typeof(int), "Integer" }, { typeof(uint), "UInteger" },
				{ typeof(Serialization._Float), "Float" }, { typeof(float), "Float" },
				{ typeof(Serialization._Double), "Double" }, { typeof(double), "Double" },
				{ typeof(Serialization._Boolean), "Boolean" }, { typeof(bool), "Boolean" },
				{ typeof(Serialization._String), "String" }, { typeof(string), "String" },
				{ typeof(short), "Short" }, { typeof(ushort), "UShort" },
				{ typeof(byte), "Byte" },{ typeof(sbyte), "SByte" },
				{ typeof(long), "Long" }, { typeof(ulong), "ULong" },
				{ typeof(object), "object" },
				{ typeof(decimal), "Decimal" }, { typeof(Serialization._Type), "Type" }
			} },
			{ '/', new Dictionary<Type, string>(4096) {
				{ typeof(Serialization._Integer), "Integer" }, { typeof(int), "Integer" }, { typeof(uint), "UInteger" },
				{ typeof(Serialization._Float), "Float" }, { typeof(float), "Float" },
				{ typeof(Serialization._Double), "Double" }, { typeof(double), "Double" },
				{ typeof(Serialization._Boolean), "Boolean" }, { typeof(bool), "Boolean" },
				{ typeof(Serialization._String), "String" }, { typeof(string), "String" },
				{ typeof(short), "Short" }, { typeof(ushort), "UShort" },
				{ typeof(byte), "Byte" },{ typeof(sbyte), "SByte" },
				{ typeof(long), "Long" }, { typeof(ulong), "ULong" },
				{ typeof(object), "System/Object" },
				{ typeof(decimal), "Decimal" }, { typeof(Serialization._Type), "System/Type" }
			} },
			{ '.', new Dictionary<Type, string>(4096) {
				{ typeof(Serialization._Integer), "Integer" }, { typeof(int), "Integer" }, { typeof(uint), "UInteger" },
				{ typeof(Serialization._Float), "Float" }, { typeof(float), "Float" },
				{ typeof(Serialization._Double), "Double" }, { typeof(double), "Double" },
				{ typeof(Serialization._Boolean), "Boolean" }, { typeof(bool), "Boolean" },
				{ typeof(Serialization._String), "String" }, { typeof(string), "String" },
				{ typeof(short), "Short" }, { typeof(ushort), "UShort" },
				{ typeof(byte), "Byte" },{ typeof(sbyte), "SByte" },
				{ typeof(long), "Long" }, { typeof(ulong), "ULong" },
				{ typeof(object), "System.Object" },
				{ typeof(decimal), "Decimal" }, { typeof(Serialization._Type), "System.Type" }
			} }
		};

		private static readonly Dictionary<Type, bool> isSerializableByUnityCache = new();


		#if UNITY_EDITOR
		internal static UnityEditor.TypeCache.TypeCollection
		#else
		[return: NotNull]
		internal static List<Type>
		#endif
		GetTypesWithAttribute<TAttribute>(bool publicOnly) where TAttribute : Attribute
		{
			#if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesWithAttribute<TAttribute>();
			#else
			var result = new List<Type>(64);

			foreach(var type in GetAllTypesThreadSafe(typeof(TAttribute).Assembly, publicOnly))
			{
				if(type.IsDefined(typeof(TAttribute)))
				{
					result.Add(type);
				}
			}

			return result;
			#endif
		}

		#if UNITY_EDITOR
		internal static UnityEditor.TypeCache.TypeCollection
		#else
		[return: NotNull]
		internal static List<Type>
		#endif
			GetTypesWithAttribute<TAttribute>([DisallowNull] Assembly mustReferenceAssembly, bool publicOnly, int estimatedResultCount) where TAttribute : Attribute
		{
			#if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesWithAttribute<TAttribute>();
			#else
			var result = new List<Type>(estimatedResultCount);

			foreach(var type in GetAllTypesThreadSafe(mustReferenceAssembly, publicOnly))
			{
				if(type.IsDefined(typeof(TAttribute)))
				{
					result.Add(type);
				}
			}

			return result;
			#endif
		}

		#if UNITY_EDITOR
		internal static UnityEditor.TypeCache.TypeCollection
		#else
		[return: NotNull]
		internal static List<Type>
		#endif
			GetDerivedTypes<T>(bool publicOnly, int estimatedResultCount)
		{
			#if UNITY_EDITOR
			//return typeCollectionArrayGetter(UnityEditor.TypeCache.GetTypesDerivedFrom(typeof(T)));
			return UnityEditor.TypeCache.GetTypesDerivedFrom(typeof(T));
			#else
			var result = new List<Type>(estimatedResultCount);

			foreach(var type in GetAllTypesThreadSafe(typeof(T).Assembly, publicOnly))
			{
				if(typeof(T).IsAssignableFrom(type))
				{
					result.Add(type);
				}
			}

			return result;
			#endif
		}

		internal static void GetDerivedTypes<T>(List<Type> results, bool publicOnly)
		{
			#if UNITY_EDITOR
			results.AddRange(UnityEditor.TypeCache.GetTypesDerivedFrom(typeof(T)));
			#else
			foreach(var type in GetAllTypesThreadSafe(typeof(T).Assembly, publicOnly))
			{
				if(typeof(T).IsAssignableFrom(type))
				{
					results.Add(type);
				}
			}
			#endif
		}

		#if UNITY_EDITOR
		internal static UnityEditor.TypeCache.TypeCollection
		#else
		[return: NotNull]
		internal static List<Type>
		#endif
		GetDerivedTypes([DisallowNull] Type inheritedType, [DisallowNull] Assembly mustReferenceAssembly, bool publicOnly, int estimatedResultCount)
		{
			#if UNITY_EDITOR
			return UnityEditor.TypeCache.GetTypesDerivedFrom(inheritedType);
			#else
			var results = new List<Type>(estimatedResultCount);

			foreach(var type in GetAllTypesThreadSafe(mustReferenceAssembly, publicOnly))
			{
				if(inheritedType.IsAssignableFrom(type))
				{
					results.Add(type);
				}
			}

			return results;
			#endif
		}

		[return: NotNull]
		internal static List<Type> GetOpenGenericTypeDerivedTypes([DisallowNull] Type openGenericType, bool publicOnly, bool concreteOnly, int estimatedResultCount)
		{
			if(openGenericType.IsSealed)
			{
				return new(0);
			}

			var results = new List<Type>(estimatedResultCount);

			if(openGenericType.IsInterface)
			{
				foreach(var assembly in GetAllAssembliesThreadSafe(openGenericType.Assembly))
				{
					foreach(var type in assembly.GetLoadableTypes(publicOnly))
					{
						if(type.IsAbstract && concreteOnly)
						{
							continue;
						}

						var interfaces = type.GetInterfaces();
						foreach(var interfaceType in interfaces)
						{
							if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == openGenericType)
							{
								results.Add(type);
							}
						}
					}
				}

				return results;
			}

			foreach(var assembly in GetAllAssembliesThreadSafe(openGenericType.Assembly))
			{
				foreach(var type in assembly.GetLoadableTypes(publicOnly))
				{
					if(type.IsAbstract && concreteOnly)
					{
						continue;
					}

					for(var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
					{
						if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == openGenericType)
						{
							results.Add(type);
						}
					}
				}
			}

			return results;
		}

		[return: NotNull]
		internal static List<Type> GetImplementingTypes<TInterface>([DisallowNull] Assembly mustReferenceAssembly, bool publicOnly, int estimatedResultCount) where TInterface : class
		{
			#if DEV_MODE
			Debug.Assert(typeof(TInterface).IsInterface);
			#endif

			var results = new List<Type>(estimatedResultCount);

			#if UNITY_EDITOR
			foreach(var type in UnityEditor.TypeCache.GetTypesDerivedFrom<TInterface>())
			{
			#else
			Type interfaceType = typeof(TInterface);
			foreach(var type in GetAllTypesThreadSafe(mustReferenceAssembly, publicOnly))
			{
				if(!interfaceType.IsAssignableFrom(type))
				{
					continue;
				}
			#endif

				if(!type.IsInterface)
				{
					results.Add(type);
				}
			}

			return results;
		}

		[return: NotNull]
		internal static List<Type> GetImplementingTypes([DisallowNull] Type interfaceType, [AllowNull] Assembly mustReferenceAssembly, bool publicOnly, int estimatedResultCount)
		{
			#if DEV_MODE
			Debug.Assert(interfaceType.IsInterface);
			#endif

			var results = new List<Type>(estimatedResultCount);

			#if UNITY_EDITOR
			foreach(var type in UnityEditor.TypeCache.GetTypesDerivedFrom(interfaceType))
			{
			#else
			foreach(var type in GetAllTypesThreadSafe(mustReferenceAssembly, publicOnly))
			{
				if(!interfaceType.IsAssignableFrom(type))
				{
					continue;
				}
			#endif

				if(!type.IsInterface)
				{
					results.Add(type);
				}
			}

			return results;
		}

		/// <summary>
		/// NOTE: Even if this looks unused, it can be used in builds.
		/// </summary>
		[return: NotNull]
		internal static List<Type> GetAllTypesThreadSafe([AllowNull] Assembly mustReferenceAssembly, bool publicOnly)
		{
			var results = new List<Type>(2048);

			foreach(var assembly in GetAllAssembliesThreadSafe(mustReferenceAssembly))
			{
				results.AddRange(assembly.GetLoadableTypes(publicOnly));
			}

			return results;
		}

		/// <summary>
		/// NOTE: Even if this looks unused, it can be used in builds.
		/// </summary>
		[return: NotNull]
		internal static List<Type> GetAllTypesVisibleTo([AllowNull] Type visibleTo, Func<Type, bool> filter, int estimatedResultCount)
		{
			var results = new List<Type>(estimatedResultCount);
			var assemblyContainingType = visibleTo.Assembly;
			Func<Type, bool> isNotUnrelatedNestedFamilyType = type => !IsUnrelatedNestedFamilyType(type, visibleTo);

			results.AddRange(assemblyContainingType.GetLoadableTypes(false)
				.Where(isNotUnrelatedNestedFamilyType)
				.Where(filter));

			foreach(var visibleAssembly in GetAllAssembliesVisibleToThreadSafe(visibleTo.Assembly))
			{
				if(visibleAssembly != assemblyContainingType)
				{
					results.AddRange(visibleAssembly.GetLoadableTypes(true).Where(filter));
				}
			}

			return results;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static bool IsUnrelatedNestedFamilyType(Type type, Type visibleTo)
			{
				// Nested private, protected, and private protected classes are only visible to their parent type, and types that share the same parent

				if(type is { IsNested : false } or { IsNestedPublic : true } or { IsNestedAssembly: true })
				{
					return false;
				}

				if(type is { IsNestedFamily : false })
				{
					return true;
				}

				var parentType = type.DeclaringType;
				return type.DeclaringType == visibleTo || parentType == visibleTo.DeclaringType;
			}
		}

		public static IEnumerable<Assembly> GetAllAssembliesVisibleToThreadSafe([AllowNull] Assembly visibleTo)
		{
			var referencedAssemblyNames = visibleTo.GetReferencedAssemblies().Select(name => name.Name).ToArray();
			int referencedAssembliesCount = referencedAssemblyNames.Length;
			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			for(int n = allAssemblies.Length - 1; n >= 0; n--)
			{
				var assembly = allAssemblies[n];

				// skip dynamic assemblies to prevent NotSupportedException from being thrown when calling GetTypes
				if(assembly.IsDynamic)
				{
					continue;
				}

				var assemblyName = assembly.GetName().Name;
				for(int r = referencedAssembliesCount - 1; r >= 0; r--)
				{
					if(string.Equals(referencedAssemblyNames[r], assemblyName))
					{
						yield return assembly;
						break;
					}
				}
			}
		}

		/// <summary>
		/// NOTE: Even if this looks unused, it can be used in builds.
		/// </summary>
		public static IEnumerable<Assembly> GetAllAssembliesThreadSafe([AllowNull] Assembly mustReferenceAssembly)
		{
			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			string mustReferenceName = mustReferenceAssembly?.GetName().Name;

			for(int n = allAssemblies.Length - 1; n >= 0; n--)
			{
				var assembly = allAssemblies[n];

				// skip dynamic assemblies to prevent NotSupportedException from being thrown when calling GetTypes
				if(assembly.IsDynamic)
				{
					continue;
				}

				if(mustReferenceAssembly is null || assembly == mustReferenceAssembly)
				{
					yield return assembly;
					continue;
				}

				var referencedAssemblies = assembly.GetReferencedAssemblies();
				for(int r = referencedAssemblies.Length - 1; r >= 0; r--)
				{
					if(string.Equals(referencedAssemblies[r].Name, mustReferenceName))
					{
						yield return assembly;
						break;
					}
				}
			}
		}

		public static string ToStringNicified([DisallowNull] Type type)
		{
			#if UNITY_EDITOR
			if(!type.IsGenericType && !type.IsInterface)
			{
				return UnityEditor.ObjectNames.NicifyVariableName(ToString(type));
			}
			#endif
			
			return ToString(type);
		}

		public static string ToString([AllowNull] Type type, char namespaceDelimiter = '\0')
		{
			return type is null ? "Null" : ToString(type, namespaceDelimiter, toStringCache);
		}

		internal static string ToString([AllowNull] IEnumerable<Type> type, string separator = ", ", char namespaceDelimiter = '\0') => string.Join(separator, type.Select(t => ToString(t, namespaceDelimiter)));

		internal static string ToString([DisallowNull] Type type, char namespaceDelimiter, Dictionary<char, Dictionary<Type, string>> cache)
		{
			if(cache[namespaceDelimiter].TryGetValue(type, out string cached))
			{
				return cached;
			}

			var builder = new StringBuilder();
			ToString(type, builder, namespaceDelimiter, cache);
			string result = builder.ToString();
			cache[namespaceDelimiter][type] = result;
			return result;
		}

		internal static bool IsSerializableByUnity([DisallowNull] Type type)
		{
			if(!isSerializableByUnityCache.TryGetValue(type, out bool result))
			{
				result = type.IsSerializable || typeof(Object).IsAssignableFrom(type) || (type.Namespace is string namespaceName && namespaceName.Contains("Unity"));
				isSerializableByUnityCache.Add(type, result);
			}

			return result;
		}

		private static void ToString([DisallowNull] Type type, [DisallowNull] StringBuilder builder, char namespaceDelimiter, Dictionary<char, Dictionary<Type, string>> cache, Type[] genericTypeArguments = null)
		{
			// E.g. List<T> generic parameter is T, in which case we just want to append "T".
			if(type.IsGenericParameter)
			{
				builder.Append(type.Name);
				return;
			}

			if(type.IsArray)
			{
				builder.Append(ToString(type.GetElementType(), namespaceDelimiter, cache));
				int rank = type.GetArrayRank();
				switch(rank)
				{
					case 1:
						builder.Append("[]");
						break;
					case 2:
						builder.Append("[,]");
						break;
					case 3:
						builder.Append("[,,]");
						break;
					default:
						builder.Append('[');
						for(int n = 1; n < rank; n++)
						{
							builder.Append(',');
						}
						builder.Append(']');
						break;
				}
				return;
			}

			if(namespaceDelimiter != '\0' && type.Namespace != null)
			{
				var namespacePart = type.Namespace;
				if(namespaceDelimiter != '.')
				{
					namespacePart = namespacePart.Replace('.', namespaceDelimiter);
				}

				builder.Append(namespacePart);
				builder.Append(namespaceDelimiter);
			}

			#if CSHARP_7_3_OR_NEWER
			// You can create instances of a constructed generic type.
			// E.g. Dictionary<int, string> instead of Dictionary<TKey, TValue>.
			if(type.IsConstructedGenericType)
			{
				genericTypeArguments = type.GenericTypeArguments;
			}
			#endif

			// If this is a nested class type then append containing type(s) before continuing.
			var containingClassType = type.DeclaringType;
			if(containingClassType != null && type != containingClassType)
			{
				// GenericTypeArguments can't be fetched from the containing class type
				// so need to pass them along to the ToString method and use them instead of
				// the results of GetGenericArguments.
				ToString(containingClassType, builder, '\0', cache, genericTypeArguments);
				builder.Append('.');
			}
			
			if(!type.IsGenericType)
			{
				builder.Append(type.Name);
				return;
			}

			var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
			if(nullableUnderlyingType != null)
			{
				// "Int?" instead of "Nullable<Int>"
				builder.Append(ToString(nullableUnderlyingType, '\0', cache));
				builder.Append("?");
				return;
			}
			
			var name = type.Name;

			// If type name doesn't end with `1, `2 etc. then it's not a generic class type
			// but type of a non-generic class nested inside a generic class.
			if(name[name.Length - 2] == '`')
			{
				// E.g. TKey, TValue
				var genericTypes = type.GetGenericArguments();

				builder.Append(name.Substring(0, name.Length - 2));
				builder.Append('<');

				// Prefer using results of GenericTypeArguments over results of GetGenericArguments if available.
				int genericTypeArgumentsLength = genericTypeArguments != null ? genericTypeArguments.Length : 0;
				if(genericTypeArgumentsLength > 0)
				{
					builder.Append(ToString(genericTypeArguments[0], '\0', cache));
				}
				else
				{
					builder.Append(ToString(genericTypes[0], '\0', cache));
				}

				for(int n = 1, count = genericTypes.Length; n < count; n++)
				{
					builder.Append(", ");

					if(genericTypeArgumentsLength > n)
					{
						builder.Append(ToString(genericTypeArguments[n], '\0', cache));
					}
					else
					{
						builder.Append(ToString(genericTypes[n], '\0', cache));
					}
				}

				builder.Append('>');
			}
			else
			{
				builder.Append(name);
			}
		}

		public static bool IsNullOrBaseType([AllowNull, NotNullWhen(false), MaybeNullWhen(true)] Type type) => type is null || IsBaseType(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBaseType([DisallowNull] Type type) => type.IsGenericType ? IsGenericBaseType(type) : baseTypes.Contains(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// simply using namespace comparison instead of checking if the type is found in genericBaseTypes,
		// so that base types found inside optional add-on packages will also be detected properly
		// (e.g. SerializedMonoBehaviour<T...> and SerializedScriptableObject<T...>).
		public static bool IsGenericBaseType([DisallowNull] Type type) => type.IsGenericType && type.Namespace == typeof(MonoBehaviour<object>).Namespace;

		public static bool DerivesFromGenericBaseType([DisallowNull] Type type)
		{
			while((type = type.BaseType) != null)
			{
				if(IsGenericBaseType(type))
				{
					return true;
				}
			}

			return false;
		}

		public static bool TryGetGenericBaseType([DisallowNull] Type type, out Type baseType)
		{
			while((type = type.BaseType) != null)
			{
				if(IsGenericBaseType(type))
				{
					baseType = type;
					return true;
				}
			}

			baseType = null;
			return false;
		}

		public static TCollection ConvertToCollection<TCollection, TElement>(TElement[] source)
		{
			if(source is TCollection result)
			{
				return result;
			}

			if(typeof(TCollection).IsArray)
			{
				var elementType = typeof(TCollection).GetElementType();
				if(elementType == typeof(TElement))
				{
					return (TCollection)(object)source;
				}

				int count = source.Length;
				var array = Array.CreateInstance(elementType, count);
				Array.Copy(source, array, count);
				return (TCollection)(object)array;
			}

			if(typeof(TCollection).IsAbstract)
			{
				if(typeof(TCollection).IsGenericType)
				{
					var typeDefinition = typeof(TCollection).GetGenericTypeDefinition();
					if(typeDefinition == typeof(IEnumerable<>) || typeDefinition == typeof(IReadOnlyList<>) || typeDefinition == typeof(IList<>))
					{
						int count = source.Length;
						var elementType = GetCollectionElementType(typeof(TCollection));
						var array = Array.CreateInstance(elementType, count);
						Array.Copy(source, array, count);
						return (TCollection)(object)array;
					}
				}
			}

			try
			{
				return To<TCollection>.Convert(source);
			}
			catch
			{
				if(!typeof(IEnumerable).IsAssignableFrom(typeof(TCollection)))
				{
					throw new InvalidCastException($"Unable to convert from {ToString(typeof(TElement))}[] to {ToString(typeof(TCollection))}.\n{ToString(typeof(TCollection))} does not seem to be a collection type.");
				}

				throw new InvalidCastException($"Unable to convert from {ToString(typeof(TElement))}[] to {ToString(typeof(TCollection))}.\n{ToString(typeof(TCollection))} must have a public constructor with an IEnumerable<{typeof(TElement).Name}> parameter.");
			}
		}

		public static Type GetCollectionElementType(Type collectionType)
		{
			if(collectionType.IsArray)
			{
				return collectionType.GetElementType();
			}

			if(collectionType.IsGenericType)
			{
				Type typeDefinition = collectionType.GetGenericTypeDefinition();
				if(typeDefinition == typeof(List<>)
				|| typeDefinition == typeof(IEnumerable<>)
				|| typeDefinition == typeof(IList<>)
				|| typeDefinition == typeof(ICollection<>)
				|| typeDefinition == typeof(IReadOnlyCollection<>)
				|| typeDefinition == typeof(IReadOnlyList<>)
				|| typeDefinition == typeof(HashSet<>)
				|| typeDefinition == typeof(Queue<>)
				|| typeDefinition == typeof(Stack<>)
				|| typeDefinition == typeof(NativeArray<>)
				|| typeDefinition == typeof(ReadOnlyCollection<>)
				)
				{
					return collectionType.GetGenericArguments()[0];
				}

				if(typeDefinition == typeof(Dictionary<,>) || typeDefinition == typeof(IDictionary<,>))
				{
					return typeof(KeyValuePair<,>).MakeGenericType(collectionType.GetGenericArguments());
				}
			}
			else if(collectionType == typeof(IEnumerable) || collectionType == typeof(IList) || collectionType == typeof(ICollection))
			{
				return typeof(object);
			}

			foreach(var interfaceType in collectionType.GetInterfaces())
			{
				if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
			}

			return null;
		}

		public static bool IsCommonCollectionType(Type collectionType)
		{
			if(collectionType.IsArray)
			{
				return true;
			}

			if(!collectionType.IsGenericType)
			{
				return collectionType == typeof(IEnumerable) || collectionType == typeof(IList) || collectionType == typeof(ICollection);
			}

			Type typeDefinition = collectionType.GetGenericTypeDefinition();

			return typeDefinition == typeof(List<>)
				|| typeDefinition == typeof(IEnumerable<>)
				|| typeDefinition == typeof(IList<>)
				|| typeDefinition == typeof(IReadOnlyList<>)
				|| typeDefinition == typeof(ICollection<>)
				|| typeDefinition == typeof(IReadOnlyCollection<>)
				|| typeDefinition == typeof(HashSet<>)
				|| typeDefinition == typeof(Queue<>)
				|| typeDefinition == typeof(Stack<>)
				|| typeDefinition == typeof(NativeArray<>)
				|| typeDefinition == typeof(ReadOnlyCollection<>)
				|| typeDefinition == typeof(Dictionary<,>)
				|| typeDefinition == typeof(IDictionary<,>);
		}
		
		[return: NotNull]
		internal static Type[] GetLoadableTypes([DisallowNull] this Assembly assembly, bool publicOnly)
		{
			try
			{
				Type[] result;
				if(publicOnly)
				{
					result = assembly.GetExportedTypes();
				}
				else
				{
					result = assembly.GetTypes();
				}

				return result;
			}
			catch(NotSupportedException) //thrown if GetExportedTypes is called for a dynamic assembly
			{
				#if DEV_MODE
				Debug.LogWarning(assembly.GetName().Name + ".GetLoadableTypes() NotSupportedException\n" + assembly.FullName);
				#endif
				return Type.EmptyTypes;
			}
			catch(ReflectionTypeLoadException e)
			{
				var TypesList = new List<Type>(100);

				var exceptionTypes = e.Types;
				int count = exceptionTypes.Length;
				for(int n = count - 1; n >= 0; n--)
				{
					var type = exceptionTypes[n];
					if(type != null)
					{
						TypesList.Add(type);
					}
				}

				#if DEV_MODE
				Debug.LogWarning(assembly.GetName().Name + ".GetLoadableTypes() ReflectionTypeLoadException, salvaged: " + TypesList.Count + "\n" + assembly.FullName);
				#endif

				return TypesList.ToArray();
			}
			#if DEV_MODE
			catch(Exception e)
			{
				Debug.LogWarning(assembly.GetName().Name + ".GetLoadableTypes() " + e + "\n" + assembly.FullName);
			#else
			catch(Exception)
			{
			#endif
				return Type.EmptyTypes;
			}
		}

		private static class To<TCollection>
		{
			private static readonly ConstructorInfo constructor;
			private static readonly object[] arguments = new object[1];
			
			static To()
			{
				var argumentType = GetCollectionElementType(typeof(TCollection));
				var parameterTypeGeneric = typeof(IEnumerable<>);
				var parameterType = parameterTypeGeneric.MakeGenericType(argumentType);
				var collectionType = !typeof(TCollection).IsAbstract ? typeof(TCollection): typeof(List<>).MakeGenericType(argumentType);
				constructor = collectionType.GetConstructor(new Type[] { parameterType });
			}

			public static TCollection Convert(object sourceArray)
			{
				arguments[0] = sourceArray;
				return (TCollection)constructor.Invoke(arguments);
			}
		}

		public static BaseTypeIterator GetBaseTypes([DisallowNull] this Type type) => new(type.BaseType);

		public struct BaseTypeIterator
		{
			private Type current;
			private bool isFirst;

			public BaseTypeIterator GetEnumerator() => this;

			internal BaseTypeIterator([AllowNull] Type type)
			{
				current = type;
				isFirst = true;
			}

			public bool MoveNext()
			{
				if(isFirst)
				{
					isFirst = false;
				}
				else
				{
					current = current.BaseType;
				}

				return !TypeUtility.IsNullOrBaseType(current);
			}

			public Type Current => current;
		}
	}
}