using System;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class AsyncOnceDisposer<T> : IAsyncDisposable
    {
        private readonly Func<T, Task> _disposeFunc;
        private readonly T _state;
        private readonly OnceFlag _disposeFlag = new OnceFlag();

        public AsyncOnceDisposer(Func<T, Task> disposeFunc, T state)
        {
            _disposeFunc = disposeFunc ?? throw new ArgumentNullException(nameof(disposeFunc));
            _state = state;
        }

        ~AsyncOnceDisposer()
        {
            DisposeAsync(false).Wait();
        }

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
        }
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
        private async Task DisposeAsync(bool disposing)
        {
            if (!_disposeFlag.TrySet())
                return;

            if (disposing)
                GC.SuppressFinalize(this);

            await _disposeFunc(_state).ConfigureAwait(false);
        }
    }
}