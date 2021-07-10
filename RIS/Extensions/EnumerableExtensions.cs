// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

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
                var exception = new ArgumentNullException(nameof(source));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
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

        public static IEnumerable<int> IndexesWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;

            foreach (T element in source)
            {
                if (predicate(element))
                    yield return index;

                ++index;
            }
        }
    }
}
