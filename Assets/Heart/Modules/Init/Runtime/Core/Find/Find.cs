//#define DEBUG_FIND_SCRIPT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Object;
using Object = UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

#if UNITY_EDITOR && UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEditor.AddressableAssets;
#endif

namespace Sisus.Init
{
	using static ConversionExtensions;

	/// <summary>
	/// Utility class for finding instances in the scene.
	/// <para>
	/// Supports finding components by their type, any derived type
	/// as well as the type of any interface they implement.
	/// </para>
	/// <para>
	/// Also supports finding <see cref="Init.Wrapper{TWrapped}">wrapped objects</see>
	/// in the scene.
	/// </para>
	/// <para>
	/// Also supports finding components from inactive GameObjects.
	/// </para>
	/// </summary>
	public static class Find
	{
		internal static readonly Dictionary<object, IWrapper> wrappedInstances = new(128);
		internal static readonly Dictionary<Type, Type[]> typesToWrapperTypes = new();
		internal static readonly Dictionary<Type, Type[]> typesToFindableTypes = new(64);
		internal static readonly Dictionary<Type, Type[]> typesToComponentTypes = new(64);

		static Find() => Setup();

		/// <summary>
		/// Returns the first loaded object of type <typeparamref name="T">type</typeparamref>.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns>
		/// Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T Any<T>(bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return default;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				Object found = ObjectByExactType(findableTypes[i], includeInactive);
				if(found)
				{
					return As<T>(found);
				}
			}

			return default;
		}

		/// <summary>
		/// Returns the first loaded object of type <typeparamref name="T">type</typeparamref>.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="result">
		/// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns> <see langword="true"/> if an object was found; otherwise, <see langword="false"/>. </returns>
		public static bool Any<T>([NotNullWhen(true), MaybeNullWhen(false)] out T result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				if(ObjectByExactType(findableTypes[i], includeInactive) is T obj)
				{
					result = obj;
					return true;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Returns the first loaded object of type <typeparamref name="T">type</typeparamref>.
		/// </summary>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="result">
		/// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns> <see langword="true"/> if an object was found; otherwise, <see langword="false"/>. </returns>
		public static bool Any([DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				if(ObjectByExactType(findableTypes[i], includeInactive) is Object unityObject)
				{
					result = As(type, unityObject);
					return result is not null;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Returns the first loaded object of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> Loaded <see cref="Init.Any{T}"/> instance, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static object Any([DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return null;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];

				if(ObjectByExactType(findableType, includeInactive) is Object unityObject)
				{
					return As(type, unityObject);
				}
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="match"> The Predicate<T> delegate that defines the conditions of the Object to search for. </param>
		/// Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		[return: MaybeNull]
		public static T Any<T>([DisallowNull] Predicate<T> match, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return default;
			}

			foreach(var obj in ObjectsByExactTypesIterator(findableTypes, includeInactive))
			{
				if(obj is T result && match(result))
				{
					return result;
				}
			}

			return default;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="match"> The Predicate<T> delegate that defines the conditions of the Object to search for. </param>
		/// <param name="result">
		/// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if a result was found; otherwise, <see langword="false"/>. </returns>
		public static bool Any<T>([DisallowNull] Predicate<T> match, [NotNullWhen(true), MaybeNullWhen(false)] out T result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				result = default;
				return false;
			}

			foreach(var obj in ObjectsByExactTypesIterator(findableTypes, includeInactive))
			{
				if(obj is T potentialResult && match(potentialResult))
				{
					result = potentialResult;
					return true;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Returns all loaded <see cref="Init.Any{T}">Objects</see> of the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
		[return: NotNull]
		public static object[] All([DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return Array.Empty<object>();
			}

			var list = Cached<object>.list;

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				ObjectsByExactType(findableTypes[i], list, includeInactive);
			}

			var results = ToArrayAndClear(list);

			if(typeof(Object).IsAssignableFrom(type))
			{
				return results;
			}

			for(int i = results.Length - 1; i >= 0; i--)
			{
				if(!type.IsInstanceOfType(results[i]))
				{
					results[i] = (results[i] as IWrapper).WrappedObject;
				}
			}

			return results;
		}

		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static T[] All<T>([DisallowNull] Predicate<T> match, bool includeInactive = false)
		{
			var list = Cached<T>.list;
			All(match, list, includeInactive);
			return ToArrayAndClear(list);
		}

		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="results"> List into which found instances are added. </param>
		public static void All<T>([DisallowNull] Predicate<T> match, [DisallowNull] List<T> results, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return;
			}

			foreach(var obj in ObjectsByExactTypesIterator(findableTypes, includeInactive))
			{
				if(obj is T result && match(result))
				{
					results.Add(result);
				}
			}
		}

		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> An array containing all found instances; if no results were found, then an empty array. </returns>
		[return: NotNull]
		public static T[] All<T>(bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return Array.Empty<T>();
			}

			var list = Cached<T>.list;
			ObjectsByExactTypes(findableTypes, list, includeInactive);
			return ToArrayAndClear(list);
		}

		/// <summary>
		/// Returns all loaded objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
		/// in which case it will try to find all wrappers for objects of that type from the loaded scenes.
		/// </typeparam>
		/// <param name="results"> List into which found instances are added. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		public static void All<T>([DisallowNull] List<T> results, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return;
			}

			ObjectsByExactTypes(findableTypes, results, includeInactive);
		}

		/// <summary>
		/// Returns all loaded objects of the given <paramref name="type"/>.
		/// </summary>
		/// <typeparam name="T">
		/// Generic type of the <see cref="List{T}"/> into which found results will be placed.
		/// </typeparam>
		/// <param name="type">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> will be included in the results.
		/// </para>
		/// </param>
		/// <param name="results"> List into which found instances are added. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		public static void All<T>([DisallowNull] Type type, [DisallowNull] List<T> results, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return;
			}

			ObjectsByExactTypes(findableTypes, results, includeInactive);
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>
		/// and all of its parent GameObjects, including grandparents.
		/// <para>
		/// The <see cref="GameObject"/> and its parents are searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool InParents<T>([DisallowNull] GameObject gameObject, [NotNullWhen(true), MaybeNullWhen(false)] out T result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				result = InParentsByExactType<T>(gameObject, findableTypes[i], includeInactive);
				if(result != null)
				{
					return true;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>
		/// and all of its parent GameObjects, including grandparents.
		/// <para>
		/// The <see cref="GameObject"/> and its parents are searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T"> Type of the object to find. <para>
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T InParents<T>([DisallowNull] GameObject gameObject, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return default;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				T result = InParentsByExactType<T>(gameObject, findableTypes[i], includeInactive);
				if(result != null)
				{
					return result;
				}
			}

			return default;
		}

		/// <summary>
		/// Returns all objects of the given <paramref name="type"/> attached the <see cref="GameObject"/> or any of its parents.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// This can be the exact type of an <see cref="Init.Any{T}"/>, any type derived by it
		/// or the type of any interface it implements.
		/// </para>
		/// <para>
		/// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
		/// in which case it will try to find a wrapper for an object of that type in the loaded scenes.
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> <see langword="true"/> if a result was found; otherwise, <see langword="false"/>. </returns>
		public static bool InParents([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				if(InParentsByExactType<Component>(gameObject, findableTypes[i], includeInactive) is Component component)
				{
					result = As(type, component);
					return result is not null;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Returns all objects of the given <paramref name="type"/> attached the <see cref="GameObject"/> or any of its parents.
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// This can be the exact type of an <see cref="Init.Any{T}"/>, any type derived by it
		/// or the type of any interface it implements.
		/// </para>
		/// <para>
		/// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
		/// in which case it will try to find a wrapper for an object of that type in the loaded scenes.
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> An object of the given type, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static object InParents([DisallowNull] GameObject gameObject, [DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return null;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				if(InParentsByExactType<Component>(gameObject, findableTypes[i], includeInactive) is Component component)
				{
					return As(type, component);
				}
			}

			return null;
		}

		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> will be included in the results.
		/// </para>
		/// </typeparam>
		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static T[] AllInParents<T>([DisallowNull] GameObject gameObject, bool includeInactive = false)
		{
			var list = Cached<T>.list;
			AllInParents(gameObject, list, includeInactive);
			return ToArrayAndClear(list);
		}

		/// <summary>
		/// Returns all components of the given <paramref name="type"/> attached to the <paramref name="gameObject"/>
		/// or any <see cref="GameObject"/> in its <see cref="Transform.parent">parent</see> chain.
		/// </summary>
		/// <param name="type">
		/// Type of the objects to find.
		/// <para>
		/// This can be the exact type of an object, any type derived by it
		/// or the type of any interface it implements.
		/// </para>
		/// <para>
		/// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
		/// in which case it will try to find all wrappers for objects of that type from the loaded scenes.
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static object[] AllInParents([DisallowNull] GameObject gameObject, [DisallowNull] Type type, bool includeInactive = false)
		{
			var list = Cached<object>.list;
			AllInParents(gameObject, type, list, includeInactive);
			return ToArrayAndClear(list);
		}

		/// <summary>
		/// Returns all components of the given <paramref name="type"/> attached to the <paramref name="gameObject"/>
		/// or any <see cref="GameObject"/> in its <see cref="Transform.parent">parent</see> chain.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> will be included in the results.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
		/// <param name="results"> List into which found instances are added. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		public static void AllInParents<T>([DisallowNull] GameObject gameObject, [DisallowNull] List<T> results, bool includeInactive = false)
		{
			AllInParents(gameObject, typeof(T), results, includeInactive);
		}

		/// <summary>
		/// Returns all components of the given <paramref name="type"/> attached to the <paramref name="gameObject"/>
		/// or any <see cref="GameObject"/> in its <see cref="Transform.parent">parent</see> chain.
		/// </summary>
		/// <typeparam name="T">
		/// Generic type of the <see cref="List{T}"/> into which found results will be placed.
		/// </typeparam>
		/// <param name="type">
		/// Type of the objects to find.
		/// <para>
		/// This can be the exact type of an object, any type derived by it
		/// or the type of any interface it implements.
		/// </para>
		/// <para>
		/// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
		/// in which case it will try to find all wrappers for objects of that type from the loaded scenes.
		/// </param>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
		/// <param name="results"> List into which found instances are added. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		public static void AllInParents<T>([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [DisallowNull] List<T> results, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return;
			}

			if(findableTypes.Length == 1 && type == findableTypes[0])
			{
				gameObject.GetComponentsInParent(includeInactive, results);
				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var components = gameObject.GetComponentsInParent(findableTypes[i], includeInactive);
				TryAddAs(results, components);
			}
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>
		/// and all of its child GameObjects, including grandchildren.
		/// <para>
		/// The <see cref="GameObject"/> and its children is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T"> Type of the object to find. <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its children. </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T InChildren<T>([DisallowNull] GameObject gameObject, bool includeInactive = false)
		{
			var type = typeof(T);
			if(typeof(Component).IsAssignableFrom(type))
			{
				return gameObject.GetComponentInChildren<T>(includeInactive);
			}

			if(!typesToComponentTypes.TryGetValue(type, out var componentTypes))
			{
				return default;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				var result = InChildrenByExactType<T>(gameObject, componentTypes[i], includeInactive);
				if(result != null)
				{
					return result;
				}
			}

			return default;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>
		/// and all of its child GameObjects, including grandchildren.
		/// <para>
		/// The <see cref="GameObject"/> and its children is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its children. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool InChildren<T>([DisallowNull] GameObject gameObject, [NotNullWhen(true), MaybeNullWhen(false)] out T result, bool includeInactive = false)
		{
			var type = typeof(T);

			if(!typesToComponentTypes.TryGetValue(type, out var componentTypes))
			{
				result = default;
				return false;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				result = InChildrenByExactType<T>(gameObject, componentTypes[i], includeInactive);
				if(result != null)
				{
					return true;
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of the given <paramref name="type"/> by searching the <paramref name="gameObject"/>
		/// and all of its child GameObjects, including grandchildren.
		/// <para>
		/// The <see cref="GameObject"/> and its children are searched for a <see cref="Component"/>
		/// which is of the given type, derives from the given type, or implements an interface of the given type.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its children. </param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// </param>
		/// <param name="includeInactive"> Should components on
		/// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
		/// be included in the search?
		/// </param>
		/// <returns>
		/// <returns> object of the given <paramref name="type"/>, if found; otherwise, <see langword="null"/>. </returns>
		/// </returns>
		[return: MaybeNull]
		public static object InChildren([DisallowNull] GameObject gameObject, [DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return default;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				var component = gameObject.GetComponentInChildren(findableTypes[i], includeInactive);
				if(component)
				{
					return As(type, component);
				}
			}

			return default;
		}

		public static bool InChildren([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				var component = gameObject.GetComponentInChildren(findableTypes[i], includeInactive);
				if(component)
				{
					result = As(type, component);
					return true;
				}
			}

			result = default;
			return false;
		}

		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static T[] AllInChildren<T>([DisallowNull] GameObject gameObject, bool includeInactive = false)
		{
			var list = Cached<T>.list;
			AllInChildren(gameObject, list, includeInactive);
			return ToArrayAndClear(list);
		}

		/// <param name="results"> List into which found instances are added. </param>
		public static void AllInChildren<T>([DisallowNull] GameObject gameObject, [DisallowNull] List<T> results, bool includeInactive = false)
		{
			var type = typeof(T);
			if(typeof(Component).IsAssignableFrom(type))
			{
				gameObject.GetComponentsInChildren(includeInactive, results);
				return;
			}

			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				var components = gameObject.GetComponentsInChildren(findableTypes[i], includeInactive);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(results, components[c]);
				}
			}
		}

			/// <param name="results"> List into which found instances are added. </param>
		public static void AllInChildren([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [DisallowNull] List<object> results, bool includeInactive = false)
		{
			if(typeof(Component).IsAssignableFrom(type))
			{
				gameObject.GetComponentsInChildren(includeInactive, results);
				return;
			}

			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				var components = gameObject.GetComponentsInChildren(findableTypes[i], includeInactive);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(results, components[c]);
				}
			}
		}

		/// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
		[return: NotNull]
		public static object[] AllInChildren([DisallowNull] GameObject gameObject, [DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return Array.Empty<Component>();
			}

			int count = findableTypes.Length;
			if(count == 1)
			{
				var findableType = findableTypes[0];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					return Array.Empty<Component>();
				}

				return gameObject.GetComponentsInChildren(findableType, includeInactive);
			}

			var list = Cached<Component>.list;

			for(int i = count - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				TryAddAs(list, gameObject.GetComponentsInChildren(findableTypes[i], includeInactive));
			}

			return ToArrayAndClear(list);
		}

		/// <summary>
		/// Returns the first loaded <see cref="GameObject"/> with the provided <paramref name="tag"/>, if any.
		/// </summary>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="includeInactive">
		/// Should <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObject">GameObjects</see> be included in the search?
		/// <para>
		/// Note that searching for inactive GameObjects is a slow operation.
		/// </para>
		/// </param>
		/// <returns> <see cref="GameObject"/> with the provided tag, if found; otherwise, <see langword="null"/>. </returns>
		public static GameObject WithTag(string tag, bool includeInactive = false)
		{
			GameObject result = GameObject.FindWithTag(tag);
			
			if(result || !includeInactive)
			{
				return result;
			}

			return WithTagFromLoadedScenesIncludingInactive(tag);
		}

		/// <summary>
		/// Returns the first loaded <see cref="GameObject"/> with the provided <paramref name="tag"/>, if any.
		/// </summary>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="result">
		/// When this method returns, contains <see cref="GameObject"/> with the provided tag, if found;
		/// otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive">
		/// Should <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObject">GameObjects</see> be included in the search?
		/// <para>
		/// Note that searching for inactive GameObjects is a slow operation.
		/// </para>
		/// </param>
		/// <returns> <see langword="true"/> if a <see cref="GameObject"/> was found; otherwise, <see langword="false"/>. </returns>
		public static bool WithTag([DisallowNull] string tag, [NotNullWhen(true), MaybeNullWhen(false)] out GameObject result, bool includeInactive = false)
		{
			result = GameObject.FindWithTag(tag);

			if(result)
			{
				return true;
			}

			if(!includeInactive)
			{
				return false;
			}

			result = WithTagFromLoadedScenesIncludingInactive(tag);
			return result;
		}

		/// <summary>
		/// Returns object of type <typeparamref name="T"/> from the first loaded <see cref="GameObject"/>
		/// with the provided <paramref name="tag"/>, if any.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="includeInactive">
		/// Should <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObject">GameObjects</see> be included in the search?
		/// <para>
		/// Note that searching for inactive GameObjects is a slow operation.
		/// </para>
		/// </param>
		/// <returns> <see cref="GameObject"/> with the provided tag, if found; otherwise, <see langword="null"/>. </returns>
		public static T WithTag<T>(string tag, bool includeInactive = false)
		{
			GameObject gameObject = WithTag(tag, includeInactive);
			return gameObject ? In<T>(gameObject) : default;
		}

		/// <summary>
		/// Returns object of type <typeparamref name="T"/> from the first loaded <see cref="GameObject"/>
		/// with the provided <paramref name="tag"/>, if any.
		/// </summary>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="includeInactive">
		/// Should <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObject">GameObjects</see> be included in the search?
		/// <para>
		/// Note that searching for inactive GameObjects is a slow operation.
		/// </para>
		/// </param>
		/// <returns> <see cref="GameObject"/> with the provided tag, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static object WithTag([DisallowNull] string tag, [DisallowNull] Type type, bool includeInactive = false)
		{
			GameObject gameObject = WithTag(tag, includeInactive);
			return gameObject ? In(gameObject, type) : default;
		}

		/// <summary>
		/// Returns all loaded <see cref="GameObject">GameObjects</see> with the provided <paramref name="tag"/>.
		/// </summary>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="includeInactive">
		/// Should <see cref="GameObject.activeInHierarchy">inactive</see>
		/// <see cref="GameObject">GameObjects</see> be included in the search?
		/// <para>
		/// Note that searching for inactive <see cref="GameObject">GameObjects</see> is a slow operation.
		/// </para>
		/// </param>
		/// <returns> Array of zero or more <see cref="GameObject">GameObjects</see>. </returns>
		[return: NotNull]
		public static GameObject[] AllWithTag(string tag, bool includeInactive = false)
		{
			if(!includeInactive)
			{
				return GameObject.FindGameObjectsWithTag(tag);
			}

			var results = new List<GameObject>();
			AllWithTagFromLoadedScenesIncludingInactive(tag, results);
			return results.ToArray();
		}

		/// <summary>
		/// Returns all loaded Components that are of type <typeparamref name="T"/>
		/// or implement an interface of type <typeparamref name="T"/>
		/// or <see cref="IWrapper">wrap</see> an object of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the components to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="results"> List into which found components are added. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		public static void Components<T>([DisallowNull] List<T> results, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				#if UNITY_2023_1_OR_NEWER
				TryAddAs(results, FindObjectsByType(findableType, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None));
				#else
				TryAddAs(results, FindObjectsOfType(findableType, includeInactive));
				#endif
			}
		}

		/// <summary>
		/// Returns GameObject containing the first loaded Component that is of type <typeparamref name="T"/> or
		/// implements an interface of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
		/// <returns>
		/// GameObject containing Component that is of type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static GameObject GameObjectWith<T>(bool includeInactive = false)
		{
			return Any<T>(includeInactive) is Component component ? component.gameObject : null;
		}

		/// <summary>
		/// Returns <see cref="GameObject"/> to which the given object is attached.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object whose containing <see cref="GameObject"/> should be returned.
		/// <para>
		/// This can be a type that derives from <see cref="Component"/>,
		/// an interface implemented by a class that derives from <see cref="Component"/>,
		/// or a plain old class or any interface implemented by such a class if it is
		/// wrapped by a <see cref="Wrapper{T}"/> component.
		/// </para>
		/// </typeparam>
		/// <param name="attachedObject">
		/// The <see cref="Component"/> or plain old class object wrapped by a <see cref="Wrapper{T}"/>
		/// component whose containing <see cref="GameObject"/> should be returned.
		/// </param>
		/// <returns>
		/// GameObject that has the given object is attached to it, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static GameObject GameObjectOf<T>([DisallowNull] T attachedObject)
		{
			if(attachedObject is Component component)
			{
				return component.gameObject;
			}

			if(WrapperOf(attachedObject, out var wrapper))
			{
				return wrapper.gameObject;
			}

			return null;
		}

		/// <summary>
		/// Returns <see cref="GameObject"/> to which the given object is attached.
		/// </summary>
		/// <param name="attachedObject">
		/// The <see cref="Component"/> or plain old class object wrapped by a <see cref="Wrapper{}"/>
		/// component whose containing <see cref="GameObject"/> should be returned.
		/// </param>
		/// <returns>
		/// GameObject that has the given object is attached to it, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static GameObject GameObjectOf([DisallowNull] object attachedObject)
		{
			if(attachedObject is Component component)
			{
				return component.gameObject;
			}

			if(WrapperOf(attachedObject, out var wrapper))
			{
				return wrapper.gameObject;
			}

			return null;
		}

		/// <summary>
		/// Gets the <see cref="GameObject"/> to which the given object is attached.
		/// </summary>
		/// <param name="attachedObject">
		/// The <see cref="Component"/> or plain old class object wrapped by a <see cref="Wrapper{}"/>
		/// component whose containing <see cref="GameObject"/> should be returned.
		/// </param>
		/// <param name="result">
		/// When this method returns, contains the GameObject to which the given object is attached to, if any; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if <see cref="GameObject"/> was found; otherwise, <see langword="false"/>. </returns>
		public static bool GameObjectOf([DisallowNull] object attachedObject, [NotNullWhen(true), MaybeNullWhen(false)] out GameObject gameObject)
		{
			if(attachedObject is Component component)
			{
				gameObject = component.gameObject;
				return true;
			}

			if(WrapperOf(attachedObject, out var wrapper))
			{
				gameObject = wrapper.gameObject;
				return true;
			}

			gameObject = null;
			return false;
		}

		/// <summary>
		/// Returns the first object of type <typeparamref name="TWrapped"/> wrapped by a loaded Object of type <typeparamref name="TWrapper"/>.
		/// </summary>
		/// <typeparam name="TWrapper"> Type of the wrapping Object. </typeparam>
		/// <typeparam name="TWrapped"> Type of the wrapped object. </typeparam>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> object of type <typeparamref name="TWrapped"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static TWrapped WrappedObject<TWrapper, TWrapped>(bool includeInactive = false) where TWrapper : Object, IWrapper<TWrapped>
		{
			if(Any(out TWrapper wrapper, includeInactive))
			{
				return wrapper.WrappedObject;
			}
			return default;
		}

		/// <summary>
		/// Returns the first object that is of type <typeparamref name="TWrapped"/>
		/// or implements an interface of type <typeparamref name="TWrapped"/> and is
		/// wrapped by an a loaded <see cref="Object"/> that implements <see cref="IWrapper"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> Type of the wrapped object. </typeparam>
		/// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
		/// <returns> object of type <typeparamref name="TWrapped"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static TWrapped WrappedObject<TWrapped>(bool includeInactive = false)
		{
			return Any<TWrapped>(includeInactive);
		}

		/// <summary>
		/// Returns the first object that is of the given <paramref name="type"/>
		/// or implements an interface of the given <paramref name="type"/> and is wrapped by an
		/// active loaded component of type <see cref="WrapperOf{}"/>.
		/// </summary>
		/// <param name="type"> Type of the wrapped object. </param>
		/// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
		/// <returns> object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static object WrappedObject([DisallowNull] Type type, bool includeInactive = false)
		{
			return Any(type, includeInactive) is IWrapper wrapper ? wrapper.WrappedObject : null;
		}

		/// <summary>
		/// Returns all objects of type <typeparamref name="TWrapped"/> wrapped by active loaded components of type <see cref="Init.Wrapper{TWrapped}"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> Type of the wrapped objects. </typeparam>
		/// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
		[return: NotNull]
		public static void AllWrappedObjects<TWrapped>(List<TWrapped> results, bool includeInactive = false) where TWrapped : class
		{
			Components(results, includeInactive);
		}

		/// <summary>
		/// Returns the <see cref="Object"/> which wraps the <paramref name="wrapped"/> <see cref="object"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> <see cref="Type"/> of the wrapped object. </typeparam>
		/// <param name="wrapped"> The wrapped <see cref="object"/>. </param>
		/// <returns> <see cref="Object"/> that implements <see cref="IWrapper{TWrapped}"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static TWrapper WrapperOf<TWrapper, TWrapped>([DisallowNull] TWrapped wrapped) where TWrapper : Object, IWrapper<TWrapped>
		{
			return wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper) ? wrapper as TWrapper : null;
		}

		/// <summary>
		/// Returns the <see cref="Object"/> which wraps the <paramref name="wrapped"/> <see cref="object"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> <see cref="Type"/> of the wrapped object. </typeparam>
		/// <param name="wrapped"> The wrapped <see cref="object"/>. </param>
		/// <returns> <see cref="Object"/> that implements <see cref="IWrapper{TWrapped}"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static IWrapper<TWrapped> WrapperOf<TWrapped>([DisallowNull] TWrapped wrapped)
		{
			return wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper) ? wrapper as IWrapper<TWrapped> : null;
		}

		/// <summary>
		/// Returns the <see cref="Object"/> which wraps the <paramref name="wrapped"/> <see cref="object"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> <see cref="Type"/> of the wrapped object. </typeparam>
		/// <param name="wrapped"> The wrapped <see cref="object"/>. </param>
		/// <param name="result">
		/// When this method returns, contains the wrapper of the <paramref name="wrapped"/> object, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if wrapper was found; otherwise, <see langword="false"/>. </returns>
		public static bool WrapperOf<TWrapped>([DisallowNull] TWrapped wrapped, [NotNullWhen(true), MaybeNullWhen(false)] out IWrapper<TWrapped> result)
		{
			if(wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper))
			{
				result = wrapper as IWrapper<TWrapped>;
				return !(result is null);
			}

			result = null;
			return false;
		}

		/// <summary>
		/// Returns the component which wraps the <paramref name="wrapped"/>.
		/// </summary>
		/// <param name="wrapped"> The wrapped object. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> Object of type <see cref="WrapperOf{TWrapped}"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static IWrapper WrapperOf([DisallowNull] object wrapped) => wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper) ? wrapper : null;

		/// <summary>
		/// Returns the component which wraps the <paramref name="wrapped"/>.
		/// </summary>
		/// <param name="wrapped"> The wrapped object. </param>
		/// <param name="result">
		/// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> <see langword="true"/> if wrapper was found; otherwise, <see langword="false"/>. </returns>
		public static bool WrapperOf([DisallowNull] object wrapped, [NotNullWhen(true), MaybeNullWhen(false)]out IWrapper result, bool includeInactive = false)
		{
			if(!wrappedInstances.TryGetValue(wrapped, out result))
			{
				return false;
			}

			if(!includeInactive && result is Component component && !component.gameObject.activeInHierarchy)
			{
				result = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns component which wraps an object of type <typeparamref name="TWrapped"/>.
		/// </summary>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <typeparam name="TWrapped">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="TWrapped"/>,
		/// if their class derives from a base class of type <typeparamref name="TWrapped"/>,
		/// or their class implements an interface of type <typeparamref name="TWrapped"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> Component of type <see cref="WrapperOf{TWrapped}"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static IWrapper Wrapper<TWrapped>(bool includeInactive = false)
		{
			foreach(var wrappedAndWrapper in wrappedInstances)
			{
				if(wrappedAndWrapper.Key is TWrapped)
				{
					if(includeInactive)
					{
						return wrappedAndWrapper.Value;
					}

					var component = wrappedAndWrapper.Value.AsMonoBehaviour;
					if(component && !component.gameObject.activeInHierarchy)
					{
						continue;
					}

					return wrappedAndWrapper.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns component which wraps an object of the provided <paramref name="type"/>.
		/// </summary>
		/// <param name="type"> Type of the wrapped object. </param>
		/// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
		/// <returns> Component of type <see cref="WrapperOf{TWrapped}"/>, if found; otherwise, <see langword="null"/> </returns>
		[return: MaybeNull]
		public static IWrapper Wrapper([DisallowNull] Type type, bool includeInactive = false)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				return null;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(IWrapper).IsAssignableFrom(findableType))
				{
					continue;
				}
				if(ObjectByExactType(findableType, includeInactive) is IWrapper wrapper)
				{
					return wrapper;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns all components that wrap objects of type <typeparamref name="TWrapped"/>.
		/// </summary>
		/// <typeparam name="TWrapped"> Type of the wrapped objects. </typeparam>
		/// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
		/// <returns> Array of zero or more objects of type <typeparamref name="TWrapped"/>. </returns>
		[return: NotNull]
		public static IWrapper<TWrapped>[] AllWrappers<TWrapped>(bool includeInactive = false)
		{
			var list = Cached<IWrapper<TWrapped>>.list;
			AllWrappers(list, includeInactive);
			return ToArrayAndClear(list);
		}

		public static void AllWrappers<TWrapped>(List<IWrapper<TWrapped>> results, bool includeInactive = false)
		{
			All(typeof(IWrapper<TWrapped>), results, includeInactive);
		}

		/// <summary>
		/// Finds object of the given <paramref name="type"/> in relation to the provided <paramref name="gameObject"/>.
		/// <para>
		/// The provided <paramref name="including"/> value determines what <see cref="GameObject">GameObjects</see>
		/// related to the provided <paramref name="gameObject"/> are included in the search.
		/// </para>
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="including">
		/// determines what <see cref="GameObject">GameObjects</see> related
		/// to the provided <paramref name="gameObject"/> are included in the search.
		/// </param>
		/// <returns>
		/// Instance of given <paramref name="type"/>, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static object In([DisallowNull] GameObject gameObject, [DisallowNull] Type type, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(In(gameObject, type, out object result))
			{
				return result;
			}

			if(HasFlag(including, Including.Children) && InChildren(gameObject, type, out result, includeInactive))
			{
				return result;
			}

			if(HasFlag(including, Including.Parents))
			{
				Transform parent = gameObject.transform.parent;
				if(parent && InParents(parent.gameObject, type, out result, includeInactive))
				{
					return result;
				}
			}

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					return InChildren(gameObject.transform.root.gameObject, type, includeInactive);
				}
				#endif

				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return default;
				}

				SceneEqualityComparerCache<object>.scene = scene;
				return Any(SceneEqualityComparerCache<object>.Predicate, includeInactive);
			}

			return default;
		}

		/// <summary>
		/// Finds object of the given <paramref name="type"/> in relation to the provided <paramref name="gameObject"/>.
		/// <para>
		/// The provided <paramref name="including"/> value determines what <see cref="GameObject">GameObjects</see>
		/// related to the provided <paramref name="gameObject"/> are included in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="including">
		/// determines what <see cref="GameObject">GameObjects</see> related
		/// to the provided <paramref name="gameObject"/> are included in the search.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool In<T>([DisallowNull] GameObject gameObject, [NotNullWhen(true), MaybeNullWhen(false)] out T result, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(In(gameObject, out result))
			{
				return true;
			}

			if(HasFlag(including, Including.Children) && InChildren(gameObject, out result, includeInactive))
			{
				return true;
			}

			if(HasFlag(including, Including.Parents))
			{
				Transform parent = gameObject.transform.parent;
				if(parent && InParents(parent.gameObject, out result, includeInactive))
				{
					return true;
				}
			}

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					return InChildren(gameObject.transform.root.gameObject, out result, includeInactive);
				}
				#endif

				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return false;
				}

				SceneEqualityComparerCache<T>.scene = scene;
				result = Any(SceneEqualityComparerCache<T>.Predicate, includeInactive);
				return result != null;
			}

			return default;
		}

		/// <summary>
		/// Finds object with the given <see paramref="id"/> in relation to the provided <paramref name="gameObject"/>.
		/// <para>
		/// The provided <paramref name="including"/> value determines what <see cref="GameObject">GameObjects</see>
		/// related to the provided <paramref name="gameObject"/> are included in the search.
		/// </para>
		/// </summary>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="id"> The identifier of the object to find. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <param name="including">
		/// determines what <see cref="GameObject">GameObjects</see> related
		/// to the provided <paramref name="gameObject"/> are included in the search.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		/// <typeparam name="T"> Type of the object to find. </typeparam>
		/// <returns></returns>
		public static bool In<T>([DisallowNull] GameObject gameObject, Id id, [NotNullWhen(true), MaybeNullWhen(false)] out T result, Including including) where T : IIdentifiable
		{
			bool includeInactive = HasFlag(including, Including.Inactive);
			
			if(!typesToFindableTypes.TryGetValue(typeof(IIdentifiable), out var findableTypes))
			{
				result = default;
				return false;
			}

			var components = Cached<Component>.list;
			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				gameObject.GetComponents(findableTypes[i], components);
				for(int c = components.Count - 1; c >= 0; c--)
				{
					var component = components[c];
					if(TryConvert(component, out IIdentifiable identifiable) && identifiable.Id == id
					&& TryConvert(component, out result)
					&& (includeInactive || component.gameObject.activeInHierarchy))
					{
						components.Clear();
						return true;
					}
				}
			}
			
			components.Clear();
			
			if(HasFlag(including, Including.Children))
			{
				for(int i = findableTypes.Length - 1; i >= 0; i--)
				{
					var componentsInChildren = gameObject.GetComponentsInChildren(findableTypes[i], includeInactive);
					for(int c = componentsInChildren.Length - 1; c >= 0; c--)
					{
						var component = componentsInChildren[c];
						if(TryConvert(component, out IIdentifiable identifiable)
							&& identifiable.Id == id
							&& TryConvert(component, out result))
						{
							return true;
						}
					}
				}
			}

			if(HasFlag(including, Including.Parents))
			{
				for(int i = findableTypes.Length - 1; i >= 0; i--)
				{
					var componentsInParent = gameObject.GetComponentsInParent(findableTypes[i], includeInactive);
					for(int c = componentsInParent.Length - 1; c >= 0; c--)
					{
						var component = componentsInParent[c];
						if(TryConvert(component, out IIdentifiable identifiable)
							&& identifiable.Id == id
							&& TryConvert(component, out result))
						{
							return true;
						}
					}
				}
			}

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					var root = gameObject.transform.root.gameObject;
					if(gameObject == root && HasFlag(including, Including.Children))
					{
						result = default;
						return false;
					}

					for(int i = findableTypes.Length - 1; i >= 0; i--)
					{
						var componentsInChildren = root.GetComponentsInChildren(findableTypes[i], includeInactive);
						for(int c = componentsInChildren.Length - 1; c >= 0; c--)
						{
							var component = componentsInChildren[c];
							if(TryConvert(component, out IIdentifiable identifiable)
							   && identifiable.Id == id
							   && TryConvert(component, out result))
							{
								return true;
							}
						}
					}
				}
				#endif

				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					result = default;
					return false;
				}

				SceneEqualityComparerCache<T>.scene = scene;
				for(int i = findableTypes.Length - 1; i >= 0; i--)
				{
					var componentsInScene = All(SceneEqualityComparerCache<T>.Predicate, includeInactive);
					for(int c = componentsInScene.Length - 1; c >= 0; c--)
					{
						var component = componentsInScene[c];
						if(TryConvert(component, out IIdentifiable identifiable)
						   && identifiable.Id == id
						   && TryConvert(component, out result))
						{
							return true;
						}
					}
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Returns an object of the given type "nearest" to the client <see cref="GameObject"/>.
		/// <para>
		/// The search for a result is performed in the following order:
		/// <list>
		/// <item> 1. The client <see cref="GameObject"/> itself. </item>
		/// <item> 2. All the children of the client. </item>
		/// <item> 3. All the parents of the client. </item>
		/// <item> 4. All the children of the hierarchy root of the client. </item>
		/// <item> 5. All the GameObjects in the same scene with the client. </item>
		/// <item> 6. All the GameObjects in other scenes. </item>
		/// </list>
		/// </para>
		/// </summary>
		/// <typeparam name="T"> Type of the object to find. </typeparam>
		/// <param name="gameObject">
		/// The <see cref="GameObject"/> relative to which the nearest object in the scene hierarchy is returned.
		/// </param>
		/// <param name="result"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static bool NearestInHierarchy<T>([DisallowNull] GameObject gameObject, [NotNullWhen(true), MaybeNullWhen(false)] out T result, bool includeInactive = false)
		{
			if(In(gameObject, out result))
			{
				return true;
			}

			if(InChildren(gameObject, out result, includeInactive))
			{
				return true;
			}

			Transform parent = gameObject.transform.parent;
			if(parent && InParents(parent.gameObject, out result, includeInactive))
			{
				return true;
			}

			var rootGameObject = gameObject.transform.root.gameObject;
			if(!ReferenceEquals(rootGameObject, gameObject) && InChildren(rootGameObject, out result, includeInactive))
			{
				return true;
			}

			#if UNITY_EDITOR
			if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
			{
				return false;
			}
			#endif

			var scene = gameObject.scene;
			if(scene.IsValid())
			{
				SceneEqualityComparerCache<T>.scene = scene;
				return Any(SceneEqualityComparerCache<T>.Predicate, out result, includeInactive) || Any(out result, includeInactive);
			}

			return false;
		}

		[return: MaybeNull]
		public static object In([DisallowNull] GameObject gameObject, [DisallowNull] Type type)
		{
			if(!typesToComponentTypes.TryGetValue(type, out var componentTypes))
			{
				if(type == typeof(GameObject))
				{
					return gameObject;
				}

				return null;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				if(gameObject.TryGetComponent(componentTypes[i], out var component))
				{
					return As(type, component);
				}
			}

			return null;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching <paramref name="obj"/>.
		/// <para>
		/// If <paramref name="obj"/> is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>, then returns <paramref name="obj"/> as <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="GameObject"/> then the <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="Component"/> or wrapped by a <see cref="Wrapper">Wrapper component</see> then
		/// the <see cref="GameObject"/> to which the <see cref="Component"/> is attached to is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="obj"> The object to search. </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T In<T>([DisallowNull] object obj)
		{
			if(obj is T objAsT)
			{
				return objAsT;
			}

			if(obj is GameObject gameObject)
			{
				return In<T>(gameObject);
			}

			if(obj is IWrapper { WrappedObject: T wrappedObject })
			{
				return wrappedObject;
			}

			if(obj is Component component)
			{
				return In<T>(component.gameObject);
			}

			if(!WrapperOf(obj, out var wrapper))
			{
				return default;
			}

			gameObject = wrapper.gameObject;
			if(!gameObject)
			{
				return default;
			}

			return In<T>(wrapper.gameObject);
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching <paramref name="obj"/>.
		/// <para>
		/// If <paramref name="obj"/> is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>, then returns <paramref name="obj"/> as <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="GameObject"/> then the <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="Component"/> or wrapped by a <see cref="Wrapper">Wrapper component</see> then
		/// the <see cref="GameObject"/> to which the <see cref="Component"/> is attached to is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="obj"> The object to search. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool In<T>([DisallowNull] object obj, [NotNullWhen(true), MaybeNullWhen(false)] out T result)
		{
			if(obj is T objAsT)
			{
				result = objAsT;
				return true;
			}

			if(obj is GameObject gameObject)
			{
				return In(gameObject, out result);
			}

			if(obj is Component component)
			{
				return In(component.gameObject, out result);
			}

			if(!WrapperOf(obj, out IWrapper wrapper))
			{
				result = default;
				return false;
			}

			if(wrapper is T wrapperAsT)
			{
				result = wrapperAsT;
				return true;
			}

			gameObject = wrapper.gameObject;
			if(gameObject)
			{
				return In(gameObject, out result);
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching <paramref name="obj"/>.
		/// <para>
		/// If <paramref name="obj"/> is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>, then returns <paramref name="obj"/> as <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="GameObject"/> then the <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// If <paramref name="obj"/> is a <see cref="Component"/> or wrapped by a <see cref="Wrapper">Wrapper component</see> then
		/// the <see cref="GameObject"/> to which the <see cref="Component"/> is attached to is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </summary>
		/// <param name="obj"> The object to search. </param>
		/// <param name="type">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or
		/// objects that are wrapped by a <see cref="IWrapper{T}"/> can be found.
		/// </para>
		/// </param>
		/// </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool In([DisallowNull] object obj, [DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
		{
			if(type.IsInstanceOfType(obj))
			{
				result = obj;
				return true;
			}

			if(obj is GameObject gameObject)
			{
				return In(gameObject, type, out result);
			}

			if(obj is IWrapper wrapper && wrapper.WrappedObject is object WrappedObject && type.IsInstanceOfType(WrappedObject))
			{
				result = WrappedObject;
				return true;
			}

			if(obj is IValueProvider valueProvider && valueProvider.TryGetFor(valueProvider as Component, out object value) && type.IsInstanceOfType(value))
			{
				result = value;
				return true;
			}

			if(WrapperOf(obj) is Component component)
			{
				return In(component.gameObject, type, out result);
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>.
		/// <para>
		/// The <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool In<T>([DisallowNull] GameObject gameObject, [NotNullWhen(true), MaybeNullWhen(false)] out T result)
		{
			if(!typesToComponentTypes.TryGetValue(typeof(T), out var componentTypes))
			{
				if(typeof(T) == typeof(GameObject))
				{
					result = As<T>(gameObject);
					return true;
				}

				result = default;
				return false;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				if(gameObject.TryGetComponent(componentTypes[i], out var component))
				{
					result = As<T>(component);
					if(result is not null)
					{
						return true;
					}
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>.
		/// <para>
		/// The <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="result">
		/// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
		public static bool In([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
		{
			if(!typesToComponentTypes.TryGetValue(type, out var componentTypes))
			{
				if(type == typeof(GameObject))
				{
					result = gameObject;
					return true;
				}

				result = default;
				return false;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				if(gameObject.TryGetComponent(componentTypes[i], out var component))
				{
					result = As(type, component);
					if(result is not null)
					{
						return true;
					}
				}
			}

			result = default;
			return false;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> by searching the <paramref name="gameObject"/>.
		/// <para>
		/// The <see cref="GameObject"/> is searched for a <see cref="Component"/>
		/// which is of type <typeparamref name="T"/>, derives from type <typeparamref name="T"/>
		/// or implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Objects wrapped by <see cref="IWrapper{T}"/> components are also considered in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T In<T>([DisallowNull] GameObject gameObject)
		{
			var type = typeof(T);
			if(typeof(Component).IsAssignableFrom(type))
			{
				return gameObject.GetComponent<T>();
			}

			if(!typesToComponentTypes.TryGetValue(type, out var componentTypes))
			{
				if(type == typeof(GameObject))
				{
					return As<T>(gameObject);
				}

				return default;
			}

			for(int i = componentTypes.Length - 1; i >= 0; i--)
			{
				if(gameObject.TryGetComponent(componentTypes[i], out var component))
				{
					if(As<T>(component) is T result)
					{
						return result;
					}
				}
			}

			return default;
		}

		/// <summary>
		/// Finds object of type <typeparamref name="T"/> in relation to the provided <paramref name="gameObject"/>.
		/// <para>
		/// The provided <paramref name="including"/> value determines what <see cref="GameObject">GameObjects</see>
		/// related to the provided <paramref name="gameObject"/> are included in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the object to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="including">
		/// determines what <see cref="GameObject">GameObjects</see> related to the provided
		/// <paramref name="gameObject"/> are included in the search.
		/// </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		[return: MaybeNull]
		public static T In<T>([DisallowNull] GameObject gameObject, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(In(gameObject, out T result))
			{
				return result;
			}

			if(HasFlag(including, Including.Children) && InChildren(gameObject, out result, includeInactive))
			{
				return result;
			}

			if(HasFlag(including, Including.Parents))
			{
				Transform parent = gameObject.transform.parent;
				if(parent && InParents(parent.gameObject, out result, includeInactive))
				{
					return result;
				}
			}

			if(HasFlag(including, Including.Scene))
			{
				var root = gameObject.transform.root.gameObject;
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					if(gameObject == root && HasFlag(including, Including.Children))
					{
						return default;
					}

					return InChildren<T>(root, includeInactive);
				}
				#endif

				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return default;
				}

				SceneEqualityComparerCache<T>.scene = scene;
				return Any(SceneEqualityComparerCache<T>.Predicate, includeInactive);
			}

			return default;
		}

		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static T[] AllIn<T>([DisallowNull] GameObject gameObject)
		{
			var type = typeof(T);
			if(typeof(Component).IsAssignableFrom(type))
			{
				return gameObject.GetComponents<T>();
			}

			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				if(type == typeof(GameObject))
				{
					return new T[] { As<T>(gameObject) };
				}

				return Array.Empty<T>();
			}

			var list = Cached<T>.list;
			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				if(!typeof(Component).IsAssignableFrom(findableType))
				{
					continue;
				}

				var components = gameObject.GetComponents(findableTypes[i]);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(list, components[c]);
				}
			}

			return ToArrayAndClear(list);
		}

		/// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
		[return: NotNull]
		public static object[] AllIn([DisallowNull] GameObject gameObject, [DisallowNull] Type type)
		{
			if(typeof(Component).IsAssignableFrom(type))
			{
				return gameObject.GetComponents(type);
			}

			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				if(type == typeof(GameObject))
				{
					return new object[] { gameObject };
				}

				return Array.Empty<object>();
			}

			var list = Cached<object>.list;
			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var components = gameObject.GetComponents(findableTypes[i]);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(list, components[c], type);
				}
			}

			return ToArrayAndClear(list);
		}

		/// <summary>
		/// Finds all objects of type <typeparamref name="T"/> in relation to the provided <paramref name="gameObject"/>.
		/// <para>
		/// The provided <paramref name="including"/> value determines what <see cref="GameObject">GameObjects</see>
		/// related to the provided <paramref name="gameObject"/> are included in the search.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the objects to find.
		/// <para>
		/// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// </typeparam>
		/// <param name="gameObject"> The <see cref="GameObject"/> to search. </param>
		/// <param name="including">
		/// determines what <see cref="GameObject">GameObjects</see> related to the provided
		/// <paramref name="gameObject"/> are included in the search.
		/// </param>
		/// <returns>
		/// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
		/// </returns>
		public static T[] AllIn<T>([DisallowNull] GameObject gameObject, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					return AllInChildren<T>(gameObject.transform.root.gameObject, includeInactive);
				}
				#endif
				
				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return Array.Empty<T>();
				}

				SceneEqualityComparerCache<T>.scene = gameObject.scene;
				return All(SceneEqualityComparerCache<T>.Predicate, includeInactive);
			}

			if(HasFlag(including, Including.Children))
			{
				var list = Cached<T>.list;

				AllInChildren(gameObject, list, includeInactive);

				if(HasFlag(including, Including.Parents))
				{
					Transform parent = gameObject.transform.parent;
					if(parent)
					{
						AllInParents(parent.gameObject, list, includeInactive);
					}
				}

				return ToArrayAndClear(list);
			}
			
			if(HasFlag(including, Including.Parents))
			{
				return AllInParents<T>(gameObject, includeInactive);
			}
			
			return AllIn<T>(gameObject);
		}

		/// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
		[return: NotNull]
		public static object[] AllIn([DisallowNull] GameObject gameObject, [DisallowNull] Type type, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					return AllInChildren(gameObject.transform.root.gameObject, type, includeInactive);
				}
				#endif
				
				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return default;
				}

				SceneEqualityComparerCache<object>.scene = scene;
				return All(SceneEqualityComparerCache<object>.Predicate, includeInactive);
			}

			if(HasFlag(including, Including.Children))
			{
				var list = Cached<object>.list;

				AllInChildren(gameObject, list, includeInactive);

				if(HasFlag(including, Including.Parents))
				{
					Transform parent = gameObject.transform.parent;
					if(parent)
					{
						AllInParents(parent.gameObject, type, list, includeInactive);
					}
				}

				return ToArrayAndClear(list);
			}
			
			if(HasFlag(including, Including.Parents))
			{
				return AllInParents(gameObject, type, includeInactive);
			}
			
			return AllIn(gameObject, type);
		}

		/// <param name="results"> List into which found instances are added. </param>
		public static void AllIn<T>([DisallowNull] GameObject gameObject, [DisallowNull] List<T> results)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				if(typeof(T) == typeof(GameObject))
				{
					results.Add(As<T>(gameObject));
				}

				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var components = gameObject.GetComponents(findableTypes[i]);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(results, components[c]);
				}
			}
		}
		

		/// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
		[return: NotNull]
		public static void AllIn<T>([DisallowNull] GameObject gameObject, [DisallowNull] List<T> results, Including including)
		{
			bool includeInactive = HasFlag(including, Including.Inactive);

			if(HasFlag(including, Including.Scene))
			{
				#if UNITY_EDITOR
				if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
				{
					AllInChildren(gameObject.transform.root.gameObject, results, includeInactive);
					return;
				}
				#endif
				
				var scene = gameObject.scene;
				if(!scene.IsValid())
				{
					return;
				}

				SceneEqualityComparerCache<T>.scene = scene;
				All(SceneEqualityComparerCache<T>.Predicate, results, includeInactive);
				return;
			}

			if(HasFlag(including, Including.Children))
			{
				AllInChildren(gameObject, results, includeInactive);

				if(HasFlag(including, Including.Parents))
				{
					Transform parent = gameObject.transform.parent;
					if(parent)
					{
						AllInParents(parent.gameObject, results, includeInactive);
					}
				}

				return;
			}
			
			if(HasFlag(including, Including.Parents))
			{
				AllInParents(gameObject, results, includeInactive);
				return;
			}
			
			AllIn(gameObject, results);
		}

		/// <typeparam name="T">
		/// Generic type of the <see cref="List{T}"/> into which found results will be placed.
		/// </typeparam>
		/// <param name="results"> List into which found instances are added. </param>
		public static void AllIn<T>([DisallowNull] GameObject gameObject, [DisallowNull] Type type, [NotNull] List<T> results)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				if(type == typeof(GameObject))
				{
					results.Add(As<T>(gameObject));
				}

				return;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var components = gameObject.GetComponents(findableTypes[i]);
				for(int c = components.Length - 1; c >= 0; c--)
				{
					TryAddAs(results, components[c]);
				}
			}
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Loads the asset of type <typeparamref name="T"/> from the asset database.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static T Asset<T>()
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				return default;
			}

			if(typeof(T).IsAbstract && typeof(Object).IsAssignableFrom(typeof(T)))
			{
				// Check derived types too because AssetDatabase.FindAssets only matches exact types.
				var typeAssembly = typeof(T).Assembly;
				foreach(var derivedType in TypeUtility.GetDerivedTypes(typeof(T), typeAssembly, false, 0))
				{
					foreach(var guid in AssetDatabase.FindAssets("t:" + derivedType))
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						if(AssetDatabase.LoadAssetAtPath(path, typeof(T)) is T result)
						{
							return result;
						}
					}
				}
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var type = findableTypes[i];
				var guids = AssetDatabase.FindAssets("t:" + type.Name);
				for(int g = guids.Length - 1; g >= 0; g--)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
					var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
					if(instance)
					{
						return As<T>(instance);
					}
				}
			}

			return default;
		}
		#endif
		
		#if UNITY_EDITOR
		/// <summary>
		/// Loads a scene asset by the given name.
		/// <remarks>
		/// This method is only available in the editor.
		/// </remarks>
		/// </summary>
		/// <param name="sceneName"> Name of the scene asset to load. </param>
		/// <returns> The loaded scene asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static SceneAsset SceneAssetByName(string sceneName)
		{
			var filenameWithExtension = sceneName + ".unity";
			var guids = AssetDatabase.FindAssets(filenameWithExtension);
			var assetPaths = guids.Select(AssetDatabase.GUIDToAssetPath).Where(assetPath => string.Equals(Path.GetFileName(assetPath), filenameWithExtension, StringComparison.OrdinalIgnoreCase)).ToArray();
			if(assetPaths.Length == 0)
			{
				return null;
			}

			if(assetPaths.Length == 1)
			{
				return AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPaths[0]);
			}

			var sceneInBuildSettings = assetPaths.Where(assetPath => SceneUtility.GetBuildIndexByScenePath(assetPath) != -1).SingleOrDefaultNoException();
			return sceneInBuildSettings is null ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneInBuildSettings);
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads a scene asset by the given build index.
		/// <remarks>
		/// This method is only available in the editor.
		/// </remarks>
		/// </summary>
		/// <param name="buildIndex"> Index of the scene in build settings. </param>
		/// <returns> The loaded scene asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static SceneAsset SceneAssetByBuildIndex(int buildIndex)
		{
			var assetPath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
			return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads the <see cref="MonoScript"/> asset that contains the definition for the <paramref name="type"/>.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <param name="type"> Type of the class whose script asset to load. </param>
		/// <param name="ignoreEditorScripts"></param>
		/// <returns> The loaded script asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static MonoScript Script([DisallowNull] Type type)
		{
			if(type.IsNested)
			{
				type = type.DeclaringType;
			}

			string name = type.Name;

			if(type.IsGenericType)
			{
				// Parse out generic type information from generic type name
				int i = name.IndexOf('`');
				if(i != -1)
				{
					name = name.Substring(0, i);
				}

				// Additionally, convert generic types to their generic type defitions.
				// E.g. List<string> to List<>.
				if(!type.IsGenericTypeDefinition)
				{
					type = type.GetGenericTypeDefinition();
				}
			}

			var guids = AssetDatabase.FindAssets(name + " t:MonoScript");

			int count = guids.Length;
			if(count == 0)
			{
				return null;
			}

			MonoScript fallback = null;

			for(int n = count - 1; n >= 0; n--)
			{
				var guid = guids[n];
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var filename = Path.GetFileNameWithoutExtension(path);
				if(string.Equals(filename, name, StringComparison.OrdinalIgnoreCase))
				{
					var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
					if(!scriptAsset)
					{
						continue;
					}

					var scriptClassType = scriptAsset.GetClass();
					if(scriptClassType == type)
					{
						return scriptAsset;
					}

					if(scriptClassType is null && (!fallback || type.Namespace is { Length: 0 } || scriptAsset.text.Contains("namespace " + type.Namespace)))
					{
						fallback = scriptAsset;
					}
					
					#if DEV_MODE && DEBUG_FIND_SCRIPT
					Debug.Log($"FindScriptFile({TypeUtility.ToString(type)}) ignoring file @ \"{path}\" because MonoScript.GetClass() result {TypeUtility.ToString(scriptClassType)} did not match type.");
					#endif
				}
			}

			// Second pass: test files where filename is only a partial match for class name.
			// E.g. class Header could be defined in file HeaderAttribute.cs.
			if(count > 1)
			{
				for(int n = count - 1; n >= 0; n--)
				{
					var guid = guids[n];
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var filename = Path.GetFileNameWithoutExtension(path);
					if(filename.Length != name.Length) // skip testing exact matches a second time
					{
						var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
						var scriptClassType = scriptAsset.GetClass();
						if(scriptClassType == type)
						{
							return scriptAsset;
						}
						
						if(!fallback && scriptClassType is null && type.Namespace is { Length: > 0 } && scriptAsset.text.Contains("namespace " + type.Namespace))
						{
							fallback = scriptAsset;
						}

						#if DEV_MODE && DEBUG_FIND_SCRIPT
						Debug.LogWarning($"FindScriptFile({TypeUtility.ToString(type)}) second pass: ignoring partial match @ \"{path}\" because MonoScript.GetClass() result {TypeUtility.ToString(scriptClassType)} did not match type.");
						#endif
					}
				}
			}

			// If was unable to verify correct script class type using MonoScript.GetClass()
			// but there was a probable match whose GetClass() returned null (seems to happen
			// with all generic types), then return that.
			if(fallback)
			{
				#if DEV_MODE && DEBUG_FIND_SCRIPT
				Debug.LogWarning($"FindScriptFile({TypeUtility.ToString(type)}) returning fallback result @ \"{AssetDatabase.GetAssetPath(fallback)}\".");
				#endif

				return fallback;
			}

			#if DEV_MODE
			Debug.Log($"FindScriptFile({TypeUtility.ToString(type)}) failed to find MonoScript for type {TypeUtility.ToString(type)} AssetDatabase.FindAssets(\"{name} t:MonoScript\") returned {count} results:\n{string.Join("\n", guids.Select(AssetDatabase.GUIDToAssetPath))}");
			#endif

			return null;
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads the <see cref="MonoScript"/> asset that contains the definition for the <paramref name="type"/>.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <param name="type"> Type of the class whose script asset to load. </param>
		/// <param name="result">
		/// When this method returns, contains loaded script asset, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if a script asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Script([DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out MonoScript result)
		{
			result = Script(type);
			return result;
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads the asset of type <typeparamref name="T"/> from the asset database.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="result">
		/// When this method returns, contains loaded asset, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Asset<T>([NotNullWhen(true), MaybeNullWhen(false)] out T result)
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				result = default;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var type = findableTypes[i];
				var guids = AssetDatabase.FindAssets("t:" + type.Name);
				for(int g = guids.Length - 1; g >= 0; g--)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
					var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
					if(instance)
					{
						result = As<T>(instance);
						if(result != null)
						{
							return true;
						}
					}
				}
			}

			result = default;
			return false;
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads the asset of the given <paramref name="type"/> from the asset database.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="result">
		/// When this method returns, contains loaded asset, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Asset([DisallowNull] Type type, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				result = null;
				return false;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				var guids = AssetDatabase.FindAssets("t:" + findableType.Name);
				for(int g = guids.Length - 1; g >= 0; g--)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
					var instance = AssetDatabase.LoadAssetAtPath(assetPath, findableType);
					if(instance)
					{
						result = As(type, instance);
						return true;
					}
				}
			}

			result = null;
			return false;
		}
		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads all assets of type <typeparamref name="T"/> from the asset database.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: NotNull]
		public static IEnumerable<T> Assets<T>()
		{
			if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
			{
				if(typeof(T) == typeof(GameObject))
				{
					var guids = AssetDatabase.FindAssets("t:prefab");
					for(int g = guids.Length - 1; g >= 0; g--)
					{
						var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
						var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
						if(prefab)
						{
							yield return As<T>(prefab);
						}
					}
				}

				yield break;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var type = findableTypes[i];
				var guids = AssetDatabase.FindAssets("t:" + type.Name);
				for(int g = guids.Length - 1; g >= 0; g--)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
					var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
					if(instance)
					{
						yield return As<T>(instance);
					}
				}
			}
		}

		#endif

		#if UNITY_EDITOR
		/// <summary>
		/// Loads all assets of the given <paramref name="type"/> from the asset database.
		/// <para>
		/// This method is only available in the editor.
		/// </para>
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
		[return: NotNull]
		public static IEnumerable<object> Assets([DisallowNull] Type type)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
			{
				if(type == typeof(GameObject))
				{
					var guids = AssetDatabase.FindAssets("t:prefab");
					for(int g = guids.Length - 1; g >= 0; g--)
					{
						var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
						var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
						if(prefab)
						{
							yield return prefab;
						}
					}
				}

				yield break;
			}

			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var findableType = findableTypes[i];
				var guids = AssetDatabase.FindAssets("t:" + findableType.Name);
				for(int g = guids.Length - 1; g >= 0; g--)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(guids[g]);
					var instance = AssetDatabase.LoadAssetAtPath(assetPath, findableType);
					if(instance)
					{
						yield return As(type, instance);
					}
				}
			}
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Loads a single Addressable asset synchronously.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to find.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// <param name="key"> The key of the Addressable object. </param>
		/// <param name="result">
		/// When this method returns, contains loaded Addressable object, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Addressable<T>([DisallowNull] string key, [NotNullWhen(true), MaybeNullWhen(false)] out T result)
		{
			Object obj = LoadAddressable(key);

			if(!obj)
			{
				result = default;
				return false;
			}

			if(obj is GameObject gameObject)
			{
				if(typeof(T).IsAssignableFrom(typeof(GameObject)))
				{
					result = (T)(object)gameObject;
					return true;
				}

				return In(gameObject, out result);
			}

			result = As<T>(obj);
			return result != null;
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Loads a single Addressable asset synchronously.
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of this type,
		/// if their class derives from a base class this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="key"> The key of the Addressable object. </param>
		/// <param name="result">
		/// When this method returns, contains loaded Addressable object, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Addressable([DisallowNull] Type type, [DisallowNull] string key, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
		{
			Object obj = LoadAddressable(key);

			if(!obj)
			{
				result = null;
				return false;
			}

			if(obj is GameObject gameObject)
			{
				if(type.IsAssignableFrom(typeof(GameObject)))
				{
					result = gameObject;
					return true;
				}

				return In(gameObject, type, out result);
			}

			result = As(type, obj);
			return result != null;
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Loads a single Addressable asset synchronously.
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// <param name="key"> The key of the Addressable object. </param>
		/// <returns>
		/// Loaded asset by the given <paramref name="key"/>, if found; otherwise, <see langword="null"/>.
		/// </returns>
		public static object Addressable([DisallowNull] Type type, [DisallowNull] string key)
		{
			Object obj = LoadAddressable(key);

			if(!obj)
			{
				return null;
			}

			if(obj is GameObject gameObject)
			{
				if(type.IsAssignableFrom(typeof(GameObject)))
				{
					return obj;
				}

				return In(gameObject, type);
			}

			return As(type, obj);
		}
		#endif

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		/// <summary>
		/// Loads a single Addressable asset synchronously.
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of this type,
		/// if their class derives from a base class of this type,
		/// or their class implements an interface of this type.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// <param name="key"> The key of the Addressable object. </param>
		/// <returns>
		/// Loaded asset by the given <paramref name="key"/>, if found; otherwise, <see langword="null"/>.
		/// </returns>
		public static T Addressable<T>([DisallowNull] string key)
		{
			Object obj = LoadAddressable(key);

			if(!obj)
			{
				return default;
			}

			if(obj is GameObject gameObject)
			{
				if(typeof(T).IsAssignableFrom(typeof(GameObject)))
				{
					return (T)(object)obj;
				}

				return In<T>(gameObject);
			}

			return As<T>(obj);
		}
		#endif

		/// <summary>
		/// Loads the asset of the given <paramref name="type"/> stored at <paramref name="path"/> in a Resources folder.
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="path"> Path to the target resource to load. </param>
		/// <param name="result">
		/// When this method returns, contains loaded instance, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Resource([DisallowNull] Type type, [DisallowNull] string path, [NotNullWhen(true), MaybeNullWhen(false)] out object result)
		{
			Object obj = Resources.Load(path, typeof(Object));
			result = As(type, obj);
			return result is not null;
		}

		/// <summary>
		/// Loads the asset of the given <paramref name="type"/> stored at <paramref name="path"/> in a Resources folder.
		/// </summary>
		/// <param name="type">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </param>
		/// <param name="path"> Path to the target resource to load. </param>
		/// <returns> Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. </returns>
		[return: MaybeNull]
		public static object Resource([DisallowNull] Type type, [DisallowNull] string path)
		{
			Object obj = Resources.Load(path, typeof(Object));
			if(!obj)
			{
				return default;
			}

			if(obj is GameObject gameObject)
			{
				if(type == typeof(GameObject))
				{
					return gameObject;
				}
				return In(gameObject, type);
			}

			return As(type, obj);
		}

		/// <summary>
		/// Loads the asset of type <typeparamref name="T"/> stored at <paramref name="path"/> in a Resources folder.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="path"> Path to the target resource to load. </param>
		/// <returns> Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. </returns>
		public static T Resource<T>(string path)
		{
			Object obj = Resources.Load(path, typeof(Object));
			if(!obj)
			{
				return default;
			}

			if(obj is GameObject gameObject)
			{
				if(typeof(T) == typeof(GameObject))
				{
					return (T)(object)gameObject;
				}
				return In<T>(gameObject);
			}

			return As<T>(obj);
		}

		/// <summary>
		/// Loads the asset of type <typeparamref name="T"/> stored at <paramref name="path"/> in a Resources folder.
		/// </summary>
		/// <typeparam name="T">
		/// Type of the asset to load.
		/// <para>
		/// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
		/// if their class derives from a base class of type <typeparamref name="T"/>,
		/// or their class implements an interface of type <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
		/// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
		/// </para>
		/// </typeparam>
		/// <param name="path"> Path to the target resource to load. </param>
		/// <param name="result">
		/// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
		public static bool Resource<T>([DisallowNull] string path, [NotNullWhen(true), MaybeNullWhen(false)] out T result)
		{
			Object obj = Resources.Load(path, typeof(Object));
			if(!obj)
			{
				result = default;
				return false;
			}

			if(obj is GameObject gameObject)
			{
				if(typeof(T) == typeof(GameObject))
				{
					result = (T)(object)gameObject;
					return true;
				}

				return In(gameObject, out result);
			}

			result = As<T>(obj);
			return true;
		}

		#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		private static Object LoadAddressable([DisallowNull] string key)
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return LoadAddressableInEditor(key);
			}
			#endif

			var asyncOperation = Addressables.LoadAssetAsync<Object>(key);
			return asyncOperation.WaitForCompletion();
		}
		#endif

		#if UNITY_EDITOR && UNITY_ADDRESSABLES_1_17_4_OR_NEWER
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		private static Object LoadAddressableInEditor([DisallowNull] string key)
		{
			foreach(var group in AddressableAssetSettingsDefaultObject.Settings.groups)
			{
				foreach(var entry in group.entries)
				{
					if(entry.address == key)
					{
						return AssetDatabase.LoadAssetAtPath(entry.AssetPath, typeof(Object));
					}
				}
			}

			return null;
		}
		#endif

		internal static GameObject WithTagFromLoadedScenesIncludingInactive(string tag)
		{
			var list = Cached<GameObject>.list;

			for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
			{
				var scene = SceneManager.GetSceneAt(s);
				scene.GetRootGameObjects(list);
				for(int g = list.Count - 1; g >= 0; g--)
				{
					if(list[g].CompareTag(tag))
					{
						var result = list[g];
						list.Clear();
						return result;
					}
				}
				list.Clear();
			}

			return null;
		}

		internal static void AllWithTagFromLoadedScenesIncludingInactive(string tag, List<GameObject> results)
		{
			var list = Cached<GameObject>.list;
			for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
			{
				var scene = SceneManager.GetSceneAt(s);
				scene.GetRootGameObjects(list);
				for(int g = list.Count - 1; g >= 0; g--)
				{
					if(list[g].CompareTag(tag))
					{
						results.Add(list[g]);
					}
				}
				list.Clear();
			}
		}

		private static T InChildrenByExactType<T>([DisallowNull] GameObject gameObject, Type findableType, bool includeInactive)
		{
			var component = gameObject.GetComponentInChildren(findableType, includeInactive);
			if(component)
			{
				return As<T>(component);
			}

			return default;
		}

		private static T InParentsByExactType<T>([DisallowNull] GameObject gameObject, Type findableType, bool includeInactive)
		{
			if(!typeof(Component).IsAssignableFrom(findableType))
			{
				return default;
			}

			var component = gameObject.GetComponentInParent(findableType, includeInactive);
			return As<T>(component);
		}

		private static Object ObjectByExactType(Type findableType, bool includeInactive = false)
		{
			#if UNITY_2023_1_OR_NEWER
			return FindAnyObjectByType(findableType, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
			#else
			return FindObjectOfType(findableType, includeInactive);
			#endif
		}

		private static IEnumerable<Object> ObjectsByExactTypesIterator(Type[] findableTypes, bool includeInactive)
		{
			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				var objects = ObjectsByExactType(findableTypes[i], includeInactive);
				for(int o = objects.Length - 1; o >= 0; o--)
				{
					yield return objects[o];
				}
			}
		}

		private static void ObjectsByExactTypes<T>(Type[] findableTypes, List<T> results, bool includeInactive)
		{
			for(int i = findableTypes.Length - 1; i >= 0; i--)
			{
				ObjectsByExactType(findableTypes[i], results, includeInactive);
			}
		}

		private static void ObjectsByExactType<T>(Type findableType, List<T> results, bool includeInactive)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(findableType.IsGenericTypeDefinition)
			{
				throw new ArgumentException($"Finding objects using generic type definitions is not supported.", nameof(findableType));
			}
			#endif

			#if UNITY_2023_1_OR_NEWER
			TryAddAs(results, FindObjectsByType(findableType, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None));
			#else
			TryAddAs(results, FindObjectsOfType(findableType, includeInactive));
			#endif
		}

		private static Object[] ObjectsByExactType(Type findableType, bool includeInactive)
		{
			#if DEBUG || INIT_ARGS_SAFE_MODE
			if(findableType.IsGenericTypeDefinition)
			{
				throw new ArgumentException($"Finding objects using generic type definitions is not supported.", nameof(findableType));
			}
			#endif

			#if UNITY_2023_1_OR_NEWER
			return FindObjectsByType(findableType, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			#else
			return FindObjectsOfType(findableType, includeInactive);
			#endif
		}

		private static bool HasFlag(Including value, Including flag)
		{
			return (value & flag) == flag;
		}

		private static void RegisterTypeToFindableTypeLinks([DisallowNull] Type type, [DisallowNull] Type[] addFindableTypes)
		{
			if(!typesToFindableTypes.TryGetValue(type, out var existingFindableTypes))
			{
				typesToFindableTypes.Add(type, addFindableTypes);
				return;
			}

			var list = Cached<Type>.list;
			list.AddRange(existingFindableTypes);
			AddAndRemoveDerivedTypes(list, addFindableTypes);
			typesToFindableTypes[type] = ToArrayAndClear(list);
		}

		private static void AddAndRemoveDerivedTypes(List<Type> list, Type[] typesToAdd)
		{
			for(int i = typesToAdd.Length - 1; i >= 0; i--)
			{
				var add = typesToAdd[i];

				if(ContainsAnyTypesAssignableFrom(list, add))
				{
					continue;
				}

				RemoveTypesDerivedFrom(list, add);

				list.Add(add);
			}
		}

		private static bool ContainsAnyTypesAssignableFrom([DisallowNull] IList<Type> list, [DisallowNull] Type type)
		{
			for(int i = list.Count - 1; i >= 0; i--)
			{
				if(list[i].IsAssignableFrom(type))
				{
					return true;
				}
			}

			return false;
		}

		private static void RemoveTypesDerivedFrom([DisallowNull] List<Type> list, [DisallowNull] Type type)
		{
			for(int i = list.Count - 1; i >= 0; i--)
			{
				if(type.IsAssignableFrom(list[i]))
				{
					list.RemoveAt(i);
				}
			}
		}

		private static void SetupWrappedType([DisallowNull] Type wrapperType)
		{
			var wrappedType = GetWrappedType(wrapperType);
			if(wrappedType != null)
			{
				SetupWrappedTypeAndItsBaseTypesAndInterfaces(wrappedType, wrapperType);
			}
		}

		[return: MaybeNull]
		private static Type GetWrappedType([DisallowNull] Type wrapperType)
		{
			foreach(var interfaceType in wrapperType.GetInterfaces())
			{
				if(!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != typeof(IWrapper<>))
				{
					continue;
				}

				return interfaceType.GetGenericArguments()[0];
			}

			return null;
		}

		private static void SetupWrappedTypeAndItsBaseTypesAndInterfaces([DisallowNull] Type wrappedType, [DisallowNull] Type wrapperType)
		{
			// In theory, it is possible that a type has more than one wrapper.
			RegisterWrappedToWrapperLink(wrappedType, wrapperType);

			// Add support for searching for wrapped objects using types they derive from.
			for(var baseType = wrappedType.BaseType; baseType != typeof(object) && baseType != null; baseType = baseType.BaseType)
			{
				RegisterWrappedToWrapperLink(baseType, wrapperType);
			}

			// Add support for searching for wrapped objects based on the interfaces that they implement,
			// even if the wrapper component doesn't implement those interfaces.
			var interfaceTypes = wrappedType.GetInterfaces();
			for(int i = interfaceTypes.Length - 1; i >= 0; i--)
			{
				var interfaceType = interfaceTypes[i];
				RegisterWrappedToWrapperLink(interfaceType, wrapperType);
			}
		}

		private static void RegisterWrappedToWrapperLink([DisallowNull] Type wrappedType, [DisallowNull] Type wrapperType)
		{
			if(!typesToWrapperTypes.TryGetValue(wrappedType, out var wrapperTypes))
			{
				typesToWrapperTypes.Add(wrappedType, new[] { wrapperType });
				return;
			}

			// If type has already been registered do nothing.
			if(Array.IndexOf(wrapperTypes, wrapperType) != -1)
			{
				return;
			}

			// A type can have more than one wrapper (esp. interface types).
			int index = wrapperTypes.Length;
			Array.Resize(ref wrapperTypes, index + 1);
			wrapperTypes[index] = wrapperType;
			typesToWrapperTypes[wrappedType] = wrapperTypes;
		}

		private static T[] ToArrayAndClear<T>(List<T> list)
		{
			if(list.Count == 0)
			{
				return Array.Empty<T>();
			}
			var results = list.ToArray();
			list.Clear();
			return results;
		}

		private static void Setup()
		{
			var reusableList = new List<Type>(8);
			var unityObjectTypes = TypeUtility.GetDerivedTypes(typeof(Object), typeof(Object).Assembly, false, 2048);

			foreach(var unityObjectType in unityObjectTypes)
			{
				if(unityObjectType.IsGenericTypeDefinition)
				{
					continue;
				}

				// All of Unity's internal search methods such as FindObjectOfType work with UnityEngine.Object types as is.
				typesToFindableTypes[unityObjectType] = new[] { unityObjectType };

				// FindObjectOfType doesn't work with interface types directly,  so we need to create a map from interface types
				// to UnityEngine.Object types that implement it.
				var interfaceTypes = unityObjectType.GetInterfaces();
				for(int i = interfaceTypes.Length - 1; i >= 0; i--)
				{
					//SetupInterfaceType(reusableList, unityObjectType, interfaceTypes[i]);
					var interfaceType = interfaceTypes[i];
					if(interfaceType.IsGenericTypeDefinition)
					{
						continue;
					}

					if(!typesToFindableTypes.TryGetValue(interfaceType, out var existingFindableTypes))
					{
						typesToFindableTypes.Add(interfaceType, new[] { unityObjectType });
						continue;
					}

					if(ContainsAnyTypesAssignableFrom(existingFindableTypes, unityObjectType))
					{
						continue;
					}

					if(Array.IndexOf(existingFindableTypes, unityObjectType) != -1)
					{
						continue;
					}

					reusableList.AddRange(existingFindableTypes);

					for(int j = existingFindableTypes.Length - 1; j >= 0; j--)
					{
						if(unityObjectType.IsAssignableFrom(existingFindableTypes[j]))
						{
							reusableList.RemoveAt(j);
						}
					}

					reusableList.Add(unityObjectType);
					typesToFindableTypes[interfaceType] = reusableList.ToArray();
					reusableList.Clear();
				}
			}

			foreach(var findableType in typesToFindableTypes.Keys)
			{
				if(typeof(Wrapper).IsAssignableFrom(findableType))
				{
					for(var type = findableType; type != typeof(Wrapper); type = type.BaseType)
					{
						if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Wrapper<>))
						{
							SetupWrappedTypeAndItsBaseTypesAndInterfaces(type.GetGenericArguments()[0], findableType);
							break;
						}
					}
				}
				else if(typeof(ScriptableWrapper).IsAssignableFrom(findableType))
				{
					for(var type = findableType; type != typeof(ScriptableWrapper); type = type.BaseType)
					{
						if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ScriptableWrapper<>))
						{
							SetupWrappedTypeAndItsBaseTypesAndInterfaces(type.GetGenericArguments()[0], findableType);
							break;
						}
					}
				}
			}

			foreach(var wrappedAndWrappers in typesToWrapperTypes)
			{
				RegisterTypeToFindableTypeLinks(wrappedAndWrappers.Key, wrappedAndWrappers.Value);
			}

			var list = Cached<Type>.list;
			foreach(var typeToFindableTypes in typesToFindableTypes)
			{
				foreach(var findableType in typeToFindableTypes.Value)
				{
					if(typeof(Component).IsAssignableFrom(findableType))
					{
						list.Add(findableType);
					}
				}

				if(list.Count == 0)
				{
					continue;
				}

				typesToComponentTypes[typeToFindableTypes.Key] = ToArrayAndClear(list);
			}

			typesToFindableTypes[typeof(object)] = new[] { typeof(Object) };
			typesToFindableTypes[typeof(Object)] = new[] { typeof(Object) };
			typesToComponentTypes[typeof(object)] = new[] { typeof(Component) };
			typesToComponentTypes[typeof(Object)] = new[] { typeof(Component) };
		}

		private static class Cached<T>
		{
			public static readonly List<T> list = new();
		}

		private static class SceneEqualityComparerCache<T>
		{
			public static readonly Predicate<T> Predicate = Equals;

			public static Scene scene;

			private static bool Equals(T obj) => obj is Component component && component.gameObject.scene == scene;
		}
	}
}