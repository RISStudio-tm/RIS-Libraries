// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Security.Cryptography;
using RIS.Collections.Caches;

namespace RIS.Randomizing
{
    public class CachedSecureRandom : ICachedUnbiasedRandom
    {
        private readonly RNGCryptoServiceProvider _randomGenerator;
        private readonly BytesCache[] _caches;
        private readonly uint _cachesCountBiasZone;

        public object SyncRoot { get; }

        public uint CachesSize { get; }
        public ushort CachesCount { get; }
        public bool CachesClearUsedValues { get; }
        public bool CachesPinned { get; }
        public bool CachesUseInitBlock { get; }

        public CachedSecureRandom(
            uint cachesSize = 1 * 1024 * 1024,
            ushort cachesCount = 3,
            bool cachesClearUsedValues = true,
            bool cachesPinned = true,
            bool cachesUseInitBlock = true)
        {
            if (cachesSize < 1024)
                cachesSize = 1024;

            if (cachesCount < 1)
                cachesCount = 1;

            _randomGenerator = new RNGCryptoServiceProvider();

            SyncRoot = new object();

            CachesSize = cachesSize;
            CachesCount = cachesCount;
            CachesClearUsedValues = cachesClearUsedValues;
            CachesPinned = cachesPinned;
            CachesUseInitBlock = cachesUseInitBlock;

            _caches = new BytesCache[cachesCount];
            BytesCache.UpdateCallback updateHandler = FillBufferUncached;

            for (int i = 0; i < _caches.Length; ++i)
            {
                _caches[i] = new BytesCache(
                    updateHandler, cachesSize, cachesClearUsedValues,
                    cachesPinned, cachesUseInitBlock);

                _caches[i].Update();
            }

            _cachesCountBiasZone =
                uint.MaxValue - (uint.MaxValue % (uint)_caches.Length) - 1;
        }

        private BytesCache GetRandomCache()
        {
            if (_caches.Length == 1)
                return _caches[0];

            uint result;

            do
            {
                result = GetUInt32Uncached();
            } while (result > _cachesCountBiasZone);

            return _caches[result % (uint)_caches.Length];
        }

        private void UpdateCache(BytesCache cache)
        {
            lock (SyncRoot)
            {
                UnsafeUpdateCache(cache);
            }
        }
#pragma warning disable U2U1002 // Method can be declared static
        private void UnsafeUpdateCache(BytesCache cache)
        {
            cache.Update();
        }
#pragma warning restore U2U1002 // Method can be declared static



        private static byte GetBiasZone(byte targetSamplingLength)
        {
            return (byte)(byte.MaxValue - ((byte.MaxValue % targetSamplingLength) + 1));
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
            var cache = GetRandomCache();

            cache.FillBuffer(buffer);
        }

        private void OptimizeBias(byte[] buffer, byte maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeOptimizeBias(buffer, maxValue);
            }
        }
        private void UnsafeOptimizeBias(byte[] buffer, byte maxValue)
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
                    element = UnsafeGetUInt8();
                }
            }
        }

        private byte GetUInt8Uncached()
        {
            var result = new byte[1];

            FillBufferUncached(result);

            return result[0];
        }

        public byte GetUInt8()
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt8();
            }
        }
        private byte UnsafeGetUInt8()
        {
            var cache = GetRandomCache();

            return cache.GetByte();
        }
        public byte GetUInt8(byte maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt8(maxValue);
            }
        }
        private byte UnsafeGetUInt8(byte maxValue)
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
                result = UnsafeGetUInt8();
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

        private void GetUInt8Uncached(byte[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt8(byte[] buffer)
        {
            if (buffer.Length > CachesSize)
            {
                FillBufferUncached(buffer);

                return;
            }

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

            if (buffer.Length > (int)Math.Ceiling(CachesSize / 2.0))
            {
                FillBufferUncached(buffer);
                OptimizeBias(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeOptimizeBias(buffer, maxValue);
            }
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

        private void OptimizeBias(ushort[] buffer, ushort maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeOptimizeBias(buffer, maxValue);
            }
        }
        private void UnsafeOptimizeBias(ushort[] buffer, ushort maxValue)
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
                    element = UnsafeGetUInt16();
                }
            }
        }

        private ushort GetUInt16Uncached()
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
        private ushort UnsafeGetUInt16()
        {
            var result = new ushort[1];

            FillBuffer(result);

            return result[0];
        }
        public ushort GetUInt16(ushort maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt16(maxValue);
            }
        }
        private ushort UnsafeGetUInt16(ushort maxValue)
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
                result = UnsafeGetUInt16();
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

        private void GetUInt16Uncached(ushort[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt16(ushort[] buffer)
        {
            if (buffer.Length > CachesSize)
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CachesSize / 2.0))
            {
                FillBufferUncached(buffer);
                OptimizeBias(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeOptimizeBias(buffer, maxValue);
            }
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

        private void OptimizeBias(uint[] buffer, uint maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeOptimizeBias(buffer, maxValue);
            }
        }
        private void UnsafeOptimizeBias(uint[] buffer, uint maxValue)
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
                    element = UnsafeGetUInt32();
                }
            }
        }

        private uint GetUInt32Uncached()
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
        private uint UnsafeGetUInt32()
        {
            var result = new uint[1];

            FillBuffer(result);

            return result[0];
        }
        public uint GetUInt32(uint maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt32(maxValue);
            }
        }
        private uint UnsafeGetUInt32(uint maxValue)
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
                result = UnsafeGetUInt32();
            } while (result > maxValue);

            return result;
        }

        public uint GetNormalizedUInt32(uint targetSamplingLength)
        {
            return GetUInt32(
                GetBiasZone(targetSamplingLength));
        }

        public uint GetNormalizedIndex(uint targetSamplingLength)
        {
            return (uint)(GetNormalizedUInt32(targetSamplingLength) % targetSamplingLength);
        }

        private void GetUInt32Uncached(uint[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt32(uint[] buffer)
        {
            if (buffer.Length > CachesSize)
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CachesSize / 2.0))
            {
                FillBufferUncached(buffer);
                OptimizeBias(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeOptimizeBias(buffer, maxValue);
            }
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

        private void OptimizeBias(ulong[] buffer, ulong maxValue)
        {
            lock (SyncRoot)
            {
                UnsafeOptimizeBias(buffer, maxValue);
            }
        }
        private void UnsafeOptimizeBias(ulong[] buffer, ulong maxValue)
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
                    element = UnsafeGetUInt64();
                }
            }
        }

        private ulong GetUInt64Uncached()
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
        private ulong UnsafeGetUInt64()
        {
            var result = new ulong[1];

            FillBuffer(result);

            return result[0];
        }
        public ulong GetUInt64(ulong maxValue)
        {
            lock (SyncRoot)
            {
                return UnsafeGetUInt64(maxValue);
            }
        }
        private ulong UnsafeGetUInt64(ulong maxValue)
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
                result = UnsafeGetUInt64();
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

        private void GetUInt64Uncached(ulong[] buffer)
        {
            FillBufferUncached(buffer);
        }

        public void GetUInt64(ulong[] buffer)
        {
            if (buffer.Length > CachesSize)
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
                var exception = new ArgumentOutOfRangeException(nameof(maxValue), $"{nameof(maxValue)} cannot be equal to 0");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            if (buffer.Length > (int)Math.Ceiling(CachesSize / 2.0))
            {
                FillBufferUncached(buffer);
                OptimizeBias(buffer, maxValue);

                return;
            }

            lock (SyncRoot)
            {
                UnsafeFillBuffer(buffer);
                UnsafeOptimizeBias(buffer, maxValue);
            }
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
