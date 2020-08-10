// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Extensions
{
    public static class StringExtensions
    {
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
