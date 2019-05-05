using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayfulSoftware.HexMaps
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector)
        {
            TSource previous = default(TSource);

            using (var it = source.GetEnumerator())
            {
                if (it.MoveNext())
                    previous = it.Current;

                while (it.MoveNext())
                    yield return selector(previous, previous = it.Current);
            }
        }

        public static IEnumerable<Tuple<TSource, TSource>> Pairwise<TSource>(this IEnumerable<TSource> source)
        {
            return source.Pairwise((a, b) => new Tuple<TSource, TSource>(a, b));
        }
    }
}