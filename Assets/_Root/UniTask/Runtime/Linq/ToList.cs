using Pancake.Core.Tasks.Internal;
using System.Collections.Generic;
using System.Threading;

namespace Pancake.Core.Tasks.Linq
{
    public static partial class UniTaskAsyncEnumerable
    {
        public static UniTask<List<TSource>> ToListAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Error.ThrowArgumentNullException(source, nameof(source));

            return Pancake.Core.Tasks.Linq.ToList.ToListAsync(source, cancellationToken);
        }
    }

    internal static class ToList
    {
        internal static async UniTask<List<TSource>> ToListAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            var list = new List<TSource>();

            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    list.Add(e.Current);
                }
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }

            return list;
        }
    }
}