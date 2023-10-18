// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using RIS.Extensions.Entities;
using RIS.Utilities;

namespace RIS.Extensions
{
    public static class UInt128Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this UInt128 number)
        {
            return (number & 1) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this UInt128 number)
        {
            return (number & 1) == 1;
        }

        public static bool IsPrime(this UInt128 number)
        {
            if (number <= 1)
                return false;
            if (number == 2 || number == 3)
                return true;
            if (number % 2 == 0 || number % 5 == 0)
                return false;

            var bound = (UInt128)Math.Floor(
                Math.Sqrt((double)number));

            for (UInt128 i = 3; i <= bound; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(this UInt128 source)
        {
            return sizeof(ulong) * 2;
        }
        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(this UInt128 source)
        {
            return BytesUtils.ToBytes(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(this UInt128 source, Span<byte> buffer)
        {
            return BytesUtils.ToBytes(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(this UInt128 source)
        {
            return BytesUtils.ToBytesLE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(this UInt128 source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesLE(source, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(this UInt128 source)
        {
            return BytesUtils.ToBytesBE(source);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(this UInt128 source, Span<byte> buffer)
        {
            return BytesUtils.ToBytesBE(source, buffer);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToGuid(this UInt128 source)
        {
            ref var sourceSplit = ref source.AsSplit();

            var lower = sourceSplit.Lower;
            var upper = sourceSplit.Upper;

            if (!BitConverter.IsLittleEndian)
            {
                lower = BinaryPrimitives
                    .ReverseEndianness(sourceSplit.Upper);
                upper = BinaryPrimitives
                    .ReverseEndianness(sourceSplit.Lower);
            }

            var a = (int)(lower);

            var b = (short)(lower >> 32);
            var c = (short)(lower >> 48);

            var d = (byte)(upper);
            var e = (byte)(upper >> 8);
            var f = (byte)(upper >> 16);
            var g = (byte)(upper >> 24);
            var h = (byte)(upper >> 32);
            var i = (byte)(upper >> 40);
            var j = (byte)(upper >> 48);
            var k = (byte)(upper >> 56);

            return new Guid(
                a,
                b, c,
                d, e, f, g, h, i, j, k);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref UInt128Split AsSplit(ref this UInt128 source)
        {
            return ref Unsafe.As<UInt128, UInt128Split>(ref source);
        }
    }
}
