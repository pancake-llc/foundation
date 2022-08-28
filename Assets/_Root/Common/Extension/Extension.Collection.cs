using System;
using System.Collections.Generic;
using System.Linq;

namespace Pancake.Core
{
    public static partial class C
    {
        /// <summary>
        /// Rounds an int to the closest int in an array (array has to be sorted)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int RoundIntToArray(int value, int[] array)
        {
            int min = 0;
            if (array[min] >= value) return array[min];

            int max = array.Length - 1;
            if (array[max] <= value) return array[max];

            while (max - min > 1)
            {
                int mid = (max + min) / 2;

                if (array[mid] == value)
                {
                    return array[mid];
                }
                else if (array[mid] < value)
                {
                    min = mid;
                }
                else
                {
                    max = mid;
                }
            }

            if (array[max] - value <= value - array[min])
            {
                return array[max];
            }
            else
            {
                return array[min];
            }
        }

        /// <summary>
        /// Rounds a float to the closest float in an array (array has to be sorted)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static float RoundFloatToArray(float value, float[] array)
        {
            int min = 0;
            if (array[min] >= value) return array[min];

            int max = array.Length - 1;
            if (array[max] <= value) return array[max];

            while (max - min > 1)
            {
                int mid = (max + min) / 2;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (array[mid] == value)
                {
                    return array[mid];
                }
                else if (array[mid] < value)
                {
                    min = mid;
                }
                else
                {
                    max = mid;
                }
            }

            if (array[max] - value <= value - array[min])
            {
                return array[max];
            }
            else
            {
                return array[min];
            }
        }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this System.Collections.Generic.ICollection<T> source) { return source == null || source.Count == 0; }

        /// <summary>
        /// Check null for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this T[] source) { return source == null || source.Length == 0; }

        /// <summary>
        /// Ensure <paramref name="source"/> have data for <paramref name="key"/>
        /// otherwise asign the value return form expression <paramref name="newValue"/> to <paramref name="key"/>
        /// </summary>
        /// <param name="source">dictionary</param>
        /// <param name="key">key exist in <paramref name="source"/>></param>
        /// <param name="newValue">expression func retun value <typeparamref name="TValue"/>></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue Ensure<TKey, TValue>(this System.Collections.Generic.Dictionary<TKey, TValue> source, TKey key, Func<TValue> newValue) where TValue : class
        {
            source.TryGetValue(key, out var result);
            if (result != null) return result;

            result = newValue();
            source[key] = result;
            return result;
        }

        /// <summary>
        /// Ensure <paramref name="source"/> have data for <paramref name="key"/>
        /// do not use Ensure for performance and GC reasons
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static List<TValue> EnsureList<TKey, TValue>(
            this IDictionary<TKey, System.Collections.Generic.List<TValue>> source,
            TKey key)
        {
            //do not use Ensure for performance and GC reasons
            source.TryGetValue(key, out var result);
            if (result != null) return result;

            result = new System.Collections.Generic.List<TValue>();
            source[key] = result;
            return result;
            //return Ensure(source, key, () => new List<TV>());
        }

        /// <summary>
        /// Return value of <paramref name="key"/> in dictionary <paramref name="source"/>
        /// if <paramref name="key"/> dose not data return null
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this System.Collections.Generic.IDictionary<TKey, TValue> source, TKey key)
        {
            source.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Return value of <paramref name="key"/> in dictionary <paramref name="source"/>
        /// if <paramref name="key"/> dose not data return <paramref name="defaultValue"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue Get<TKey, TValue>(this System.Collections.Generic.Dictionary<TKey, TValue> source, TKey key, TValue defaultValue)
        {
            return !source.TryGetValue(key, out var value) ? defaultValue : value;
        }

        /// <summary>
        /// Return value of <paramref name="key"/> in dictionary <paramref name="source"/>
        /// if <paramref name="key"/> dose not data return null
        /// </summary>
        /// <param name="source">if null retun default value for <typeparamref name="TValue"/>></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetNullable<TKey, TValue>(this System.Collections.Generic.Dictionary<TKey, TValue> source, TKey key)
        {
            return source == null ? default : Get(source, key);
        }

        /// <summary>
        /// Iterate over all elements in the <paramref name="source"/> and call <paramref name="action"/> for it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this System.Collections.Generic.List<T> source, Action<T> action)
        {
            if (source == null) return;
            for (var i = 0; i < source.Count; i++) action(source[i]);
        }

        /// <summary>
        /// equals value of <paramref name="source"/> and <paramref name="target"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Equal<T>(this System.Collections.Generic.List<T> source, System.Collections.Generic.List<T> target)
        {
            if (source == null && target == null) return true;

            if (source == null || target == null) return false;

            if (source.Count != target.Count) return false;

            if (source.Count == 0) return true;

            var comparer = System.Collections.Generic.EqualityComparer<T>.Default;
            var equal = true;
            for (var i = 0; i < source.Count; i++)
            {
                var val = source[i];
                if (comparer.Equals(target[i], val)) continue;
                equal = false;
                break;
            }

            return equal;
        }

        /// <summary>
        /// Convert to array
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static byte[] ToArray(this ArraySegment<byte> segment)
        {
            if (segment.Count == 0) return Array.Empty<byte>(); // new byte[0];
            if (segment.Array == null) return null;
            var result = new byte[segment.Count];
            Buffer.BlockCopy(segment.Array,
                segment.Offset,
                result,
                0,
                segment.Count);
            return result;
        }

        /// <summary>
        /// Indicate the location of <paramref name="item"/> in <paramref name="source"/>
        /// if not exists in <paramref name="source"/>, return -1
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int IndexOf<T>(this T[] source, in T item) where T : IEquatable<T>
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].Equals(item)) return i;
            }

            return -1;
        }

        /// <summary>
        /// Indicate the location of <paramref name="item"/> in <paramref name="source"/>
        /// if not exists in <paramref name="source"/>, return -1
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int IndexOf<T>(this System.Collections.Generic.List<T> source, in T item) where T : IEquatable<T>
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].Equals(item)) return i;
            }

            return -1;
        }
        
        /// <summary>
        /// Gets collection count if <see cref="source"/> is materialized, otherwise null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int? CountIfMaterialized<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source == Enumerable.Empty<T>()) return 0;
            if (source == Array.Empty<T>()) return 0;
            if (source is ICollection<T> a) return a.Count;
            if (source is IReadOnlyCollection<T> b) return b.Count;

            return null;
        }


        /// <summary>
        /// Gets collection if <see cref="source"/> is materialized, otherwise ToArray();ed collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="nullToEmpty"></param>
        public static IEnumerable<T> Materialize<T>(this IEnumerable<T> source, bool nullToEmpty = true)
        {
            if (source == null)
            {
                if (nullToEmpty)
                    return Enumerable.Empty<T>();
                throw new ArgumentNullException(nameof(source));
            }

            if (source is ICollection<T>) return source;
            if (source is IReadOnlyCollection<T>) return source;
            return source.ToArray();
        }
    }
}