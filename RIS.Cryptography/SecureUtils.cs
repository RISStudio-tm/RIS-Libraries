// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace RIS.Cryptography
{
    public static class SecureUtils
    {
        public static Encoding SecureUTF8 { get; }

        static SecureUtils()
        {
            SecureUTF8 = new UTF8Encoding(false, true);
        }

        public static byte[] GetBytes(string text)
        {
            return SecureUTF8.GetBytes(text);
        }

        public static string GetString(byte[] text)
        {
            return SecureUTF8.GetString(text);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(string left, string right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            if (ignoreCase)
            {
                left = left.ToLower(culture);
                right = right.ToLower(culture);
            }

            return SecureEquals(
                GetBytes(left),
                GetBytes(right));
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(char[] left, char[] right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            var xorFunction = ignoreCase
                ? new Func<char, char, int>((leftChar, rightChar) =>
                    char.ToLower(leftChar, culture) ^ char.ToLower(rightChar, culture))
                : new Func<char, char, int>((leftChar, rightChar) =>
                    leftChar ^ rightChar);

            var minLength = Math.Min(left.Length, right.Length);
            var difference = (uint)(left.Length ^ right.Length);

            for (int i = 0; i < minLength; ++i)
            {
                difference |= (uint)xorFunction(left[i], right[i]);
            }

            return difference == 0;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(byte[] left, byte[] right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            var minLength = Math.Min(left.Length, right.Length);
            var difference = (uint)(left.Length ^ right.Length);

            for (int i = 0; i < minLength; ++i)
            {
                difference |= (uint)(left[i] ^ right[i]);
            }

            return difference == 0;
        }
    }
}
