// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Randomizing
{
    internal sealed class SingletonRandom : Random, IGaussianRandom
    {
        private static readonly object InstanceSyncRoot = new object();
        private static volatile SingletonRandom _instance;
        public static SingletonRandom Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_instance == null)
                            _instance = new SingletonRandom();
                    }
                }

                return _instance;
            }
        }

        private SingletonRandom()
            : base(0)
        {

        }

        public override int Next()
        {
            return ThreadLocalRandom.Current.Next();
        }

        public override int Next(int maxValue)
        {
            return ThreadLocalRandom.Current.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return ThreadLocalRandom.Current.Next(minValue, maxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            ThreadLocalRandom.Current.NextBytes(buffer);
        }

        public override double NextDouble()
        {
            return ThreadLocalRandom.Current.NextDouble();
        }

        protected override double Sample()
        {
            return ThreadLocalRandom.Current.NextDouble();
        }

        double IGaussianRandom.NextGaussian()
        {
            return ThreadLocalRandom.Current.NextGaussian();
        }
    }
}
