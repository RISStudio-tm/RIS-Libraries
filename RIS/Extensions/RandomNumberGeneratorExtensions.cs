// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
#if (NETFRAMEWORK)
using System.Buffers;
using System.Runtime.InteropServices;
#endif
using System.Security.Cryptography;
using RIS.Randomizing;

namespace RIS.Extensions
{
    public static class RandomNumberGeneratorExtensions
    {
        public static Random AsRandom(this RandomNumberGenerator random)
        {
            if (random == null)
            {
                var exception = new ArgumentNullException(nameof(random));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return new RNGRandom(random);
        }

        public static int GenerateInt(this RandomNumberGenerator random)
        {
#if NETFRAMEWORK

            return GenerateInt(random, int.MaxValue);

#elif NETCOREAPP

            return RandomNumberGenerator.GetInt32(int.MaxValue);

#endif
        }
        public static int GenerateInt(this RandomNumberGenerator random,
            int max)
        {

#if NETFRAMEWORK

            return GenerateInt(random, 0, max);

#elif NETCOREAPP

            return RandomNumberGenerator.GetInt32(max);

#endif

        }
        public static int GenerateInt(this RandomNumberGenerator random,
            int min, int max)
        {

#if NETFRAMEWORK

            if (min >= max)
            {
                var exception = new ArgumentException("Invalid random range");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            uint range = (uint)max - (uint)min - 1;

            if (range == 0)
                return min;

            uint mask = range;

            mask |= mask >> 1;
            mask |= mask >> 2;
            mask |= mask >> 4;
            mask |= mask >> 8;
            mask |= mask >> 16;

            Span<uint> resultSpan = stackalloc uint[1];
            uint result;

            do
            {
                Span<byte> data = MemoryMarshal.AsBytes(resultSpan);

                GenerateBytesSpan(random, data);

                result = mask & resultSpan[0];
            }
            while (result > range);

            return (int)result + min;

#elif NETCOREAPP

            return RandomNumberGenerator.GetInt32(min, max);

#endif

        }

        public static Span<byte> GenerateBytesSpan(this RandomNumberGenerator random,
            int size)
        {
            Span<byte> result = new byte[size];

            GenerateBytesSpan(random, result);

            return result;
        }
        public static void GenerateBytesSpan(this RandomNumberGenerator random,
            Span<byte> data)
        {

#if NETFRAMEWORK

            byte[] array = ArrayPool<byte>.Shared.Rent(data.Length);

            try
            {
                random.GetBytes(array, 0, data.Length);
                new ReadOnlySpan<byte>(array, 0, data.Length).CopyTo(data);
            }
            finally
            {
                Array.Clear(array, 0, data.Length);
                ArrayPool<byte>.Shared.Return(array);
            }

#elif NETCOREAPP

            random.GetBytes(data);

#endif

        }

        public static byte[] GenerateBytes(this RandomNumberGenerator random,
            int size)
        {
            byte[] result = new byte[size];

            GenerateBytes(random, result);

            return result;
        }
        public static void GenerateBytes(this RandomNumberGenerator random,
            byte[] data)
        {
            random.GetBytes(data);
        }
    }
}
