using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake
{
    public static partial class C
    {
        #region helper

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

            var index = UnityEngine.Random.Range(0, collection.Length);
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
        public static T PickRandom<T>(this IList<T> collection)
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
        public static (T, int) PickRandomWithIndex<T>(this IList<T> collection)
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
        public static T PopRandom<T>(this IList<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
                return default;
            var i = UnityEngine.Random.Range(0, collection.Count);
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
        public static (T, int) PopRandomWithIndex<T>(this IList<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
                return default;
            var i = UnityEngine.Random.Range(0, collection.Count);
            var value = collection[i];
            collection.RemoveAt(i);
            return (value, i);
        }

        /// <summary>
        /// shuffle element in array <paramref name="source"/> parameter
        /// </summary>
        /// <param name="source">array</param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this T[] source)
        {
            var n = source.Length;
            while (n > 1)
            {
                n--;
                var k = UnityEngine.Random.Range(0, n);
                var value = source[k];
                source[k] = source[n];
                source[n] = value;
            }
        }

        /// <summary>
        /// shuffle element in <paramref name="source"/> parameter.
        /// </summary>
        /// <param name="source">IList</param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this IList<T> source)
        {
            var n = source.Count;
            while (n > 1)
            {
                n--;
                var k = UnityEngine.Random.Range(0, n);
                var value = source[k];
                source[k] = source[n];
                source[n] = value;
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

            var n = source.Count;
            while (n > 1)
            {
                n--;
                var k = UnityEngine.Random.Range(0, n);
                var keyValue = keys[k];
                keys[k] = keys[n];
                keys[n] = keyValue;

                var value = values[k];
                values[k] = values[n];
                values[n] = value;
            }

            return MakeDictionary(keys, values);
        }

        /// <summary>
        /// sub array from <paramref name="source"/> parameter a <paramref name="count"/> elements starting at index <paramref name="start"/> parameter.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="source">source array datas</param>
        /// <param name="start">start index</param>
        /// <param name="count">number sub</param>
        /// <returns>sub array</returns>
        public static T[] Sub<T>(this T[] source, int start, int count)
        {
            var result = new T[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = source[start + i];
            }

            return result;
        }

        /// <summary>
        /// Add element <paramref name="value"/> parameter with <paramref name="key"/> parameter in to <paramref name="dictionary"/> and indicate whether additional success or faild
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="key" /> parameter added success in to <paramref name="dictionary"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> parameter is null</exception>
        public static bool Add<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key)) return false;
            dictionary.Add(key, value);
            return true;
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

            if (keys.Length != values.Length)
            {
                throw new ArgumentException("Size keys and size values diffirent!");
            }

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

            if (keys.Count != values.Count)
            {
                throw new ArgumentException("Size keys and size values diffirent!");
            }

            IDictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (var i = 0; i < keys.Count; i++)
            {
                result.Add(keys[i], values[i]);
            }

            return result;
        }

        /// <summary>
        /// get or default same linq
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultOverride"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <returns></returns>
        public static TK GetOrDefault<T, TK>(this IDictionary<T, TK> dictionary, T key, TK defaultOverride = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultOverride;
        }

        /// <summary>
        /// copy <paramref name="data"/> parameter data in to clipboard
        /// </summary>
        /// <param name="data">string</param>
        public static void CopyToClipboard(this string data)
        {
            var textEditor = new TextEditor {text = data};
            textEditor.SelectAll();
            textEditor.Copy();
        }

        // /// <summary>
        // /// Generates a random double. Values returned are from <paramref name="lowerBound"/> up to but not including <paramref name="upperBound"/>.
        // /// </summary>
        // /// <param name="rand"></param>
        // /// <param name="lowerBound"></param>
        // /// <param name="upperBound"></param>
        // /// <returns></returns>
        // public static double Next(this Random.Random rand, double lowerBound, double upperBound) { return rand.NextDouble() * (upperBound - lowerBound) + lowerBound; }
        //
        // /// <summary>
        // /// Generates a random float. Values returned are from <paramref name="lowerBound"/> up to but not including <paramref name="upperBound"/>.
        // /// </summary>
        // /// <param name="rand"></param>
        // /// <param name="lowerBound"></param>
        // /// <param name="upperBound"></param>
        // /// <returns></returns>
        // public static float Next(this Random.Random rand, float lowerBound, float upperBound) { return (float) (rand.NextDouble() * (upperBound - lowerBound) + lowerBound); }

        /// <summary>
        /// formatting Big Numbers: The “aa” Notation
        ///
        /// number                alphabet
        /// 1                        1
        /// 1000                     1K
        /// 1000000                  1M
        /// 1000000000               1B
        /// 1000000000000            1T
        /// 1000000000000000         1AA
        ///
        /// source  : https://gram.gs/gramlog/formatting-big-numbers-aa-notation/
        /// </summary>
        /// <param name="number">BigInteger</param>
        /// <returns></returns>
        public static string ToAlphabet(this System.Numerics.BigInteger number)
        {
            var str = number.ToString();
            var len = str.Length;
            if (number.Sign < 0 && len <= 4 || number.Sign > 0 && len <= 3) return str;
            var stringBuilder = new System.Text.StringBuilder();
            var index = 0;
            if (number.Sign < 0)
            {
                stringBuilder.Append('-');
                len--;
                index = 1;
            }

            //{0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}
            var intPart = len % 3;
            if (intPart == 0) intPart = 3;
            intPart += index;
            intPart += 2; // for floating point
            if (intPart > len) intPart = len;

            var tempString = stringBuilder.ToString();

            stringBuilder.Clear();
            for (int i = index; i < intPart; i++)
            {
                stringBuilder.Append(str[i]);
            }

            var floating = double.Parse(stringBuilder.ToString());
            floating /= 100;
            stringBuilder.Clear();
            stringBuilder.Append(tempString).Append(floating);

            if (len > 15)
            {
                var n = (len - 16) / 3;
                var firstChar = (char) (65 + n / 26);
                var secondChar = (char) (65 + n % 26);
                stringBuilder.Append(firstChar);
                stringBuilder.Append(secondChar);
            }
            else if (len > 12) stringBuilder.Append('T');
            else if (len > 9) stringBuilder.Append('B');
            else if (len > 6) stringBuilder.Append('M');
            else if (len > 3) stringBuilder.Append('K');

            return stringBuilder.ToString();
        }

        /// <summary>
        /// formatting Big Numbers: The “aa” Notation
        ///
        /// number                alphabet
        /// 1                        1
        /// 1000                     1K
        /// 1000000                  1M
        /// 1000000000               1B
        /// 1000000000000            1T
        /// 1000000000000000         1AA
        ///
        /// </summary>
        /// <param name="value">string number</param>
        /// <returns></returns>
        public static string ToAlphabet(this string value)
        {
            value = value.Split('.')[0];
            var len = value.Length;
            var stringBuilder = new System.Text.StringBuilder();
            var index = 0;
            var num = 3;
            if (value[0] == '-')
            {
                stringBuilder.Append('-');
                len--;
                index = 1;
                num = 4;
            }

            if (len <= num) return value; // return here if not converted to alphabet

            //{0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"}
            var intPart = len % 3;
            if (intPart == 0) intPart = 3;
            intPart += index;
            intPart += 2; // for floating point
            if (intPart > len) intPart = len;

            var tempString = stringBuilder.ToString();

            stringBuilder.Clear();
            for (int i = index; i < intPart; i++)
            {
                stringBuilder.Append(value[i]);
            }

            var floating = double.Parse(stringBuilder.ToString());
            floating /= 100;
            stringBuilder.Clear();
            stringBuilder.Append(tempString).Append(floating);

            if (len > 15)
            {
                var n = (len - 16) / 3;
                var firstChar = (char) (65 + n / 26);
                var secondChar = (char) (65 + n % 26);
                stringBuilder.Append(firstChar);
                stringBuilder.Append(secondChar);
            }
            else if (len > 12) stringBuilder.Append('T');
            else if (len > 9) stringBuilder.Append('B');
            else if (len > 6) stringBuilder.Append('M');
            else stringBuilder.Append('K');

            return stringBuilder.ToString();
        }

        /// <summary>This method gives you the time-independent 't' value for lerp when used for dampening. This returns 1 in edit mode, or if dampening is less than 0.</summary>
        public static float DampenFactor(float dampening, float elapsed)
        {
            if (dampening < 0.0f)
            {
                return 1.0f;
            }
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return 1.0f;
            }
#endif
            return 1.0f - Mathf.Exp(-dampening * elapsed);
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
                Debug.LogError("Index out of range!");
#endif
                return;
            }

            if (oldIndex == newIndex) return;
#if CSHARP_7_3_OR_NEWER
            (source[oldIndex], source[newIndex]) = (source[newIndex], source[oldIndex]);
#else
            T temp = source[oldIndex];
            source[oldIndex] = source[newIndex];
            source[newIndex] = temp;
#endif
        }

        /// <summary>
        /// swap element in array <paramref name="source"/> parameter (<paramref name="oldIndex"/> swap for <paramref name="newIndex"/>)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oldIndex">old index</param>
        /// <param name="newIndex">new index</param>
        /// <typeparam name="T">type</typeparam>
        public static void Swap<T>(this IList<T> source, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex > source.Count || newIndex > source.Count)
            {
#if UNITY_EDITOR
                Debug.LogError("Index out of range!");
#endif
                return;
            }

            if (oldIndex == newIndex) return;
#if CSHARP_7_3_OR_NEWER
            (source[oldIndex], source[newIndex]) = (source[newIndex], source[oldIndex]);
#else
            T temp = source[oldIndex];
            source[oldIndex] = source[newIndex];
            source[newIndex] = temp;
#endif
        }

        /// <summary>
        /// swap value of <paramref name="keyA"/> and <paramref name="keyB"/>
        /// </summary>
        /// <param name="keyA"></param>
        /// <param name="keyB"></param>
        public static void SwapPlayerPrefs<T>(string keyA, string keyB)
        {
            switch (typeof(T))
            {
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type intType when intType == typeof(int):
                    int tempInt = PlayerPrefs.GetInt(keyA);
                    PlayerPrefs.SetInt(keyA, PlayerPrefs.GetInt(keyB));
                    PlayerPrefs.SetInt(keyB, tempInt);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type stringType when stringType == typeof(string):
                    string tempString = PlayerPrefs.GetString(keyA);
                    PlayerPrefs.SetString(keyA, PlayerPrefs.GetString(keyB));
                    PlayerPrefs.SetString(keyB, tempString);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type floatType when floatType == typeof(float):
                    float tempFloat = PlayerPrefs.GetFloat(keyA);
                    PlayerPrefs.SetFloat(keyA, PlayerPrefs.GetFloat(keyB));
                    PlayerPrefs.SetFloat(keyB, tempFloat);
                    break;
            }
        }

        public static string Format(this string fmt, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture.NumberFormat, fmt, args);

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refAction"></param>
        // ReSharper disable Unity.PerformanceAnalysis
        public static void CallCacheCleanAction(ref Action refAction)
        {
            var action = refAction;
            if (action == null) return;
            
            action();
            refAction = null;
        }
        #endregion
    }
}