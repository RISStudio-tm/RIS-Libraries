// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class TaskExtensions
    {
        private static readonly Task<bool> TrueTask = Task.FromResult(true);

        public static Task<bool> WaitAsync(this Task task)
        {
            return WaitAsync(task, Timeout.InfiniteTimeSpan);
        }
        public static Task<bool> WaitAsync(this Task task, TimeSpan timeout)
        {
            if (task == null)
            {
                var exception = new ArgumentNullException(nameof(task));
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            timeout.ThrowIfNotValidTaskTimeout();

            if (task.IsCompleted)
                return TrueTask;

            return InternalWaitAsync(task, timeout);
        }

        internal static async Task<bool> InternalWaitAsync(Task task, TimeSpan timeout)
        {
            CancellationTokenSource timeoutCanceler = new CancellationTokenSource();
            Task timeoutTask = Task.Delay(timeout, timeoutCanceler.Token);

            Task completed = await Task.WhenAny(task, timeoutTask)
                .ConfigureAwait(false);

            if (completed == task)
            {
                timeoutCanceler.Cancel();

                await completed
                    .ConfigureAwait(false);

                return true;
            }

            return false;
        }

        public static Task SynchronousContinueWith(this Task task, Action<Task, object> continuationFunction, object state,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None, CancellationToken cancellationToken = default)
        {
            return task.ContinueWith(continuationFunction, state, cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously | continuationOptions, TaskScheduler.Default);
        }

        public static Task Then(this Task task, Action continuationAction, TaskScheduler scheduler = null,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None, CancellationToken cancellationToken = default)
        {
            return new TaskCompletionSource<bool>()
                .InternalCompleteWith(task, (_, state) => { ((Action)state)(); return true; },
                    continuationAction, scheduler, continuationOptions, cancellationToken)
                .Task;
        }
        public static Task<TResult> Then<TResult>(this Task task, Func<TResult> continuationFunction, TaskScheduler scheduler = null,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None, CancellationToken cancellationToken = default)
        {
            return new TaskCompletionSource<TResult>()
                .InternalCompleteWith(task, (_, state) => ((Func<TResult>)state)(),
                    continuationFunction, scheduler, continuationOptions, cancellationToken)
                .Task;
        }
        public static Task Then<TResult>(this Task<TResult> task, Action<TResult> continuationAction, TaskScheduler scheduler = null,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None, CancellationToken cancellationToken = default)
        {
            return new TaskCompletionSource<bool>()
                .InternalCompleteWith(task, (t, state) => { ((Action<TResult>)state)(t.Result); return true; },
                    continuationAction, scheduler, continuationOptions, cancellationToken)
                .Task;
        }
        public static Task Then<TTaskResult, TContinuationResult>(this Task<TTaskResult> task,
            Func<TTaskResult, TContinuationResult> continuationFunction)
        {
            return new TaskCompletionSource<TContinuationResult>()
                .InternalCompleteWith(task, (t, state) => ((Func<TTaskResult, TContinuationResult>)state)(t.Result), 
                    continuationFunction)
                .Task;
        }
    }
}
