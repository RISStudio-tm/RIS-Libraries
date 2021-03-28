// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;
using RIS.Collections.Caches;

namespace RIS.Randomizing
{
    public class SecureRandom : IBiasedRandom
    {
        private readonly RNGCryptoServiceProvider _randomGenerator;

        public object SyncRoot { get; }

        public SecureRandom()
        {
            _randomGenerator = new RNGCryptoServiceProvider();

            SyncRoot = new object();
        }



        private void FillBuffer(byte[] buffer)
        {
            _randomGenerator.GetBytes(buffer);
        }

        private void OptimizeBias(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = GetByte();
                }
            }
        }

        public byte GetByte()
        {
            var result = new byte[1];

            FillBuffer(result);

            return result[0];
        }
        public byte GetByte(byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            byte result;

            do
            {
                result = GetByte();
            } while (result >= maxValue);

            return result;
        }

        public void GetByte(byte[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetByte(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            ushort result;

            do
            {
                result = GetUInt16();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt16(ushort[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt16(ushort[] buffer, ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            uint result;

            do
            {
                result = GetUInt32();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt32(uint[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt32(uint[] buffer, uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            ulong result;

            do
            {
                result = GetUInt64();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt64(ulong[] buffer)
        {
            FillBuffer(buffer);
        }
        public void GetUInt64(ulong[] buffer, ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            FillBuffer(buffer);
            OptimizeBias(buffer, maxValue);
        }
    }
}
