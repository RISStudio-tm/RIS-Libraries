using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Buffers
{
    public abstract class RingBuffer : IRingBuffer
    {
        protected readonly bool CanOverwrite;
        protected readonly int Capacity;
        protected byte[] Buffer;
        protected int BufferHeadOffset = 0, BufferTailOffset;
        protected int ContentLength;

        public int MaximumCapacity
        {
            get
            {
                return Capacity;
            }
        }
        public virtual int CurrentLength
        {
            get
            {
                return ContentLength;
            }
        }
        public virtual int SpareLength
        {
            get
            {
                return Capacity - ContentLength;
            }
        }
        public bool Overwritable
        {
            get
            {
                return CanOverwrite;
            }
        }

        protected RingBuffer(int maximumCapacity, byte[] buffer = null, bool allowOverwrite = false)
        {
            if (maximumCapacity < 2)
            {
                throw new ArgumentException("Capacity must be at least 2 bytes.");
            }

            if (buffer != null && buffer.Length > maximumCapacity)
            {
                throw new ArgumentException("Initialisation data length exceeds allocated capacity.", nameof(buffer));
            }

            Capacity = maximumCapacity;
            CanOverwrite = allowOverwrite;
            Buffer = new byte[Mathematics.Math.NextPowerOfTwo(maximumCapacity)];

            if (buffer != null)
            {
                buffer.CopyBytesNoChecks(0, Buffer, 0, buffer.Length);

                BufferTailOffset += buffer.Length;
            }
            else
            {
                BufferTailOffset = 0;
            }

            ContentLength = BufferTailOffset;
        }

        public abstract void Put(byte input);
        public void Put(byte[] buffer)
        {
            Put(buffer, 0, buffer.Length);
        }
        public abstract void Put(byte[] buffer, int offset, int count);

        public abstract int PutFrom(Stream source, int count);
        public abstract Task<int> PutFromAsync(Stream source, int count, CancellationToken cancellationToken);

        public abstract void PutExactlyFrom(Stream source, int count);
        public abstract Task PutExactlyFromAsync(Stream source, int count, CancellationToken cancellationToken);

        public abstract byte Take();
        public byte[] Take(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
                return Array.Empty<byte>();

            var output = new byte[count];

            Take(output, 0, count);

            return output;
        }
        public void Take(byte[] buffer)
        {
            Take(buffer, 0, buffer.Length);
        }
        public abstract void Take(byte[] buffer, int offset, int count);

        public abstract void TakeTo(Stream destination, int count);
        public abstract Task TakeToAsync(Stream destination, int count, CancellationToken cancellationToken);

        public virtual void Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Negative count specified. Count must be positive.");
            }

            if (count > ContentLength)
            {
                throw new ArgumentException("Ringbuffer contents insufficient for operation.", nameof(count));
            }

            BufferHeadOffset = (BufferHeadOffset + count) % Capacity;
            ContentLength -= count;
        }

        public virtual void Reset()
        {
            Array.Clear(Buffer, 0, Buffer.Length);

            BufferHeadOffset = 0;
            BufferTailOffset = 0;
            ContentLength = 0;
        }

        public virtual byte[] ToArray()
        {
            return Take(ContentLength);
        }
    }
}
