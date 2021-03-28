// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Randomizing
{
    public interface IGaussianRandom
    {
        double NextGaussian();
    }

    public interface IBiasedRandom
    {
        object SyncRoot { get; }



        byte GetByte();
        byte GetByte(byte maxValue);

        void GetByte(byte[] buffer);
        void GetByte(byte[] buffer, byte maxValue);



        ushort GetUInt16();
        ushort GetUInt16(ushort maxValue);

        void GetUInt16(ushort[] buffer);
        void GetUInt16(ushort[] buffer, ushort maxValue);



        uint GetUInt32();
        uint GetUInt32(uint maxValue);

        void GetUInt32(uint[] buffer);
        void GetUInt32(uint[] buffer, uint maxValue);



        ulong GetUInt64();
        ulong GetUInt64(ulong maxValue);

        void GetUInt64(ulong[] buffer);
        void GetUInt64(ulong[] buffer, ulong maxValue);
    }

    public interface ICachedBiasedRandom : IBiasedRandom
    {
        byte GetByteUncached();

        void GetByteUncached(byte[] buffer);



        ushort GetUInt16Uncached();

        void GetUInt16Uncached(ushort[] buffer);



        uint GetUInt32Uncached();

        void GetUInt32Uncached(uint[] buffer);



        ulong GetUInt64Uncached();

        void GetUInt64Uncached(ulong[] buffer);
    }
}
