using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pancake
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
        public static bool IsNullOrEmpty<T>(this List<T> source) { return source == null || source.Count == 0; }

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
        public static List<TValue> EnsureList<TKey, TValue>(this IDictionary<TKey, System.Collections.Generic.List<TValue>> source, TKey key)
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

        /// <summary>
        /// Set values of elements in an array.
        /// </summary>
        /// <param name="index"> Start index. </param>
        /// <param name="count"> A negative or zero value means all elements after start index. </param>
        public static void SetValues<T>(this T[] array, T value = default, int index = 0, int count = 0)
        {
            int lastIndex = count > 0 ? (index + count) : array.Length;
            while (index < lastIndex) array[index++] = value;
        }


        /// <summary>
        /// Set values of elements in a 2d array.
        /// </summary>
        /// <param name="beginRowIndex">  Start row index </param>
        /// <param name="beginColIndex"> Start col index </param>
        /// <param name="endRowIndex"> A negative or zero value means all elements after start index. </param>
        /// <param name="endColIndex"> A negative or zero value means all elements after start index. </param>
        public static void SetValues<T>(this T[,] array, T value = default, int beginRowIndex = 0, int beginColIndex = 0, int endRowIndex = 0, int endColIndex = 0)
        {
            if (endRowIndex <= 0) endRowIndex = array.GetLength(0) - 1;
            if (endColIndex <= 0) endColIndex = array.GetLength(1) - 1;

            for (int i = beginRowIndex; i <= endRowIndex; i++)
            {
                for (int j = beginColIndex; j <= endColIndex; j++)
                {
                    array[i, j] = value;
                }
            }
        }


        /// <summary>
        /// Find the nearest element with specific value in an array.
        /// </summary>
        public static int FindNearestIndex(this float[] items, float value)
        {
            if (items == null || items.Length == 0) return -1;

            int result = 0;
            float minError = M.Abs(value - items[0]);
            float error;

            for (int i = 1; i < items.Length; i++)
            {
                error = M.Abs(value - items[i]);
                if (error < minError)
                {
                    minError = error;
                    result = i;
                }
            }

            return result;
        }


        /// <summary>
        /// Change the size of the list.
        /// </summary>
        public static void Resize<T>(this List<T> list, int newSize, T newValue = default)
        {
            if (list.Count != newSize)
            {
                if (list.Count > newSize)
                {
                    list.RemoveRange(newSize, list.Count - newSize);
                }
                else
                {
                    int addCount = newSize - list.Count;

                    while (addCount > 0)
                    {
                        list.Add(newValue);
                        addCount--;
                    }
                }
            }
        }


        /// <summary>
        /// Change the size of the list.
        /// </summary>
        public static void Resize(this IList list, int newSize, object newValue = null)
        {
            if (list.Count != newSize)
            {
                if (list.Count > newSize)
                {
                    for (int i = list.Count - 1; i >= newSize; i--)
                    {
                        list.RemoveAt(i);
                    }
                }
                else
                {
                    int addCount = newSize - list.Count;

                    while (addCount > 0)
                    {
                        list.Add(newValue);
                        addCount--;
                    }
                }
            }
        }


        /// <summary>
        /// Sort elements part of the list.
        /// </summary>
        /// <param name="index"> Start index </param>
        /// <param name="count"> A negative or zero value means all elements after start index. </param>
        public static void Sort<T>(this IList<T> list, Comparison<T> compare, int index = 0, int count = 0)
        {
            if (count <= 0) count = list.Count - index;
            int lastIndex = index + count - 1;
            T temp;
            bool changed;

            for (int i = 0; i < count - 1; i++)
            {
                changed = false;
                for (int j = index; j < lastIndex; j++)
                {
                    if (compare(list[j], list[j + 1]) > 0)
                    {
                        temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                        changed = true;
                    }
                }

                if (!changed) break;

                lastIndex--;
            }
        }


        public static T Last<T>(this IList<T> list) { return list[list.Count - 1]; }


        /// <summary>
        /// Traverse any array.
        /// </summary>
        /// <param name="onElement"> param1 is dimension index, param2 is element indexes in every dimension </param>
        /// <param name="beginDimension"> param1 is dimension index, param2 is indexes in every dimension before this dimension </param>
        /// <param name="endDimension"> param1 is dimension index, param2 is indexes in every dimension before this dimension </param>
        public static void Traverse(this Array array, Action<int, int[]> onElement, Action<int, int[]> beginDimension = null, Action<int, int[]> endDimension = null)
        {
            if (array.Length != 0)
            {
                TraverseArrayDimension(0, new int[array.Rank]);
            }

            void TraverseArrayDimension(int dimension, int[] indices)
            {
                int size = array.GetLength(dimension);
                bool isFinal = (dimension + 1 == array.Rank);

                beginDimension?.Invoke(dimension, indices);

                for (int i = 0; i < size; i++)
                {
                    indices[dimension] = i;
                    if (isFinal)
                    {
                        onElement?.Invoke(dimension, indices);
                    }
                    else TraverseArrayDimension(dimension + 1, indices);
                }

                endDimension?.Invoke(dimension, indices);
            }
        }


        /// <summary>
        /// Get the text description of the array, the text is similar to C# code.
        /// </summary>
        public static string ToCodeString(this Array array, Func<object, string> elementToString = null)
        {
            if (array == null) return "Null Array";

            if (elementToString == null)
            {
                elementToString = obj =>
                {
                    if (ReferenceEquals(obj, null))
                    {
                        return "null";
                    }

                    if (obj.GetType() == typeof(string))
                    {
                        return string.Format("\"{0}\"", obj);
                    }

                    return obj.ToString();
                };
            }

            var builder = new System.Text.StringBuilder(array.Length * 4);

            Traverse(array,
                (d, i) =>
                {
                    if (i[d] != 0) builder.Append(',');
                    builder.Append(' ');
                    object obj = array.GetValue(i);
                    builder.Append(elementToString(obj));
                },
                (d, i) =>
                {
                    if (d != 0)
                    {
                        if (i[d - 1] != 0) builder.Append(',');
                        builder.Append('\n');
                        while (d != 0)
                        {
                            builder.Append('\t');
                            d--;
                        }
                    }

                    builder.Append('{');
                },
                (d, i) =>
                {
                    if (d + 1 == array.Rank) builder.Append(" }");
                    else
                    {
                        builder.Append('\n');
                        while (d != 0)
                        {
                            builder.Append('\t');
                            d--;
                        }

                        builder.Append('}');
                    }
                });

            return builder.ToString();
        }

        public static T Pop<T>(this HashSet<T> set)
        {
            if (set == null) throw new System.ArgumentNullException(nameof(set));

            using var e = set.GetEnumerator();
            if (e.MoveNext())
            {
                set.Remove(e.Current);
                return e.Current;
            }

            throw new System.ArgumentException("HashSet must not be empty.");
        }

        
    }
}