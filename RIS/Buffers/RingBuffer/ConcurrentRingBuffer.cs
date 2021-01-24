// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RIS.Extensions;

namespace RIS.Buffers
{
    public class ConcurrentRingBuffer : RingBuffer
    {
        protected SpinLock Lock = new SpinLock();
        protected bool PendingPut = false;
        protected bool PendingTake = false;
        protected int ContentLengthDirty;
        protected int BufferHeadOffsetDirty;
        protected int BufferTailOffsetDirty;

        public override int CurrentLength
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    Lock.Enter(ref lockTaken);

                    localValue = ContentLength;
                }
                finally
                {
                    if (lockTaken)
                        Lock.Exit(false);
                }

                return localValue;
            }
        }
        public override int SpareLength
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    Lock.Enter(ref lockTaken);

                    localValue = Capacity - ContentLength;
                }
                finally
                {
                    if (lockTaken)
                        Lock.Exit(false);
                }

                return localValue;
            }
        }

        public ConcurrentRingBuffer(int maximumCapacity, byte[] buffer = null, bool allowOverwrite = false)
            : base(maximumCapacity, buffer, allowOverwrite)
        {
            BufferTailOffsetDirty = BufferTailOffset;
            ContentLengthDirty = ContentLength;
        }

        public override void Put(byte input)
        {
            bool lockTaken = false;

            try
            {
                Lock.Enter(ref lockTaken);

                if (ContentLength + 1 > Capacity)
                {
                    if (CanOverwrite)
                    {
                        if (BufferHeadOffset + 1 == Capacity)
                        {
                            BufferHeadOffset = 0;
                            ContentLength--;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Buffer capacity insufficient for write operation.");
                    }
                }

                Buffer[BufferTailOffset++] = input;

                if (BufferTailOffset == Capacity)
                    BufferTailOffset = 0;

                ContentLength++;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }
        public override void Put(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offset must be positive.");
            }

            PutAllocate(out var localBufferTailOffset, count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Source array too small for requested input.");
            }

            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, length);

                buffer.CopyBytesNoChecks(offset, Buffer, localBufferTailOffset, chunk);

                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                offset += chunk;
                length -= chunk;
            }

            PutPublish(localBufferTailOffset, count);
        }
        public async Task PutAsync(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offset must be positive.");
            }

            PutAllocate(out var localBufferTailOffset, count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Source array too small for requested input.");
            }

            var ts = new Task[2];
            int tsi = 0;
            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, length);
                int offsetClosure = offset;
                int tailOffsetClosure = localBufferTailOffset;
                ts[tsi] = Task.Run(() => buffer.CopyBytesNoChecks(offsetClosure, Buffer, tailOffsetClosure, chunk));
                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                offset += chunk;
                length -= chunk;
                tsi++;
            }

            await Task.WhenAll(ts).ConfigureAwait(false);

            PutPublish(localBufferTailOffset, count);
        }

        public override int PutFrom(Stream source, int count)
        {
            PutAllocate(out var localBufferTailOffset, count);

            int remaining = count;
            bool earlyFinish = false;

            while (remaining > 0 || !earlyFinish)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, remaining);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = source.Read(Buffer, localBufferTailOffset, chunk - chunkIn);

                    if (iterIn < 1)
                        earlyFinish = true;

                    chunkIn += iterIn;
                }

                if (earlyFinish)
                    continue;

                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                remaining -= chunk;
            }

            PutPublish(localBufferTailOffset, count - remaining);

            return count - remaining;
        }
        public override async Task<int> PutFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            PutAllocate(out var localBufferTailOffset, count);

            int remaining = count;
            bool earlyFinish = false;

            while (remaining > 0 || !earlyFinish)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, remaining);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = await source.ReadAsync(Buffer, localBufferTailOffset, chunk - chunkIn, cancellationToken).ConfigureAwait(false);

                    if (iterIn < 1 || cancellationToken.IsCancellationRequested)
                        earlyFinish = true;

                    chunkIn += iterIn;
                }

                if (earlyFinish)
                    continue;

                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                remaining -= chunk;
            }

            PutPublish(localBufferTailOffset, count - remaining);

            return count - remaining;
        }

        public override void PutExactlyFrom(Stream source, int count)
        {
            PutAllocate(out var localBufferTailOffset, count);

            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, length);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    var iterIn = source.Read(Buffer, localBufferTailOffset, chunk - chunkIn);

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                length -= chunk;
            }

            PutPublish(localBufferTailOffset, count);
        }
        public override async Task PutExactlyFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            PutAllocate(out var localBufferTailOffset, count);

            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferTailOffset, length);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = await source.ReadAsync(Buffer, localBufferTailOffset, chunk - chunkIn, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                localBufferTailOffset = (localBufferTailOffset + chunk == Capacity) ? 0 : localBufferTailOffset + chunk;
                length -= chunk;
            }

            PutPublish(localBufferTailOffset, count);
        }

        protected void PutAllocate(out int tailOffset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            bool lockTaken = false;

            try {
                Lock.Enter(ref lockTaken);

                if (PendingPut)
                {
                    throw new InvalidOperationException();
                }

                tailOffset = BufferTailOffset;

                if (ContentLength + count > Capacity)
                {
                    if (CanOverwrite && !PendingTake)
                    {
                        int skip = Capacity - (ContentLength + count);

                        SkipLocal(ref BufferHeadOffsetDirty, skip);
                    }
                    else
                    {
                        throw new ArgumentException("Ringbuffer capacity insufficient for put/write operation.", nameof(count));
                    }
                }

                BufferTailOffsetDirty = (tailOffset + count) % Capacity;
                ContentLengthDirty = ContentLength + count;
                PendingPut = true;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        protected void PutPublish(int tailOffset, int count)
        {
            bool lockTaken = false;

            try
            {
                Lock.Enter(ref lockTaken);

                BufferTailOffset = tailOffset;
                ContentLength += count;
                PendingPut = false;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        public override byte Take()
        {
            byte output;
            bool lockTaken = false;

            try {
                Lock.Enter(ref lockTaken);

                if (ContentLength == 0)
                {
                    throw new InvalidOperationException("Ringbuffer contents insufficient for take/read operation.");
                }

                output = Buffer[BufferHeadOffset++];

                if (BufferHeadOffset == Capacity)
                    BufferHeadOffset = 0;

                ContentLength--;
            }
            finally {
                if (lockTaken)
                    Lock.Exit(false);
            }

            return output;
        }
        public override void Take(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offsets must be positive.");
            }

            TakeInitial(out var localBufferHeadOffset, count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Destination array too small for requested output.");
            }

            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferHeadOffset, length);

                Buffer.CopyBytesNoChecks(localBufferHeadOffset, buffer, offset, chunk);

                localBufferHeadOffset = (localBufferHeadOffset + chunk == Capacity) ? 0 : localBufferHeadOffset + chunk;
                offset += chunk;
                length -= chunk;
            }

            TakePublish(localBufferHeadOffset, count);
        }
        public async Task TakeAsync(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offsets must be positive.");
            }

            TakeInitial(out var localBufferHeadOffset, count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Destination array too small for requested output.");
            }

            var ts = new Task[2];
            int tsi = 0;
            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferHeadOffset, length);
                int offsetClosure = offset;
                int headOffsetClosure = localBufferHeadOffset;
                ts[tsi] = Task.Run(() => Buffer.CopyBytesNoChecks(headOffsetClosure, buffer, offsetClosure, chunk));
                localBufferHeadOffset = (localBufferHeadOffset + chunk == Capacity) ? 0 : localBufferHeadOffset + chunk;
                offset += chunk;
                length -= chunk;
                tsi++;
            }

            await Task.WhenAll(ts).ConfigureAwait(false);

            TakePublish(localBufferHeadOffset, count);
        }

        public override void TakeTo(Stream destination, int count)
        {
            TakeInitial(out var localBufferHeadOffset, count);

            int length = count;

            while (length > 0)
            {
                int chunk = Math.Min(Capacity - localBufferHeadOffset, length);

                destination.Write(Buffer, localBufferHeadOffset, chunk);

                localBufferHeadOffset = (localBufferHeadOffset + chunk == Capacity) ? 0 : localBufferHeadOffset + chunk;
                length -= chunk;
            }

            TakePublish(localBufferHeadOffset, count);
        }
        public override async Task TakeToAsync(Stream destination, int count, CancellationToken cancellationToken)
        {
            TakeInitial(out var localBufferHeadOffset, count);

            int length = count;

            while (length > 0) {
                int chunk = Math.Min(Capacity - localBufferHeadOffset, length);

                await destination.WriteAsync(Buffer, localBufferHeadOffset, chunk, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return;

                localBufferHeadOffset = (localBufferHeadOffset + chunk == Capacity) ? 0 : localBufferHeadOffset + chunk;
                length -= chunk;
            }

            TakePublish(localBufferHeadOffset, count);
        }

        protected void TakeInitial(out int headOffset, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            bool lockTaken = false;

            try {
                Lock.Enter(ref lockTaken);

                if (PendingTake)
                {
                    throw new InvalidOperationException();
                }

                headOffset = BufferHeadOffset;

                if (count > ContentLength)
                {
                    throw new ArgumentException("Ringbuffer contents insufficient for take/read operation.", nameof(count));
                }

                BufferHeadOffsetDirty = (headOffset + count) % Capacity;
                ContentLengthDirty = ContentLength - count;
                PendingTake = true;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        protected void TakePublish(int headOffset, int count)
        {
            bool lockTaken = false;

            try
            {
                Lock.Enter(ref lockTaken);

                BufferHeadOffset = headOffset;
                ContentLength -= count;
                PendingTake = false;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        public override void Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            bool lockTaken = false;

            try
            {
                if (PendingTake)
                {
                    throw new InvalidOperationException("Ringbuffer is already executing a take operation - cannot do skip concurrently.");
                }

                if (count > ContentLength)
                {
                    throw new ArgumentException("Ringbuffer contents insufficient for operation.", nameof(count));
                }

                Lock.Enter(ref lockTaken);
                SkipLocal(ref BufferHeadOffset, count);

                ContentLength -= count;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        protected void SkipLocal(ref int headOffset, int count)
        {
            headOffset = (headOffset + count) % Capacity;
        }

        public override void Reset()
        {
            bool lockTaken = false;

            try
            {
                Lock.Enter(ref lockTaken);
                Array.Clear(Buffer, 0, Buffer.Length);

                BufferHeadOffset = 0;
                BufferTailOffset = 0;
                ContentLength = 0;
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }
        }

        public override byte[] ToArray()
        {
            byte[] buffer;
            bool lockTaken = false;

            try {
                Lock.Enter(ref lockTaken);

                buffer = Take(ContentLength);

                Reset();
            }
            finally
            {
                if (lockTaken)
                    Lock.Exit(false);
            }

            return buffer;
        }
    }
}
