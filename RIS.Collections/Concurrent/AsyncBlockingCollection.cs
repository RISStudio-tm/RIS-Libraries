// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Collections.Concurrent
{
    public class AsyncBlockingCollection<T>
    {
        private readonly SemaphoreSlim _fullSemaphoreSlim;
        private readonly SemaphoreSlim _emptySemaphoreSlim;
        private readonly IProducerConsumerCollection<T> _collection;

        public int Count {
            get
            {
                return _fullSemaphoreSlim.CurrentCount;
            }
        }
        public int BoundaryCapacity { get; }

        private sealed class ConsumingEnumerator : IAsyncEnumerator<T>
        {
            private const int STA_FREE = 0;
            private const int STA_WORKING = 1;
            private readonly AsyncBlockingCollection<T> _collection;
            private readonly CancellationTokenSource _disposeToken;
            private int _status = STA_FREE;

            public T Current { get; private set; }

            public ConsumingEnumerator(AsyncBlockingCollection<T> collection, CancellationToken cancellation)
            {
                _collection = collection;
                _disposeToken = cancellation.CanBeCanceled
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellation)
                    : new CancellationTokenSource();
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (_disposeToken.IsCancellationRequested)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                if (Interlocked.CompareExchange(ref _status, STA_WORKING, STA_FREE) != STA_FREE)
                {
                    throw new InvalidOperationException("Don't call MoveNextAsync in parallel");
                }

                try
                {
                    Current = await _collection.TakeAsync(_disposeToken.Token).ConfigureAwait(false);

                    return true;
                }
                finally
                {
                    _status = STA_FREE;
                }
            }

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
            public async ValueTask DisposeAsync()
            {
                _disposeToken.Cancel();

                await Task.CompletedTask.ConfigureAwait(false);
            }
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
        }

        public sealed class ConsumingEnumerable : IAsyncEnumerable<T>
        {
            private readonly AsyncBlockingCollection<T> _collection;

            public ConsumingEnumerable(AsyncBlockingCollection<T> collection)
            {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
            {
                return new ConsumingEnumerator(_collection, cancellationToken);
            }
        }

        public AsyncBlockingCollection() : this(new ConcurrentQueue<T>())
        {

        }
        public AsyncBlockingCollection(int boundary) : this(new ConcurrentQueue<T>(), boundary)
        {

        }
        public AsyncBlockingCollection(IProducerConsumerCollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _fullSemaphoreSlim = new SemaphoreSlim(_collection.Count);
            BoundaryCapacity = int.MaxValue;
        }
        public AsyncBlockingCollection(IProducerConsumerCollection<T> collection, int boundary)
            : this(collection)
        {
            if (collection.Count > boundary || boundary < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(boundary));
            }

            _emptySemaphoreSlim = new SemaphoreSlim(boundary - collection.Count, boundary);
            BoundaryCapacity = boundary;
        }

        public async Task AddAsync(T item, CancellationToken cancellation = default)
        {
            if (_emptySemaphoreSlim != null)
                await _emptySemaphoreSlim.WaitAsync(cancellation).ConfigureAwait(false);

            if (!_collection.TryAdd(item))
            {
                _emptySemaphoreSlim?.Release();

                return;
            }

            _fullSemaphoreSlim.Release();
        }

        public bool TryAdd(T item)
        {
            if (_emptySemaphoreSlim?.Wait(0) == false)
                return false;

            if (!_collection.TryAdd(item))
            {
                _emptySemaphoreSlim?.Release();

                return false;
            }

            _fullSemaphoreSlim.Release();

            return true;
        }

        public async Task<T> TakeAsync(CancellationToken cancellation = default)
        {
            await _fullSemaphoreSlim.WaitAsync(cancellation).ConfigureAwait(false);

            try
            {
                if (!_collection.TryTake(out var item))
                {
                    throw new InvalidDataException();
                }

                return item;
            }
            finally
            {
                _emptySemaphoreSlim?.Release();
            }
        }

        public bool TryTake(out T item)
        {
            item = default;

            if (!_fullSemaphoreSlim.Wait(0))
                return false;

            try
            {
                if (!_collection.TryTake(out item))
                {
                    throw new InvalidDataException();
                }

                return true;
            }
            finally
            {
                _emptySemaphoreSlim?.Release();
            }
        }

        public IAsyncEnumerable<T> GetConsumingEnumerable()
        {
            return new ConsumingEnumerable(this);
        }
    }
}
