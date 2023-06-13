// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Hash.Algorithms
{
    public sealed class CRC32 : HashAlgorithm
    {
        private const uint Polynomial = 0xEDB88320;
        private static readonly uint[] Table = new uint[16 * 256];
        private uint CurrentInitial { get; set; }

        public new static CRC32 Create()
        {
            return new CRC32();
        }
        public new static CRC32 Create(string hashName)
        {
            return new CRC32();
        }

        static CRC32()
        {
            ref uint[] table = ref Table;

            for (uint i = 0; i < 256; ++i)
            {
                uint result = i;

                for (int t = 0; t < 16; ++t)
                {
                    for (int k = 0; k < 8; ++k)
                    {
                        result = (result & 1) == 1
                            ? Polynomial ^ (result >> 1)
                            : (result >> 1);
                    }

                    table[(t * 256) + i] = result;
                }
            }
        }

        public CRC32()
        {
            HashSizeValue = 32;
        }

        public override void Initialize()
        {
            CurrentInitial = 0xFFFFFFFF;
        }

        public static uint Append(uint initial, byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();

            return AppendInternal(initial, input, 0, input.Length);
        }
        public static uint Append(uint initial, byte[] input, int offset, int length)
        {
            if (input == null)
                throw new ArgumentNullException();

            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException();

            return AppendInternal(initial, input, offset, length);
        }
        private static uint AppendInternal(uint initial, byte[] input, int offset, int length)
        {
            if (length <= 0)
                return initial;

            uint crcLocal = initial;
            uint[] table = Table;

            while (length >= 16)
            {
                crcLocal = table[(15 * 256) + ((crcLocal ^ input[offset]) & 0xff)]
                           ^ table[(14 * 256) + (((crcLocal >> 8) ^ input[offset + 1]) & 0xff)]
                           ^ table[(13 * 256) + (((crcLocal >> 16) ^ input[offset + 2]) & 0xff)]
                           ^ table[(12 * 256) + (((crcLocal >> 24) ^ input[offset + 3]) & 0xff)]
                           ^ table[(11 * 256) + input[offset + 4]]
                           ^ table[(10 * 256) + input[offset + 5]]
                           ^ table[(9 * 256) + input[offset + 6]]
                           ^ table[(8 * 256) + input[offset + 7]]
                           ^ table[(7 * 256) + input[offset + 8]]
                           ^ table[(6 * 256) + input[offset + 9]]
                           ^ table[(5 * 256) + input[offset + 10]]
                           ^ table[(4 * 256) + input[offset + 11]]
                           ^ table[(3 * 256) + input[offset + 12]]
                           ^ table[(2 * 256) + input[offset + 13]]
                           ^ table[(1 * 256) + input[offset + 14]]
                           ^ table[(0 * 256) + input[offset + 15]];
                offset += 16;
                length -= 16;
            }

            while (--length >= 0)
            {
                crcLocal = table[(crcLocal ^ input[offset++]) & 0xff] ^ crcLocal >> 8;
            }

            return crcLocal ^ 0xFFFFFFFF;
        }

        public static uint Compute(byte[] input)
        {
            return Append(0, input);
        }
        public static uint Compute(byte[] input, int offset, int length)
        {
            return Append(0, input, offset, length);
        }

        protected override void HashCore(byte[] input, int offset, int length)
        {
            CurrentInitial = AppendInternal(CurrentInitial, input, offset, length);
        }

        protected override byte[] HashFinal()
        {
            return BytesUtils.ToBytesBE(CurrentInitial);
        }
    }
}
