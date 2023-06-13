// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization.Context
{
    public sealed class SingleThreadedSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback callback, object state)> _queue;

        public SingleThreadedSynchronizationContext()
        {
            _queue = new BlockingCollection<(SendOrPostCallback, object)>();
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            _queue.Add((callback, state));
        }

        public static void Await(Func<Task> task)
        {
            var originalContext = Current;

            try
            {
                var context = new SingleThreadedSynchronizationContext();

                SetSynchronizationContext(context);

                var resultTask = task();

                resultTask.ContinueWith(_ =>
                    context._queue.CompleteAdding());

                while (context._queue.TryTake(
                    out var work, Timeout.Infinite))
                {
                    work.callback(work.state);
                }

                resultTask.GetAwaiter()
                    .GetResult();
            }
            finally
            {
                SetSynchronizationContext(originalContext);
            }
        }
    }
}
