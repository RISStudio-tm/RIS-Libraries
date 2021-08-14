// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Math = RIS.Mathematics.Math;

namespace RIS.Cryptography
{
    public static class SecureUtils
    {
        public static UTF8Encoding SecureUTF8 { get; }



        static SecureUtils()
        {
            SecureUTF8 = new UTF8Encoding(false, true);
        }



        public static byte[] ToRawBytesArray(string text)
        {
            return ToRawBytes(text).ToArray();
        }
        public static ReadOnlySpan<byte> ToRawBytes(string text)
        {
            return MemoryMarshal.AsBytes(text.AsSpan());
        }
        public static byte[] ToCharsRawBytesArray(char[] text)
        {
            return ToCharsRawChars(text).ToArray();
        }
        public static ReadOnlySpan<byte> ToCharsRawChars(char[] text)
        {
            return MemoryMarshal.AsBytes(text.AsSpan());
        }

        public static string FromRawBytesArray(byte[] bytes)
        {
            return FromRawBytes(new ReadOnlySpan<byte>(bytes));
        }
        public static string FromRawBytes(ReadOnlySpan<byte> bytes)
        {
#if NETFRAMEWORK

            return new string(MemoryMarshal.Cast<byte, char>(bytes).ToArray());

#elif NETCOREAPP

            return new string(MemoryMarshal.Cast<byte, char>(bytes));

#endif
        }
        public static char[] FromCharsRawBytesArray(byte[] bytes)
        {
            return FromCharsRawBytes(new ReadOnlySpan<byte>(bytes));
        }
        public static char[] FromCharsRawBytes(ReadOnlySpan<byte> bytes)
        {
            return MemoryMarshal.Cast<byte, char>(bytes).ToArray();
        }


        public static byte[] GetBytes(string text)
        {
            return SecureUTF8.GetBytes(text);
        }

        public static string GetString(byte[] bytes)
        {
            return SecureUTF8.GetString(bytes);
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(string left, string right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            if (ignoreCase)
            {
                left = left.ToLower(
                    culture);
                right = right.ToLower(
                    culture);
            }

            return SecureEquals(
                ToRawBytes(left),
                ToRawBytes(right));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(char[] left, char[] right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            return SecureEquals(new ReadOnlySpan<char>(left), new ReadOnlySpan<char>(right),
                ignoreCase, culture);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(Span<char> left, Span<char> right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEquals((ReadOnlySpan<char>)left, (ReadOnlySpan<char>)right,
                ignoreCase, culture);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(ReadOnlySpan<char> left, ReadOnlySpan<char> right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            Func<char, char, int> compareFunction;

            if (ignoreCase)
            {
                compareFunction = (leftChar, rightChar) =>
                    char.ToLower(leftChar, culture) ^ char.ToLower(rightChar, culture);
            }
            else
            {
                compareFunction = (leftChar, rightChar) =>
                    leftChar ^ rightChar;
            }

            return SecureEquals(left, right,
                compareFunction);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(byte[] left, byte[] right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement ^ rightElement);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(Span<byte> left, Span<byte> right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement ^ rightElement);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement ^ rightElement);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals<T>(T[] left, T[] right,
            Func<T, T, int> compareFunction)
        {
            return SecureEquals(new ReadOnlySpan<T>(left), new ReadOnlySpan<T>(right),
                compareFunction);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals<T>(Span<T> left, Span<T> right,
            Func<T, T, int> compareFunction)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEquals((ReadOnlySpan<T>)left, (ReadOnlySpan<T>)right,
                compareFunction);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right,
            Func<T, T, int> compareFunction)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            if (compareFunction == null)
                return false;

            var minLength = System.Math.Min(
                left.Length, right.Length);
            var difference =
                (uint)(left.Length ^ right.Length);

            for (int i = 0; i < minLength; ++i)
            {
                difference |= Math.AbsNoOverflow(
                    compareFunction(left[i], right[i]));
            }

            return difference == 0;
        }
    }
}
