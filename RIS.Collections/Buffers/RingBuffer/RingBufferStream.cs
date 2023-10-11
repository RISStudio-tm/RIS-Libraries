// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Collections.Buffers
{
    public class RingBufferStream : Stream
    {
        protected readonly IRingBuffer _ringBuffer;

        public override bool CanRead
        {
            get
            {
                return _ringBuffer.CurrentLength > 0;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return _ringBuffer.CurrentLength > 0;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return _ringBuffer.CurrentLength < _ringBuffer.MaximumCapacity;
            }
        }
        public override long Length
        {
            get
            {
                return _ringBuffer.CurrentLength;
            }
        }
        public int Capacity
        {
            get
            {
                return _ringBuffer.MaximumCapacity;
            }
        }
        public int Spare
        {
            get
            {
                return _ringBuffer.SpareLength;
            }
        }
        public override long Position
        {
            get
            {
                return 0;
            }
            set
            {
                throw new InvalidOperationException("Setting position not supported.");
            }
        }

        protected RingBufferStream(IRingBuffer ringBuffer)
        {
            _ringBuffer = ringBuffer;
        }
        public RingBufferStream(int capacity, bool allowOverwrite)
        {
            _ringBuffer = new ConcurrentRingBuffer(capacity, null, allowOverwrite);
        }

        public static RingBufferStream CreateSequential(int capacity, bool allowOverwrite = false)
        {
            var ringBuffer = new SequentialRingBuffer(capacity, null, allowOverwrite);

            return new RingBufferStream(ringBuffer);
        }
        public static RingBufferStream CreateConcurrent(int capacity, bool allowOverwrite = false)
        {
            var ringBuffer = new ConcurrentRingBuffer(capacity, null, allowOverwrite);

            return new RingBufferStream(ringBuffer);
        }

        public override int ReadByte()
        {
            return _ringBuffer.Take();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, _ringBuffer.CurrentLength);

            _ringBuffer.Take(buffer, offset, count);

            return count;
        }
        public int Read(byte[] buffer, int offset, int count, bool exact)
        {
            if (_ringBuffer.CurrentLength == 0 && exact && count > 0)
            {
                throw new EndOfStreamException();
            }

            if (exact && _ringBuffer.CurrentLength < count)
                count = _ringBuffer.CurrentLength;

            _ringBuffer.Take(buffer, offset, count);

            return count;
        }

        public int ReadTo(Stream destination, int count)
        {
            if (_ringBuffer.CurrentLength == 0 && count > 0)
            {
                throw new EndOfStreamException();
            }

            if (_ringBuffer.CurrentLength < count)
                count = _ringBuffer.CurrentLength;

            _ringBuffer.TakeTo(destination, count);

            return count;
        }
        public Task ReadToAsync(Stream destination, int count)
        {
            if (_ringBuffer.CurrentLength == 0 && count > 0)
            {
                throw new EndOfStreamException();
            }

            return _ringBuffer.TakeToAsync(destination, count, CancellationToken.None);
        }
        public Task ReadToAsync(Stream destination, int count, CancellationToken cancellationToken)
        {
            if (_ringBuffer.CurrentLength == 0 && count > 0)
            {
                throw new EndOfStreamException();
            }

            return _ringBuffer.TakeToAsync(destination, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            _ringBuffer.Put(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _ringBuffer.Put(buffer, offset, count);
        }

        public int WriteFrom(Stream source, int count)
        {
            _ringBuffer.PutFrom(source, count);

            return count;
        }
        public Task WriteFromAsync(Stream source, int count)
        {
            return _ringBuffer.PutFromAsync(source, count, CancellationToken.None);
        }
        public Task WriteFromAsync(Stream source, int count, CancellationToken cancellationToken)
        {
            return _ringBuffer.PutFromAsync(source, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                throw new NotSupportedException("Seek only possible from current stream position (Begin/Current).");
            }

            _ringBuffer.Skip((int) offset);

            return offset;
        }

        public override void SetLength(long value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative.");
            }

            if (value > _ringBuffer.CurrentLength) {
                throw new NotSupportedException("Cannot extend contents of ringbuffer.");
            }

            _ringBuffer.Skip(_ringBuffer.CurrentLength - (int) value);
        }

        public override void Flush()
        {

        }

        protected override void Dispose(bool disposing)
        {
            _ringBuffer.Reset();

            base.Dispose(disposing);
        }
    }
}
