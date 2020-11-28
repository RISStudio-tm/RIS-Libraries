// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;
using System.Text;
using RIS.Extensions;
using RIS.Randomizing;

namespace RIS.Text.Generating
{
    public static class StringGenerator
    {
        private static readonly RNGCryptoServiceProvider RandomGenerator;

        public static readonly char[] LettersAndDigits;

        static StringGenerator()
        {
            RandomGenerator = new RNGCryptoServiceProvider();
            LettersAndDigits = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        }

        public static string GetAlphabet(int charsCount)
        {
            StringBuilder result = new StringBuilder(charsCount);

            int i = 0;
            int count = 0;

            do
            {
                char c = (char)i;

                if (!char.IsControl(c) && !char.IsWhiteSpace(c))
                {
                    result.Append(c);
                    ++count;
                }

                ++i;
            }
            while (count < charsCount);

            return result.ToString();
        }

        public static string GetRandom(int minSize, int maxSize,
            bool onlyLettersAndDigits = true)
        {
            int size = minSize < maxSize
                ? Rand.Next(minSize, maxSize)
                : minSize;

            return GetRandom(size, onlyLettersAndDigits);
        }
        public static string GetRandom(int size,
            bool onlyLettersAndDigits = true)
        {
            Random random = Rand.CreateRandom();

            if (onlyLettersAndDigits)
            {
                char[] result = new char[size];

                for (int i = 0; i < size; ++i)
                {
                    result[i] = LettersAndDigits[random.Next(LettersAndDigits.Length)];
                }

                return new string(result);
            }

            byte[] resultBytes = new byte[size];
            ASCIIEncoding encoding = new ASCIIEncoding();

            for (int i = 0; i < size; ++i)
            {
                resultBytes[i] = (byte)random.Next(32, 127);
            }

            return encoding.GetString(resultBytes);
        }

        public static string GenerateString(int minSize, int maxSize,
            bool onlyLettersAndDigits = true)
        {
            int size = minSize < maxSize
                ? RandomGenerator.GenerateInt(minSize, maxSize)
                : minSize;

            return GenerateString(size, onlyLettersAndDigits);
        }
        public static string GenerateString(int size,
            bool onlyLettersAndDigits = true)
        {
            Span<byte> randomBytes = RandomGenerator.GenerateBytes(size * 4);

            if (onlyLettersAndDigits)
            {
                StringBuilder result = new StringBuilder(size);

                for (int i = 0; i < size; ++i)
                {
                    uint randomNumber = BitConverter.ToUInt32(randomBytes.ToArray(), i * 4);
                    long charIndex = randomNumber % LettersAndDigits.Length;

                    result.Append(LettersAndDigits[charIndex]);
                }

                return result.ToString();
            }

            byte[] resultBytes = new byte[size];
            ASCIIEncoding encoding = new ASCIIEncoding();

            for (int i = 0; i < size; ++i)
            {
                uint randomNumber = BitConverter.ToUInt32(randomBytes.ToArray(), i * 4);
                long charIndex = randomNumber % (127 - 32);

                resultBytes[i] = (byte)(32 + charIndex);
            }

            return encoding.GetString(resultBytes);
        }
    }
}
