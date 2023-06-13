// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RIS.Tasks
{
    public static class TaskCompletionSourceExtensions
    {
        public static bool TryCompleteFromCompletedTask(this TaskCompletionSource tcs, Task task)
        {
            if (task.IsFaulted)
            {
                return tcs.TrySetException(task.Exception?.InnerExceptions);
            }

            if (task.IsCanceled)
            {
                return tcs.TrySetCanceled();
            }

            return tcs.TrySetResult();
        }
        public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> tcs,
            Task<TSourceResult> task)
            where TSourceResult : TResult
        {
            if (task.IsFaulted)
            {
                return tcs.TrySetException(task.Exception?.InnerExceptions);
            }

            if (task.IsCanceled)
            {
                return tcs.TrySetCanceled();
            }

            return tcs.TrySetResult(task.Result);
        }

        public static bool TryCompleteFromEventArgs(this TaskCompletionSource tcs,
            AsyncCompletedEventArgs eventArgs)
        {
            if (eventArgs.Cancelled)
            {
                return tcs.TrySetCanceled();
            }

            if (eventArgs.Error != null)
            {
                return tcs.TrySetException(eventArgs.Error);
            }

            return tcs.TrySetResult();
        }
        public static bool TryCompleteFromEventArgs<TResult>(this TaskCompletionSource<TResult> tcs,
            AsyncCompletedEventArgs eventArgs, Func<TResult> getResult)
        {
            if (eventArgs.Cancelled)
            {
                return tcs.TrySetCanceled();
            }

            if (eventArgs.Error != null)
            {
                return tcs.TrySetException(eventArgs.Error);
            }

            return tcs.TrySetResult(getResult());
        }

        public static void TrySetResultWithBackgroundContinuations(this TaskCompletionSource tcs)
        {
            Task.Run(tcs.TrySetResult);

            tcs.Task.Wait();
        }
        public static void TrySetResultWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> tcs,
            TResult result)
        {
            Task.Run(() => tcs.TrySetResult(result));

            tcs.Task.Wait();
        }

        public static void TrySetCanceledWithBackgroundContinuations(this TaskCompletionSource tcs)
        {
            Task.Run(() => tcs.TrySetCanceled());

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }
        public static void TrySetCanceledWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> tcs)
        {
            Task.Run(tcs.TrySetCanceled);

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }

        public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource tcs,
            Exception exception)
        {
            Task.Run(() => tcs.TrySetException(exception));

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }
        public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> tcs,
            Exception exception)
        {
            Task.Run(() => tcs.TrySetException(exception));

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }
        public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource tcs,
            IEnumerable<Exception> exceptions)
        {
            Task.Run(() => tcs.TrySetException(exceptions));

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }
        public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> tcs,
            IEnumerable<Exception> exceptions)
        {
            Task.Run(() => tcs.TrySetException(exceptions));

            try
            {
                tcs.Task.Wait();
            }
            catch (AggregateException)
            {

            }
        }
    }
}
