using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RIS.Synchronization
{
    public class LightAsyncLock : IAwaitable<IDisposable>
    {
        private readonly ConcurrentQueue<WaiterBase> _waiters;
        private object _current;
        private int WaitingCount
        {
            get
            {
                return _waiters.Count;
            }
        }

        public bool HasLock
        {
            get
            {
                return _current != null;
            }
        }

        public LightAsyncLock()
        {
            _waiters = new ConcurrentQueue<WaiterBase>();
        }

        public IAwaiter<IDisposable> GetAwaiter()
        {
            WaiterBase waiter;

            if (TryTakeControl())
            {
                waiter = new NonBlockedWaiter(this);

                RunWaiter(waiter);
            }
            else
            {
                waiter = new AsyncLockWaiter(this);

                _waiters.Enqueue(waiter);
                TryNext();
            }

            return waiter;
        }

        public override string ToString()
        {
            return "AsyncLock: " + (HasLock ? "Locked with " + WaitingCount + " queued waiters" : "Unlocked");
        }

        internal void Done(WaiterBase waiter)
        {
            Interlocked.Exchange(ref _current, null);

            TryNext();
        }

        private void ReleaseControl()
        {
            Interlocked.Exchange(ref _current, null);
        }

        private void RunWaiter(WaiterBase waiter)
        {
            _current = waiter;

            waiter.Ready();
        }

        private void TryNext()
        {
            if (TryTakeControl())
            {
                if (_waiters.TryDequeue(out var waiter))
                {
                    RunWaiter(waiter);
                }
                else
                {
                    ReleaseControl();
                }
            }
        }

        private bool TryTakeControl()
        {
            return Interlocked.CompareExchange(ref _current, Sentinel.Value, null) == null;
        }
    }
}