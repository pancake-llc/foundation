using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using Object = UnityEngine.Object;
using static UnityEngine.Object;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR && UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Pancake.Init
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
    /// </summary>
    public static class Find
    {
        internal static readonly Dictionary<object, IWrapper> wrappedInstances = new Dictionary<object, IWrapper>(128);

        internal static readonly Dictionary<Type, Type[]> typeToWrapperTypes = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, Type[]> typesToFindableTypes = new Dictionary<Type, Type[]>(64);
        private static readonly Dictionary<Type, Type[]> typesToComponentTypes = new Dictionary<Type, Type[]>(64);

        static Find() => Setup();

        /// <summary>
        /// Returns the first loaded object of type <typeparamref name="T">type</typeparamref>.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <param name="includeInactive"> Should <see cref="Components"/> on
        /// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
        /// be included in the search?
        /// </param>
        /// <returns>
        /// Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
        /// </returns>
        [CanBeNull]
        public static T Any<T>(bool includeInactive = false)
        {
            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
            {
                return default;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                Object found = ObjectByExactType(findableTypes[i], includeInactive);
                if(found != null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        /// <param name="includeInactive"> Should <see cref="Components"/> on
        /// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
        /// be included in the search?
        /// </param>
        /// <returns> <see langword="true"/> if an object was found; otherwise, <see langword="false"/>. </returns>
        [CanBeNull]
        public static bool Any<T>(out T result, bool includeInactive = false)
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
        /// Returns the first loaded <see cref="Any"/> of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Type of the object to find.
        /// <para>
        /// This can be the exact type of an <see cref="Any"/>, any type derived by it
        /// or the type of any interface it implements.
        /// </para>
        /// <para>
        /// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
        /// in which case it will try to find a wrapper for an object of that type in the loaded scenes.
        /// </param>
        /// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
        /// <returns> Loaded <see cref="Any"/> instance, if found; otherwise, <see langword="null"/>. </returns>
        [CanBeNull]
        public static object Any([NotNull] Type type, bool includeInactive = false)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                return null;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                var findableType = findableTypes[i];

                Object obj = ObjectByExactType(findableType, includeInactive);

                if(obj != null)
                {
                    return As(type, obj);
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <param name="match"> The Predicate<T> delegate that defines the conditions of the Object to search for. </param>
        /// <returns></returns>
        [CanBeNull]
        public static T Any<T>([NotNull] Predicate<T> match, bool includeInactive = false)
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
        /// Returns all loaded <see cref="Any">Objects</see> of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Type of the objects to find.
        /// <para>
        /// This can be the exact type of an <see cref="Any"/>, any type derived by it
        /// or the type of any interface it implements.
        /// </para>
        /// <para>
        /// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
        /// in which case it will try to find all wrappers for objects of that type from the loaded scenes.
        /// </param>
        /// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
        /// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
        [NotNull]
        public static object[] All([NotNull] Type type, bool includeInactive = false)
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
                if(!type.IsAssignableFrom(results[i].GetType()))
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
        [NotNull]
        public static T[] All<T>([NotNull] Predicate<T> match, bool includeInactive = false)
        {
            var list = Cached<T>.list;
            All(match, list, includeInactive);
            return ToArrayAndClear(list);
        }

        /// <typeparam name="T">
        /// Type of the objects to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <param name="results"> List into which found instances are added. </param>
        [NotNull]
        public static void All<T>([NotNull] Predicate<T> match, [NotNull] List<T> results, bool includeInactive = false)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> An array containing all found instances; if no results were found, then an empty array. </returns>
        [NotNull]
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        [NotNull]
        public static void All<T>([NotNull] List<T> results, bool includeInactive = false)
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
        /// This can be the exact type of an object, any type derived by it
        /// or the type of any interface it implements.
        /// </para>
        /// <para>
        /// Can also be the type of a <see cref="Init.Wrapper{TWrapped}">wrapped object</see>,
        /// in which case it will try to find all wrappers for objects of that type from the loaded scenes.
        /// </param>
        /// <param name="results"> List into which found instances are added. </param>
        /// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
        [NotNull]
        public static void All<T>([NotNull] Type type, [NotNull] List<T> results, bool includeInactive = false)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                return;
            }

            ObjectsByExactTypes(findableTypes, results, includeInactive);
        }

        /// <typeparam name="T">
        /// Type of the objects to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        public static bool InParents<T>([NotNull] GameObject gameObject, out T result, bool includeInactive = false)
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

        /// <typeparam name="T">
        /// Type of the objects to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        public static T InParents<T>([NotNull] GameObject gameObject, bool includeInactive = false)
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

        public static bool InParents([NotNull] GameObject gameObject, Type type, out object result, bool includeInactive = false)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                result = default;
                return false;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                result = InParentsByExactType<object>(gameObject, findableTypes[i], includeInactive);
                if(result != null)
                {
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <typeparam name="T">
        /// Type of the objects to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
        [NotNull]
        public static T[] AllInParents<T>([NotNull] GameObject gameObject, bool includeInactive = false)
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
        [NotNull]
        public static object[] AllInParents([NotNull] GameObject gameObject, Type type, bool includeInactive = false)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <param name="gameObject"> The <see cref="GameObject"/> to search along with all its parents. </param>
        /// <param name="results"> List into which found instances are added. </param>
        /// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
        public static void AllInParents<T>([NotNull] GameObject gameObject, [NotNull] List<T> results, bool includeInactive = false)
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
        public static void AllInParents<T>([NotNull] GameObject gameObject, [NotNull] Type type, [NotNull] List<T> results, bool includeInactive = false)
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
        /// <param name="includeInactive"> Should <see cref="Components"/> on
        /// <see cref="GameObject.activeInHierarchy">inactive</see> <see cref="GameObject">GameObjects</see>
        /// be included in the search?
        /// </param>
        /// <returns>
        /// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
        /// </returns>
        public static T InChildren<T>([NotNull] GameObject gameObject, bool includeInactive = false)
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

        public static bool InChildren<T>([NotNull] GameObject gameObject, out T result, bool includeInactive = false)
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

        public static object InChildren([NotNull] GameObject gameObject, [NotNull] Type type, bool includeInactive = false)
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
                if(component != null)
                {
                    return As(type, component);
                }
            }

            return default;
        }

        public static bool InChildren([NotNull] GameObject gameObject, [NotNull] Type type, out object result, bool includeInactive = false)
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
                if(component != null)
                {
                    result = As(type, component);
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
        [NotNull]
        public static T[] AllInChildren<T>([NotNull] GameObject gameObject, bool includeInactive = false)
        {
            var list = Cached<T>.list;
            AllInChildren(gameObject, list, includeInactive);
            return ToArrayAndClear(list);
        }

        /// <param name="results"> List into which found instances are added. </param>
        public static void AllInChildren<T>([NotNull] GameObject gameObject, [NotNull] List<T> results, bool includeInactive = false)
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

        /// <returns> Array of zero or more objects of the given <paramref name="type"/>. </returns>
        public static object[] AllInChildren([NotNull] GameObject gameObject, [NotNull] Type type, bool includeInactive = false)
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
            
            if(result != null || !includeInactive)
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
        public static bool WithTag(string tag, out GameObject result, bool includeInactive = false)
        {
            result = GameObject.FindWithTag(tag);

            if(result != null)
            {
                return true;
            }

            if(!includeInactive)
            {
                return false;
            }

            result = WithTagFromLoadedScenesIncludingInactive(tag);
            return result != null;
        }

        /// <summary>
        /// Returns object of type <typeparamref name="T"/> from the first loaded <see cref="GameObject"/>
        /// with the provided <paramref name="tag"/>, if any.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to find.
        /// <para>
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
            return gameObject != null ? In<T>(gameObject) : default;
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
        [NotNull]
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <param name="results"> List into which found components are added. </param>
        /// <param name="includeInactive"> Should components on inactive GameObjects be included in the search? </param>
        [CanBeNull]
        public static void Components<T>(List<T> results, bool includeInactive = false)
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

                #if UNITY_2020_1_OR_NEWER
                TryAddAs(results, FindObjectsOfType(findableType, includeInactive));
                #else
                if(!includeInactive)
                {
                    TryAddAs(results, FindObjectsOfType(findableType));
                }
                else
                {
                    ComponentsByExactTypeInLoadedScenesIncludingInactive(findableType, results);
                }
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
        [CanBeNull]
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
        [CanBeNull]
        public static GameObject GameObjectOf<T>(T attachedObject)
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
        [CanBeNull]
        public static GameObject GameObjectOf(object attachedObject)
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
        [CanBeNull]
        public static bool GameObjectOf(object attachedObject, out GameObject gameObject)
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
        /// <returns> object of type <typeparamref name="TWrapped"/>, if found. </returns>
        [CanBeNull]
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
        /// <returns> object of type <typeparamref name="TWrapped"/>, if found. </returns>
        [CanBeNull]
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
        /// <returns> object of type <typeparamref name="T"/>, if found. </returns>
        [CanBeNull]
        public static object WrappedObject([NotNull] Type type, bool includeInactive = false)
        {
            return Any(type, includeInactive) is IWrapper wrapper ? wrapper.WrappedObject : null;
        }

        /// <summary>
        /// Returns all objects of type <typeparamref name="TWrapped"/> wrapped by active loaded components of type <see cref="Init.Wrapper{TWrapped}"/>.
        /// </summary>
        /// <typeparam name="TWrapped"> Type of the wrapped objects. </typeparam>
        /// <param name="includeInactive"> Should inactive GameObjects be included in the search? </param>
        [NotNull]
        public static void AllWrappedObjects<TWrapped>(List<TWrapped> results, bool includeInactive = false) where TWrapped : class
        {
            Components(results, includeInactive);
        }

        /// <summary>
        /// Returns the <see cref="Object"/> which wraps the <paramref name="wrapped"/> <see cref="object"/>.
        /// </summary>
        /// <typeparam name="TWrapped"> <see cref="Type"/> of the wrapped object. </typeparam>
        /// <param name="wrapped"> The wrapped <see cref="object"/>. </param>
        /// <returns> <see cref="Object"/> that implements <see cref="IWrapper{TWrapped}"/>, if found. </returns>
        [CanBeNull]
        public static TWrapper WrapperOf<TWrapper, TWrapped>([NotNull] TWrapped wrapped) where TWrapper : Object, IWrapper<TWrapped>
        {
            return wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper) ? wrapper as TWrapper : null;
        }

        /// <summary>
        /// Returns the <see cref="Object"/> which wraps the <paramref name="wrapped"/> <see cref="object"/>.
        /// </summary>
        /// <typeparam name="TWrapped"> <see cref="Type"/> of the wrapped object. </typeparam>
        /// <param name="wrapped"> The wrapped <see cref="object"/>. </param>
        /// <returns> <see cref="Object"/> that implements <see cref="IWrapper{TWrapped}"/>, if found. </returns>
        [CanBeNull]
        public static IWrapper<TWrapped> WrapperOf<TWrapped>([NotNull] TWrapped wrapped)
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
        [CanBeNull]
        public static bool WrapperOf<TWrapped>([NotNull] TWrapped wrapped, out IWrapper<TWrapped> result)
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
        /// <returns> Object of type <see cref="WrapperOf{TWrapped}"/>, if found. </returns>
        [CanBeNull]
        public static IWrapper WrapperOf([NotNull] object wrapped)
        {
            return wrappedInstances.TryGetValue(wrapped, out IWrapper wrapper) ? wrapper : null;
        }

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
        [CanBeNull]
        public static bool WrapperOf([NotNull] object wrapped, out IWrapper result, bool includeInactive = false)
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
        /// if their class derives from a class of type <typeparamref name="TWrapped"/>,
        /// or their class implements an interface of type <typeparamref name="TWrapped"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> Component of type <see cref="WrapperOf{TWrapped}"/>, if found. </returns>
        [CanBeNull]
        public static IWrapper Wrapper<TWrapped>(bool includeInactive = false)
        {
            foreach(var wrappedAndWrapper in wrappedInstances)
            {
                if(wrappedAndWrapper.Key is TWrapped)
                {
                    if(!includeInactive && wrappedAndWrapper.Value is Component component && component != null && !component.gameObject.activeInHierarchy)
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
        /// <returns> Component of type <see cref="WrapperOf{TWrapped}"/>, if found. </returns>
        [CanBeNull]
        public static IWrapper Wrapper([NotNull] Type type, bool includeInactive = false)
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
        [NotNull]
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
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a base class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// </param>
        /// <param name="including">
        /// determines what <see cref="GameObject">GameObjects</see> related to the provided
        /// <paramref name="gameObject"/> are included in the search.
        /// </param>
        /// <returns>
        /// Instance of given <paramref name="type"/>, if found; otherwise, <see langword="null"/>.
        /// </returns>
        public static object In([NotNull] GameObject gameObject, Type type, Including including)
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
                if(parent != null && InParents(parent.gameObject, type, out result, includeInactive))
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
                    var rootGameObject = gameObject.transform.root.gameObject;
                    if(rootGameObject == gameObject)
                    {
                        return default;
                    }

                    return InChildren(rootGameObject, type, includeInactive);
                }

                SceneEqualityComparerCache<object>.scene = scene;
                return Any(SceneEqualityComparerCache<object>.Predicate, includeInactive);
            }

            return default;
        }

        [CanBeNull]
        public static object In([NotNull] GameObject gameObject, Type type)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                return null;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                var findableType = findableTypes[i];
                if(!typeof(Component).IsAssignableFrom(findableType))
                {
                    continue;
                }

                if(gameObject.TryGetComponent(findableType, out var component))
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// </typeparam>
        /// <param name="obj"> The object to search. </param>
        /// <returns>
        /// <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>.
        /// </returns>
        public static T In<T>([NotNull] object obj)
        {
            if(obj is GameObject gameObject)
            {
                return In<T>(gameObject);
            }

            var wrapper = WrapperOf(obj) as Component;
            if(wrapper == null)
            {
                return default;
            }
            gameObject = wrapper.gameObject;

            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
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

                if(gameObject.TryGetComponent(findableType, out var component))
                {
                    return As<T>(component);
                }
            }

            return default;
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
        public static bool In<T>([NotNull] object obj, out T result)
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

            var wrapper = WrapperOf(obj);

            if(wrapper is T wrapperAsT)
            {
                result = wrapperAsT;
                return true;
            }

            gameObject = wrapper.gameObject;
            if(gameObject != null)
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
        /// Objects match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a base class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// </param>
        /// <param name="result">
        /// When this method returns, contains object of type <typeparamref name="T"/>, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns> <see langword="true"/> if object of the given type was found; otherwise, <see langword="false"/>. </returns>
        public static bool In([NotNull] object obj, Type type, out object result)
        {
            if(obj is GameObject gameObject)
            {
                return In(gameObject, type, out result);
            }

            var wrapper = WrapperOf(obj) as Component;
            if(wrapper == null)
            {
                result = default;
                return false;
            }
            gameObject = wrapper.gameObject;

            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                result = default;
                return false;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                if(InGameObjectByExactType(gameObject, findableTypes[i], out result))
                {
                    return true;
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
        public static bool In<T>([NotNull] GameObject gameObject, out T result)
        {
            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
            {
                result = default;
                return false;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                if(InGameObjectByExactType(gameObject, findableTypes[i], out result))
                {
                    return true;
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
        public static bool In([NotNull] GameObject gameObject, Type type, out object result)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                result = default;
                return false;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                if(InGameObjectByExactType(gameObject, findableTypes[i], out result))
                {
                    return true;
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
        public static T In<T>([NotNull] GameObject gameObject)
        {
            var type = typeof(T);
            if(typeof(Component).IsAssignableFrom(type))
            {
                return gameObject.GetComponent<T>();
            }

            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                return default;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                if(InGameObjectByExactType(gameObject, findableTypes[i], out T result))
                {
                    return result;
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
        public static T In<T>([NotNull] GameObject gameObject, Including including)
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
                if(parent != null && InParents(parent.gameObject, out result, includeInactive))
                {
                    return result;
                }
            }

            if(HasFlag(including, Including.Scene))
            {
                #if UNITY_EDITOR
                if(gameObject.IsPartOfPrefabAssetOrOpenInPrefabStage())
                {
                    return InChildren<T>(gameObject.transform.root.gameObject, includeInactive);
                }
                #endif

                var scene = gameObject.scene;
                if(!scene.IsValid())
                {
                    var rootGameObject = gameObject.transform.root.gameObject;
                    if(rootGameObject == gameObject)
                    {
                        return default;
                    }

                    return InChildren<T>(rootGameObject, includeInactive);
                }

                SceneEqualityComparerCache<T>.scene = scene;
                return Any(SceneEqualityComparerCache<T>.Predicate, includeInactive);
            }

            return default;
        }

        /// <returns> Array of zero or more objects of type <typeparamref name="T"/>. </returns>
        public static T[] AllIn<T>([NotNull] GameObject gameObject)
        {
            var type = typeof(T);
            if(typeof(Component).IsAssignableFrom(type))
            {
                return gameObject.GetComponents<T>();
            }

            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
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
        public static object[] AllIn([NotNull] GameObject gameObject, Type type)
        {
            if(typeof(Component).IsAssignableFrom(type))
            {
                return gameObject.GetComponents(type);
            }

            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
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
        public static T[] AllIn<T>([NotNull] GameObject gameObject, Including including)
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
                    if(parent != null)
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
        public static object[] AllIn([NotNull] GameObject gameObject, Type type, Including including)
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

                SceneEqualityComparerCache<object>.scene = gameObject.scene;
                return All(SceneEqualityComparerCache<object>.Predicate, includeInactive);
            }

            if(HasFlag(including, Including.Children))
            {
                var list = Cached<object>.list;

                AllInChildren(gameObject, list, includeInactive);

                if(HasFlag(including, Including.Parents))
                {
                    Transform parent = gameObject.transform.parent;
                    if(parent != null)
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
        public static void AllIn<T>([NotNull] GameObject gameObject, List<T> results)
        {
            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
            {
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

        /// <typeparam name="T">
        /// Generic type of the <see cref="List{T}"/> into which found results will be placed.
        /// </typeparam>
        /// <param name="results"> List into which found instances are added. </param>
        public static void AllIn<T>([NotNull] GameObject gameObject, Type type, List<T> results)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
        public static T Asset<T>()
        {
            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
            {
                return default;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                var type = findableTypes[i];
                var guids = AssetDatabase.FindAssets("t:" + type.Name);
                for(int g = guids.Length - 1; g >= 0; g--)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    if(instance != null)
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
        /// Loads the <see cref="MonoScript"/> asset that contains the definition for the <paramref name="classType"/>.
        /// <para>
        /// This method is only available in the editor.
        /// </para>
        /// <param name="classType"> Type of the class whose script asset to load. </param>
        /// <returns> The loaded script asset, if found; otherwise, <see langword="null"/>. </returns>
        public static MonoScript Script(Type classType)
        {
			string name = classType.Name;

			if(classType.IsGenericType)
			{
				// Parse out generic type information from generic type name
				int i = name.IndexOf('`');
				if(i != -1)
				{
					name = name.Substring(0, i);
				}

				// Additionally, convert generic types to their generic type defitions.
				// E.g. List<string> to List<>.
				if(!classType.IsGenericTypeDefinition)
				{
					classType = classType.GetGenericTypeDefinition();
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
				var filename = System.IO.Path.GetFileNameWithoutExtension(path);
				if(string.Equals(filename, name, StringComparison.OrdinalIgnoreCase))
				{
					var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
					var scriptClassType = scriptAsset.GetClass();
					if(scriptClassType == classType)
					{
						return scriptAsset;
					}

					if(scriptClassType == null)
					{
						fallback = scriptAsset;
					}
					
					#if DEV_MODE && DEBUG_FIND_SCRIPT
					Debug.Log($"FindScriptFile({TypeUtility.ToString(classType)}) ignoring file @ \"{path}\" because MonoScript.GetClass() result {TypeUtility.ToString(scriptClassType)} did not match classType.");
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
					var filename = System.IO.Path.GetFileNameWithoutExtension(path);
					if(filename.Length != name.Length) // skip testing exact matches a second time
					{
						var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
						var scriptClassType = scriptAsset.GetClass();
						if(scriptClassType == classType)
						{
							return scriptAsset;
						}

						#if DEV_MODE && DEBUG_FIND_SCRIPT
						Debug.LogWarning($"FindScriptFile({TypeUtility.ToString(classType)}) second pass: ignoring partial match @ \"{path}\" because MonoScript.GetClass() result {TypeUtility.ToString(scriptClassType)} did not match classType.");
						#endif
					}
				}
			}

			// If was unable to verify correct script class type using MonoScript.GetClass()
			// but there was a probable match whose GetClass() returned null (seems to happen
			// with all generic types), then return that.
			if(fallback != null)
			{
				#if DEV_MODE && DEBUG_FIND_SCRIPT
				Debug.LogWarning($"FindScriptFile({TypeUtility.ToString(classType)}) returning fallback result @ \"{AssetDatabase.GetAssetPath(fallback)}\".");
				#endif

				return fallback;
			}

			#if DEV_MODE
			Debug.Log($"FindScriptFile({TypeUtility.ToString(classType)}) failed to find MonoScript for classType {TypeUtility.ToString(classType)} AssetDatabase.FindAssets(\"{name} t:MonoScript\") returned {count} results.");
			#endif

			return null;
        }
        #endif

        #if UNITY_EDITOR
        /// <summary>
        /// Loads the <see cref="MonoScript"/> asset that contains the definition for the <paramref name="classType"/>.
        /// <para>
        /// This method is only available in the editor.
        /// </para>
        /// <param name="classType"> Type of the class whose script asset to load. </param>
        /// <param name="result">
        /// When this method returns, contains loaded script asset, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns> <see langword="true"/> if a script asset was found; otherwise, <see langword="false"/>. </returns>
        public static bool Script(Type classType, out MonoScript result)
		{
            result = Script(classType);
            return result != null;
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        public static bool Asset<T>(out T result)
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
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    if(instance != null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        public static bool Asset(Type type, out object result)
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
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var instance = AssetDatabase.LoadAssetAtPath(assetPath, findableType);
                    if(instance != null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
        public static IEnumerable<T> Assets<T>()
        {
            if(!typesToFindableTypes.TryGetValue(typeof(T), out var findableTypes))
            {
                yield break;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                var type = findableTypes[i];
                var guids = AssetDatabase.FindAssets("t:" + type.Name);
                for(int g = guids.Length - 1; g >= 0; g--)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var instance = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    if(instance != null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </typeparam>
        /// <returns> The loaded asset, if found; otherwise, <see langword="null"/>. </returns>
        public static IEnumerable<object> Assets(Type type)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var findableTypes))
            {
                yield break;
            }

            for(int i = findableTypes.Length - 1; i >= 0; i--)
            {
                var findableType = findableTypes[i];
                var guids = AssetDatabase.FindAssets("t:" + findableType.Name);
                for(int g = guids.Length - 1; g >= 0; g--)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var instance = AssetDatabase.LoadAssetAtPath(assetPath, findableType);
                    if(instance != null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        public static bool Addressable<T>([NotNull] string key, [CanBeNull] out T result)
        {
            Object obj = LoadAddressable(key);

            if(obj == null)
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
        /// if their class derives from a class of this type,
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
        public static bool Addressable([NotNull] Type type, [NotNull] string key, [CanBeNull] out object result)
        {
            Object obj = LoadAddressable(key);

            if(obj == null)
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
        /// if their class derives from a class of this type,
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
        public static object Addressable([NotNull] Type type, [NotNull] string key)
        {
            Object obj = LoadAddressable(key);

            if(obj == null)
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
        /// if their class derives from a class of this type,
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
        public static T Addressable<T>([NotNull] string key)
        {
            Object obj = LoadAddressable(key);

            if(obj == null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </param>
        /// <param name="path"> Path to the target resource to load. </param>
        /// <param name="result">
        /// When this method returns, contains loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns> <see langword="true"/> if an asset was found; otherwise, <see langword="false"/>. </returns>
        public static bool Resource(Type type, string path, out object result)
        {
            Object obj = Resources.Load(path, typeof(Object));
            result = As(type, obj);
            return !(result is null);
        }

        /// <summary>
        /// Loads the asset of the given <paramref name="type"/> stored at <paramref name="path"/> in a Resources folder.
        /// </summary>
        /// <param name="type">
        /// Type of the asset to load.
        /// <para>
        /// Assets match the search criteria if their class is of type <typeparamref name="T"/>,
        /// if their class derives from a class of type <typeparamref name="T"/>,
        /// or their class implements an interface of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// Only objects whose classes derive from <see cref="Component"/> or <see cref="ScriptableObject"/>
        /// or objects that are wrapped by a <see cref="IWrapper{T}"/> can be returned.
        /// </para>
        /// </param>
        /// <param name="path"> Path to the target resource to load. </param>
        /// <returns> Loaded <typeparamref name="T"/> instance, if found; otherwise, <see langword="null"/>. </returns>
        public static object Resource(Type type, string path)
        {
            Object obj = Resources.Load(path, typeof(Object));
            if(obj == null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
            if(obj == null)
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
        /// if their class derives from a class of type <typeparamref name="T"/>,
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
        public static bool Resource<T>(string path, out T result)
        {
            Object obj = Resources.Load(path, typeof(Object));
            if(obj == null)
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
        [CanBeNull, System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Object LoadAddressable([NotNull] string key)
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
        [CanBeNull, System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Object LoadAddressableInEditor([NotNull] string key)
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

        private static void Setup()
        {
            SetupUnityObjectAndInterfaceTypes();

            SetupWrappedTypes();

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

            SetupObjectTypes();
        }

        private static void SetupUnityObjectAndInterfaceTypes()
        {
            var reusableList = new List<Type>(8);
            var unityObjectTypes = TypeUtility.GetDerivedTypes(typeof(Object), typeof(Object).Assembly, typeof(object).Assembly);

            foreach(var unityObjectType in unityObjectTypes)
            {
                SetupUnityObjectType(reusableList, unityObjectType);
            }
        }

        private static void SetupUnityObjectType(List<Type> reusableList, Type unityObjectType)
        {
            if(unityObjectType.IsGenericTypeDefinition)
            {
                return;
            }

            // All of Unity's internal search methods such as FindObjectOfType
            // work with UnityEngine.Object types as is.
            typesToFindableTypes[unityObjectType] = new Type[] { unityObjectType };

            // FindObjectOfType doesn't work with interface types directly,
            // so we need to create a map from interface types to UnityEngine.Object
            // types that implement it.
            var interfaceTypes = unityObjectType.GetInterfaces();
            for(int i = interfaceTypes.Length - 1; i >= 0; i--)
            {
                SetupInterfaceType(reusableList, unityObjectType, interfaceTypes[i]);
            }
        }

        private static void SetupWrappedTypes()
        {
            if(!typesToFindableTypes.TryGetValue(typeof(IWrapper), out var wrapperTypes))
            {
                return;
            }

            foreach(var wrapperType in wrapperTypes)
            {
                SetupWrappedType(wrapperType);
            }

            foreach(var wrappedAndWrappers in typeToWrapperTypes)
            {
                RegisterTypeToFindableTypeLinks(wrappedAndWrappers.Key, wrappedAndWrappers.Value);
            }
        }

        private static void SetupObjectTypes()
        {
            typesToFindableTypes[typeof(object)] = new Type[] { typeof(Object) };
            typesToFindableTypes[typeof(Object)] = new Type[] { typeof(Object) };
            typesToComponentTypes[typeof(object)] = new Type[] { typeof(Component) };
            typesToComponentTypes[typeof(Object)] = new Type[] { typeof(Component) };
        }

        private static void SetupInterfaceType(List<Type> reusableList, Type unityObjectType, Type interfaceType)
        {
            if(interfaceType.IsGenericTypeDefinition)
            {
                return;
            }

            if(!typesToFindableTypes.TryGetValue(interfaceType, out var existingFindableTypes))
            {
                typesToFindableTypes[interfaceType] = new Type[] { unityObjectType };
                return;
            }

            if(ContainsAnyTypesAssignableFrom(existingFindableTypes, unityObjectType))
            {
                return;
            }

            reusableList.AddRange(existingFindableTypes);

            for(int i = existingFindableTypes.Length - 1; i >= 0; i--)
            {
                if(unityObjectType.IsAssignableFrom(existingFindableTypes[i]))
                {
                    reusableList.RemoveAt(i);
                }
            }

            reusableList.Add(unityObjectType);

            typesToFindableTypes[interfaceType] = reusableList.ToArray();
            reusableList.Clear();
        }

        private static bool InGameObjectByExactType(GameObject gameObject, Type findableType, out Component result)
        {
            if(!typeof(Component).IsAssignableFrom(findableType))
            {
                result = null;
                return false;
            }

            if(gameObject.TryGetComponent(findableType, out result))
            {
                return true;
            }

            result = null;
            return false;
        }

        private static bool InGameObjectByExactType<T>(GameObject gameObject, Type findableType, out T result)
        {
            if(!typeof(Component).IsAssignableFrom(findableType))
            {
                result = default;
                return false;
            }

            if(gameObject.TryGetComponent(findableType, out Component component))
            {
                result = As<T>(component);
                return true;
            }

            result = default;
            return false;
        }

        private static T InChildrenByExactType<T>([NotNull] GameObject gameObject, Type findableType, bool includeInactive)
        {
            var component = gameObject.GetComponentInChildren(findableType, includeInactive);
            if(component != null)
            {
                return As<T>(component);
            }

            return default;
        }

        private static T InParentsByExactType<T>([NotNull] GameObject gameObject, Type findableType, bool includeInactive)
        {
            if(!typeof(Component).IsAssignableFrom(findableType))
            {
                return default;
            }

            #if UNITY_2020_1_OR_NEWER
            var component = gameObject.GetComponentInParent(findableType, includeInactive);
            return As<T>(component);
            #else
            if(!includeInactive)
            {
                var component = gameObject.GetComponentInParent(findableType);
                return As<T>(component);
            }

            for(var transform = gameObject.transform; transform != null; transform = transform.parent)
            {
                if(transform.TryGetComponent(findableType, out var component))
                {
                    return As<T>(component);
                }
            }

            return default;
            #endif
        }

        private static Object ObjectByExactType(Type findableType, bool includeInactive = false)
        {
            #if UNITY_2020_1_OR_NEWER
            return FindObjectOfType(findableType, includeInactive);
            #else
            if(!includeInactive || typeof(ScriptableObject).IsAssignableFrom(findableType))
            {
                return FindObjectOfType(findableType);
            }
            return ComponentByExactTypeInLoadedScenesIncludingInactive(findableType);
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
            #if DEBUG
            if(findableType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Finding objects using generic type definitions is not supported.", nameof(findableType));
            }
            #endif

            #if UNITY_2020_1_OR_NEWER
            TryAddAs(results, FindObjectsOfType(findableType, includeInactive));
            #else
            if(!includeInactive || typeof(ScriptableObject).IsAssignableFrom(findableType))
            {
                TryAddAs(results, FindObjectsOfType(findableType));
                return;
            }

            ComponentsByExactTypeInLoadedScenesIncludingInactive(findableType, results);
            #endif
        }
        
        private static Object[] ObjectsByExactType(Type findableType, bool includeInactive)
        {
            #if DEBUG
            if(findableType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Finding objects using generic type definitions is not supported.", nameof(findableType));
            }
            #endif

            #if UNITY_2020_1_OR_NEWER
            return FindObjectsOfType(findableType, includeInactive);
            #else
            if(!includeInactive)
            {
                return FindObjectsOfType(findableType);
            }

            var list = Cached<Object>.list;
            ComponentsByExactTypeInLoadedScenesIncludingInactive(findableType, list);
            return ToArrayAndClear(list);
            #endif
        }

        #if !UNITY_2020_1_OR_NEWER
        private static void ComponentsByExactTypeInLoadedScenesIncludingInactive<T>([NotNull] Type findableType, [NotNull] List<T> results)
        {
            #if DEBUG
            if(findableType.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"Finding objects using generic type definitions is not supported.", nameof(findableType));
            }
            #endif

            var list = Cached<GameObject>.list;

            for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
            {
                var scene = SceneManager.GetSceneAt(s);
                scene.GetRootGameObjects(list);
                for(int g = list.Count - 1; g >= 0; g--)
                {
                    TryAddAs(results, list[g].GetComponentsInChildren(findableType, true));
                }
                list.Clear();
            }
        }
        #endif

        #if !UNITY_2020_1_OR_NEWER
        private static Component ComponentByExactTypeInLoadedScenesIncludingInactive([NotNull] Type type)
        {
            var list = Cached<GameObject>.list;

            for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
            {
                var scene = SceneManager.GetSceneAt(s);
                scene.GetRootGameObjects(list);
                for(int g = list.Count - 1; g >= 0; g--)
                {
                    var component = list[g].GetComponentInChildren(type, true);
                    if(component != null)
                    {
                        return component;
                    }
                }
                list.Clear();
            }
            return null;
        }
        #endif

        private static bool HasFlag(Including value, Including flag)
		{
			return (value & flag) == flag;
		}

        private static void RegisterTypeToFindableTypeLinks(Type type, Type[] addFindableTypes)
        {
            if(!typesToFindableTypes.TryGetValue(type, out var existingFindableTypes))
            {
                typesToFindableTypes[type] = addFindableTypes;
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

        private static bool ContainsAnyTypesAssignableFrom(IList<Type> list, Type type)
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

        private static void RemoveTypesDerivedFrom(List<Type> list, Type type)
        {
            for(int i = list.Count - 1; i >= 0; i--)
            {
                if(type.IsAssignableFrom(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        private static void SetupWrappedType([NotNull] Type wrapperType)
        {
            var wrappedType = GetWrappedType(wrapperType);
            if(wrappedType != null)
            {
                SetupWrappedTypeAndItsBaseTypesAndInterfaces(wrappedType, wrapperType);
            }
        }

        private static Type GetWrappedType(Type wrapperType)
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

        private static void SetupWrappedTypeAndItsBaseTypesAndInterfaces([NotNull] Type wrappedType, [NotNull] Type wrapperType)
        {
            // In theory it is possible that a type has more than one wrapper.
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

        private static void RegisterWrappedToWrapperLink(Type wrappedType, Type wrapperType)
        {
            if(!typeToWrapperTypes.TryGetValue(wrappedType, out var wrapperTypes))
            {
                typeToWrapperTypes[wrappedType] = new Type[] { wrapperType };
                return;
            }

            // If type has already been registered do nothing.
            if(Array.IndexOf(wrapperTypes, wrapperType) != -1)
            {
                return;
            }

            // In theory it is possible that a type has more than one wrapper.
            int index = wrapperTypes.Length;
            Array.Resize(ref wrapperTypes, index + 1);
            wrapperTypes[index] = wrapperType;
            typeToWrapperTypes[wrappedType] = wrapperTypes;
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

        private static class Cached<T>
        {
            public static readonly List<T> list = new List<T>();
        }

        private static class SceneEqualityComparerCache<T>
        {
            public static readonly Predicate<T> Predicate = Equals;

            public static Scene scene;

            private static bool Equals(T obj) => obj is Component component && component.gameObject.scene == scene;
        }
    }
}