// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{

#pragma warning disable ConfigureAwaitEnforcer

    public sealed class AsyncLock
    {
        private const long UnlockedId = 0x00;



        private static readonly NullDisposable _nullDisposableValue;
        private static readonly AsyncLocal<long> _asyncId;

        private static long _asyncStackCount;

        private static long AsyncId
        {
            get
            {
                return _asyncId.Value;
            }
        }
        private static int ThreadId
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }



        private readonly SemaphoreSlim _retry;
        private readonly SemaphoreSlim _reentrance;

        private int _reentranceCount;
        private long _owningId;
        private int _owningThreadId;



        static AsyncLock()
        {
            _nullDisposableValue = new NullDisposable();
            _asyncId = new AsyncLocal<long>();

            _asyncStackCount = 0;
        }

        public AsyncLock()
        {
            _retry = new SemaphoreSlim(0, 1);
            _reentrance = new SemaphoreSlim(1, 1);

            _reentranceCount = 0;
            _owningId = UnlockedId;
            _owningThreadId = (int)UnlockedId;
        }



        private sealed class NullDisposable : IDisposable
        {
            public void Dispose()
            {

            }
        }

        private readonly struct InternalLock : IDisposable
        {
            private readonly AsyncLock _parent;
            private readonly long _oldId;
            private readonly int _oldThreadId;



            internal InternalLock(AsyncLock parent,
                long oldId, int oldThreadId)
            {
                _parent = parent;
                _oldId = oldId;
                _oldThreadId = oldThreadId;
            }



            internal async Task<IDisposable> ObtainLockAsync(
                CancellationToken cancellationToken = default)
            {
                while (!await TryEnterAsync(cancellationToken))
                {
                    await _parent._retry.WaitAsync(cancellationToken);
                }

                _parent._owningThreadId = ThreadId;

                _parent._reentrance.Release();

                return this;
            }

            internal async Task<IDisposable?> TryObtainLockAsync(
                CancellationToken cancellationToken)
            {
                try
                {
                    while (!await TryEnterAsync(cancellationToken))
                    {
                        await _parent._retry.WaitAsync(cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    return null;
                }

                _parent._owningThreadId = ThreadId;

                _parent._reentrance.Release();

                return this;
            }
            internal async Task<IDisposable?> TryObtainLockAsync(
                TimeSpan timeout)
            {
                if (timeout == TimeSpan.Zero)
                {
                    if (await TryEnterAsync(timeout))
                        return this;

                    return null;
                }

                var now = DateTimeOffset.UtcNow;
                var last = now;
                var remainder = timeout;

                while (remainder > TimeSpan.Zero)
                {
                    if (await TryEnterAsync(remainder))
                    {
                        _parent._owningThreadId = ThreadId;

                        _parent._reentrance.Release();

                        return this;
                    }

                    now = DateTimeOffset.UtcNow;
                    remainder -= now - last;
                    last = now;

                    if (remainder < TimeSpan.Zero
                        || !await _parent._retry.WaitAsync(remainder))
                    {
                        return null;
                    }

                    now = DateTimeOffset.UtcNow;
                    remainder -= now - last;
                    last = now;
                }

                return null;
            }

            private async Task<bool> TryEnterAsync(
                CancellationToken cancellationToken = default)
            {
                await _parent._reentrance.WaitAsync(cancellationToken);

                return TryEnterInternal();
            }
            private async Task<bool> TryEnterAsync(
                TimeSpan timeout)
            {
                if (!await _parent._reentrance.WaitAsync(timeout))
                    return false;

                return TryEnterInternal();
            }



            internal IDisposable ObtainLock(
                CancellationToken cancellationToken = default)
            {
                while (!TryEnter())
                {
                    _parent._retry.Wait(cancellationToken);
                }

                return this;
            }

            internal IDisposable? TryObtainLock(
                TimeSpan timeout)
            {
                if (timeout == TimeSpan.Zero)
                {
                    if (TryEnter(timeout))
                        return this;

                    return null;
                }

                var now = DateTimeOffset.UtcNow;
                var last = now;
                var remainder = timeout;

                while (remainder > TimeSpan.Zero)
                {
                    if (TryEnter(remainder))
                        return this;

                    now = DateTimeOffset.UtcNow;
                    remainder -= now - last;
                    last = now;

                    if (!_parent._retry.Wait(remainder))
                        return null;

                    now = DateTimeOffset.UtcNow;
                    remainder -= now - last;
                    last = now;
                }

                return null;
            }

            private bool TryEnter()
            {
                _parent._reentrance.Wait();

                return TryEnterInternal(true);
            }
            private bool TryEnter(
                TimeSpan timeout)
            {
                if (!_parent._reentrance.Wait(timeout))
                    return false;

                return TryEnterInternal(true);
            }



            private bool TryEnterInternal(
                bool synchronous = false)
            {
                var result = false;

                try
                {
                    if (synchronous)
                    {
#pragma warning disable ParallelChecker

                        if (_parent._owningThreadId == UnlockedId)
                            _parent._owningThreadId = ThreadId;
                        else if (_parent._owningThreadId != ThreadId)
                            return false;

                        _parent._owningId = AsyncId;

#pragma warning restore ParallelChecker
                    }
                    else
                    {
                        if (_parent._owningId == UnlockedId)
                            _parent._owningId = AsyncId;
                        else if (_parent._owningId != _oldId)
                            return false;
                        else
                            _parent._owningId = AsyncId;
                    }

                    Interlocked.Increment(
                        ref _parent._reentranceCount);

                    result = true;

                    return result;
                }
                finally
                {
                    if (!result || synchronous)
                        _parent._reentrance.Release();
                }
            }



            public void Dispose()
            {
                var @lock = this;
                var oldId = _oldId;
                var oldThreadId = _oldThreadId;

                Task.Run(async () =>
                {
                    await @lock._parent._reentrance.WaitAsync();

                    try
                    {
                        Interlocked.Decrement(
                            ref @lock._parent._reentranceCount);

                        @lock._parent._owningId = oldId;
                        @lock._parent._owningThreadId = oldThreadId;

                        if (@lock._parent._reentranceCount == 0)
                        {
                            @lock._parent._owningId = UnlockedId;
                            @lock._parent._owningThreadId = (int)UnlockedId;

                            if (@lock._parent._retry.CurrentCount == 0)
                                @lock._parent._retry.Release();
                        }
                    }
                    finally
                    {
                        @lock._parent._reentrance.Release();
                    }
                });
            }
        }



        public Task<IDisposable> LockAsync(
            CancellationToken cancellationToken = default)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.ObtainLockAsync(cancellationToken);
        }

        // ReSharper disable ConvertTypeCheckPatternToNullCheck

        public Task<bool> TryLockAsync(
            Action callback,
            CancellationToken cancellationToken = default)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.TryObtainLockAsync(cancellationToken)
                .ContinueWith(task =>
                {
                    if (task.Exception is AggregateException taskEx)
                    {
                        ExceptionDispatchInfo
                            .Capture(taskEx.InnerException!)
                            .Throw();
                    }

                    var result = task.Result;

                    if (result is null)
                        return false;

                    try
                    {
                        callback();
                    }
                    finally
                    {
                        result.Dispose();
                    }

                    return true;
                });
        }
        public Task<bool> TryLockAsync(
            Action callback,
            TimeSpan timeout)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.TryObtainLockAsync(timeout)
                .ContinueWith(task =>
                {
                    if (task.Exception is AggregateException taskEx)
                    {
                        ExceptionDispatchInfo
                            .Capture(taskEx.InnerException!)
                            .Throw();
                    }

                    var result = task.Result;

                    if (result is null)
                        return false;

                    try
                    {
                        callback();
                    }
                    finally
                    {
                        result.Dispose();
                    }

                    return true;
                });
        }
        public Task<bool> TryLockAsync(
            Func<Task> callback,
            CancellationToken cancellationToken = default)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.TryObtainLockAsync(cancellationToken)
                .ContinueWith(task =>
                {
                    if (task.Exception is AggregateException taskEx)
                    {
                        ExceptionDispatchInfo
                            .Capture(taskEx.InnerException!)
                            .Throw();
                    }

                    var result = task.Result;

                    if (result is null)
                        return Task.FromResult(false);

                    return callback()
                        .ContinueWith(taskCallback =>
                        {
                            result.Dispose();

                            if (taskCallback.Exception is AggregateException taskCallbackEx)
                            {
                                ExceptionDispatchInfo
                                    .Capture(taskCallbackEx.InnerException!)
                                    .Throw();
                            }

                            return true;
                        });
                }).Unwrap();
        }
        public Task<bool> TryLockAsync(
            Func<Task> callback,
            TimeSpan timeout)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.TryObtainLockAsync(timeout)
                .ContinueWith(task =>
                {
                    if (task.Exception is AggregateException taskEx)
                    {
                        ExceptionDispatchInfo
                            .Capture(taskEx.InnerException!)
                            .Throw();
                    }

                    var result = task.Result;

                    if (result is null)
                        return Task.FromResult(false);

                    return callback()
                        .ContinueWith(taskCallback =>
                        {
                            result.Dispose();

                            if (taskCallback.Exception is AggregateException taskCallbackEx)
                            {
                                ExceptionDispatchInfo
                                    .Capture(taskCallbackEx.InnerException!)
                                    .Throw();
                            }

                            return true;
                        });
                }).Unwrap();
        }

        // ReSharper restore ConvertTypeCheckPatternToNullCheck

        public Task<IDisposable> TryLockAsync(
            out bool locked)
        {
            return TryLockAsync(
                CancellationToken.None,
                out locked);
        }
        public unsafe Task<IDisposable> TryLockAsync(
            CancellationToken cancellationToken,
            out bool locked)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            locked = false;

            fixed (bool* pointer = &locked)
            {
                var pointerLong = (ulong)pointer;

                return @lock.TryObtainLockAsync(cancellationToken)
                    .ContinueWith(task =>
                    {
                        var result = task.Result;

                        *(bool*)pointerLong = result is not null;

                        return result ?? _nullDisposableValue;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
        public unsafe Task<IDisposable> TryLockAsync(
            TimeSpan timeout,
            out bool locked)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            locked = false;

            fixed (bool* pointer = &locked)
            {
                var pointerLong = (ulong)pointer;

                return @lock.TryObtainLockAsync(timeout)
                    .ContinueWith(task =>
                    {
                        var result = task.Result;

                        *(bool*)pointerLong = result is not null;

                        return result ?? _nullDisposableValue;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }



        public IDisposable Lock(
            CancellationToken cancellationToken = default)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            return @lock.ObtainLock(cancellationToken);
        }

        public bool TryLock(
            Action callback,
            TimeSpan timeout)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            var result = @lock.TryObtainLock(timeout);

            if (result is null)
                return false;

            try
            {
                callback();
            }
            finally
            {
                result.Dispose();
            }

            return true;
        }

        public IDisposable TryLock(
            TimeSpan timeout,
            out bool locked)
        {
            var @lock = new InternalLock(
                this, _asyncId.Value, ThreadId);

            _asyncId.Value = Interlocked.Increment(
                ref _asyncStackCount);

            var result = @lock.TryObtainLock(timeout);

            locked = result is not null;

            return result ?? _nullDisposableValue;
        }
    }

#pragma warning restore ConfigureAwaitEnforcer

}
