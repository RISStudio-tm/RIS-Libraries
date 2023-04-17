// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class ReAsyncLock
    {
        private readonly SemaphoreSlim _rootSemaphore;
        private readonly AsyncLocal<SemaphoreSlim> _currentSemaphore;



        public ReAsyncLock()
        {
            _rootSemaphore = new SemaphoreSlim(1);
            _currentSemaphore = new AsyncLocal<SemaphoreSlim>();
        }



        public void Lock(
            Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            lockSemaphore.Wait();

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                action();
            }
            finally
            {
                localSemaphore.Wait();

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public void Lock(
            Action action,
            CancellationToken cancellationToken)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!lockSemaphore.Wait(0, CancellationToken.None))
                lockSemaphore.Wait(cancellationToken);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                action();
            }
            finally
            {
                localSemaphore.Wait(CancellationToken.None);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public T Lock<T>(
            Func<T> function)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            lockSemaphore.Wait();

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return function();
            }
            finally
            {
                localSemaphore.Wait();

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public T Lock<T>(
            Func<T> function,
            CancellationToken cancellationToken)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!lockSemaphore.Wait(0, CancellationToken.None))
                lockSemaphore.Wait(cancellationToken);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return function();
            }
            finally
            {
                localSemaphore.Wait(CancellationToken.None);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }



        public async Task LockAsync(
            Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            await lockSemaphore.WaitAsync()
                .ConfigureAwait(false);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                action();
            }
            finally
            {
                await localSemaphore.WaitAsync()
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task LockAsync(
            Action action,
            CancellationToken cancellationToken)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!await lockSemaphore.WaitAsync(0, CancellationToken.None)
                    .ConfigureAwait(false))
            {
                await lockSemaphore.WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                action();
            }
            finally
            {
                await localSemaphore.WaitAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task<T> LockAsync<T>(
            Func<T> function)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            await lockSemaphore.WaitAsync()
                .ConfigureAwait(false);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return function();
            }
            finally
            {
                await localSemaphore.WaitAsync()
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task<T> LockAsync<T>(
            Func<T> function,
            CancellationToken cancellationToken)
        {
            if (function is null)
                throw new ArgumentNullException(nameof(function));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!await lockSemaphore.WaitAsync(0, CancellationToken.None)
                    .ConfigureAwait(false))
            {
                await lockSemaphore.WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return function();
            }
            finally
            {
                await localSemaphore.WaitAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }

        public async Task LockAsync(
            Func<Task> asyncAction)
        {
            if (asyncAction is null)
                throw new ArgumentNullException(nameof(asyncAction));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            await lockSemaphore.WaitAsync()
                .ConfigureAwait(false);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                await asyncAction()
                    .ConfigureAwait(false);
            }
            finally
            {
                await localSemaphore.WaitAsync()
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task LockAsync(
            Func<Task> asyncAction,
            CancellationToken cancellationToken)
        {
            if (asyncAction is null)
                throw new ArgumentNullException(nameof(asyncAction));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!await lockSemaphore.WaitAsync(0, CancellationToken.None)
                    .ConfigureAwait(false))
            {
                await lockSemaphore.WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                await asyncAction()
                    .ConfigureAwait(false);
            }
            finally
            {
                await localSemaphore.WaitAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task<T> LockAsync<T>(
            Func<Task<T>> asyncFunction)
        {
            if (asyncFunction is null)
                throw new ArgumentNullException(nameof(asyncFunction));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            await lockSemaphore.WaitAsync()
                .ConfigureAwait(false);

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return await asyncFunction()
                    .ConfigureAwait(false);
            }
            finally
            {
                await localSemaphore.WaitAsync()
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
        public async Task<T> LockAsync<T>(
            Func<Task<T>> asyncFunction,
            CancellationToken cancellationToken)
        {
            if (asyncFunction is null)
                throw new ArgumentNullException(nameof(asyncFunction));

            var lockSemaphore = _currentSemaphore.Value ?? _rootSemaphore;

            if (!await lockSemaphore.WaitAsync(0, CancellationToken.None)
                    .ConfigureAwait(false))
            {
                await lockSemaphore.WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            using var localSemaphore = new SemaphoreSlim(1);

            _currentSemaphore.Value = localSemaphore;

            try
            {
                return await asyncFunction()
                    .ConfigureAwait(false);
            }
            finally
            {
                await localSemaphore.WaitAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                _currentSemaphore.Value = lockSemaphore;

                lockSemaphore.Release();
            }
        }
    }
}
