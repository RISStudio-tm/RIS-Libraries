using System;
using System.Threading;

namespace RIS.Synchronization
{
    public struct DetachSynchronizationContextAwaiter : IAwaiter
    {
        public bool IsCompleted
        {
            get
            {
                return SynchronizationContext.Current == null;
            }
        }

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(_ => continuation());
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ => continuation(), null);
        }

        public void GetResult()
        {

        }

        public DetachSynchronizationContextAwaiter GetAwaiter()
        {
            return this;
        }
    }
}
