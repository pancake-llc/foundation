using System;
using System.Collections.Generic;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Extensions methods for <see cref="IEnumerable{T}"/>.
	/// </summary>
	internal static class EnumerableExtensions
	{
		/// <summary>
		/// Returns the first element in the <paramref name="sequence"/>, if the sequence
		/// contains only one element; otherwise,
		/// the default value of <typeparamref name="T"/>.
		/// <para>
		/// Does not throw any exceptions if the collection is empty or contains more than one element.
		/// An exception is still thrown is this <paramref name="sequence"/> is <see langword="null"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="T"> The type of the elements in the sequence. </typeparam>
		/// <param name="sequence"> The sequence from which to return the element. </param>
		/// <returns>
		/// the first element in the <paramref name="sequence"/>, if the sequence contains only one element;
		/// otherwise, the default value of <typeparamref name="T"/>.
		/// </returns>
		public static T SingleOrDefaultNoException<T>(this IEnumerable<T> sequence)
		{
			var enumerator = sequence.GetEnumerator();

			// If collection is empty
			if(!enumerator.MoveNext())
			{
				return default;
			}

			T first = enumerator.Current;

			// If collection contains more than one item
			if(enumerator.MoveNext())
			{
				return default;
			}

			return first;
		}

		/// <summary>
		/// Returns the first element in the <paramref name="sequence"/> that satisfies a specified condition,
		/// if the sequence contains only one element that satisfies the condition; otherwise, the default
		/// value of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"> The type of the elements in the sequence. </typeparam>
		/// <param name="sequence"> The sequence from which to return the element. </param>
		/// <param name="condition"> The condition to test if only one elements satisfies. </param>
		/// <returns>
		/// the first element in the <paramref name="sequence"/>, if the sequence contains only one element;
		/// otherwise, the default value of <typeparamref name="T"/>.
		/// </returns>
		public static T SingleOrDefaultNoException<T>(this IEnumerable<T> sequence, Predicate<T> condition)
		{
			bool oneFound = false;
			T result = default;
			var enumerator = sequence.GetEnumerator();
			while(enumerator.MoveNext())
			{
				T item = enumerator.Current;
				if(!condition(item))
				{
					continue;
				}

				if(oneFound)
				{
					return default;
				}

				oneFound = true;
				result = item;
			}

			return result;
		}
	}
}