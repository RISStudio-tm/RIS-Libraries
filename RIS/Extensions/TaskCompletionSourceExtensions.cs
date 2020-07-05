using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class TaskCompletionSourceExtensions
    {
        public static TaskCompletionSource<TResult> CancelWith<TResult>(this TaskCompletionSource<TResult> taskCompletionSource,
            CancellationToken cancellationToken)
        {
            if (taskCompletionSource == null)
            {
                throw new ArgumentNullException(nameof(taskCompletionSource));
            }

            if (cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration tokenRegistration =
                    cancellationToken.Register(state => ((TaskCompletionSource<TResult>) state)?.TrySetCanceled(), taskCompletionSource);

                taskCompletionSource.Task.SynchronousContinueWith(
                    (_, state) => ((CancellationTokenRegistration)state).Dispose(),
                    tokenRegistration, TaskContinuationOptions.None, cancellationToken);
            }

            return taskCompletionSource;
        }

        public static TaskCompletionSource<TResult> TimeoutAfter<TResult>(this TaskCompletionSource<TResult> taskCompletionSource,
            TimeSpan timeout)
        {
            if (taskCompletionSource == null)
            {
                throw new ArgumentNullException(nameof(taskCompletionSource));
            }

            timeout.ThrowIfNotValidTaskTimeout();

            if (timeout != Timeout.InfiniteTimeSpan)
            {
                CancellationTokenSource timeoutCanceler = new CancellationTokenSource();
                Task timeoutTask = Task.Delay(timeout, timeoutCanceler.Token);

                timeoutTask.SynchronousContinueWith((_, state) =>
                    {
                        ((TaskCompletionSource<TResult>) state).TrySetException(
                            new TimeoutException("Timeout of " + timeout + " expired!"));
                    },
                    taskCompletionSource, TaskContinuationOptions.OnlyOnRanToCompletion, timeoutCanceler.Token);

                taskCompletionSource.Task.SynchronousContinueWith(
                    (_, state) => ((CancellationTokenSource)state).Cancel(),
                    timeoutCanceler, TaskContinuationOptions.None ,timeoutCanceler.Token);
            }

            return taskCompletionSource;
        }

        public static TaskCompletionSource<TResult> CompleteWith<TResult>(this TaskCompletionSource<TResult> taskCompletionSource,
            Task<TResult> task)
        {
            return taskCompletionSource.InternalCompleteWith(task, (t, _) => t.Result,
                null, null, TaskContinuationOptions.ExecuteSynchronously);
        }
        public static TaskCompletionSource<TResult> CompleteWith<TResult, TTaskResult>(this TaskCompletionSource<TResult> taskCompletionSource,
            Task<TTaskResult> task, Func<TTaskResult, TResult> resultFactory)
        {
            if (resultFactory == null)
            {
                throw new ArgumentNullException(nameof(resultFactory));
            }

            return taskCompletionSource.InternalCompleteWith(task,
                (t, state) => ((Func<TTaskResult, TResult>)state)(t.Result), resultFactory);
        }
        public static TaskCompletionSource<TResult> CompleteWith<TResult>(this TaskCompletionSource<TResult> taskCompletionSource,
            Task task, Func<TResult> resultFactory = null)
        {
            TaskContinuationOptions continuationOptions = resultFactory == null ? TaskContinuationOptions.ExecuteSynchronously : TaskContinuationOptions.None;

            return taskCompletionSource.InternalCompleteWith(task,
                (_, state) => state != null ? ((Func<TResult>)state)() : default,
                resultFactory, null, continuationOptions);
        }

        internal static TaskCompletionSource<TResult> InternalCompleteWith<TResult, TTask>(this TaskCompletionSource<TResult> taskCompletionSource,
            TTask task, Func<TTask, object, TResult> resultFactory, object resultFactoryState, TaskScheduler scheduler = null,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None, CancellationToken cancellationToken = default)
            where TTask : Task
        {
            if (taskCompletionSource == null)
            {
                throw new ArgumentNullException(nameof(taskCompletionSource));
            }

            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ContinueWith((taskContinuation, state) =>
                {
                    (TaskCompletionSource<TResult>, Func<TTask, object, TResult>, object) tupleState = ((TaskCompletionSource<TResult>, Func<TTask, object, TResult>, object)) state;

                    if (taskContinuation.IsFaulted)
                    {
                        if (taskContinuation.Exception?.InnerExceptions.Count == 1)
                            tupleState.Item1.TrySetException(taskContinuation.Exception.InnerException);
                        else
                            tupleState.Item1.TrySetException(taskContinuation.Exception.InnerExceptions);
                    }
                    else if (taskContinuation.IsCanceled)
                    {
                        tupleState.Item1.TrySetCanceled();
                    }
                    else if (!tupleState.Item1.Task.IsCompleted)
                    {
                        try
                        {
                            tupleState.Item1.TrySetResult(tupleState.Item2((TTask)taskContinuation, tupleState.Item3));
                        }
                        catch (Exception ex)
                        {
                            tupleState.Item1.TrySetException(ex);
                        }
                    }
                },
                (taskCompletionSource, resultFactory, resultFactoryState), cancellationToken,
                continuationOptions, scheduler ?? TaskScheduler.Default);

            return taskCompletionSource;
        }
    }
}
