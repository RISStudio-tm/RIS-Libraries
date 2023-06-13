// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using RIS.Randomizing;

namespace RIS.Extensions
{
    public static class RandomExtensions
    {
        public static double NextGaussian(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            IGaussianRandom gaussianRandom = random as IGaussianRandom;

            if (gaussianRandom != null)
                return gaussianRandom.NextGaussian();

            return random.NextTwoGaussianInternal().Number1;
        }

        internal static (double Number1, double Number2) NextTwoGaussianInternal(this Random random)
        {
            double number1, number2, s;

            do
            {
                number1 = (2 * random.NextDouble()) - 1;
                number2 = (2 * random.NextDouble()) - 1;
                s = (number1 * number1) + (number2 * number2);
            }
            while (s >= 1 || s == 0);

            double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);

            return (number1 * multiplier, number2 * multiplier);
        }

        public static bool NextBoolean(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return random.NextBits(1) != 0;
        }
        public static bool NextBoolean(this Random random, double probability)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (probability == 0)
            {
                return false;
            }
            else if (probability == 1)
            {
                return true;
            }
            else if (probability < 0 || probability > 1)
            {
                var exception = new ArgumentOutOfRangeException(nameof(probability), probability,
                    $"Probability must be in [0, 1]. Found {probability}");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            else if (double.IsNaN(probability))
            {
                var exception = new ArgumentException(
                    "Probability must not be NaN", nameof(probability));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return random.NextDouble() < probability;
        }

        public static int NextInt32(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return random.NextBits(32);
        }

        public static long NextInt64(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            NextBitsRandom nextBitsRandom = random as NextBitsRandom;

            if (nextBitsRandom != null)
                return ((long)nextBitsRandom.NextBits(32) << 32) + nextBitsRandom.NextBits(32);

            return ((long)random.Next30OrFewerBits(22) << 42)
                   + ((long)random.Next30OrFewerBits(21) << 21)
                   + random.Next30OrFewerBits(21);
        }

        public static float NextSingle(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return random.NextBits(24) / ((float)(1 << 24));
        }

        public static double NextDouble(this Random random, double max)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (max < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(max), max,
                    "Max must be non-negative");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            else if (double.IsNaN(max) || double.IsInfinity(max))
            {
                var exception = new ArgumentException(
                    "Max must not be infinity or NaN", nameof(max));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (max == 0)
                return 0;

            return max * random.NextDouble();
        }
        public static double NextDouble(this Random random, double min, double max)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            double range = max - min;

            if (double.IsNaN(range) || double.IsInfinity(range))
            {
                if (double.IsNaN(min))
                {
                    var exception = new ArgumentException(
                        "Min must not be NaN", nameof(min));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
                else if (double.IsNaN(max))
                {
                    var exception = new ArgumentException(
                        "Max must not be NaN", nameof(max));
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
                else if (double.IsInfinity(min))
                {
                    var exception = new ArgumentOutOfRangeException(nameof(min), min,
                        "Min must not be infinite");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
                else if (double.IsInfinity(max))
                {
                    var exception = new ArgumentOutOfRangeException(nameof(max), max,
                        "Max must not be infinite");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
                else
                {
                    var exception = new ArgumentOutOfRangeException(nameof(range), range,
                        $"Range (difference between {min} and {max}) is too large to be represented by {typeof(double)}");
                    Events.OnError(new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }
            }

            if (range < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(range), range,
                    "Range must be greater than or equal to " + nameof(min));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (range == 0)
                return min;

            return min + (range * random.NextDouble());
        }

        public static IEnumerable<double> NextDoubles(this Random random, int count)
        {
            if (count <= 0)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(count), "Count must be greater than 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return NextDoubles(random).Take(count);
        }
        public static IEnumerable<double> NextDoubles(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return NextDoublesIterator(random);
        }

        private static IEnumerable<double> NextDoublesIterator(Random random)
        {
            while (true)
            {
                yield return random.NextDouble();
            }
        }

        public static IEnumerable<byte> NextBytes(this Random random, int count)
        {
            if (count <= 0)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(count), "Count must be greater than 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return NextBytes(random).Take(count);
        }
        public static IEnumerable<byte> NextBytes(this Random random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return NextBytesIterator(random);
        }

        internal static IEnumerable<byte> NextBytesIterator(Random random)
        {
            byte[] buffer = new byte[256];

            while (true)
            {
                random.NextBytes(buffer);

                for (int i = 0; i < buffer.Length; ++i)
                {
                    yield return buffer[i];
                }
            }
        }

        internal static int NextBits(this Random random, int countBits)
        {
            NextBitsRandom nextBitsRandom = random as NextBitsRandom;

            if (nextBitsRandom != null)
                return nextBitsRandom.NextBits(countBits);

            if (countBits <= 30)
                return random.Next30OrFewerBits(countBits);

            int upperBits = random.Next30OrFewerBits(countBits - 16) << 16;
            int lowerBits = random.Next30OrFewerBits(16);

            return upperBits | lowerBits;
        }

        internal static int Next30OrFewerBits(this Random random, int countBits)
        {
            int sample;

            int maxValue = 1 << countBits;
            int firstBiasedValue = int.MaxValue - (int.MaxValue & (maxValue - 1));

            do
            {
                sample = random.Next();
            }
            while (sample >= firstBiasedValue);

            return sample & (maxValue - 1);
        }
    }
}
