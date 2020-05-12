using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    internal interface ILockDisposable
    {
        Task InternalUnlockAsync();
    }

    public interface IAsyncLock
    {
        Task<IAsyncDisposable> LockAsync(CancellationToken cancellation = default);

        bool TryLock(out IAsyncDisposable lockDisposer);
    }
}
