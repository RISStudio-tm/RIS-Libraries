// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class SemaphoreLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        public async Task<IAsyncDisposable> LockAsync(CancellationToken cancellation = default)
        {
            await _semaphore.WaitAsync(cancellation).ConfigureAwait(false);

            return new AsyncOnceDisposer<SemaphoreLock>(
                locker => locker.UnlockAsyncInternal(),
                this);
        }

        public bool TryLock(out IAsyncDisposable lockDisposer)
        {
            lockDisposer = null;

            if (!_semaphore.Wait(0))
                return false;

            lockDisposer = new AsyncOnceDisposer<SemaphoreLock>(
                locker => locker.UnlockAsyncInternal(),
                this);

            return true;
        }

        private void UnlockInternal()
        {
            _semaphore.Release();
        }

        private Task UnlockAsyncInternal()
        {
            UnlockInternal();

            return Task.CompletedTask;
        }
    }
}
