using System;
using System.Collections.Generic;

namespace Pancake.Linq
{
    public static partial class L
    {
        // --------------------------- Arrays ----------------------------

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence or default if no elements exist.</returns>
        public static T Single<T>(this T[] source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Length == 0)
            {
                Error.ThrowNoElements();
            }

            if (source.Length > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence, or the default if no elements exist, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence</returns>
        public static T SingleOrDefault<T>(this T[] source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Length == 0)
            {
                return default(T);
            }

            if (source.Length > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition.</returns>
        public static T Single<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }

            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch)
                    {
                        Error.ThrowMoreThanOneMatch();
                    }

                    result = source[i];
                    foundMatch = true;
                }
            }

            if (foundMatch) return result;

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, or a default value if
        /// no such element exists, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition or default value if no such element is found.</returns>
        public static T SingleOrDefault<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }


            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch)
                    {
                        Error.ThrowMoreThanOneMatch();
                    }

                    result = source[i];
                    foundMatch = true;
                }
            }

            return result;
        }

        // --------------------------- Spans ----------------------------

#if UNITY_2021_3_OR_NEWER
        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence or default if no elements exist.</returns>
        public static T Single<T>(this Span<T> source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Length == 0)
            {
                Error.ThrowNoElements();
            }

            if (source.Length > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence, or the default if no elements exist, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence</returns>
        public static T SingleOrDefault<T>(this Span<T> source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Length == 0)
            {
                return default(T);
            }

            if (source.Length > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition.</returns>
        public static T Single<T>(this Span<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }

            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch)
                    {
                        Error.ThrowMoreThanOneMatch();
                    }

                    result = source[i];
                    foundMatch = true;
                }
            }

            if (foundMatch)
            {
                return result;
            }
            else
            {
                Error.ThrowNoMatch();
            }
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, or a default value if
        /// no such element exists, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition or default value if no such element is found.</returns>
        public static T SingleOrDefault<T>(this Span<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }


            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch) Error.ThrowMoreThanOneMatch();

                    result = source[i];
                    foundMatch = true;
                }
            }

            return result;
        }
#endif

        // --------------------------- Lists ----------------------------

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence</returns>
        public static T Single<T>(this List<T> source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Count == 0)
            {
                Error.ThrowNoElements();
            }

            if (source.Count > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence, or default if no elements exist, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>        
        /// <param name="source">A sequence to return the single element of</param>
        /// <returns>The single element of the input sequence or default if no elements exist.</returns>
        public static T SingleOrDefault<T>(this List<T> source)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (source.Count == 0)
            {
                return default(T);
            }

            if (source.Count > 1)
            {
                Error.ThrowMoreThanOneElement();
            }

            return source[0];
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition.</returns>
        public static T Single<T>(this List<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }

            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch)
                    {
                        Error.ThrowMoreThanOneMatch();
                    }

                    result = source[i];
                    foundMatch = true;
                }
            }

            if (foundMatch)
            {
                return result;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, or a default value if
        /// no such element exists, and throws an exception if more than one such element exists.
        /// </summary>        
        /// <param name="source">A sequence to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies a condition or default value if no such element is found.</returns>
        public static T SingleOrDefault<T>(this List<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                Error.ThrowArgumentNull("source");
            }

            if (predicate == null)
            {
                Error.ThrowArgumentNull("predicate");
            }

            T result = default(T);
            bool foundMatch = false;
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    if (foundMatch)
                    {
                        Error.ThrowMoreThanOneMatch();
                    }

                    result = source[i];
                    foundMatch = true;
                }
            }

            return result;
        }
    }
}