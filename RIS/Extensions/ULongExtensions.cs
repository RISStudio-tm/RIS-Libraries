// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using RIS.Utilities;

namespace RIS.Extensions
{
    public static class ULongExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this ulong number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this ulong number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this ulong number)
        {
            if (number <= 1)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 5 == 0)
                return false;

            var bound = (ulong)Math.Floor(
                Math.Sqrt(number));

            for (var i = 3UL; i <= bound; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(this ulong source)
        {
            return sizeof(ulong);
        }
        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this ulong source)
        {
            return BytesUtils.ToBytes(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(this ulong source, Span<byte> buffer)
        {
            return BytesUtils.ToBytes(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(this ulong source)
        {
            return BytesUtils.ToBytesLE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(this ulong source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesLE(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(this ulong source)
        {
            return BytesUtils.ToBytesBE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(this ulong source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesBE(source, buffer);
        }
    }
}
