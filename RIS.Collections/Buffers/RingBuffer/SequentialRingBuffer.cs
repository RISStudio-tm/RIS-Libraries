// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RIS.Extensions;

namespace RIS.Collections.Buffers
{
    public class SequentialRingBuffer : RingBuffer
    {
        public SequentialRingBuffer(int maximumCapacity, byte[] buffer = null, bool allowOverwrite = false)
            : base(maximumCapacity, buffer, allowOverwrite)
        {

        }

        public override void Put(byte input)
        {
            if (ContentLength + 1 > Capacity)
            {
                if (CanOverwrite)
                {
                    Skip(1);
                }
                else
                {
                    throw new InvalidOperationException("Ringbuffer capacity insufficient for put/write operation.");
                }
            }

            Buffer[BufferTailOffset++] = input;

            if (BufferTailOffset == Capacity)
            {
                BufferTailOffset = 0;
            }

            ContentLength++;
        }
        public override void Put(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offset must be positive.");
            }

            PutInitial(count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Source array too small for requested input.");
            }

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferTailOffset, count);

                buffer.CopyBytes(offset, Buffer, BufferTailOffset, chunk);

                BufferTailOffset = (BufferTailOffset + chunk == Capacity) ? 0 : BufferTailOffset + chunk;
                ContentLength += chunk;
                offset += chunk;
                count -= chunk;
            }
        }

        public override int PutFrom(Stream source, int count)
        {
            PutInitial(count);

            int remaining = count;

            while (remaining > 0)
            {
                int chunk = Math.Min(Capacity - BufferTailOffset, remaining);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = source.Read(Buffer, BufferTailOffset, chunk - chunkIn);

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                BufferTailOffset = (BufferTailOffset + chunk == Capacity) ? 0 : BufferTailOffset + chunk;
                ContentLength += chunk;
                remaining -= chunk;
            }

            return count - remaining;
        }
        public override async Task<int> PutFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            PutInitial(count);

            int remaining = count;

            while (remaining > 0)
            {
                int chunk = Math.Min(Capacity - BufferTailOffset, remaining);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = await source.ReadAsync(Buffer, BufferTailOffset, chunk - chunkIn, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        return count - remaining;

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                BufferTailOffset = (BufferTailOffset + chunk == Capacity) ? 0 : BufferTailOffset + chunk;
                ContentLength += chunk;
                remaining -= chunk;
            }

            return count - remaining;
        }

        public override void PutExactlyFrom(Stream source, int count)
        {
            PutInitial(count);

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferTailOffset, count);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = source.Read(Buffer, BufferTailOffset, chunk - chunkIn);

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                BufferTailOffset = (BufferTailOffset + chunk == Capacity) ? 0 : BufferTailOffset + chunk;
                ContentLength += chunk;
                count -= chunk;
            }
        }
        public override async Task PutExactlyFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            PutInitial(count);

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferTailOffset, count);
                int chunkIn = 0;

                while (chunkIn < chunk)
                {
                    int iterIn = await source.ReadAsync(Buffer, BufferTailOffset, chunk - chunkIn, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (iterIn < 1)
                    {
                        throw new EndOfStreamException();
                    }

                    chunkIn += iterIn;
                }

                BufferTailOffset = (BufferTailOffset + chunk == Capacity) ? 0 : BufferTailOffset + chunk;
                ContentLength += chunk;
                count -= chunk;
            }
        }

        private void PutInitial(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            if (ContentLength + count > Capacity)
            {
                if (CanOverwrite)
                {
                    int skip = Capacity - (ContentLength + count);

                    Skip(skip);
                }
                else
                {
                    throw new ArgumentException("Ringbuffer capacity insufficient for put/write operation.", nameof(count));
                }
            }
        }

        public override byte Take()
        {
            if (ContentLength == 0)
            {
                throw new InvalidOperationException("Ringbuffer contents insufficient for read operation.");
            }

            byte output = Buffer[BufferHeadOffset++];

            if (BufferHeadOffset == Capacity)
            {
                BufferHeadOffset = 0;
            }

            ContentLength--;

            return output;
        }
        public override void Take(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Negative offset specified. Offsets must be positive.");
            }

            TakeInitial(count);

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Destination array too small for requested output.");
            }

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferHeadOffset, count);

                Buffer.CopyBytes(BufferHeadOffset, buffer, offset, chunk);

                BufferHeadOffset = (BufferHeadOffset + chunk == Capacity) ? 0 : BufferHeadOffset + chunk;
                ContentLength -= chunk;
                offset += chunk;
                count -= chunk;
            }
        }

        public override void TakeTo(Stream destination, int count)
        {
            TakeInitial(count);

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferHeadOffset, count);

                destination.Write(Buffer, BufferHeadOffset, chunk);

                BufferHeadOffset = (BufferHeadOffset + chunk == Capacity) ? 0 : BufferHeadOffset + chunk;
                ContentLength -= chunk;
                count -= chunk;
            }
        }
        public override async Task TakeToAsync(Stream destination, int count, CancellationToken cancellationToken)
        {
            TakeInitial(count);

            while (count > 0)
            {
                int chunk = Math.Min(Capacity - BufferHeadOffset, count);

                await destination.WriteAsync(Buffer, BufferHeadOffset, chunk, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    return;

                BufferHeadOffset = (BufferHeadOffset + chunk == Capacity) ? 0 : BufferHeadOffset + chunk;
                ContentLength -= chunk;
                count -= chunk;
            }
        }

        private void TakeInitial(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            if (count > ContentLength)
            {
                throw new ArgumentException("Ringbuffer contents insufficient for take/read operation.", nameof(count));
            }
        }
    }
}
