// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class AsyncLockResult<T> : IAwaiter<T>, IAwaitable<T, AsyncLockResult<T>>
    {
        private readonly CancellationToken _token;
        private readonly CancellationTokenRegistration _tokenRegistration;
        private readonly TaskAwaiter<T> _taskAwaiter;

        private Action _continuation;
        private int _hasCompleted;



        public bool IsCompleted
        {
            get
            {
                return false;
            }
        }



        public AsyncLockResult(
            TaskAwaiter<T> taskAwaiter,
            CancellationToken token)
        {
            _token = token;
            _tokenRegistration = token.Register(
                ExecuteOnce);
            _taskAwaiter = taskAwaiter;
        }



        private void ExecuteOnce()
        {
#pragma warning disable ParallelChecker

            Interlocked
                .Exchange(ref _continuation, null)?
                .Invoke();

#pragma warning restore ParallelChecker
        }



        public AsyncLockResult<T> GetAwaiter()
        {
            return this;
        }

        public T GetResult()
        {
            _tokenRegistration.Dispose();
            _token.ThrowIfCancellationRequested();

            return _taskAwaiter.GetResult();
        }



        public void OnCompleted(
            Action continuation)
        {
            var context = ExecutionContext.Capture();

            if (!ReferenceEquals(context, null))
            {
                void WrappedContinuation()
                {
                    ExecutionContext.Run(
                        context,
                        state => ((Action)state!).Invoke(),
                        continuation
                    );
                }

                UnsafeOnCompleted(WrappedContinuation);
            }
            else
            {
                UnsafeOnCompleted(continuation);
            }
        }

        public void UnsafeOnCompleted(
            Action continuation)
        {
            if (Interlocked.Exchange(ref _hasCompleted, 1) != 0)
            {
                throw new InvalidOperationException(
                    $"This method must only be called once. Simply await {nameof(AsyncLockResult<T>)}.");
            }

#pragma warning disable ParallelChecker

            _continuation = continuation;

#pragma warning restore ParallelChecker

            if (_token.IsCancellationRequested)
                ExecuteOnce();
            else
                _taskAwaiter.OnCompleted(ExecuteOnce);
        }
    }
}