// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RIS.Cryptography.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UInt128 : IEquatable<UInt128>
    {
        public const int Size = 16; // bytes



        public readonly ulong Lower;
        public readonly ulong Upper;



        public UInt128(
            ulong upper,
            ulong lower)
        {
            Lower = lower;
            Upper = upper;
        }



#pragma warning disable SS010 // Attempted to create empty guid
        public Guid ToGuid()
        {
            var lower = Lower;
            var upper = Upper;

            if (!BitConverter.IsLittleEndian)
            {
                lower = BinaryPrimitives
                    .ReverseEndianness(Upper);
                upper = BinaryPrimitives
                    .ReverseEndianness(Lower);
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
                a, b, c, d, e, f, g, h, i, j, k);
        }
#pragma warning restore SS010 // Attempted to create empty guid

        public byte[] ToBytes()
        {
            return BytesUtils.ToBytesLE(this);
        }
        public bool ToBytes(Span<byte> buffer)
        {
            return BytesUtils.ToBytesLE(this, buffer);
        }



        public override bool Equals(object obj)
        {
            return obj is UInt128 target
                   && Equals(target);
        }

        public bool Equals(UInt128 target)
        {
            return Lower == target.Lower
                   && Upper == target.Upper;
        }

       

        public override string ToString()
        {
            return ((BigInteger)this).ToString();
        }

        public override int GetHashCode()
        {
#if NETCOREAPP

            return HashCode.Combine(Lower, Upper);

#elif NETFRAMEWORK

            var hashCode = 591964497;

            hashCode = hashCode * -1521134295 + Lower.GetHashCode();
            hashCode = hashCode * -1521134295 + Upper.GetHashCode();

            return hashCode;

#endif
        }

        public static bool operator ==(UInt128 left, UInt128 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UInt128 left, UInt128 right)
        {
            return !(left == right);
        }



        public static explicit operator UInt128(BigInteger source)
        {
            if (source.Sign is -1 || source.Sign is 0)
                return new UInt128(0, 0);

            return new UInt128(
                (ulong)(source >> 64),
                (ulong)(source & ulong.MaxValue));
        }

        public static implicit operator BigInteger(UInt128 source)
        {
            if (source.Upper == 0)
                return source.Lower;

            return (BigInteger)source.Upper << 64
                   | source.Lower;
        }
    }
}
