// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security.Cryptography;

namespace RIS.Randomizing
{
    internal sealed class RNGRandom : NextBitsRandom
    {
        internal const int BufferLength = 512;

        private readonly RandomNumberGenerator _random;
        private readonly byte[] _buffer;
        private int _nextByteIndex;

        public RNGRandom(RandomNumberGenerator random)
            : base(0)
        {
            _random = random;
            _buffer = new byte[BufferLength];
            _nextByteIndex = BufferLength;
        }

        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length <= BufferLength - _nextByteIndex)
            {
                for (int i = _nextByteIndex; i < buffer.Length; ++i)
                {
                    buffer[i] = _buffer[i];
                }

                _nextByteIndex += buffer.Length;
            }
            else
            {
                _random.GetBytes(buffer);
            }
        }

        internal override int NextBits(int countBits)
        {
            uint result = 0;
            int i = 0;

            while (true)
            {
                if (_nextByteIndex == BufferLength)
                {
                    _random.GetBytes(_buffer);
                    _nextByteIndex = 0;
                }

                checked
                {
                    result += (uint)_buffer[_nextByteIndex++] << i;
                }

                i += 8;

                if (i < countBits)
                    continue;

                uint nextBits = result >> (i - countBits);

                return unchecked((int)nextBits);
            }
        }
    }
}
