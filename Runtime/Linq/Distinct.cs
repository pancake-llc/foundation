namespace Pancake.Linq
{
    public static partial class L
    {
        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource>(this System.Collections.Generic.IEnumerable<TSource> source)
        {
            return System.Linq.Enumerable.Distinct(source);
        }

        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, System.Collections.Generic.IEqualityComparer<TSource> comparer)
        {
            return System.Linq.Enumerable.Distinct(source, comparer);
        }

        public static System.Linq.ParallelQuery<TSource> Distinct<TSource>(this System.Linq.ParallelQuery<TSource> source)
        {
            return System.Linq.ParallelEnumerable.Distinct(source);
        }

        public static System.Linq.ParallelQuery<TSource> Distinct<TSource>(this System.Linq.ParallelQuery<TSource> source, System.Collections.Generic.IEqualityComparer<TSource> comparer)
        {
            return System.Linq.ParallelEnumerable.Distinct(source, comparer);
        }
        
        /// <summary>
        /// Returns distinct elements from the given sequence using the default equality comparer
        /// to compare values projected by <paramref name="projection"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type of the projected value for each element of the sequence.</typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="projection">The projection that is applied to each element to retrieve the value which is being compared.</param>
        /// <returns>A sequence of elements whose projected values are distinct.</returns>
        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource, TResult>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TResult> projection)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (projection == null) Error.ArgumentNull("projection");

            return DistinctIterator(source, projection, System.Collections.Generic.EqualityComparer<TResult>.Default);
        }

        /// <summary>
        /// Returns distinct elements from the given sequence using the specified equality comparer
        /// to compare values projected by <paramref name="projection"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type of the projected value for each element of the sequence.</typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="projection">The projection that is applied to each element to retrieve the value which is being compared.</param>
        /// <param name="equalityComparer">The equality comparer to use for comparing the projected values.</param>
        /// <returns>A sequence of elements whose projected values are considered distinct by the specified equality comparer.</returns>
        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource, TResult>(this System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TResult> projection, System.Collections.Generic.IEqualityComparer<TResult> equalityComparer)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (projection == null) Error.ArgumentNull("projection");
            if (equalityComparer == null) Error.ArgumentNull("equalityComparer");

            return DistinctIterator(source, projection, equalityComparer);
        }

        private static System.Collections.Generic.IEnumerable<TSource> DistinctIterator<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource> source, System.Func<TSource, TResult> projection, System.Collections.Generic.IEqualityComparer<TResult> equalityComparer)
        {
            var alreadySeenValues = new System.Collections.Generic.HashSet<TResult>(equalityComparer);

            foreach (var element in source)
            {
                var value = projection(element);

                if (alreadySeenValues.Contains(value))
                {
                    continue;
                }

                yield return element;
                alreadySeenValues.Add(value);
            }
        }
    }
}