using System;
using System.Threading;

namespace RIS.Synchronization
{
    public class SimpleCounter
    {
        private long _value = 0;

        public long Current
        {
            get
            {
                return Interlocked.Read(ref _value);
            }
        }

        public long Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        public long Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }
    }
}