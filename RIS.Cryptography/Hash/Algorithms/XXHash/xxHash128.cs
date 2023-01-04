// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.CompilerServices;
using RIS.Cryptography.Entities;

namespace RIS.Cryptography.Hash.Algorithms
{
    // ReSharper disable InconsistentNaming
    public static partial class XXHash128
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe UInt128 UnsafeComputeHash(
            byte* input, int len, ulong seed)
        {
            fixed (byte* secret = &XXH3_SECRET[0])
            {
                return XXH3_128bits_internal(input, len, seed, secret, XXH3_SECRET_DEFAULT_SIZE);
            }
        }



        public static unsafe UInt128 ComputeHash(
            byte[] data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        public static unsafe UInt128 ComputeHash(
            Span<byte> data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        public static unsafe UInt128 ComputeHash(
            ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        public static unsafe UInt128 ComputeHash(
            string data, ulong seed = 0)
        {
            fixed (char* c = data)
            {
                var ptr = (byte*) c;
                var length = data.Length * 2;
                
                return UnsafeComputeHash(ptr, length, seed);
            }
        }
        

        public static unsafe byte[] ComputeHashBytes(
            byte[] data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        public static unsafe byte[] ComputeHashBytes(
            Span<byte> data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        public static unsafe byte[] ComputeHashBytes(
            ReadOnlySpan<byte> data, int length, ulong seed = 0)
        {
            fixed (byte* ptr = &data[0])
            {
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
        public static unsafe byte[] ComputeHashBytes(
            string data, ulong seed = 0)
        {
            fixed (char* c = data)
            {
                var ptr = (byte*) c;
                var length = data.Length * 2;
                
                return UnsafeComputeHash(ptr, length, seed).ToBytes();
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
