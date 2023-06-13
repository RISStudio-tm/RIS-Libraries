// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace RIS.Extensions
{
    public static class BoolExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ToSByte(this bool source)
        {
            return (sbyte)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ToSByte(this bool? source)
        {
            return source?.ToSByte() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static byte ToByte(this bool source)
        {
            return (byte)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToByte(this bool? source)
        {
            return source?.ToByte() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static short ToShort(this bool source)
        {
            return (short)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ToShort(this bool? source)
        {
            return source?.ToShort() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static ushort ToUShort(this bool source)
        {
            return (ushort)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ToUShort(this bool? source)
        {
            return source?.ToUShort() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static int ToInt(this bool source)
        {
            return (int)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this bool? source)
        {
            return source?.ToInt() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static uint ToUInt(this bool source)
        {
            return (uint)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt(this bool? source)
        {
            return source?.ToUInt() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static long ToLong(this bool source)
        {
            return (long)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToLong(this bool? source)
        {
            return source?.ToLong() ?? 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static ulong ToULong(this bool source)
        {
            return (ulong)(source ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToULong(this bool? source)
        {
            return source?.ToULong() ?? 0;
        }
    }
}
