// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using RIS.Utilities;

namespace RIS.Cryptography.Hash.Digests
{
    public class KeccakDigest
    {
        private static readonly ulong[] KeccakRoundConstants =
        {
            0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
            0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
            0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
            0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
            0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
            0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL
        };



        private readonly ulong[] _state = new ulong[25];
        private readonly byte[] _dataQueue = new byte[192];
        private int _bitsInQueue;
        private int _rate;
        private bool _squeezing;



        public int Size { get; protected set; }
        public int SizeBytes { get; protected set; }



        public KeccakDigest()
            : this(288)
        {

        }
        public KeccakDigest(
            int size)
        {
            Init(
                size);
        }



        private void Init(
            int size)
        {
            switch (size)
            {
                case 128:
                case 224:
                case 256:
                case 288:
                case 384:
                case 512:
                    InitSponge(
                        1600 - (size << 1));
                    break;
                default:
                {
                    var exception = new ArgumentException(
                        $"{nameof(size)} must be one of 128, 224, 256, 288, 384 or 512",
                        nameof(size));
                    Events.OnError(new RErrorEventArgs(
                        exception, exception.Message));
                    throw exception;
                }
            }
        }

        private void InitSponge(
            int rate)
        {
            if (rate <= 0 || rate >= 1600 || (rate & 63) != 0)
            {
                var exception = new ArgumentException(
                    $"Invalid rate[{rate}] value",
                    nameof(rate));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            for (var i = 0; i < _state.Length; ++i)
            {
                _state[i] = 0;
            }

            for (var i = 0; i < _dataQueue.Length; ++i)
            {
                _dataQueue[i] = 0;
            }

            _bitsInQueue = 0;
            _rate = rate;
            _squeezing = false;

            Size = (1600 - rate) >> 1;
            SizeBytes = Size >> 3;
        }


        private void Pad()
        {
            _dataQueue[_bitsInQueue >> 3] |=
                (byte)(1 << (_bitsInQueue & 7));

            ++_bitsInQueue;

            if (_bitsInQueue == _rate)
            {
                Absorb(_dataQueue, 0);
            }
            else
            {
                var full = _bitsInQueue >> 6;
                var partial = _bitsInQueue & 63;
                var offset = 0;

                for (var i = 0; i < full; ++i)
                {
                    _state[i] ^= ToUInt64LE(
                        _dataQueue, offset);

                    offset += 8;
                }

                if (partial > 0)
                {
                    var mask = (1UL << partial) - 1UL;

                    _state[full] ^= ToUInt64LE(
                        _dataQueue, offset) & mask;
                }
            }

            _state[(_rate - 1) >> 6] ^= 1UL << 63;

            _bitsInQueue = 0;
        }

        private void Extract()
        {
            Permutation();

            ToBytesLE(
                _state, 0,
                _dataQueue, 0,
                _rate >> 6);

            _bitsInQueue = _rate;
        }

        private void Permutation()
        {
            var a = _state;

            ulong
                a00 = a[0], a01 = a[1], a02 = a[2], a03 = a[3], a04 = a[4],
                a05 = a[5], a06 = a[6], a07 = a[7], a08 = a[8], a09 = a[9],
                a10 = a[10], a11 = a[11], a12 = a[12], a13 = a[13], a14 = a[14],
                a15 = a[15], a16 = a[16], a17 = a[17], a18 = a[18], a19 = a[19],
                a20 = a[20], a21 = a[21], a22 = a[22], a23 = a[23], a24 = a[24];

            for (var i = 0; i < KeccakRoundConstants.Length; ++i)
            {
                var c0 = a00 ^ a05 ^ a10 ^ a15 ^ a20;
                var c1 = a01 ^ a06 ^ a11 ^ a16 ^ a21;
                var c2 = a02 ^ a07 ^ a12 ^ a17 ^ a22;
                var c3 = a03 ^ a08 ^ a13 ^ a18 ^ a23;
                var c4 = a04 ^ a09 ^ a14 ^ a19 ^ a24;

                var d1 = ((c1 << 1) | (c1 >> -1)) ^ c4;
                var d2 = ((c2 << 1) | (c2 >> -1)) ^ c0;
                var d3 = ((c3 << 1) | (c3 >> -1)) ^ c1;
                var d4 = ((c4 << 1) | (c4 >> -1)) ^ c2;
                var d0 = ((c0 << 1) | (c0 >> -1)) ^ c3;

                a00 ^= d1;
                a05 ^= d1;
                a10 ^= d1;
                a15 ^= d1;
                a20 ^= d1;
                a01 ^= d2;
                a06 ^= d2;
                a11 ^= d2;
                a16 ^= d2;
                a21 ^= d2;
                a02 ^= d3;
                a07 ^= d3;
                a12 ^= d3;
                a17 ^= d3;
                a22 ^= d3;
                a03 ^= d4;
                a08 ^= d4;
                a13 ^= d4;
                a18 ^= d4;
                a23 ^= d4;
                a04 ^= d0;
                a09 ^= d0;
                a14 ^= d0;
                a19 ^= d0;
                a24 ^= d0;

                c1 = (a01 << 1) | (a01 >> 63);
                a01 = (a06 << 44) | (a06 >> 20);
                a06 = (a09 << 20) | (a09 >> 44);
                a09 = (a22 << 61) | (a22 >> 3);
                a22 = (a14 << 39) | (a14 >> 25);
                a14 = (a20 << 18) | (a20 >> 46);
                a20 = (a02 << 62) | (a02 >> 2);
                a02 = (a12 << 43) | (a12 >> 21);
                a12 = (a13 << 25) | (a13 >> 39);
                a13 = (a19 << 8) | (a19 >> 56);
                a19 = (a23 << 56) | (a23 >> 8);
                a23 = (a15 << 41) | (a15 >> 23);
                a15 = (a04 << 27) | (a04 >> 37);
                a04 = (a24 << 14) | (a24 >> 50);
                a24 = (a21 << 2) | (a21 >> 62);
                a21 = (a08 << 55) | (a08 >> 9);
                a08 = (a16 << 45) | (a16 >> 19);
                a16 = (a05 << 36) | (a05 >> 28);
                a05 = (a03 << 28) | (a03 >> 36);
                a03 = (a18 << 21) | (a18 >> 43);
                a18 = (a17 << 15) | (a17 >> 49);
                a17 = (a11 << 10) | (a11 >> 54);
                a11 = (a07 << 6) | (a07 >> 58);
                a07 = (a10 << 3) | (a10 >> 61);
                a10 = c1;

                c0 = a00 ^ (~a01 & a02);
                c1 = a01 ^ (~a02 & a03);
                a02 ^= ~a03 & a04;
                a03 ^= ~a04 & a00;
                a04 ^= ~a00 & a01;
                a00 = c0;
                a01 = c1;

                c0 = a05 ^ (~a06 & a07);
                c1 = a06 ^ (~a07 & a08);
                a07 ^= ~a08 & a09;
                a08 ^= ~a09 & a05;
                a09 ^= ~a05 & a06;
                a05 = c0;
                a06 = c1;

                c0 = a10 ^ (~a11 & a12);
                c1 = a11 ^ (~a12 & a13);
                a12 ^= ~a13 & a14;
                a13 ^= ~a14 & a10;
                a14 ^= ~a10 & a11;
                a10 = c0;
                a11 = c1;

                c0 = a15 ^ (~a16 & a17);
                c1 = a16 ^ (~a17 & a18);
                a17 ^= ~a18 & a19;
                a18 ^= ~a19 & a15;
                a19 ^= ~a15 & a16;
                a15 = c0;
                a16 = c1;

                c0 = a20 ^ (~a21 & a22);
                c1 = a21 ^ (~a22 & a23);
                a22 ^= ~a23 & a24;
                a23 ^= ~a24 & a20;
                a24 ^= ~a20 & a21;
                a20 = c0;
                a21 = c1;

                a00 ^= KeccakRoundConstants[i];
            }

            a[0] = a00;
            a[1] = a01;
            a[2] = a02;
            a[3] = a03;
            a[4] = a04;
            a[5] = a05;
            a[6] = a06;
            a[7] = a07;
            a[8] = a08;
            a[9] = a09;
            a[10] = a10;
            a[11] = a11;
            a[12] = a12;
            a[13] = a13;
            a[14] = a14;
            a[15] = a15;
            a[16] = a16;
            a[17] = a17;
            a[18] = a18;
            a[19] = a19;
            a[20] = a20;
            a[21] = a21;
            a[22] = a22;
            a[23] = a23;
            a[24] = a24;
        }


        private void ToBytesLE(
            ulong[] source, int sourceOffset,
            byte[] destination, int destinationOffset,
            int length)
        {
            for (var i = 0; i < length; ++i)
            {
                BytesUtils.ToBytesLEUnsafe(
                    source[sourceOffset + i],
                    destination.AsSpan(destinationOffset));

                destinationOffset += 8;
            }
        }

        private uint ToUInt32LE(
            byte[] source, int offset)
        {
            return source[offset]
                   | ((uint)source[offset + 1] << 8)
                   | ((uint)source[offset + 2] << 16)
                   | ((uint)source[offset + 3] << 24);
        }

        private ulong ToUInt64LE(
            byte[] source, int offset)
        {
            var lower = ToUInt32LE(
                source, offset);
            var upper = ToUInt32LE(
                source, offset + 4);

            return ((ulong)upper << 32) | lower;
        }



        protected void Absorb(
            byte data)
        {
            if ((_bitsInQueue & 7) != 0)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb with odd length queue");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (_squeezing)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb while squeezing");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            _dataQueue[_bitsInQueue >> 3] = data;

            if ((_bitsInQueue += 8) == _rate)
            {
                Absorb(_dataQueue, 0);

                _bitsInQueue = 0;
            }
        }
        protected void Absorb(
            byte[] data, int offset)
        {
            var length = _rate >> 6;

            for (var i = 0; i < length; ++i)
            {
                _state[i] ^= ToUInt64LE(
                    data, offset);

                offset += 8;
            }

            Permutation();
        }
        protected void Absorb(
            byte[] data, int offset,
            int length)
        {
            if ((_bitsInQueue & 7) != 0)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb with odd length queue");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (_squeezing)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb while squeezing");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            var bytesInQueue = _bitsInQueue >> 3;
            var rateBytes = _rate >> 3;
            var availableBytes = rateBytes - bytesInQueue;

            if (length < availableBytes)
            {
                Array.Copy(
                    data, offset,
                    _dataQueue, bytesInQueue,
                    length);

                _bitsInQueue += length << 3;

                return;
            }

            var count = 0;

            if (bytesInQueue > 0)
            {
                Array.Copy(
                    data, offset,
                    _dataQueue, bytesInQueue,
                    availableBytes);
                
                count += availableBytes;
                
                Absorb(_dataQueue, 0);
            }

            int remaining;

            while ((remaining = length - count) >= rateBytes)
            {
                Absorb(
                    data, offset + count);

                count += rateBytes;
            }

            Array.Copy(
                data, offset + count,
                _dataQueue, 0,
                remaining);

            _bitsInQueue = remaining << 3;
        }

        protected void AbsorbBits(
            int data, int bits)
        {
            if (bits < 1 || bits > 7)
            {
                var exception = new ArgumentException(
                    $"{nameof(bits)}[{bits}] must be in the range [0,7]",
                    nameof(bits));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if ((_bitsInQueue & 7) != 0)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb with odd length queue");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }
            if (_squeezing)
            {
                var exception = new InvalidOperationException(
                    "Attempt to absorb while squeezing");
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            var mask = (1 << bits) - 1;

            _dataQueue[_bitsInQueue >> 3] =
                (byte)(data & mask);

            _bitsInQueue += bits;
        }


        protected void Squeeze(
            byte[] data, int offset,
            long length)
        {
            if (!_squeezing)
                Pad();

            _squeezing = true;

            if ((length & 7L) != 0L)
            {
                var exception = new ArgumentException(
                    $"{nameof(length)}[{length}] not a multiple of 8",
                    nameof(length));
                Events.OnError(new RErrorEventArgs(
                    exception, exception.Message));
                throw exception;
            }

            for (long i = 0; i < length; ++i)
            {
                if (_bitsInQueue == 0)
                    Extract();

                var partialBlock = (int)Math.Min(
                    _bitsInQueue, length - i);

                Array.Copy(
                    _dataQueue, (_rate - _bitsInQueue) >> 3,
                    data, offset + (int)(i >> 3),
                    partialBlock >> 3);

                _bitsInQueue -= partialBlock;
                i += partialBlock;
            }
        }


        protected virtual int DoFinal(
            byte[] data, int offset,
            byte partialByte, int partialBits)
        {
            if (partialBits > 0)
                AbsorbBits(partialByte, partialBits);

            Squeeze(
                data, offset,
                Size);

            Reset();

            return SizeBytes;
        }



        public virtual void Update(
            byte data)
        {
            Absorb(
                data);
        }


        public virtual void BlockUpdate(
            byte[] data, int offset,
            int length)
        {
            Absorb(
                data, offset,
                length);
        }


        public virtual int DoFinal(
            byte[] data, int offset)
        {
            Squeeze(
                data, offset,
                Size);

            Reset();

            return SizeBytes;
        }


        public virtual void Reset()
        {
            Init(Size);
        }
    }
}