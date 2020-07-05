﻿using System;
using System.Collections.Generic;
using System.Linq;
using RIS.Randomizing;

namespace RIS.Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random random = null)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            Random rand = random ?? ThreadLocalRandom.Current;

            for (int i = 0; i < list.Count - 1; ++i)
            {
                int randomIndex = rand.Next(i, list.Count);
                T randomValue = list[randomIndex];

                list[randomIndex] = list[i];
                list[i] = randomValue;
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, uint chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
