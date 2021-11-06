// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RIS.Collections.Trees;
using RIS.Extensions.Entities;

namespace RIS.Extensions
{
    public static class StringExtensions
    {
        public static (int Index, int Count) IndexOfAny(this string source,
            string[] values, int startIndex = 0, int length = -1)
        {
            return IndexOfAnyTrie(source,
                values, startIndex, length);
        }

        public static (int Index, int Count) IndexOfAnyLinq(this string source,
            string[] values, int startIndex = 0, int length = -1)
        {
            var rootIndex = RootIndex
                .FromStrings(values);

            return IndexOfAnyLinq(source,
                rootIndex, startIndex, length);
        }
        public static (int Index, int Count) IndexOfAnyLinq(this string source,
            IEnumerable<string> values, int startIndex = 0, int length = -1)
        {
            var rootIndex = RootIndex
                .FromStrings(values);

            return IndexOfAnyLinq(source,
                rootIndex, startIndex, length);
        }
        public static (int Index, int Count) IndexOfAnyLinq(this string source,
            RootIndex rootIndex, int startIndex = 0, int length = -1)
        {
            if (rootIndex == null)
                return (-1, 0);

            if (startIndex > source.Length - 1)
                startIndex = source.Length - 1;
            if (startIndex < 0)
                startIndex = 0;
            if (length > source.Length - startIndex)
                length = source.Length - startIndex;
            if (length < 0)
                length = source.Length - startIndex;

            var index = startIndex;
            var endIndex = startIndex + length;

            while (index < endIndex)
            {
                var occurrenceIndex = source.IndexOfAny(rootIndex.Chars, index);

                if (occurrenceIndex < 0)
                    return (occurrenceIndex, 0);

                var @char = source[occurrenceIndex];

                // only one character available
                if (source.Length == occurrenceIndex + 1)
                {
                    if (rootIndex.SingleChars.Contains(@char))
                        return (occurrenceIndex, 1);

                    return (-1, 0);
                }

                var leafIndex = rootIndex.MultipleChars[@char];
                var nextChar = source[occurrenceIndex + 1];

                // only two characters available
                if (source.Length == occurrenceIndex + 2)
                {
                    if (leafIndex.SingleChars.Contains(nextChar))
                        return (occurrenceIndex, 2);

                    if (rootIndex.Chars.Contains(nextChar))
                        return (occurrenceIndex + 1, 1);

                    return (-1, 0);
                }

                // several characters available
                if (leafIndex.Chars.Contains(nextChar))
                {
                    foreach (var @string in leafIndex.MultipleChars[nextChar])
                    {
                        if (string.CompareOrdinal(source, occurrenceIndex + 2, @string, 0, @string.Length) == 0)
                            return (occurrenceIndex, @string.Length + 2);
                    }
                }

                if (leafIndex.SingleChars.Contains(nextChar))
                    return (occurrenceIndex, 2);

                if (rootIndex.SingleChars.Contains(@char))
                    return (occurrenceIndex, 1);

                index = occurrenceIndex + 1;
            }

            return (-1, 0);
        }

        public static (int Index, int Count) IndexOfAnyTrie(this string source,
            string[] values, int startIndex = 0, int length = -1)
        {
            var trie = new Trie(
                values);

            return IndexOfAnyTrie(source,
                trie, startIndex, length);
        }
        public static (int Index, int Count) IndexOfAnyTrie(this string source,
            IEnumerable<string> values, int startIndex = 0, int length = -1)
        {
            var trie = new Trie(
                values);

            return IndexOfAnyTrie(source,
                trie, startIndex, length);
        }
        public static (int Index, int Count) IndexOfAnyTrie(this string source,
            Trie trie, int startIndex = 0, int length = -1)
        {
            if (trie == null)
                return (-1, 0);

            return trie.IndexOfAny(source,
                startIndex, length);
        }



        public static string ReplaceEOLChars(
            this string source, string newValue)
        {
            return source
                .Replace("\u000D\u000A", newValue)
                .Replace("\u000A", newValue)
                .Replace("\u0085", newValue)
                .Replace("\u2028", newValue)
                .Replace("\u2029", newValue);
        }



        public static sbyte ToSbyte(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return sbyte.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static byte ToByte(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return byte.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static short ToShort(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return short.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static ushort ToUShort(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return ushort.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static int ToInt(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return int.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static uint ToUInt(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return uint.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static long ToLong(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return long.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static ulong ToULong(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Integer)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return ulong.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static float ToFloat(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return float.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static double ToDouble(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return double.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static decimal ToDecimal(this string source, IFormatProvider provider = null,
            NumberStyles style = NumberStyles.Number)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return decimal.Parse(source,
                    style, provider);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static bool ToBoolean(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            try
            {
                return bool.Parse(source);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static char ToChar(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            try
            {
                return char.Parse(source);
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(
                    ex, ex.Message));
                throw;
            }
        }

        public static DateTime ToDateTime(this string source, IFormatProvider provider = null,
            DateTimeStyles style = DateTimeStyles.None)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return DateTime.Parse(source,
                    provider, style);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }
        public static DateTime ToDateTime(this string source, string format,
            IFormatProvider provider = null, DateTimeStyles style = DateTimeStyles.None)
        {
            if (string.IsNullOrEmpty(source))
            {
                var exception = new ArgumentException(
                    $"{nameof(source)} must not be null or empty",
                    nameof(source));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (string.IsNullOrEmpty(format))
            {
                var exception = new ArgumentException(
                    $"{nameof(format)} must not be null or empty",
                    nameof(format));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            if (provider == null)
                provider = CultureInfo.InvariantCulture;

            try
            {
                return DateTime.ParseExact(source,
                    format, provider, style);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }
    }
}
