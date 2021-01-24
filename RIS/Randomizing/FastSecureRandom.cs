// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Security.Cryptography;
using RIS.Extensions;

namespace RIS.Randomizing
{
    public class FastSecureRandom
    {
        private readonly RNGCryptoServiceProvider _randomGenerator;
        private readonly byte[] _cache;
        private int _cachePositionOffset;
        private int _cacheRemainingCount;

        public object SyncRoot { get; }
        public int CacheSize { get; }

        public FastSecureRandom()
            : this(2 * 1024 * 1024)
        {

        }
        public FastSecureRandom(int cacheSize)
        {
            if (cacheSize < 1024)
                cacheSize = 1024;

            _randomGenerator = new RNGCryptoServiceProvider();
            _cache = new byte[cacheSize];

            SyncRoot = new object();
            CacheSize = cacheSize;

            UnsafeUpdateCache();
        }

        private void UpdateCache()
        {
            lock (SyncRoot)
            {
                UnsafeUpdateCache();
            }
        }

        private void UnsafeUpdateCache()
        {
            FillBufferUncached(_cache);

            _cachePositionOffset = 0;
            _cacheRemainingCount = CacheSize;
        }



        private void FillBufferUncached(byte[] buffer)
        {
            _randomGenerator.GetBytes(buffer);
        }

        private void FillBuffer(byte[] buffer)
        {
            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
            }
        }

        private void UnsafeFillBuffer(byte[] buffer)
        {
            //Recache if not enough remainingCount, discarding remainingCount - too much work to join two blocks
            if (_cacheRemainingCount < buffer.Length)
                UnsafeUpdateCache();

            _cache.DeepCopy(_cachePositionOffset, buffer, 0, buffer.Length);

            _cachePositionOffset += buffer.Length;
            _cacheRemainingCount -= buffer.Length;
        }

        private void AdjustingValues(byte[] buffer, byte maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }

        private void UnsafeAdjustingValues(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = UnsafeGetByte();
                }
            }
        }

        public byte GetByteUncached()
        {
            var result = new byte[1];

            FillBufferUncached(result);

            return result[0];
        }

        public byte GetByte()
        {
            lock (SyncRoot)
            {
                return UnsafeGetByte();
            }
        }
        public byte GetByte(byte maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetByte(maxValue);
            }
        }

        private byte UnsafeGetByte()
        {
            if (_cacheRemainingCount < 1)
                UnsafeUpdateCache();

            ++_cachePositionOffset;
            --_cacheRemainingCount;

            return _cache[_cachePositionOffset - 1];
        }
        private byte UnsafeGetByte(byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            byte result;

            do
            {
                result = UnsafeGetByte();
            } while (result >= maxValue);

            return result;
        }

        public void GetBytesUncached(byte[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetBytes(byte[] buffer)
        {
            if (buffer.Length > CacheSize)
            {
                FillBufferUncached(buffer);

                return;
            }

            FillBuffer(buffer);
        }
        public void GetBytes(byte[] buffer, byte maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CacheSize / 2.0))
            {
                FillBufferUncached(buffer);
                AdjustingValues(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }



        private void FillBufferUncached(ushort[] buffer)
        {
            var result = new byte[buffer.Length * 2];

            FillBufferUncached(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt16(result, i * 2);
            }
        }

        private void FillBuffer(ushort[] buffer)
        {
            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
            }
        }

        private void UnsafeFillBuffer(ushort[] buffer)
        {
            var result = new byte[buffer.Length * 2];

            UnsafeFillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt16(result, i * 2);
            }
        }

        private void AdjustingValues(ushort[] buffer, ushort maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }

        private void UnsafeAdjustingValues(ushort[] buffer, ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = UnsafeGetUInt16();
                }
            }
        }

        public ushort GetUInt16Uncached()
        {
            var result = new ushort[1];

            FillBufferUncached(result);

            return result[0];
        }

        public ushort GetUInt16()
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt16();
            }
        }
        public ushort GetUInt16(ushort maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt16(maxValue);
            }
        }

        private ushort UnsafeGetUInt16()
        {
            var result = new ushort[1];

            FillBuffer(result);

            return result[0];
        }
        private ushort UnsafeGetUInt16(ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            ushort result;

            do
            {
                result = UnsafeGetUInt16();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt16Uncached(ushort[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt16(ushort[] buffer)
        {
            if (buffer.Length > CacheSize)
            {
                FillBufferUncached(buffer);

                return;
            }

            FillBuffer(buffer);
        }
        public void GetUInt16(ushort[] buffer, ushort maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CacheSize / 2.0))
            {
                FillBufferUncached(buffer);
                AdjustingValues(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }



        private void FillBufferUncached(uint[] buffer)
        {
            var result = new byte[buffer.Length * 4];

            FillBufferUncached(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt32(result, i * 4);
            }
        }

        private void FillBuffer(uint[] buffer)
        {
            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
            }
        }

        private void UnsafeFillBuffer(uint[] buffer)
        {
            var result = new byte[buffer.Length * 4];

            UnsafeFillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt32(result, i * 4);
            }
        }

        private void AdjustingValues(uint[] buffer, uint maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }

        private void UnsafeAdjustingValues(uint[] buffer, uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = UnsafeGetUInt32();
                }
            }
        }

        public uint GetUInt32Uncached()
        {
            var result = new uint[1];

            FillBufferUncached(result);

            return result[0];
        }

        public uint GetUInt32()
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt32();
            }
        }
        public uint GetUInt32(uint maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt32(maxValue);
            }
        }

        private uint UnsafeGetUInt32()
        {
            var result = new uint[1];

            FillBuffer(result);

            return result[0];
        }
        private uint UnsafeGetUInt32(uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            uint result;

            do
            {
                result = UnsafeGetUInt32();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt32Uncached(uint[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt32(uint[] buffer)
        {
            if (buffer.Length > CacheSize)
            {
                FillBufferUncached(buffer);

                return;
            }

            FillBuffer(buffer);
        }
        public void GetUInt32(uint[] buffer, uint maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CacheSize / 2.0))
            {
                FillBufferUncached(buffer);
                AdjustingValues(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }



        private void FillBufferUncached(ulong[] buffer)
        {
            var result = new byte[buffer.Length * 8];

            FillBufferUncached(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt64(result, i * 8);
            }
        }

        private void FillBuffer(ulong[] buffer)
        {
            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
            }
        }

        private void UnsafeFillBuffer(ulong[] buffer)
        {
            var result = new byte[buffer.Length * 8];

            UnsafeFillBuffer(result);

            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = BitConverter.ToUInt64(result, i * 8);
            }
        }

        private void AdjustingValues(ulong[] buffer, ulong maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }

        private void UnsafeAdjustingValues(ulong[] buffer, ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            for (int i = 0; i < buffer.Length; ++i)
            {
                ref var element = ref buffer[i];

                while (element > maxValue)
                {
                    element = UnsafeGetUInt64();
                }
            }
        }

        public ulong GetUInt64Uncached()
        {
            var result = new ulong[1];

            FillBufferUncached(result);

            return result[0];
        }

        public ulong GetUInt64()
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt64();
            }
        }
        public ulong GetUInt64(ulong maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt64(maxValue);
            }
        }

        private ulong UnsafeGetUInt64()
        {
            var result = new ulong[1];

            FillBuffer(result);

            return result[0];
        }
        private ulong UnsafeGetUInt64(ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            ulong result;

            do
            {
                result = UnsafeGetUInt64();
            } while (result >= maxValue);

            return result;
        }

        public void GetUInt64Uncached(ulong[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt64(ulong[] buffer)
        {
            if (buffer.Length > CacheSize)
            {
                FillBufferUncached(buffer);

                return;
            }

            FillBuffer(buffer);
        }
        public void GetUInt64(ulong[] buffer, ulong maxValue)
        {
            if (maxValue == 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), "MaxValue cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CacheSize / 2.0))
            {
                FillBufferUncached(buffer);
                AdjustingValues(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeAdjustingValues(buffer, maxValue);
            }
        }
    }
}
