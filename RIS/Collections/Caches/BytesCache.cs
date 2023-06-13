// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using RIS.Runtime.Unsafe;
using RIS.Extensions;

namespace RIS.Collections.Caches
{
    public class BytesCache
    {
        private delegate void ClearCallback(int startIndex, int count);

        public delegate void UpdateCallback(byte[] cache);

        private readonly byte[] _storage;
        private readonly ClearCallback _clearStorageHandler;
        private readonly UpdateCallback _updateStorageHandler;

        public uint Size { get; }
        public int PositionOffset { get; private set; }
        public int RemainingCount { get; private set; }
        public GCHandle Handle { get; }
        public IntPtr PinnedAddress { get; }
        public bool ClearUsedValues { get; }
        public bool IsPinned { get; }
        public bool UseInitBlock { get; }

        public BytesCache(UpdateCallback updateHandler,
            uint cacheSize = 32, bool clearUsedValues = true,
            bool pinned = true, bool useInitBlock = true)
        {
            if (cacheSize < 32)
                cacheSize = 32;

            _storage = new byte[cacheSize];

            Size = cacheSize;
            PositionOffset = 0;
            RemainingCount = 0;

            var handleType = pinned
                ? GCHandleType.Pinned
                : GCHandleType.Normal;
            Handle = GCHandle.Alloc(_storage,
                handleType);

            if (pinned)
            {
                PinnedAddress = Handle
                    .AddrOfPinnedObject();
            }
            else
            {
                PinnedAddress = IntPtr.Zero;
            }

            ClearUsedValues = clearUsedValues;
            IsPinned = pinned;
            UseInitBlock = useInitBlock;

            _clearStorageHandler =
                CreateClearStorageHandler();
            _updateStorageHandler = updateHandler;
        }

        ~BytesCache()
        {
            if (Handle.IsAllocated)
                Handle.Free();
        }

        private ClearCallback CreateClearStorageHandler()
        {
            Expression<ClearCallback> result = (startIndex, count) =>
                Array.Clear(_storage, startIndex,
                    count);

            if (!UseInitBlock)
                return result.Compile();

            if (IsPinned)
            {
                result = (startIndex, count) =>
                    InitBlock.Set(PinnedAddress, startIndex,
                        (uint)count, 0);
            }
            else
            {
                result = (startIndex, count) =>
                    InitBlock.Set(_storage, startIndex,
                        (uint)count, 0);
            }

            return result.Compile();
        }

        public void FillBuffer(byte[] buffer)
        {
            //Recache if not enough remainingCount, discarding remainingCount - too much work to join two blocks
            if (RemainingCount < buffer.Length)
                Update();

            _storage.DeepCopy(PositionOffset,
                buffer, 0, buffer.Length);

            PositionOffset += buffer.Length;
            RemainingCount -= buffer.Length;

            if (ClearUsedValues)
            {
                _clearStorageHandler(
                    PositionOffset - buffer.Length,
                    buffer.Length);
            }
        }

        public byte GetByte()
        {
            //Recache if not enough remainingCount, discarding remainingCount - too much work to join two blocks
            if (RemainingCount < 1)
                Update();

            var value = _storage[PositionOffset];

            ++PositionOffset;
            --RemainingCount;

            if (ClearUsedValues)
                _storage[PositionOffset - 1] = 0;

            return value;
        }

        public void Update()
        {
            _updateStorageHandler?.Invoke(_storage);

            Reset();
        }

        public void Reset()
        {
            PositionOffset = 0;
            RemainingCount = _storage.Length;
        }

        public void Clear()
        {
            _clearStorageHandler(0,
                _storage.Length);
        }
    }
}
