// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using RIS.Extensions;

namespace RIS.Randomizing
{
    internal sealed class ThreadLocalRandom : Random, IGaussianRandom
    {
        private static readonly object InstanceSyncRoot = new object();
        [ThreadStatic]
        private static volatile ThreadLocalRandom _current;
        public static ThreadLocalRandom Current
        {
            get
            {
                if (_current == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_current == null)
                            _current = new ThreadLocalRandom();
                    }
                }

                return _current;
            }
        }

        internal static readonly DateTime SeedTime = DateTime.UtcNow;
        private double? _nextGaussian;

        private ThreadLocalRandom()
            : base(Rand.HashCombine(Rand.HashCombine(
                    SeedTime.GetHashCode(),
                    Thread.CurrentThread.ManagedThreadId),
                System.Environment.TickCount))
        {

        }

        double IGaussianRandom.NextGaussian()
        {
            if (_nextGaussian.HasValue)
            {
                double result = _nextGaussian.Value;
                _nextGaussian = null;

                return result;
            }

            (double number1, double number2) = this.NextTwoGaussianInternal();

            _nextGaussian = number2;

            return number1;
        }
    }
}
