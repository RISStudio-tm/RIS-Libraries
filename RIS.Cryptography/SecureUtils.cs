// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
            return ToCharsRawBytes(text).ToArray();
        }
        public static ReadOnlySpan<byte> ToCharsRawBytes(char[] text)
        {
            return MemoryMarshal.AsBytes(text.AsSpan());
        }
        public static byte[] ToCharsSpanRawBytesArray(ReadOnlySpan<char> text)
        {
            return ToCharsSpanRawBytes(text).ToArray();
        }
        public static ReadOnlySpan<byte> ToCharsSpanRawBytes(ReadOnlySpan<char> text)
        {
            return MemoryMarshal.AsBytes(text);
        }

        public static string FromRawBytesArray(byte[] bytes)
        {
            return FromRawBytes(new ReadOnlySpan<byte>(bytes));
        }
        public static string FromRawBytes(ReadOnlySpan<byte> bytes)
        {
            return new string(MemoryMarshal.Cast<byte, char>(bytes));
        }
        public static char[] FromCharsRawBytesArray(byte[] bytes)
        {
            return FromCharsRawBytes(new ReadOnlySpan<byte>(bytes));
        }
        public static char[] FromCharsRawBytes(ReadOnlySpan<byte> bytes)
        {
            return MemoryMarshal.Cast<byte, char>(bytes).ToArray();
        }
        public static ReadOnlySpan<char> FromCharsSpanRawBytesArray(byte[] bytes)
        {
            return FromCharsSpanRawBytes(new ReadOnlySpan<byte>(bytes));
        }
        public static ReadOnlySpan<char> FromCharsSpanRawBytes(ReadOnlySpan<byte> bytes)
        {
            return MemoryMarshal.Cast<byte, char>(bytes);
        }



        public static byte[] GetBytes(string text)
        {
            return SecureUTF8.GetBytes(text);
        }

        public static string GetString(byte[] bytes)
        {
            return SecureUTF8.GetString(bytes);
        }



        public static bool SecureEquals(
            string left, string right,
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
                ToRawBytes(left), ToRawBytes(right));
        }

        public static bool SecureEquals(
            char[] left, char[] right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            return SecureEquals(
                new ReadOnlySpan<char>(left), new ReadOnlySpan<char>(right),
                ignoreCase, culture);
        }
        public static bool SecureEquals(
            Span<char> left, Span<char> right,
            bool ignoreCase = false, CultureInfo culture = null)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEquals(
                (ReadOnlySpan<char>)left, (ReadOnlySpan<char>)right,
                ignoreCase, culture);
        }
        public static bool SecureEquals(
            ReadOnlySpan<char> left, ReadOnlySpan<char> right,
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
                    char.ToLower(leftChar, culture) - char.ToLower(rightChar, culture);
            }
            else
            {
                compareFunction = (leftChar, rightChar) =>
                    leftChar - rightChar;
            }

            return SecureEquals(left, right,
                compareFunction);
        }

        public static bool SecureEquals(
            byte[] left, byte[] right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement - rightElement);
        }
        public static bool SecureEquals(
            Span<byte> left, Span<byte> right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement - rightElement);
        }
        public static bool SecureEquals(
            ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            return SecureEquals(left, right,
                (leftElement, rightElement) =>
                    leftElement - rightElement);
        }

        public static bool SecureEquals<T>(
            T[] left, T[] right,
            Func<T, T, int> compareFunction)
        {
            return SecureEquals(
                new ReadOnlySpan<T>(left), new ReadOnlySpan<T>(right),
                compareFunction);
        }
        public static bool SecureEquals<T>(
            Span<T> left, Span<T> right,
            Func<T, T, int> compareFunction)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEquals(
                (ReadOnlySpan<T>)left, (ReadOnlySpan<T>)right,
                compareFunction);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool SecureEquals<T>(
            ReadOnlySpan<T> left, ReadOnlySpan<T> right,
            Func<T, T, int> compareFunction)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            if (compareFunction == null)
                return false;

            var minLength = Math.Min(
                left.Length, right.Length);
            var difference =
                left.Length - right.Length;

            for (int i = 0; i < minLength; ++i)
            {
                difference |=
                    compareFunction(left[i], right[i]);
            }

            return difference == 0;
        }



        public static bool SecureEqualsUnsafe(
            string left, string right,
            bool ignoreCase = false,
            CultureInfo culture = null)
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

            return SecureEqualsUnsafe(
                ToRawBytes(left), ToRawBytes(right));
        }

        public static bool SecureEqualsUnsafe(
            char[] left, char[] right,
            bool ignoreCase = false,
            CultureInfo culture = null)
        {
            if (ignoreCase)
            {
                return SecureEqualsUnsafe(
                    new Span<char>(left), new Span<char>(right),
                    true, culture);
            }

            return SecureEqualsUnsafe(
                new ReadOnlySpan<char>(left), new ReadOnlySpan<char>(right),
                false, culture);
        }
        public static bool SecureEqualsUnsafe(
            Span<char> left, Span<char> right,
            bool ignoreCase = false,
            CultureInfo culture = null)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            if (ignoreCase)
            {
                var minLength = Math.Min(
                    left.Length, right.Length);

                for (int i = 0; i < minLength; ++i)
                {
                    left[i] = char.ToLower(left[i], culture);
                    right[i] = char.ToLower(right[i], culture);
                }
                for (int i = minLength; i < left.Length; ++i)
                {
                    left[i] = char.ToLower(left[i], culture);
                }
                for (int i = minLength; i < right.Length; ++i)
                {
                    right[i] = char.ToLower(right[i], culture);
                }
            }

            return SecureEqualsUnsafe(
                (ReadOnlySpan<char>)left, (ReadOnlySpan<char>)right,
                false, culture);
        }
        public static bool SecureEqualsUnsafe(
            ReadOnlySpan<char> left, ReadOnlySpan<char> right,
            bool ignoreCase = false,
            CultureInfo culture = null)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            if (ignoreCase)
            {
                var newLeft = new char[left.Length];
                var newRight = new char[right.Length];
                var minLength = Math.Min(
                    left.Length, right.Length);

                for (int i = 0; i < minLength; ++i)
                {
                    newLeft[i] = char.ToLower(left[i], culture);
                    newRight[i] = char.ToLower(right[i], culture);
                }
                for (int i = minLength; i < left.Length; ++i)
                {
                    newLeft[i] = char.ToLower(left[i], culture);
                }
                for (int i = minLength; i < right.Length; ++i)
                {
                    newRight[i] = char.ToLower(right[i], culture);
                }

                left = new ReadOnlySpan<char>(newLeft);
                right = new ReadOnlySpan<char>(newRight);
            }

            return SecureEqualsUnsafe(
                ToCharsSpanRawBytes(left), ToCharsSpanRawBytes(right));
        }

        public static bool SecureEqualsUnsafe(
            byte[] left, byte[] right)
        {
            return SecureEqualsUnsafe(
                new ReadOnlySpan<byte>(left), new ReadOnlySpan<byte>(right));
        }
        public static bool SecureEqualsUnsafe(
            Span<byte> left, Span<byte> right)
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEqualsUnsafe(
                (ReadOnlySpan<byte>)left, (ReadOnlySpan<byte>)right);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static unsafe bool SecureEqualsUnsafe(
            ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            var minLength = Math.Min(
                left.Length, right.Length);
            var lengthDifference =
                left.Length - right.Length;

            var length = minLength - (minLength % sizeof(long));

            fixed (byte* leftPointer = left)
            fixed (byte* rightPointer = right)
            {
                long difference = lengthDifference;

                for (var i = 0; i < length; i += sizeof(long))
                {
                    difference |=
                        *(long*)(leftPointer + i) - *(long*)(rightPointer + i);
                }

                for (var i = length; i < minLength; ++i)
                {
                    difference |=
                        (long)(*(leftPointer + i) - *(rightPointer + i));
                }

                return difference == 0;
            }
        }

        public static bool SecureEqualsUnsafe<T>(
            T[] left, T[] right)
            where T : unmanaged
        {
            return SecureEqualsUnsafe(
                new ReadOnlySpan<T>(left), new ReadOnlySpan<T>(right));
        }
        public static bool SecureEqualsUnsafe<T>(
            Span<T> left, Span<T> right)
            where T : unmanaged
        {
            if (left == null)
                left = default;
            if (right == null)
                right = default;

            return SecureEqualsUnsafe(
                (ReadOnlySpan<T>)left, (ReadOnlySpan<T>)right);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static unsafe bool SecureEqualsUnsafe<T>(
            ReadOnlySpan<T> left, ReadOnlySpan<T> right)
            where T : unmanaged
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;

            var minLength = Math.Min(
                left.Length, right.Length);
            var lengthDifference =
                left.Length - right.Length;

            var length = minLength - (minLength % sizeof(long));

            fixed (T* leftPointer = left)
            fixed (T* rightPointer = right)
            {
                long difference = lengthDifference;

                for (var i = 0; i < length; i += sizeof(long))
                {
                    difference |=
                        *(long*)(leftPointer + i) - *(long*)(rightPointer + i);
                }

                for (var i = length; i < minLength; ++i)
                {
                    difference |=
                        (long)(*(byte*)(leftPointer + i) - *(byte*)(rightPointer + i));
                }

                return difference == 0;
            }
        }
    }
}
