// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using RIS.Extensions;

namespace RIS.Utilities
{
    public static class BytesUtils
    {
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



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(UInt128 source)
        {
            var buffer = new byte[sizeof(ulong) * 2];

            ToBytesUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(UInt128 source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong) * 2)
                return false;

            ToBytesUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesUnsafe(UInt128 source, Span<byte> buffer)
        {
            ref var sourceSplit = ref source.AsSplit();

            Unsafe.As<byte, ulong>(ref buffer[0]) = sourceSplit.Lower;
            Unsafe.As<byte, ulong>(ref buffer[8]) = sourceSplit.Upper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(UInt128 source)
        {
            var buffer = new byte[sizeof(ulong) * 2];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(UInt128 source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong) * 2)
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesLEUnsafe(UInt128 source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<UInt128>)source;

            result.WriteLittleEndian(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(UInt128 source)
        {
            var buffer = new byte[sizeof(ulong) * 2];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(UInt128 source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong) * 2)
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesBEUnsafe(UInt128 source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<UInt128>)source;

            result.WriteBigEndian(buffer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(ulong source)
        {
            var buffer = new byte[sizeof(ulong)];

            ToBytesUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(ulong source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
                return false;

            ToBytesUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesUnsafe(ulong source, Span<byte> buffer)
        {
            Unsafe.As<byte, ulong>(ref buffer[0]) = source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(ulong source)
        {
            var buffer = new byte[sizeof(ulong)];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(ulong source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesLEUnsafe(ulong source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<ulong>)source;

            result.WriteLittleEndian(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(ulong source)
        {
            var buffer = new byte[sizeof(ulong)];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(ulong source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ulong))
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesBEUnsafe(ulong source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<ulong>)source;

            result.WriteBigEndian(buffer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(uint source)
        {
            var buffer = new byte[sizeof(uint)];

            ToBytesUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(uint source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(uint))
                return false;

            ToBytesUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesUnsafe(uint source, Span<byte> buffer)
        {
            Unsafe.As<byte, uint>(ref buffer[0]) = source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(uint source)
        {
            var buffer = new byte[sizeof(uint)];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(uint source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(uint))
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesLEUnsafe(uint source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<uint>)source;

            result.WriteLittleEndian(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(uint source)
        {
            var buffer = new byte[sizeof(uint)];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(uint source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(uint))
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesBEUnsafe(uint source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<uint>)source;

            result.WriteBigEndian(buffer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytes(ushort source)
        {
            var buffer = new byte[sizeof(ushort)];

            ToBytesUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytes(ushort source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ushort))
                return false;

            ToBytesUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesUnsafe(ushort source, Span<byte> buffer)
        {
            Unsafe.As<byte, ushort>(ref buffer[0]) = source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesLE(ushort source)
        {
            var buffer = new byte[sizeof(ushort)];

            ToBytesLEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesLE(ushort source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ushort))
                return false;

            ToBytesLEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesLEUnsafe(ushort source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<ushort>)source;

            result.WriteLittleEndian(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToBytesBE(ushort source)
        {
            var buffer = new byte[sizeof(ushort)];

            ToBytesBEUnsafe(source, buffer);

            return buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToBytesBE(ushort source, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(ushort))
                return false;

            ToBytesBEUnsafe(source, buffer);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesBEUnsafe(ushort source, Span<byte> buffer)
        {
            var result = (IBinaryInteger<ushort>)source;

            result.WriteBigEndian(buffer);
        }
    }
}
