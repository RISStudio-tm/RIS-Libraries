// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Linq;

namespace RIS.Extensions
{
    public static class StringExtensions
    {
        public static (int Index, int Count) IndexOfAny(this string sourceCode,
            string[] values, int startIndex = 0)
        {
            var rootIndex = RootIndex.FromStrings(values);

            while (startIndex < sourceCode.Length)
            {
                var index = sourceCode.IndexOfAny(rootIndex.Chars, startIndex);

                if (index < 0)
                    return (index, 0);

                // only one character available
                var @char = sourceCode[index];
                if (sourceCode.Length == index + 1)
                {
                    if (rootIndex.SingleChars.Contains(@char))
                        return (index, 1);

                    return (-1, 0);
                }

                // only two characters available
                var leafIndex = rootIndex.MultipleChars[@char];
                var nextChar = sourceCode[index + 1];
                if (sourceCode.Length == index + 2)
                {
                    if (leafIndex.SingleChars.Contains(nextChar))
                        return (index, 2);

                    if (rootIndex.Chars.Contains(nextChar))
                        return (index + 1, 1);

                    return (-1, 0);
                }

                // several characters available
                if (leafIndex.Chars.Contains(nextChar))
                {
                    foreach (var s in leafIndex.Strings[nextChar])
                    {
                        if (string.CompareOrdinal(sourceCode, index + 2, s, 0, s.Length) == 0)
                            return (index, s.Length + 2);
                    }
                }

                if (leafIndex.SingleChars.Contains(nextChar))
                    return (index, 2);

                if (rootIndex.SingleChars.Contains(@char))
                    return (index, 1);

                startIndex = index + 1;
            }

            return (-1, 0);
        }

        public static bool ToBoolean(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return bool.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static sbyte ToSbyte(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return sbyte.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static byte ToByte(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return byte.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static short ToShort(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return short.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static ushort ToUShort(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return ushort.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static int ToInt(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return int.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static uint ToUInt(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return uint.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static long ToLong(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return long.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static ulong ToULong(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return ulong.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static float ToFloat(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return float.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static double ToDouble(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return double.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static decimal ToDecimal(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return decimal.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }

        public static char ToChar(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("Parse error. String is null or empty");
            }

            try
            {
                return char.Parse(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Parse error", ex);
            }
        }
    }
}
