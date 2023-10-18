// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using RIS.Utilities;

namespace RIS.Extensions
{
    public static class UIntExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this uint number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this uint number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this uint number)
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
        public static int GetSize(this uint source)
        {
            return sizeof(uint);
        }
        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this uint source)
        {
            return BytesUtils.ToBytes(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(this uint source, Span<byte> buffer)
        {
            return BytesUtils.ToBytes(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(this uint source)
        {
            return BytesUtils.ToBytesLE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(this uint source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesLE(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(this uint source)
        {
            return BytesUtils.ToBytesBE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(this uint source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesBE(source, buffer);
        }
    }
}
