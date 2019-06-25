using System;
using System.Collections.Generic;
using System.Linq;

namespace KanoopCommon.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random(Guid.NewGuid().GetHashCode()));
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rnd == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rnd);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rnd)
        {
            List<T> buffer = source.ToList();

            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rnd.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }
    }
}
