using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class WaitHandleExtensions
    {
        public static Task<bool> WaitOneAsync(this WaitHandle waitHandle, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            if (waitHandle == null)
            {
                throw new ArgumentNullException(nameof(waitHandle));
            }

            if (timeout.HasValue)
                timeout.Value.ThrowIfNotValidTaskTimeout();

            return InternalWaitOneAsync(waitHandle,
                timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infinite, cancellationToken);
        }

        private static async Task<bool> InternalWaitOneAsync(WaitHandle handle, int timeoutMillis,
            CancellationToken cancellationToken)
        {
            RegisteredWaitHandle registeredHandle = null;
            try
            {
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>()
                    .CancelWith(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    registeredHandle = ThreadPool.RegisterWaitForSingleObject(handle,
                        (state, timedOut) => ((TaskCompletionSource<bool>) state)?.TrySetResult(!timedOut),
                        taskCompletionSource, timeoutMillis, true);
                }

                return await taskCompletionSource.Task.ConfigureAwait(false);
            }
            finally
            {
                registeredHandle?.Unregister(null);
            }
        }
    }
}
