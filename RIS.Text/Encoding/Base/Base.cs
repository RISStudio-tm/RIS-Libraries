﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace RIS.Text.Encoding.Base
{
    public abstract class Base
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public uint CharsCount { get; }

        public double BitsPerChars => (double)BlockBitsCount / BlockCharsCount;

        public int BlockBitsCount { get; protected set; }

        public int BlockCharsCount { get; protected set; }

        public string Alphabet { get; }

        public char Special { get; }

        public abstract bool HasSpecial { get; }

        public System.Text.Encoding Encoding { get; set; }

        public bool Parallel { get; set; }

        protected readonly int[] InvAlphabet;

        public Base(uint charsCount, string alphabet, char special, System.Text.Encoding encoding = null, bool parallel = false)
        {
            if (alphabet.Length != charsCount)
                throw new ArgumentException($"Base string should contain {charsCount} chars");

            for (int i = 0; i < charsCount; i++)
                for (int j = i + 1; j < charsCount; j++)
                    if (alphabet[i] == alphabet[j])
                        throw new ArgumentException("Base string should contain distinct chars");

            if (alphabet.Contains(special))
                throw new ArgumentException("Base string should not contain special char");

            CharsCount = charsCount;
            Alphabet = alphabet;
            Special = special;
            int bitsPerChar = LogBase2(charsCount);
            BlockBitsCount = LCM(bitsPerChar, 8);
            BlockCharsCount = BlockBitsCount / bitsPerChar;

            InvAlphabet = new int[Alphabet.Max() + 1];

            for (int i = 0; i < InvAlphabet.Length; i++)
                InvAlphabet[i] = -1;

            for (int i = 0; i < charsCount; i++)
                InvAlphabet[Alphabet[i]] = i;

            Encoding = encoding ?? System.Text.Encoding.UTF8;
            Parallel = parallel;
        }

        public virtual string EncodeString(string data)
        {
            return Encode(Encoding.GetBytes(data));
        }

        public abstract string Encode(byte[] data);

        public virtual string DecodeToString(string data)
        {
            return Encoding.GetString(Decode(Regex.Replace(data, @"\r\n?|\n", "")));
        }

        public abstract byte[] Decode(string data);

        public static bool IsPowerOf2(uint x)
        {
            uint xint = x;
            if (x - xint != 0)
                return false;

            return (xint & (xint - 1)) == 0;
        }

        public static int LCM(int a, int b)
        {
            int num1, num2;
            if (a > b)
            {
                num1 = a;
                num2 = b;
            }
            else
            {
                num1 = b;
                num2 = a;
            }

            for (int i = 1; i <= num2; i++)
                if ((num1 * i) % num2 == 0)
                    return i * num1;
            return num2;
        }

        public static uint NextPowOf2(uint x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }

        public static ulong IntPow(ulong x, int exp)
        {
            ulong result = 1;
            for (int i = 0; i < exp; i++)
                result *= x;
            return result;
        }

        public static BigInteger BigIntPow(BigInteger x, int exp)
        {
            BigInteger result = 1;
            for (int i = 0; i < exp; i++)
                result *= x;
            return result;
        }

        public static int LogBase2(uint x)
        {
            int r = 0;
            while ((x >>= 1) != 0)
                r++;
            return r;
        }

        public static int LogBase2(ulong x)
        {
            int r = 0;
            while ((x >>= 1) != 0)
                r++;
            return r;
        }

        public static int LogBaseN(uint x, uint n)
        {
            int r = 0;
            while ((x /= n) != 0)
                r++;
            return r;
        }

        public static int LogBaseN(ulong x, uint n)
        {
            int r = 0;
            while ((x /= n) != 0)
                r++;
            return r;
        }

        public static int GetOptimalBitsCount2(uint charsCount, out uint charsCountInBits,
            uint maxBitsCount = 64, bool base2BitsCount = false)
        {
            int result = 0;
            charsCountInBits = 0;
            int n1 = LogBase2(charsCount);
            double charsCountLog = System.Math.Log(2, charsCount);
            double maxRatio = 0;

            for (int n = n1; n <= maxBitsCount; n++)
            {
                if (base2BitsCount && n % 8 != 0)
                    continue;

                uint l1 = (uint)System.Math.Ceiling(n * charsCountLog);
                double ratio = (double)n / l1;
                if (ratio > maxRatio)
                {
                    maxRatio = ratio;
                    result = n;
                    charsCountInBits = l1;
                }
            }

            return result;
        }

        public static int GetOptimalBitsCount(uint charsCount, out uint charsCountInBits,
            uint maxBitsCount = 64, uint radix = 2)
        {
            int result = 0;
            charsCountInBits = 0;
            int n0 = LogBaseN(charsCount, radix);
            double charsCountLog = System.Math.Log(radix, charsCount);
            double maxRatio = 0;

            for (int n = n0; n <= maxBitsCount; n++)
            {
                uint k = (uint)System.Math.Ceiling(n * charsCountLog);
                double ratio = (double)n / k;
                if (ratio > maxRatio)
                {
                    maxRatio = ratio;
                    result = n;
                    charsCountInBits = k;
                }
            }

            return result;
        }
    }
}
