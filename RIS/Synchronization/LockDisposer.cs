using System;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    internal class LockDisposer : IAsyncDisposable
    {
        private readonly ILockDisposable _locker;
        private readonly OnceFlag _flag = new OnceFlag();

        public LockDisposer(ILockDisposable locker)
        {
            _locker = locker;
        }

        ~LockDisposer()
        {
            DisposeAsync(false).AsTask().Wait();
        }

        private async ValueTask DisposeAsync(bool disposing)
        {
            if (!_flag.TrySet())
                return;

            await _locker.InternalUnlockAsync().ConfigureAwait(false);

            if (disposing)
                GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }
    }
}