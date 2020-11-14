// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class AsyncLock : IDisposable
    {
        private object _reentrancy = new object();
        private int _reentrances = 0;
        internal SemaphoreSlim _retry = new SemaphoreSlim(0, 1);
        private static readonly long UnlockedThreadId = 0;
        internal long _owningId = UnlockedThreadId;
        private static int _globalThreadCounter;
        private static readonly ThreadLocal<int> _threadId;
        //private static readonly AsyncLocal<int> _threadId;
        public static long ThreadId
        {
            get
            {
                return (long) (((ulong) _threadId.Value) << 32) | ((uint) (Task.CurrentId ?? 0));
            }
        }

        static AsyncLock()
        {
            _threadId = new ThreadLocal<int>(() => Interlocked.Increment(ref _globalThreadCounter));
            //_threadId = new AsyncLocal<int>((_) => Interlocked.Increment(ref _globalThreadCounter));
        }

        private readonly struct InternalLock : IDisposable
        {
            private readonly AsyncLock _parent;

            internal InternalLock(AsyncLock parent)
            {
                _parent = parent;
            }

            private bool TryEnter()
            {
                lock (_parent._reentrancy)
                {
                    if (_parent._owningId != UnlockedThreadId && _parent._owningId != ThreadId)
                    {
                        return false;
                    }

                    Interlocked.Increment(ref _parent._reentrances);
                    _parent._owningId = ThreadId;

                    return true;
                }
            }

            internal void ObtainLock()
            {
                while (!TryEnter())
                {
                    _parent._retry.Wait();
                }
            }
            internal async Task ObtainLockAsync(bool continueOnCapturedContext = false)
            {
                while (!TryEnter())
                {
                    await _parent._retry.WaitAsync().ConfigureAwait(continueOnCapturedContext);
                }
            }
            internal async Task ObtainLockAsync(CancellationToken cancellationToken, bool continueOnCapturedContext = false)
            {
                while (!TryEnter())
                {
                    await _parent._retry.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
                }
            }

            public void Dispose()
            {
                lock (_parent._reentrancy)
                {
                    Interlocked.Decrement(ref _parent._reentrances);

                    if (_parent._reentrances == 0)
                    {
                        _parent._owningId = UnlockedThreadId;

                        if (_parent._retry.CurrentCount == 0)
                        {
                            _parent._retry.Release();
                        }
                    }
                }
            }
        }

        public IDisposable Lock()
        {
            var internalLock = new InternalLock(this);
            internalLock.ObtainLock();

            return internalLock;
        }
        public async Task<IDisposable> LockAsync(bool continueOnCapturedContext = false)
        {
            var internalLock = new InternalLock(this);
            await internalLock.ObtainLockAsync().ConfigureAwait(continueOnCapturedContext);

            return internalLock;
        }
        public async Task<IDisposable> LockAsync(CancellationToken cancellationToken, bool continueOnCapturedContext = false)
        {
            var internalLock = new InternalLock(this);
            await internalLock.ObtainLockAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return internalLock;
        }

        public void Dispose()
        {
            _retry?.Dispose();
        }
    }
}
