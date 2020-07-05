using System;
using System.Collections.Generic;
using System.Linq;
using RIS.Randomizing;

namespace RIS.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffled<T>(this IEnumerable<T> source, Random random = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ShuffledIterator(source, random ?? SingletonRandom.Instance);
        }

        internal static IEnumerable<T> ShuffledIterator<T>(IEnumerable<T> source, Random random)
        {
            List<T> list = source.ToList();

            if (list.Count == 0)
                yield break;

            for (int i = 0; i < list.Count - 1; ++i)
            {
                int randomIndex = random.Next(i, list.Count);
                T randomValue = list[randomIndex];

                list[randomIndex] = list[i];

                yield return randomValue;
            }

            yield return list[list.Count - 1];
        }
    }
}
