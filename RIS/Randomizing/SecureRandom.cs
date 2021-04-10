// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;

namespace RIS.Randomizing
{
    public class SecureRandom : IUnbiasedRandom
    {
        private readonly RNGCryptoServiceProvider _randomGenerator;

        public object SyncRoot { get; }

        public SecureRandom()
        {
            _randomGenerator = new RNGCryptoServiceProvider();

            SyncRoot = new object();
        }



        private static byte GetBiasZone(byte targetSamplingLength)
        {
            return (byte)(byte.MaxValue - ((byte.MaxValue % targetSamplingLength) + 1));
        }

        private void FillBuffer(byte[] buffer)
        {
            _randomGenerator.GetBytes(buffer);
        }

        private void OptimizeBias(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = GetUInt8();
                }
            }
        }

        public byte GetUInt8()
        {
            var result = new byte[1];

            FillBuffer(result);

            return result[0];
        }
        public byte GetUInt8(byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            byte result;

            do
            {
                result = GetUInt8();
            } while (result > maxValue);

            return result;
        }

        public byte GetNormalizedUInt8(byte targetSamplingLength)
        {
            return GetUInt8(
                GetBiasZone(targetSamplingLength));
        }

        public byte GetNormalizedIndex(byte targetSamplingLength)
        {
            return (byte)(GetNormalizedUInt8(targetSamplingLength) % targetSamplingLength);
        }

        public void GetUInt8(byte[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt8(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
        }

        public void GetNormalizedUInt8(byte[] buffer, byte targetSamplingLength)
        {
            GetUInt8(
                buffer,
                GetBiasZone(targetSamplingLength));
        }

        public void GetNormalizedIndex(byte[] buffer, byte targetSamplingLength)
        {
            GetNormalizedUInt8(buffer, targetSamplingLength);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] %= targetSamplingLength;
            }
        }



        private static ushort GetBiasZone(ushort targetSamplingLength)
        {
            return (ushort)(ushort.MaxValue - ((ushort.MaxValue % targetSamplingLength) + 1));
        }

        private void FillBuffer(ushort[] buffer)
        {
            var result = new byte[buffer.Length * 2];

            FillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt16(result, i * 2);
            }
        }

        private void OptimizeBias(ushort[] buffer, ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = GetUInt16();
                }
            }
        }

        public ushort GetUInt16()
        {
            var result = new ushort[1];

            FillBuffer(result);

            return result[0];
        }
        public ushort GetUInt16(ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            ushort result;

            do
            {
                result = GetUInt16();
            } while (result > maxValue);

            return result;
        }

        public ushort GetNormalizedUInt16(ushort targetSamplingLength)
        {
            return GetUInt16(
                GetBiasZone(targetSamplingLength));
        }

        public ushort GetNormalizedIndex(ushort targetSamplingLength)
        {
            return (ushort)(GetNormalizedUInt16(targetSamplingLength) % targetSamplingLength);
        }

        public void GetUInt16(ushort[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt16(ushort[] buffer, ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
        }

        public void GetNormalizedUInt16(ushort[] buffer, ushort targetSamplingLength)
        {
            GetUInt16(
                buffer,
                GetBiasZone(targetSamplingLength));
        }

        public void GetNormalizedIndex(ushort[] buffer, ushort targetSamplingLength)
        {
            GetNormalizedUInt16(buffer, targetSamplingLength);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] %= targetSamplingLength;
            }
        }



        private static uint GetBiasZone(uint targetSamplingLength)
        {
            return (uint)(uint.MaxValue - ((uint.MaxValue % targetSamplingLength) + 1));
        }

        private void FillBuffer(uint[] buffer)
        {
            var result = new byte[buffer.Length * 4];

            FillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt32(result, i * 4);
            }
        }

        private void OptimizeBias(uint[] buffer, uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = GetUInt32();
                }
            }
        }

        public uint GetUInt32()
        {
            var result = new uint[1];

            FillBuffer(result);

            return result[0];
        }
        public uint GetUInt32(uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            uint result;

            do
            {
                result = GetUInt32();
            } while (result > maxValue);

            return result;
        }

        public uint GetNormalizedUInt32(uint targetSamplingLength)
        {
            return GetUInt32(
                GetBiasZone(targetSamplingLength));
        }

        public ulong GetNormalizedIndex(uint targetSamplingLength)
        {
            return (uint)(GetNormalizedUInt32(targetSamplingLength) % targetSamplingLength);
        }

        public void GetUInt32(uint[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt32(uint[] buffer, uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
        }

        public void GetNormalizedUInt32(uint[] buffer, uint targetSamplingLength)
        {
            GetUInt32(
                buffer,
                GetBiasZone(targetSamplingLength));
        }

        public void GetNormalizedIndex(uint[] buffer, uint targetSamplingLength)
        {
            GetNormalizedUInt32(buffer, targetSamplingLength);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] %= targetSamplingLength;
            }
        }



        private static ulong GetBiasZone(ulong targetSamplingLength)
        {
            return (ulong)(ulong.MaxValue - ((ulong.MaxValue % targetSamplingLength) + 1));
        }

        private void FillBuffer(ulong[] buffer)
        {
            var result = new byte[buffer.Length * 8];

            FillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt64(result, i * 8);
            }
        }

        private void OptimizeBias(ulong[] buffer, ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = GetUInt64();
                }
            }
        }

        public ulong GetUInt64()
        {
            var result = new ulong[1];

            FillBuffer(result);

            return result[0];
        }
        public ulong GetUInt64(ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            ulong result;

            do
            {
                result = GetUInt64();
            } while (result > maxValue);

            return result;
        }

        public ulong GetNormalizedUInt64(ulong targetSamplingLength)
        {
            return GetUInt64(
                GetBiasZone(targetSamplingLength));
        }

        public ulong GetNormalizedIndex(ulong targetSamplingLength)
        {
            return (ulong)(GetNormalizedUInt64(targetSamplingLength) % targetSamplingLength);
        }

        public void GetUInt64(ulong[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt64(ulong[] buffer, ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
        }

        public void GetNormalizedUInt64(ulong[] buffer, ulong targetSamplingLength)
        {
            GetUInt64(
                buffer,
                GetBiasZone(targetSamplingLength));
        }

        public void GetNormalizedIndex(ulong[] buffer, ulong targetSamplingLength)
        {
            GetNormalizedUInt64(buffer, targetSamplingLength);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] %= targetSamplingLength;
            }
        }
    }
}
