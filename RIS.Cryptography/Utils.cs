// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace RIS.Cryptography
{
    public static class Utils
    {
        public static Encoding SecureUTF8 { get; }

        static Utils()
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

            return SecureEquals(GetBytes(left), GetBytes(right));
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(byte[] left, byte[] right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            uint diff = (uint)(left.Length ^ right.Length);
            for (int i = 0; i < left.Length && i < right.Length; i++)
            {
                diff |= (uint)(left[i] ^ right[i]);
            }

            return diff == 0;
        }
    }
}
