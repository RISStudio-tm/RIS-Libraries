// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RIS.Cryptography.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UInt128 : IEquatable<UInt128>
    {
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
            var a = (int)(Lower);
            var b = (short)(Lower >> 32);
            var c = (short)(Lower >> 48);

            var d = (byte)(Upper);
            var e = (byte)(Upper >> 8);
            var f = (byte)(Upper >> 16);
            var g = (byte)(Upper >> 24);
            var h = (byte)(Upper >> 32);
            var i = (byte)(Upper >> 40);
            var j = (byte)(Upper >> 48);
            var k = (byte)(Upper >> 56);

            return new Guid(
                a, b, c, d, e, f, g, h, i, j, k);
        }
#pragma warning restore SS010 // Attempted to create empty guid

        public byte[] ToBytes()
        {
            var bytes = new byte[sizeof(ulong) * 2];

            Unsafe.As<byte, ulong>(ref bytes[0]) = Lower;
            Unsafe.As<byte, ulong>(ref bytes[8]) = Upper;

            return bytes;
        }
        public void ToBytes(Span<byte> buffer)
        {
            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                Lower);
            Unsafe.WriteUnaligned(
                ref Unsafe.AddByteOffset(
                    ref address,
                    sizeof(ulong)),
                Upper);
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
