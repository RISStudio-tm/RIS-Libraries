// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;

namespace RIS.Cryptography.Hash.Algorithms
{
    public abstract class CRC32Base : HashAlgorithm
    {
        private uint _polynomial = 0xEDB88320;
        protected uint Polynomial
        {
            get
            {
                return _polynomial;
            }
            set
            {
                _polynomial = value;

                CreateTable();
            }
        }
        protected uint Initial = 0xFFFFFFFF;
        protected uint OutputXOR = 0xFFFFFFFF;
        protected bool ReflectedPolynomial = true;
        protected bool ReflectedInput = true;
        protected bool ReflectedOutput = true;
        private uint[] _table;
        protected uint[] Table
        {
            get
            {
                return _table;
            }
            private set
            {
                _table = value;
            }
        }
        protected uint CurrentInitial { get; set; }

        protected CRC32Base()
        {
            HashSizeValue = 32;

            CreateTable();
        }

        private void CreateTable()
        {
            _table = new uint[16 * 256];

            uint localPolynomial = !ReflectedPolynomial
                ? Environment.ReflectBits(Polynomial)
                : Polynomial;
            ref uint[] table = ref _table;

            for (uint i = 0; i < 256; ++i)
            {
                uint result = i;

                for (int t = 0; t < 16; ++t)
                {
                    for (int k = 0; k < 8; ++k)
                    {
                        result = (result & 1) == 1
                            ? localPolynomial ^ (result >> 1)
                            : (result >> 1);
                    }

                    table[(t * 256) + i] = result;
                }
            }
        }

        public override void Initialize()
        {
            CurrentInitial = Initial;
        }

        public uint Append(uint initial, byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();

            return AppendInternal(initial, input, 0, input.Length);
        }
        public uint Append(uint initial, byte[] input, int offset, int length)
        {
            if (input == null)
                throw new ArgumentNullException();

            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException();

            return AppendInternal(initial, input, offset, length);
        }
        private uint AppendInternal(uint initial, byte[] input, int offset, int length)
        {
            if (length <= 0)
                return initial;

            if (ReflectedInput)
            {
                for (int i = 0; i < input.Length; ++i)
                {
                    ref var element = ref input[i];

                    element = Environment.ReflectBits(element);
                }
            }

            uint crcLocal = initial;
            ref uint[] table = ref _table;

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

            return ReflectedOutput
                ? Environment.ReflectBits(crcLocal) ^ OutputXOR
                : crcLocal ^ OutputXOR;

        }

        public uint Compute(byte[] input)
        {
            return Append(0, input);
        }
        public uint Compute(byte[] input, int offset, int length)
        {
            return Append(0, input, offset, length);
        }

        protected override void HashCore(byte[] input, int offset, int length)
        {
            CurrentInitial = AppendInternal(CurrentInitial, input, offset, length);
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(CurrentInitial);
        }
    }
}
