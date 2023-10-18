// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace RIS.Synchronization
{

#pragma warning disable ConfigureAwaitEnforcer

    public sealed class ContextReAsyncLock
    {
        private static readonly Action<object> CancelTaskCompletionSource;



        private readonly AsyncLocal<object> _asyncLocalScope;
        private readonly object _gate;
        private readonly WorkQueue _queue;

        private TaskCompletionSource<object> _pendingTaskCompletionSource;
        private object _owningScope;
        private ulong _count;



        [DisallowNull]
        private object LocalScope
        {
            get
            {
                return _asyncLocalScope.Value;
            }
            set
            {
                _asyncLocalScope.Value = value;
            }
        }



        public bool IsOnQueue
        {
            get
            {
                return System.Environment.CurrentManagedThreadId == _queue.CurrentThreadId;
            }
        }



        static ContextReAsyncLock()
        {
            CancelTaskCompletionSource = state =>
            {
                var tcs = (TaskCompletionSource<object>)state!;

                tcs.TrySetCanceled();
            };
        }

        public ContextReAsyncLock()
        {
            _asyncLocalScope = new AsyncLocal<object>();
            _gate = new object();
            _queue = new WorkQueue();
        }



        private readonly struct YieldToSynchronizationContextValueTaskSource : IValueTaskSource
        {
            private static readonly SynchronizationContext DefaultContext;



            private readonly SynchronizationContext _context;



            static YieldToSynchronizationContextValueTaskSource()
            {
                DefaultContext = new SynchronizationContext();
            }

            public YieldToSynchronizationContextValueTaskSource(
                SynchronizationContext context)
            {
                _context = context;
            }



            public void GetResult(
                short token)
            {

            }

            public ValueTaskSourceStatus GetStatus(
                short token)
            {
                return ValueTaskSourceStatus.Pending;
            }



            public void OnCompleted(
                Action<object> continuation, object state,
                short token, ValueTaskSourceOnCompletedFlags flags)
            {
                (_context ?? DefaultContext).Post(
                    new SendOrPostCallback(continuation),
                    state);
            }
        }

        private sealed class WorkQueue : SynchronizationContext
        {
            public event EventHandler<Exception> ExceptionOccurred;



            private static readonly Action<object> PumpDelegate;
            private static readonly SendOrPostCallback SetManualResetEventDelegate;

            private static readonly ConcurrentBag<ManualResetEventSlim> UnusedManualResetEvents;



            private readonly Queue<Entry> _queue;
            private readonly object _gate;

            private bool _isPumping;



            public int? CurrentThreadId { get; private set; }



            static WorkQueue()
            {
                PumpDelegate = Pump;
                SetManualResetEventDelegate = SetManualResetEvent;

                UnusedManualResetEvents = new ConcurrentBag<ManualResetEventSlim>();
            }

            public WorkQueue()
            {
                _queue = new Queue<Entry>();
                _gate = new object();
            }



            readonly struct Entry
            {
                public readonly SendOrPostCallback Callback;
                public readonly object State;
                public readonly ExecutionContext ExecutionContext;



                public Entry(
                    SendOrPostCallback callback, object state,
                    ExecutionContext executionContext)
                {
                    Callback = callback;
                    State = state;
                    ExecutionContext = executionContext;
                }
            }



            void Pump()
            {

#pragma warning disable ParallelChecker

                CurrentThreadId = System.Environment.CurrentManagedThreadId;

#pragma warning restore ParallelChecker

                var oldContext = Current;

                while (true)
                {
                    Entry entry;

                    lock (_gate)
                    {
                        if (!_queue.TryDequeue(out entry))
                        {
                            _isPumping = false;

#pragma warning disable ParallelChecker

                            CurrentThreadId = null;

#pragma warning restore ParallelChecker

                            SetSynchronizationContext(
                                oldContext);

                            return;
                        }
                    }

                    try
                    {
                        SetSynchronizationContext(this);

                        if (entry.ExecutionContext is { } executionContext)
                        {
                            ExecutionContext.Run(
                                executionContext,
                                new ContextCallback(entry.Callback),
                                entry.State);
                        }
                        else
                        {
                            entry.Callback(
                                entry.State);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionOccurred?.Invoke(
                            this, ex);
                    }
                }
            }



            public override void Post(
                SendOrPostCallback callback,
                object state)
            {
                var executionContext = ExecutionContext.Capture();

                lock (_gate)
                {
                    _queue.Enqueue(new Entry(
                        callback, state, executionContext));

                    if (_isPumping)
                        return;

                    _isPumping = true;
                }

                ThreadPool.QueueUserWorkItem(PumpDelegate, this, false);
            }

            public override void Send(
                SendOrPostCallback callback,
                object state)
            {
                Post(
                    callback,
                    state);

                if (!UnusedManualResetEvents.TryTake(out var manualResetEvent))
                    manualResetEvent = new ManualResetEventSlim();

                Post(
                    SetManualResetEventDelegate,
                    manualResetEvent);

                manualResetEvent.Wait();
                manualResetEvent.Reset();

                UnusedManualResetEvents.Add(
                    manualResetEvent);
            }



            public override SynchronizationContext CreateCopy()
            {
                return new WorkQueue();
            }



            private static void Pump(
                object state)
            {
                var queue = (WorkQueue)state!;

                queue.Pump();
            }

            private static void SetManualResetEvent(
                object state)
            {
                var manualResetEvent = (ManualResetEventSlim)state!;

                manualResetEvent.Set();
            }
        }



        public ContextReAsyncLockResult<IAsyncDisposable> LockAsync(
            CancellationToken cancellationToken)
        {
            LocalScope ??= new object();

            var previousContext = SynchronizationContext.Current;

            SynchronizationContext.SetSynchronizationContext(
                _queue);

            var task = LockAsyncInternal(
                previousContext, cancellationToken);
            var taskAwaiter = task.GetAwaiter();

            return new ContextReAsyncLockResult<IAsyncDisposable>(
                taskAwaiter, cancellationToken);
        }
        private async Task<IAsyncDisposable> LockAsyncInternal(
            SynchronizationContext previousContext,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                var task = TryLockImmediately();

                if (task is null)
                {
                    return AsyncDisposable.Create(() =>
                    {
                        Unlock();

                        if (SynchronizationContext.Current == _queue)
                        {
                            SynchronizationContext.SetSynchronizationContext(
                                previousContext);
                        }

                        return new ValueTask(
                            new YieldToSynchronizationContextValueTaskSource(
                                previousContext),
                            default);
                    });
                }

                if (cancellationToken.CanBeCanceled)
                {
                    var taskCompletionSource = new TaskCompletionSource<object>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    await using (cancellationToken.Register(
                                     CancelTaskCompletionSource,
                                     taskCompletionSource))
                    {
                        await await Task.WhenAny(
                            task,
                            taskCompletionSource.Task
                        );
                    }
                }
                else
                {
                    await task;
                }
            }
        }



#pragma warning disable RCS1210
        private Task TryLockImmediately()
        {
            lock (_gate)
            {
                if (_count == 0)
                {
                    ++_count;

                    _owningScope = LocalScope;

                    return null;
                }

                if (_owningScope == LocalScope)
                {
                    ++_count;

                    return null;
                }

                _pendingTaskCompletionSource ??= new TaskCompletionSource<object>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                return _pendingTaskCompletionSource.Task;
            }
        }
#pragma warning restore RCS1210

        private void Unlock()
        {
            lock (_gate)
            {
                --_count;

                if (_count != 0)
                    return;

                _owningScope = null;

                if (_pendingTaskCompletionSource is null)
                    return;

                _pendingTaskCompletionSource.TrySetResult(
                    null);

                _pendingTaskCompletionSource = null;
            }
        }
    }

#pragma warning restore ConfigureAwaitEnforcer

}