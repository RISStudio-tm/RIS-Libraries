// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using RIS.Extensions;

namespace RIS.Randomizing
{
    internal abstract class NextBitsRandom : Random, IGaussianRandom
    {
        private double? _nextGaussian;

        protected NextBitsRandom(int seed)
            : base(seed)
        {

        }

        public sealed override int Next()
        {
            return Next(int.MaxValue);
        }

        public sealed override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be positive.");
            }

            if (maxValue == 0)
                return 0;

            unchecked
            {
                if ((maxValue & -maxValue) == maxValue)
                    return (int)((maxValue * (long)NextBits(31)) >> 31);

                int bits, val;

                do
                {
                    bits = NextBits(31);
                    val = bits % maxValue;
                }
                while (bits - val + (maxValue - 1) < 0);

                return val;
            }
        }

        public sealed override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), $"minValue ({minValue}) must not be > maxValue ({maxValue})");
            }

            if (minValue == maxValue)
                return minValue;

            long range = (long)maxValue - minValue;

            if (range <= int.MaxValue)
                return minValue + Next((int)range);

            long rand = this.NextInt64();
            long max = range - 1;

            if ((range & max) == 0L)
            {
                rand &= max;
            }
            else
            {
                // reject over-represented candidates
                for (
                    long u = unchecked((long)((ulong)rand >> 1)); // ensure non-negative
                    u + max - (rand = u % range) < 0; // rejection check
                    u = unchecked((long)((ulong)this.NextInt64() >> 1)) // retry
                ) ;
            }

            return checked((int)(rand + minValue));
        }

        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            for (int i = 0; i < buffer.Length;)
            {
                for (int rand = NextBits(32), n = Math.Min(buffer.Length - i, 4); n-- > 0; rand >>= 8)
                {
                    buffer[i++] = unchecked((byte)rand);
                }
            }
        }

        public sealed override double NextDouble()
        {
            return Sample();
        }

        protected sealed override double Sample()
        {
            return (((long)NextBits(26) << 27) + NextBits(27)) / (double)(1L << 53);
        }

        internal abstract int NextBits(int countBits);

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
