// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
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
            return RandomNumberGenerator.GetInt32(int.MaxValue);
        }
        public static int GenerateInt(this RandomNumberGenerator random,
            int max)
        {
            return RandomNumberGenerator.GetInt32(max);
        }
        public static int GenerateInt(this RandomNumberGenerator random,
            int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
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
            random.GetBytes(data);
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
