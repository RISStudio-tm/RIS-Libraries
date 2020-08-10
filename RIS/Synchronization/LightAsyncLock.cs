// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RIS.Synchronization
{
    public class LightAsyncLock : IAwaitable<IDisposable>
    {
        private sealed class Sentinel
        {
            public static readonly object Value = new Sentinel();

            public override string ToString()
            {
                return GetType().Name;
            }
        }

        private readonly ConcurrentQueue<AwaiterBase> _waiters;
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
            _waiters = new ConcurrentQueue<AwaiterBase>();
        }

        public IAwaiter<IDisposable> GetAwaiter()
        {
            AwaiterBase waiter;

            if (TryTakeControl())
            {
                waiter = new NonBlockedAwaiter(this);

                RunWaiter(waiter);
            }
            else
            {
                waiter = new AsyncLockAwaiter(this);

                _waiters.Enqueue(waiter);
                TryNext();
            }

            return waiter;
        }

        public override string ToString()
        {
            return "AsyncLock: " + (HasLock ? "Locked with " + WaitingCount + " queued waiters" : "Unlocked");
        }

        internal void Done(AwaiterBase waiter)
        {
            Interlocked.Exchange(ref _current, null);

            TryNext();
        }

        private void ReleaseControl()
        {
            Interlocked.Exchange(ref _current, null);
        }

        private void RunWaiter(AwaiterBase waiter)
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