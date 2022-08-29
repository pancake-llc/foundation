namespace Pancake.Linq
{
    public static partial class L
    {
        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource>(this System.Collections.Generic.IEnumerable<TSource> source)
        {
            return System.Linq.Enumerable.Distinct(source);
        }

        public static System.Collections.Generic.IEnumerable<TSource> Distinct<TSource>(
            this System.Collections.Generic.IEnumerable<TSource> source,
            System.Collections.Generic.IEqualityComparer<TSource> comparer)
        {
            return System.Linq.Enumerable.Distinct(source, comparer);
        }

        public static System.Linq.ParallelQuery<TSource> Distinct<TSource>(this System.Linq.ParallelQuery<TSource> source)
        {
            return System.Linq.ParallelEnumerable.Distinct(source);
        }

        public static System.Linq.ParallelQuery<TSource> Distinct<TSource>(
            this System.Linq.ParallelQuery<TSource> source,
            System.Collections.Generic.IEqualityComparer<TSource> comparer)
        {
            return System.Linq.ParallelEnumerable.Distinct(source, comparer);
        }
    }
}