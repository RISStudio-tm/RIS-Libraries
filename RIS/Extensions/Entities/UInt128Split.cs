// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace RIS.Extensions.Entities
{
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UInt128Split
    {
        /// <summary>
        /// Size in bytes
        /// </summary>
        public const int Size = 16;



        [FieldOffset(0)]
        public fixed byte Bytes[Size];

        [FieldOffset(0)]
        public UInt128 UInt128;

        /// <summary>
        /// Lower value
        /// </summary>
        [FieldOffset(0)]
        public ulong ULong1;
        /// <summary>
        /// Lower value
        /// </summary>
        [FieldOffset(0)]
        public ulong Lower;
        /// <summary>
        /// Upper value
        /// </summary>
        [FieldOffset(8)]
        public ulong ULong2;
        /// <summary>
        /// Upper value
        /// </summary>
        [FieldOffset(8)]
        public ulong Upper;

        [FieldOffset(0)]
        public uint UInt1;
        [FieldOffset(4)]
        public uint UInt2;
        [FieldOffset(8)]
        public uint UInt3;
        [FieldOffset(12)]
        public uint UInt4;

        [FieldOffset(0)]
        public ushort UShort1;
        [FieldOffset(2)]
        public ushort UShort2;
        [FieldOffset(4)]
        public ushort UShort3;
        [FieldOffset(6)]
        public ushort UShort4;
        [FieldOffset(8)]
        public ushort UShort5;
        [FieldOffset(10)]
        public ushort UShort6;
        [FieldOffset(12)]
        public ushort UShort7;
        [FieldOffset(14)]
        public ushort UShort8;

        [FieldOffset(0)]
        public byte Byte1;
        [FieldOffset(1)]
        public byte Byte2;
        [FieldOffset(2)]
        public byte Byte3;
        [FieldOffset(3)]
        public byte Byte4;
        [FieldOffset(4)]
        public byte Byte5;
        [FieldOffset(5)]
        public byte Byte6;
        [FieldOffset(6)]
        public byte Byte7;
        [FieldOffset(7)]
        public byte Byte8;
        [FieldOffset(8)]
        public byte Byte9;
        [FieldOffset(9)]
        public byte Byte10;
        [FieldOffset(10)]
        public byte Byte11;
        [FieldOffset(11)]
        public byte Byte12;
        [FieldOffset(12)]
        public byte Byte13;
        [FieldOffset(13)]
        public byte Byte14;
        [FieldOffset(14)]
        public byte Byte15;
        [FieldOffset(15)]
        public byte Byte16;
    }
    // ReSharper restore FieldCanBeMadeReadOnly.Global
}
