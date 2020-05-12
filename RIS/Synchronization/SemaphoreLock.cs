using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class SemaphoreLock : IAsyncLock
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
