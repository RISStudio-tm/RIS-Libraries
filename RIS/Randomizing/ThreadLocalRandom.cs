// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;

namespace RIS.Randomizing
{
    internal sealed class ThreadLocalRandom : Random, IGaussianRandom
    {
        internal static readonly DateTime SeedTime = DateTime.UtcNow;
        private double? _nextGaussian;

        [ThreadStatic]
        private static ThreadLocalRandom _current;
        public static ThreadLocalRandom Current
        {
            get
            {
                return _current ?? (_current = new ThreadLocalRandom());
            }
        }

        private ThreadLocalRandom()
            : base(Rand.HashCombine(Rand.HashCombine(SeedTime.GetHashCode(), Thread.CurrentThread.ManagedThreadId), System.Environment.TickCount))
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

            (double number1, double number2) = this.InternalNextTwoGaussian();

            _nextGaussian = number2;

            return number1;
        }
    }
}
