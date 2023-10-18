// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Collections.Concurrent
{
    public sealed class AsyncCollection<T> : IDisposable, IAsyncEnumerable<T>
    {
        private const int COMPLETE_ADDING_ON_MASK = unchecked((int)0x80000000);

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly CancellationTokenSource _doneCancellationToken = new CancellationTokenSource();
        private volatile int _currentAdders = 0;
        private bool _isDisposed = false;

        public bool IsCompleted
        {
            get
            {
                CheckDisposed();
                return IsAddingCompleted && (_semaphore.CurrentCount == 0);
            }
        }
        public bool IsAddingCompleted
        {
            get
            {
                CheckDisposed();
                return _currentAdders == COMPLETE_ADDING_ON_MASK;
            }
        }
        public int Count
        {
            get
            {
                CheckDisposed();
                return _semaphore.CurrentCount;
            }
        }

        ~AsyncCollection()
        {
            Dispose();
        }

        public void Add(T item)
        {
            CheckDisposed();

            SpinWait spinner = new SpinWait();

            while (true)
            {
                int observedAdders = _currentAdders;

                if ((observedAdders & COMPLETE_ADDING_ON_MASK) != 0)
                {
                    spinner.Reset();

                    while (_currentAdders != COMPLETE_ADDING_ON_MASK)
                        spinner.SpinOnce();

                    throw new InvalidOperationException("Adding is not completed");
                }

                if (Interlocked.CompareExchange(ref _currentAdders, observedAdders + 1, observedAdders) == observedAdders)
                {
                    break;
                }

                spinner.SpinOnce();
            }


            _queue.Enqueue(item);
            _semaphore.Release();

            Interlocked.Decrement(ref _currentAdders);
        }

        public void CompleteAdding()
        {
            CheckDisposed();

            if (IsAddingCompleted)
                return;

            SpinWait spinner = new SpinWait();

            while (true)
            {
                int observedAdders = _currentAdders;

                if ((observedAdders & COMPLETE_ADDING_ON_MASK) != 0)
                {
                    spinner.Reset();

                    while (_currentAdders != COMPLETE_ADDING_ON_MASK)
                        spinner.SpinOnce();

                    return;
                }

                if (Interlocked.CompareExchange(ref _currentAdders, observedAdders | COMPLETE_ADDING_ON_MASK, observedAdders) == observedAdders)
                {
                    spinner.Reset();

                    while (_currentAdders != COMPLETE_ADDING_ON_MASK)
                        spinner.SpinOnce();

                    if (_queue.Count == 0)
                        _doneCancellationToken.Cancel();

                    return;
                }

                spinner.SpinOnce();
            }
        }

        private async Task<(bool, T)> TryTakeAsync(CancellationToken cancellationToken, CancellationToken linked)
        {
            if (IsCompleted)
                return (false, default(T));

            if (await _semaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            {
                if (_queue.TryDequeue(out T result))
                    return (true, result);

                throw new InvalidOperationException();
            }

            try
            {
                await _semaphore.WaitAsync(linked).ConfigureAwait(false);

                if (_queue.TryDequeue(out T result))
                    return (true, result);

                throw new InvalidOperationException();
            }
            catch (OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();

                return (false, default(T));
            }
        }

        public bool TryTake(out T result, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            return TryTake(out result, (int)timeout.TotalMilliseconds, cancellationToken);
        }
        public bool TryTake(out T result, int millisecondsTimeout, CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();

            CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(_doneCancellationToken.Token, cancellationToken);

            try
            {
                if (IsCompleted)
                {
                    result = default(T);

                    return false;
                }

                if (_semaphore.Wait(0))
                {
                    if (_queue.TryDequeue(out result))
                        return true;

                    throw new InvalidOperationException();
                }

                try
                {
                    if (_semaphore.Wait(millisecondsTimeout, linked.Token))
                    {
                        if (_queue.TryDequeue(out result))
                            return true;

                        throw new InvalidOperationException();
                    }
                    else
                    {
                        result = default(T);

                        return false;
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    result = default(T);

                    return false;
                }
            }
            finally
            {
                linked.Dispose();
            }
        }

        public async Task<T> TakeAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();

            CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(_doneCancellationToken.Token, cancellationToken);

            try
            {
                var (success, result) = await TryTakeAsync(cancellationToken, linked.Token).ConfigureAwait(false);

                if (success)
                    return result;

                throw new InvalidOperationException("Can't take when done");
            }
            finally
            {
                linked.Dispose();
            }
        }

        public T Take(CancellationToken cancellationToken = new CancellationToken())
        {
            if (TryTake(out T result, Timeout.Infinite, cancellationToken))
                return result;

            throw new InvalidOperationException("Can't take when done");
        }

        public IEnumerable<T> GetConsumingEnumerable()
        {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            CheckDisposed();

            CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(_doneCancellationToken.Token, cancellationToken);

            try
            {
                while (TryTake(out T result, Timeout.Infinite, linked.Token))
                {
                    yield return result;
                }
            }
            finally
            {
                linked.Dispose();
            }
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator([EnumeratorCancellation] CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();

            CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(_doneCancellationToken.Token, cancellationToken);

            try
            {
                while (true)
                {
                    var (success, result) = await TryTakeAsync(cancellationToken, linked.Token).ConfigureAwait(false);

                    if (success)
                        yield return result;
                    else
                        break;
                }
            }
            finally
            {
                linked.Dispose();
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _semaphore.Dispose();
                _doneCancellationToken.Dispose();

                GC.SuppressFinalize(this);

                _isDisposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("AsyncCollection");
            }
        }
    }
}
