// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using RIS.Utilities;

namespace RIS.Extensions
{
    public static class UShortExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this ushort number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this ushort number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this ushort number)
        {
            if (number <= 1)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 5 == 0)
                return false;

            var bound = (int)Math.Floor(
                Math.Sqrt(number));

            for (var i = 3; i <= bound; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(this ushort source)
        {
            return sizeof(ushort);
        }
        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this ushort source)
        {
            return BytesUtils.ToBytes(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(this ushort source, Span<byte> buffer)
        {
            return BytesUtils.ToBytes(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(this ushort source)
        {
            return BytesUtils.ToBytesLE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(this ushort source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesLE(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(this ushort source)
        {
            return BytesUtils.ToBytesBE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(this ushort source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesBE(source, buffer);
        }
    }
}
