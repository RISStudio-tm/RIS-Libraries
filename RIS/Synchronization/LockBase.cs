// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public abstract class LockBase : IAsyncLock
    {
        private readonly LinkedList<TaskCompletionSource<LockStatus>> _subscriberList = new LinkedList<TaskCompletionSource<LockStatus>>();

        protected abstract Task EnterLockAsync(CancellationToken cancellation);

        protected abstract bool TryEnterLock();

        protected abstract void ExitLock();

        public async Task<IAsyncDisposable> LockAsync(CancellationToken cancellation = default)
        {
            if (TryLock(out var disposer))
                return disposer;

            return await LockInternal(cancellation).ConfigureAwait(false);
        }
        private async Task<IAsyncDisposable> LockInternal(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            var completion = new TaskCompletionSource<LockStatus>();
            var isFirst = false;
            LinkedListNode<TaskCompletionSource<LockStatus>> node;

            await EnterLockAsync(cancellation).ConfigureAwait(false);

            try
            {
                cancellation.ThrowIfCancellationRequested();
                node = _subscriberList.AddLast(completion);
                if (_subscriberList.Count == 1)
                {
                    isFirst = true;
                }
            }
            finally
            {
                ExitLock();
            }

            if (isFirst)
                completion.SetResult(LockStatus.Activated);

#if NETCOREAPP
            await
#endif
            using (cancellation.Register(() => completion.TrySetResult(LockStatus.Cancelled)))
            {
                if (cancellation.IsCancellationRequested)
                    completion.TrySetResult(LockStatus.Cancelled);

                var status = await completion.Task.ConfigureAwait(false);

                if (status == LockStatus.Activated)
                {
                    return new AsyncOnceDisposer<LockBase>(
                        locker => locker.UnlockAsyncInternal(), this);
                }

                TaskCompletionSource<LockStatus> next = null;

                await EnterLockAsync(CancellationToken.None).ConfigureAwait(false);

                try
                {
                    var activateNext = _subscriberList.First?.Value == completion;

                    _subscriberList.Remove(node);

                    if (activateNext)
                        next = _subscriberList.First?.Value;
                }
                finally
                {
                    ExitLock();
                }

                next?.TrySetResult(LockStatus.Activated);

                throw new OperationCanceledException(cancellation);
            }
        }

        private async Task UnlockAsyncInternal()
        {
            TaskCompletionSource<LockStatus> next = null;

            await EnterLockAsync(CancellationToken.None).ConfigureAwait(false);

            try
            {
                _subscriberList.RemoveFirst();

                if (_subscriberList.Count > 0)
                    next = _subscriberList.First?.Value;
            }
            finally
            {
                ExitLock();
            }

            next?.TrySetResult(LockStatus.Activated);
        }

        public bool TryLock(out IAsyncDisposable lockDisposer)
        {
            return TryLockInternal(out lockDisposer);
        }
        private bool TryLockInternal(out IAsyncDisposable lockDisposer)
        {
            lockDisposer = null;

            if (!TryEnterLock())
                return false;

            try
            {
                if (_subscriberList.Count > 0)
                    return false;

                var completion = new TaskCompletionSource<LockStatus>();

                completion.SetResult(LockStatus.Activated);
                _subscriberList.AddFirst(completion);

                lockDisposer = new AsyncOnceDisposer<LockBase>(
                    locker => locker.UnlockAsyncInternal(), this);
            }
            finally
            {
                ExitLock();
            }

            return true;
        }
    }
}
