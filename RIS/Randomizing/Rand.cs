// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Randomizing
{
    public static class Rand
    {
        public static Random Current
        {
            get
            {
                return SingletonRandom.Instance;
            }
        }

        public static Random CreateRandom()
        {
            return new Random(HashCombine(System.Environment.TickCount, ThreadLocalRandom.Current.Next()));
        }

        public static Random CreateJavaRandom()
        {
            return CreateJavaRandom(ThreadLocalRandom.Current.NextInt64() ^ System.Environment.TickCount);
        }
        public static Random CreateJavaRandom(long seed)
        {
            return new JavaRandom(seed);
        }

        internal static int HashCombine(int hash1, int hash2)
        {
            return unchecked(((hash1 << 5) + hash1) ^ hash2);
        }

        public static int Next()
        {
            return ThreadLocalRandom.Current.Next();
        }
        public static int Next(int maxValue)
        {
            return ThreadLocalRandom.Current.Next(maxValue);
        }
        public static int Next(int minValue, int maxValue)
        {
            return ThreadLocalRandom.Current.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            return ThreadLocalRandom.Current.NextDouble();
        }
    }
}
