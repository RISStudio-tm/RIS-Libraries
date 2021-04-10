// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;

namespace RIS.Randomizing
{
    public interface IGaussianRandom
    {
        double NextGaussian();
    }

    public interface IUnbiasedRandom
    {
        object SyncRoot { get; }



        byte GetUInt8();
        byte GetUInt8(byte maxValue);

        byte GetNormalizedUInt8(byte targetSamplingLength);

        byte GetNormalizedIndex(byte targetSamplingLength);

        void GetUInt8(byte[] buffer);
        void GetUInt8(byte[] buffer, byte maxValue);

        void GetNormalizedUInt8(byte[] buffer, byte targetSamplingLength);

        void GetNormalizedIndex(byte[] buffer, byte targetSamplingLength);



        ushort GetUInt16();
        ushort GetUInt16(ushort maxValue);

        ushort GetNormalizedUInt16(ushort targetSamplingLength);

        ushort GetNormalizedIndex(ushort targetSamplingLength);

        void GetUInt16(ushort[] buffer);
        void GetUInt16(ushort[] buffer, ushort maxValue);

        void GetNormalizedUInt16(ushort[] buffer, ushort targetSamplingLength);

        void GetNormalizedIndex(ushort[] buffer, ushort targetSamplingLength);



        uint GetUInt32();
        uint GetUInt32(uint maxValue);

        uint GetNormalizedUInt32(uint targetSamplingLength);

        ulong GetNormalizedIndex(uint targetSamplingLength);

        void GetUInt32(uint[] buffer);
        void GetUInt32(uint[] buffer, uint maxValue);

        void GetNormalizedUInt32(uint[] buffer, uint targetSamplingLength);

        void GetNormalizedIndex(uint[] buffer, uint targetSamplingLength);



        ulong GetUInt64();
        ulong GetUInt64(ulong maxValue);

        ulong GetNormalizedUInt64(ulong targetSamplingLength);

        ulong GetNormalizedIndex(ulong targetSamplingLength);

        void GetUInt64(ulong[] buffer);
        void GetUInt64(ulong[] buffer, ulong maxValue);

        void GetNormalizedUInt64(ulong[] buffer, ulong targetSamplingLength);

        void GetNormalizedIndex(ulong[] buffer, ulong targetSamplingLength);
    }

    public interface ICachedUnbiasedRandom : IUnbiasedRandom
    {

    }
}
