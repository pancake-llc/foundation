using System;
using System.Collections.Generic;
using System.Linq;

namespace Pancake
{
    public static partial class C
    {
        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this List<T> source) { return source == null || source.Count == 0; }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this T[] source) { return source == null || source.Length == 0; }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> source) { return source == null || source.Keys.Count == 0; }

        /// <summary>
        /// shuffle element in array <paramref name="source"/> parameter
        /// </summary>
        /// <param name="source">array</param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this T[] source)
        {
            int n = source.Length;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n);
                (source[k], source[n]) = (source[n], source[k]);
            }
        }

        /// <summary>
        /// shuffle element in <paramref name="source"/> parameter.
        /// </summary>
        /// <param name="source">IList</param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this List<T> source)
        {
            int n = source.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n);
                (source[k], source[n]) = (source[n], source[k]);
            }
        }

        /// <summary>
        /// shuffle element in dictionary <paramref name="source"/> parameter.
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns>new dictionary (shuffled)</returns>
        public static IDictionary<T1, T2> Shuffle<T1, T2>(this IDictionary<T1, T2> source)
        {
            var keys = source.Keys.ToArray();
            var values = source.Values.ToArray();

            int n = source.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n);
                (keys[k], keys[n]) = (keys[n], keys[k]);
                (values[k], values[n]) = (values[n], values[k]);
            }

            return MakeDictionary(keys, values);
        }

        /// <summary>
        /// make dictionay from elements has <paramref name="values"/> parameter with <paramref name="keys"/> parameter
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/> parameter is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> parameter is null</exception>
        /// <exception cref="ArgumentException">Size <paramref name="keys"/> and size <paramref name="values"/> diffirent!</exception>
        public static IDictionary<TKey, TValue> MakeDictionary<TKey, TValue>(this TKey[] keys, TValue[] values)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (keys.Length != values.Length) throw new ArgumentException("Size keys and size values diffirent!");

            IDictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (var i = 0; i < keys.Length; i++)
            {
                result.Add(keys[i], values[i]);
            }

            return result;
        }

        /// <summary>
        /// make dictionay from elements has <paramref name="values"/> parameter with <paramref name="keys"/> parameter
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="keys"/> parameter is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> parameter is null</exception>
        /// <exception cref="ArgumentException">Size <paramref name="keys"/> and size <paramref name="values"/> diffirent!</exception>
        public static IDictionary<TKey, TValue> MakeDictionary<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (keys.Count != values.Count) throw new ArgumentException("Size keys and size values diffirent!");

            IDictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (var i = 0; i < keys.Count; i++)
            {
                result.Add(keys[i], values[i]);
            }

            return result;
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/>. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static T PickRandom<T>(this T[] collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.Length == 0 ? default : collection[UnityEngine.Random.Range(0, collection.Length)];
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/>. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static (T, int) PickRandomWithIndex<T>(this T[] collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            int index = UnityEngine.Random.Range(0, collection.Length);
            return collection.Length == 0 ? (default, -1) : (collection[index], index);
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/>. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static T PickRandom<T>(this List<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.Count == 0 ? default : collection[UnityEngine.Random.Range(0, collection.Count)];
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/>. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static (T, int) PickRandomWithIndex<T>(this List<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var index = UnityEngine.Random.Range(0, collection.Count);
            return collection.Count == 0 ? (default, -1) : (collection[index], index);
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/> and also remove that element. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static T PopRandom<T>(this List<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0) return default;
            int i = UnityEngine.Random.Range(0, collection.Count);
            var value = collection[i];
            collection.RemoveAt(i);
            return value;
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="collection"/> and also remove that element. If <paramref name="collection"/> is empty return default vaule of T</para>
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="collection"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="collection"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        public static (T, int) PopRandomWithIndex<T>(this List<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0) return default;
            int i = UnityEngine.Random.Range(0, collection.Count);
            var value = collection[i];
            collection.RemoveAt(i);
            return (value, i);
        }

        /// <summary>
        /// swap element in array <paramref name="source"/> parameter (<paramref name="oldIndex"/> swap for <paramref name="newIndex"/>)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldIndex">old index</param>
        /// <param name="newIndex">new index</param>
        /// <typeparam name="T">type</typeparam>
        public static void Swap<T>(this T[] source, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex > source.Length || newIndex > source.Length)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Index out of range!");
#endif
                return;
            }

            if (oldIndex == newIndex) return;
            (source[oldIndex], source[newIndex]) = (source[newIndex], source[oldIndex]);
        }

        /// <summary>
        /// swap element in array <paramref name="source"/> parameter (<paramref name="oldIndex"/> swap for <paramref name="newIndex"/>)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldIndex">old index</param>
        /// <param name="newIndex">new index</param>
        /// <typeparam name="T">type</typeparam>
        public static void Swap<T>(this List<T> source, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex > source.Count || newIndex > source.Count)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Index out of range!");
#endif
                return;
            }

            if (oldIndex == newIndex) return;
            (source[oldIndex], source[newIndex]) = (source[newIndex], source[oldIndex]);
        }
    }
}