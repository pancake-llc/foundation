using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pancake.Common
{
    public static partial class C
    {
        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this List<T> source) { return source == null || source.Count == 0; }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T[] source) { return source == null || source.Length == 0; }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> source) { return source == null || source.Keys.Count == 0; }

        /// <summary>
        /// shuffle element in array <paramref name="source"/> parameter
        /// </summary>
        /// <param name="source">array</param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// <para>Indicates the random value in the <paramref name="source"/>. If <paramref name="source"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandom<T>(this T[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Length == 0 ? default : source[UnityEngine.Random.Range(0, source.Length)];
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="source"/>. If <paramref name="source"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T, int) PickRandomWithIndex<T>(this T[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int index = UnityEngine.Random.Range(0, source.Length);
            return source.Length == 0 ? (default, -1) : (source[index], index);
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="source"/>. If <paramref name="source"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickRandom<T>(this List<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Count == 0 ? default : source[UnityEngine.Random.Range(0, source.Count)];
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="source"/>. If <paramref name="source"/> is empty return default vaule of T</para>
        /// 
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T, int) PickRandomWithIndex<T>(this List<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var index = UnityEngine.Random.Range(0, source.Count);
            return source.Count == 0 ? (default, -1) : (source[index], index);
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="source"/> and also remove that element. If <paramref name="source"/> is empty return default vaule of T</para>
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PopRandom<T>(this List<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source.Count == 0) return default;
            int i = UnityEngine.Random.Range(0, source.Count);
            var value = source[i];
            source.RemoveAt(i);
            return value;
        }

        /// <summary>
        /// <para>Indicates the random value in the <paramref name="source"/> and also remove that element. If <paramref name="source"/> is empty return default vaule of T</para>
        /// <para>Return a random value of index within [0..maxExclusive) (Read Only).</para>
        /// <para>maxExclusive = length of <paramref name="source"/></para>
        /// <para>maxExcusive is exclusive, so for example <paramref name="source"/> has 10 element will return a value of index between 0 and 9, each with approximately equal probability.</para>
        ///
        /// <para>If maxExclusive equal 0, value of index 0 will be returned.</para>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// http://docs.unity3d.com/ScriptReference/Random.Range.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (T, int) PopRandomWithIndex<T>(this List<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source.Count == 0) return default;
            int i = UnityEngine.Random.Range(0, source.Count);
            var value = source[i];
            source.RemoveAt(i);
            return (value, i);
        }

        /// <summary>
        /// swap element in array <paramref name="source"/> parameter (<paramref name="oldIndex"/> swap for <paramref name="newIndex"/>)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldIndex">old index</param>
        /// <param name="newIndex">new index</param>
        /// <typeparam name="T">type</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Added the ability to access all elements in Linq
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ForEach<T>(this T[] source, Action<T> action)
        {
            for (int i = source.Length - 1; i >= 0; i--)
            {
                action(source[i]);
            }

            return source;
        }

        /// <summary>
        /// Added the ability to access all elements in Linq
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ForEach<T>(this List<T> source, Action<T> action)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                action(source[i]);
            }

            return source;
        }

        /// <summary>
        /// Added the ability to access all elements in Linq (including index)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ForEach<T>(this T[] source, Action<T, int> action)
        {
            for (int i = source.Length - 1; i >= 0; i--)
            {
                action(source[i], i);
            }

            return source;
        }

        /// <summary>
        /// Added the ability to access all elements in Linq (including index)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> ForEach<T>(this List<T> source, Action<T, int> action)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                action(source[i], i);
            }

            return source;
        }

        /// <summary> Eliminate an items range. </summary>
        /// <param name="source"> The list. </param>
        /// <param name="entries">Elements to be removed. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Removes<T>(this List<T> source, List<T> entries)
        {
            foreach (var item in entries) source.Remove(item);
        }

        /// <summary> Eliminate an items range. </summary>
        /// <param name="source"> The list. </param>
        /// <param name="entries">Elements to be removed. </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Removes<T>(this List<T> source, T[] entries)
        {
            foreach (var item in entries) source.Remove(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Adds<T>(this List<T> source, List<T> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                source.Add(entries[i]);
            }

            return source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Adds<T>(this List<T> source, T[] entries)
        {
            foreach (var e in entries)
            {
                source.Add(e);
            }

            return source;
        }
    }
}