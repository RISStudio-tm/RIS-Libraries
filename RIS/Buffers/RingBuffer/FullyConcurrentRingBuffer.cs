// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RIS.Tasks;

namespace RIS.Buffers
{
    public class FullyConcurrentRingBuffer : IRingBuffer
    {
        protected readonly int Capacity;
        protected byte[] Buffer;
        protected int BufferHeadOffsetDirty, BufferTailOffsetDirty;
        protected int ContentLength, ContentLengthDirty;
        protected SemaphoreSlim OperationBeginLock = new SemaphoreSlim(0, 1);
        protected SemaphoreSlim OperationEndLock = new SemaphoreSlim(0, 1);
        protected int PendingPutSequenceIdentity = 0, PendingTakeSequenceIdentity = 0;
        protected AsyncManualResetEvent PutCompletedEvent = new AsyncManualResetEvent();
        protected AsyncManualResetEvent TakeCompletedEvent = new AsyncManualResetEvent();
        protected int LatestPutSequenceIdentity = 0, LatestTakeSequenceIdentity = 0;
        protected SemaphoreSlim StateController;
        protected SpinLock StateModificationLock = new SpinLock();

        public int CurrentLengthInFlight
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    StateModificationLock.Enter(ref lockTaken);

                    localValue = ContentLengthDirty;
                }
                finally
                {
                    if (lockTaken)
                        StateModificationLock.Exit(false);
                }

                return localValue;
            }
        }
        public int SpareLengthInFlight
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    StateModificationLock.Enter(ref lockTaken);

                    localValue = Capacity - ContentLengthDirty;
                }
                finally
                {
                    if (lockTaken)
                        StateModificationLock.Exit(false);
                }

                return localValue;
            }
        }
        public int CurrentLength
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    StateModificationLock.Enter(ref lockTaken);

                    localValue = ContentLength;
                }
                finally
                {
                    if (lockTaken)
                        StateModificationLock.Exit(false);
                }

                return localValue;
            }
        }
        public int SpareLength
        {
            get
            {
                int localValue;
                bool lockTaken = false;

                try
                {
                    StateModificationLock.Enter(ref lockTaken);

                    localValue = Capacity - ContentLength;
                }
                finally
                {
                    if (lockTaken)
                        StateModificationLock.Exit(false);
                }

                return localValue;
            }
        }
        public int MaximumCapacity
        {
            get
            {
                return Capacity;
            }
        }
        public bool Overwritable
        {
            get
            {
                return false;
            }
        }

        public FullyConcurrentRingBuffer(int maximumCapacity, byte[] buffer = null, int? maxOperations = null)
        {
            Capacity = maximumCapacity;
            Buffer = new byte[Mathematics.Math.NextPowerOfTwo(maximumCapacity)];

            if (buffer != null)
            {
                buffer.CopyBytesNoChecks(0, Buffer, 0, buffer.Length);

                BufferTailOffsetDirty = buffer.Length;
                ContentLength = BufferTailOffsetDirty;
                ContentLengthDirty = BufferTailOffsetDirty;
            }

            StateController = new SemaphoreSlim(0, maxOperations ?? System.Environment.ProcessorCount);
        }

        public void Put(byte input)
        {
            StateController.Wait();

            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                if (ContentLengthDirty + 1 > Capacity)
                {
                    throw new InvalidOperationException("Buffer capacity insufficient for write operation.");
                }

                Buffer[BufferTailOffsetDirty] = input;

                Interlocked.Increment(ref BufferTailOffsetDirty);

                if (BufferTailOffsetDirty == Capacity)
                    Interlocked.Exchange(ref BufferTailOffsetDirty, 0);

                Interlocked.Increment(ref ContentLength);
                Interlocked.Increment(ref ContentLengthDirty);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                StateController.Release();
            }
        }
        public void Put(byte[] buffer)
        {
            Put(buffer, 0, buffer.Length);
        }
        public void Put(byte[] buffer, int offset, int count)
        {
            Put(buffer, offset, count, CancellationToken.None).Wait();
        }

        public int PutFrom(Stream source, int count)
        {
            throw new InvalidOperationException("Indeterminate length operations not supported.");
        }
        public Task<int> PutFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Indeterminate length operations not supported.");
        }

        public void PutExactlyFrom(Stream source, int count)
        {
            PutExactlyFromAsync(source, count, CancellationToken.None).Wait();
        }
        public async Task PutExactlyFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            var putFunc = new Func<int, int, Task>(async (tailOffset, length) =>
            {
                while (length > 0)
                {
                    int chunk = Math.Min(Capacity - tailOffset, length);
                    int chunkIn = 0;

                    while (chunkIn < chunk)
                    {
                        int iterIn = await source.ReadAsync(Buffer, tailOffset, chunk - chunkIn, cancellationToken).ConfigureAwait(false);

                        if (iterIn < 1)
                        {
                            throw new EndOfStreamException();
                        }

                        chunkIn += iterIn;
                    }

                    tailOffset = (tailOffset + chunk == Capacity) ? 0 : tailOffset + chunk;
                    length -= chunk;
                }

                PutPublish(tailOffset, count);
            });

            await Put(count, putFunc, cancellationToken).ConfigureAwait(false);
        }

        public byte Take()
        {
            StateController.Wait();

            byte output;
            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                if (ContentLength < 1)
                {
                    throw new InvalidOperationException("Ringbuffer contents insufficient for take/read operation.");
                }

                output = Buffer[BufferHeadOffsetDirty];

                Interlocked.Increment(ref BufferHeadOffsetDirty);

                if (BufferHeadOffsetDirty == Capacity)
                {
                    BufferHeadOffsetDirty = 0;

                    Interlocked.Exchange(ref BufferHeadOffsetDirty, 0);
                }

                Interlocked.Decrement(ref ContentLength);
                Interlocked.Decrement(ref ContentLengthDirty);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                StateController.Release();
            }

            return output;
        }
        public byte[] Take(int count)
        {
            var buf = new byte[count];

            Take(buf, 0, count);

            return buf;
        }
        public void Take(byte[] buffer)
        {
            Take(buffer, 0, buffer.Length);
        }
        public void Take(byte[] buffer, int offset, int count)
        {
            Take(buffer, offset, count, CancellationToken.None).Wait();
        }

        public void TakeTo(Stream destination, int count)
        {
            TakeToAsync(destination, count, CancellationToken.None).Wait();
        }
        public async Task TakeToAsync(Stream destination, int count, CancellationToken cancellationToken)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            var takeFunc = new Func<int, int, Task>(async (headOffset, length) =>
            {
                while (length > 0)
                {
                    int chunk = Math.Min(Capacity - headOffset, length);

                    await destination.WriteAsync(Buffer, headOffset, chunk, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    headOffset = (headOffset + chunk == Capacity) ? 0 : headOffset + chunk;
                    length -= chunk;
                }
            });

            await Take(count, takeFunc, cancellationToken).ConfigureAwait(false);
        }

        public Task Put(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offset must be positive.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Source array too small for requested input.");
            }

            var putFunc = new Func<int, int, Task>((tailOffset, length) =>
            {
                while (length > 0)
                {
                    int chunk = Math.Min(Capacity - tailOffset, length);

                    buffer.CopyBytesNoChecks(offset, Buffer, tailOffset, chunk);

                    tailOffset = (tailOffset + chunk == Capacity) ? 0 : tailOffset + chunk;
                    offset += chunk;
                    length -= chunk;
                }

                return null;
            });

            return Put(count, putFunc, cancellationToken);
        }
        protected async Task Put(int length, Func<int, int, Task> putFunc, CancellationToken cancellationToken)
        {
            await StateController.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                PutAllocate(length, out var startBufferTailOffset, out var endBufferTailOffset, out var sequenceId);

                await putFunc(startBufferTailOffset, length).ConfigureAwait(false);

                PutCompletedEvent.Set();

                while (PendingPutSequenceIdentity > sequenceId)
                {
                    await PutCompletedEvent.WaitAsync().ConfigureAwait(false);
                }

                await OperationEndLock.WaitAsync(cancellationToken).ConfigureAwait(false);

                Interlocked.Increment(ref PendingPutSequenceIdentity);

                PutPublish(endBufferTailOffset, length);

                PutCompletedEvent.Reset();
            }
            finally
            {
                StateController.Release();
            }
        }

        protected void PutAllocate(int count, out int startTailOffset, out int endTailOffset, out int sequenceId)
        {
            bool resourceConstrained = false;

            if (ContentLengthDirty + count > Capacity)
            {
                resourceConstrained = true;

                OperationBeginLock.Wait();

                while (LatestTakeSequenceIdentity >= PendingTakeSequenceIdentity)
                {
                    TakeCompletedEvent.Wait();
                    OperationEndLock.Wait();
                }

                if (ContentLength + count > Capacity)
                {
                    throw new ArgumentException("Ringbuffer capacity insufficient for put/write operation.", nameof(count));
                }
            }

            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                startTailOffset = BufferTailOffsetDirty;
                endTailOffset = (startTailOffset + count) % Capacity;

                Interlocked.Exchange(ref BufferTailOffsetDirty, endTailOffset);
                Interlocked.Add(ref ContentLengthDirty, count);

                sequenceId = Interlocked.Increment(ref LatestPutSequenceIdentity);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                if (resourceConstrained)
                {
                    OperationBeginLock.Release();
                    OperationEndLock.Release();
                }
            }
        }

        protected void PutPublish(int endTailOffset, int count)
        {
            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                Interlocked.Exchange(ref BufferTailOffsetDirty, endTailOffset);
                Interlocked.Add(ref ContentLength, count);
                //Interlocked.Increment(ref PendingPutSequenceIdentity);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                OperationEndLock.Release();
            }
        }

        public Task Take(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offsets must be positive.");
            }

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Destination array too small for requested output.");
            }

            var takeFunc = new Func<int, int, Task>((headOffset, length) =>
            {
                while (length > 0)
                {
                    int chunk = Math.Min(Capacity - headOffset, length);

                    Buffer.CopyBytesNoChecks(headOffset, buffer, offset, chunk);

                    headOffset = (headOffset + chunk == Capacity) ? 0 : headOffset + chunk;
                    offset += chunk;
                    length -= chunk;
                }

                return null;
            });

            return Take(count, takeFunc, cancellationToken);
        }
        protected async Task Take(int length, Func<int, int, Task> takeFunc, CancellationToken cancellationToken)
        {
            await StateController.WaitAsync(cancellationToken).ConfigureAwait(false);

            try {
                cancellationToken.ThrowIfCancellationRequested();

                TakeDeallocate(length, out var startBufferHeadOffset, out var endBufferHeadOffset, out var sequenceNumber);

                await takeFunc(startBufferHeadOffset, length).ConfigureAwait(false);

                TakeCompletedEvent.Set();

                while (PendingTakeSequenceIdentity > sequenceNumber)
                {
                    await TakeCompletedEvent.WaitAsync().ConfigureAwait(false);
                }

                await OperationEndLock.WaitAsync().ConfigureAwait(false);

                Interlocked.Increment(ref PendingTakeSequenceIdentity);

                TakePublish(endBufferHeadOffset, length);

                TakeCompletedEvent.Reset();
            }
            finally
            {
                StateController.Release();
            }
        }

        protected void TakeDeallocate(int count, out int startHeadOffset, out int endHeadOffset, out int sequenceNumber)
        {
            bool resourceConstrained = false;

            if (count > ContentLength)
            {
                resourceConstrained = true;

                OperationBeginLock.Wait();

                while (LatestPutSequenceIdentity >= PendingPutSequenceIdentity)
                {
                    PutCompletedEvent.Wait();
                    OperationEndLock.Wait();
                }

                if (count > ContentLength)
                {
                    throw new ArgumentException("Ringbuffer contents insufficient for take/read operation.", nameof(count));
                }
            }

            bool lockTaken = false;

            try {
                StateModificationLock.Enter(ref lockTaken);

                startHeadOffset = BufferHeadOffsetDirty;
                endHeadOffset = (startHeadOffset + count) % Capacity;

                Interlocked.Exchange(ref BufferHeadOffsetDirty, endHeadOffset);
                Interlocked.Add(ref ContentLengthDirty, -count);

                sequenceNumber = Interlocked.Increment(ref LatestTakeSequenceIdentity);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                if (resourceConstrained)
                {
                    OperationBeginLock.Release();
                    OperationEndLock.Release();
                }
            }
        }

        protected void TakePublish(int headOffset, int count)
        {
            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                Interlocked.Add(ref ContentLength, -count);
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                OperationEndLock.Release();
            }
        }

        public void Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            try
            {
                OperationBeginLock.Wait();
                OperationEndLock.Wait();

                if (count > ContentLength)
                {
                    throw new ArgumentException("Ringbuffer contents insufficient for operation.", nameof(count));
                }

                bool lockTaken = false;

                try
                {
                    StateModificationLock.Enter(ref lockTaken);

                    Interlocked.Exchange(ref BufferHeadOffsetDirty, (BufferHeadOffsetDirty + count) % Capacity);
                    Interlocked.Add(ref ContentLength, -count);
                }
                finally
                {
                    if (lockTaken)
                        StateModificationLock.Exit(false);
                }
            }
            finally
            {
                OperationBeginLock.Release();
                OperationEndLock.Release();
            }
        }

        public void Reset()
        {
            OperationBeginLock.Wait();
            OperationEndLock.Wait();

            bool lockTaken = false;

            try
            {
                StateModificationLock.Enter(ref lockTaken);

                Array.Clear(Buffer, 0, Buffer.Length);

                BufferHeadOffsetDirty = 0;
                BufferTailOffsetDirty = 0;
                ContentLength = 0;
                ContentLengthDirty = 0;
            }
            finally
            {
                if (lockTaken)
                    StateModificationLock.Exit(false);

                OperationBeginLock.Release();
                OperationEndLock.Release();
            }
        }

        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}
