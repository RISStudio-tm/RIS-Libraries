using System;
using System.Globalization;

namespace RIS.Synchronization
{
    internal abstract class AwaiterBase : IAwaiter<IDisposable>, IDisposable
    {
        protected readonly LightAsyncLock Lock;

        public abstract bool IsCompleted { get; }

        protected AwaiterBase(LightAsyncLock @lock)
        {
            Lock = @lock;
        }

        public IDisposable GetResult()
        {
            return this;
        }

        public virtual void Ready()
        {

        }

        public override string ToString()
        {
            return GetHashCode().ToString("x8", CultureInfo.InvariantCulture);
        }

        public virtual void Dispose()
        {
            Lock.Done(this);
        }

        public void OnCompleted(Action continuation)
        {
            OnCompleted(continuation, true);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation, false);
        }

        protected abstract void OnCompleted(Action continuation, bool captureExecutionContext);
    }
}