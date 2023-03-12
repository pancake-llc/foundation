using System.Collections.Generic;

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
    }
}