// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RIS.Cryptography.Entities;

namespace RIS.Cryptography
{
    public static class BytesUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void BlockCopy(
            byte[] source, int sourceOffset,
            byte[] destination, int destinationOffset,
            int count)
        {
            fixed (byte* fixedSourcePointer = &source[sourceOffset])
            fixed (byte* fixedDestinationPointer = &destination[destinationOffset])
            {
                var sourcePointer = fixedSourcePointer;
                var destinationPointer = fixedDestinationPointer;
                
                // Label
                Table32C:



                switch (count)
                {
                    case 0:
                        return;
                    case 1:
                        *destinationPointer = *sourcePointer;
                        return;
                    case 2:
                        *(short*)destinationPointer = *(short*)sourcePointer;
                        return;
                    case 3:
                        *(short*)(destinationPointer + 0) = *(short*)(sourcePointer + 0);
                        *(destinationPointer + 2) = *(sourcePointer + 2);
                        return;
                    case 4:
                        *(int*)destinationPointer = *(int*)sourcePointer;
                        return;
                    case 5:
                        *(int*)(destinationPointer + 0) = *(int*)(sourcePointer + 0);
                        *(destinationPointer + 4) = *(sourcePointer + 4);
                        return;
                    case 6:
                        *(int*)(destinationPointer + 0) = *(int*)(sourcePointer + 0);
                        *(short*)(destinationPointer + 4) = *(short*)(sourcePointer + 4);
                        return;
                    case 7:
                        *(int*)(destinationPointer + 0) = *(int*)(sourcePointer + 0);
                        *(short*)(destinationPointer + 4) = *(short*)(sourcePointer + 4);
                        *(destinationPointer + 6) = *(sourcePointer + 6);
                        return;
                    case 8:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        return;
                    case 9:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(destinationPointer + 8) = *(sourcePointer + 8);
                        return;
                    case 10:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(short*)(destinationPointer + 8) = *(short*)(sourcePointer + 8);
                        return;
                    case 11:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(short*)(destinationPointer + 8) = *(short*)(sourcePointer + 8);
                        *(destinationPointer + 10) = *(sourcePointer + 10);
                        return;
                    case 12:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(int*)(destinationPointer + 8) = *(int*)(sourcePointer + 8);
                        return;
                    case 13:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(int*)(destinationPointer + 8) = *(int*)(sourcePointer + 8);
                        *(destinationPointer + 12) = *(sourcePointer + 12);
                        return;
                    case 14:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(int*)(destinationPointer + 8) = *(int*)(sourcePointer + 8);
                        *(short*)(destinationPointer + 12) = *(short*)(sourcePointer + 12);
                        return;
                    case 15:
                        *(long*)(destinationPointer + 0) = *(long*)(sourcePointer + 0);
                        *(int*)(destinationPointer + 8) = *(int*)(sourcePointer + 8);
                        *(short*)(destinationPointer + 12) = *(short*)(sourcePointer + 12);
                        *(destinationPointer + 14) = *(sourcePointer + 14);
                        return;
                    case 16:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        return;
                    case 17:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(destinationPointer + 16) = *(sourcePointer + 16);
                        return;
                    case 18:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(short*)(destinationPointer + 16) = *(short*)(sourcePointer + 16);
                        return;
                    case 19:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(short*)(destinationPointer + 16) = *(short*)(sourcePointer + 16);
                        *(destinationPointer + 18) = *(sourcePointer + 18);
                        return;
                    case 20:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(int*)(destinationPointer + 16) = *(int*)(sourcePointer + 16);
                        return;
    
                    case 21:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(int*)(destinationPointer + 16) = *(int*)(sourcePointer + 16);
                        *(destinationPointer + 20) = *(sourcePointer + 20);
                        return;
                    case 22:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(int*)(destinationPointer + 16) = *(int*)(sourcePointer + 16);
                        *(short*)(destinationPointer + 20) = *(short*)(sourcePointer + 20);
                        return;
                    case 23:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(int*)(destinationPointer + 16) = *(int*)(sourcePointer + 16);
                        *(short*)(destinationPointer + 20) = *(short*)(sourcePointer + 20);
                        *(destinationPointer + 22) = *(sourcePointer + 22);
                        return;
                    case 24:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        return;
                    case 25:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(destinationPointer + 24) = *(sourcePointer + 24);
                        return;
                    case 26:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(short*)(destinationPointer + 24) = *(short*)(sourcePointer + 24);
                        return;
                    case 27:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(short*)(destinationPointer + 24) = *(short*)(sourcePointer + 24);
                        *(destinationPointer + 26) = *(sourcePointer + 26);
                        return;
                    case 28:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(int*)(destinationPointer + 24) = *(int*)(sourcePointer + 24);
                        return;
                    case 29:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(int*)(destinationPointer + 24) = *(int*)(sourcePointer + 24);
                        *(destinationPointer + 28) = *(sourcePointer + 28);
                        return;
                    case 30:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(int*)(destinationPointer + 24) = *(int*)(sourcePointer + 24);
                        *(short*)(destinationPointer + 28) = *(short*)(sourcePointer + 28);
                        return;
                    case 31:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(int*)(destinationPointer + 24) = *(int*)(sourcePointer + 24);
                        *(short*)(destinationPointer + 28) = *(short*)(sourcePointer + 28);
                        *(destinationPointer + 30) = *(sourcePointer + 30);
                        return;
                    case 32:
                        *(long*)destinationPointer = *(long*)sourcePointer;
                        *(long*)(destinationPointer + 8) = *(long*)(sourcePointer + 8);
                        *(long*)(destinationPointer + 16) = *(long*)(sourcePointer + 16);
                        *(long*)(destinationPointer + 24) = *(long*)(sourcePointer + 24);
                        return;
                }

                var longSourcePointer = (long*)sourcePointer;
                var longDestinationPointer = (long*)destinationPointer;

                while (count >= 64)
                {
                    *(longDestinationPointer + 0) = *(longSourcePointer + 0);
                    *(longDestinationPointer + 1) = *(longSourcePointer + 1);
                    *(longDestinationPointer + 2) = *(longSourcePointer + 2);
                    *(longDestinationPointer + 3) = *(longSourcePointer + 3);
                    *(longDestinationPointer + 4) = *(longSourcePointer + 4);
                    *(longDestinationPointer + 5) = *(longSourcePointer + 5);
                    *(longDestinationPointer + 6) = *(longSourcePointer + 6);
                    *(longDestinationPointer + 7) = *(longSourcePointer + 7);

                    if (count == 64)
                        return;

                    count -= 64;
                    longSourcePointer += 8;
                    longDestinationPointer += 8;
                }

                if (count > 32)
                {
                    *(longDestinationPointer + 0) = *(longSourcePointer + 0);
                    *(longDestinationPointer + 1) = *(longSourcePointer + 1);
                    *(longDestinationPointer + 2) = *(longSourcePointer + 2);
                    *(longDestinationPointer + 3) = *(longSourcePointer + 3);

                    count -= 32;
                    longSourcePointer += 4;
                    longDestinationPointer += 4;
                }
                
                sourcePointer = (byte*)longSourcePointer;
                destinationPointer = (byte*)longDestinationPointer;

                goto Table32C;
            }
        }



        public static byte[] ToBytes(UInt128 source)
        {
            var bytes = new byte[sizeof(ulong) * 2];

            Unsafe.As<byte, ulong>(ref bytes[0]) = source.Lower;
            Unsafe.As<byte, ulong>(ref bytes[8]) = source.Upper;

            return bytes;
        }
        public static byte[] ToBytesLE(UInt128 source)
        {
            var buffer = new byte[sizeof(ulong) * 2];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesLE(UInt128 source, Span<byte> buffer)
        {
            if (buffer.Length < UInt128.Size)
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesLEUnsafe(UInt128 source, Span<byte> buffer)
        {
            var lower = source.Lower;
            var upper = source.Upper;

            if (!BitConverter.IsLittleEndian)
            {
                lower = BinaryPrimitives
                    .ReverseEndianness(lower);
                upper = BinaryPrimitives
                    .ReverseEndianness(upper);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                lower);
            Unsafe.WriteUnaligned(
                ref Unsafe.AddByteOffset(
                    ref address,
                    sizeof(ulong)),
                upper);
        }
        public static byte[] ToBytesBE(UInt128 source)
        {
            var buffer = new byte[sizeof(ulong) * 2];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesBE(UInt128 source, Span<byte> buffer)
        {
            if (buffer.Length < UInt128.Size)
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesBEUnsafe(UInt128 source, Span<byte> buffer)
        {
            var lower = source.Lower;
            var upper = source.Upper;

            if (BitConverter.IsLittleEndian)
            {
                lower = BinaryPrimitives
                    .ReverseEndianness(lower);
                upper = BinaryPrimitives
                    .ReverseEndianness(upper);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                upper);
            Unsafe.WriteUnaligned(
                ref Unsafe.AddByteOffset(
                    ref address,
                    sizeof(ulong)),
                lower);
        }

        public static byte[] ToBytes(ulong source)
        {
            var bytes = new byte[sizeof(ulong)];

            Unsafe.As<byte, ulong>(ref bytes[0]) = source;

            return bytes;
        }
        public static byte[] ToBytesLE(ulong source)
        {
            var buffer = new byte[sizeof(ulong)];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesLE(ulong source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesLEUnsafe(ulong source, Span<byte> buffer)
        {
            var value = source;

            if (!BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives
                    .ReverseEndianness(value);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                value);
        }
        public static byte[] ToBytesBE(ulong source)
        {
            var buffer = new byte[sizeof(ulong)];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesBE(ulong source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesBEUnsafe(ulong source, Span<byte> buffer)
        {
            var value = source;

            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives
                    .ReverseEndianness(value);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                value);
        }

        public static byte[] ToBytes(uint source)
        {
            var bytes = new byte[sizeof(uint)];

            Unsafe.As<byte, uint>(ref bytes[0]) = source;

            return bytes;
        }
        public static byte[] ToBytesLE(uint source)
        {
            var buffer = new byte[sizeof(uint)];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesLE(uint source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(uint))
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesLEUnsafe(uint source, Span<byte> buffer)
        {
            var value = source;

            if (!BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives
                    .ReverseEndianness(value);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                value);
        }
        public static byte[] ToBytesBE(uint source)
        {
            var buffer = new byte[sizeof(uint)];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        public static bool ToBytesBE(uint source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(uint))
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        public static void ToBytesBEUnsafe(uint source, Span<byte> buffer)
        {
            var value = source;

            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives
                    .ReverseEndianness(value);
            }

            ref var address =
                ref MemoryMarshal.GetReference(
                    buffer);

            Unsafe.WriteUnaligned(
                ref address,
                value);
        }
    }
}
