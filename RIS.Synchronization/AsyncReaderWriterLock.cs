// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LockWaiterEvent = System.Threading.Tasks.TaskCompletionSource<RIS.Synchronization.LockStatus>;

namespace RIS.Synchronization
{
    public sealed class AsyncReaderWriterLock
    {
        private abstract class StateBase
        {
            protected readonly AsyncReaderWriterLock Owner;

            protected StateBase(AsyncReaderWriterLock owner)
            {
                Owner = owner;
            }

            public abstract LinkedListNode<LockWaiterEvent> WaitForRead();

            public abstract LinkedListNode<LockWaiterEvent> WaitForWrite();

            public virtual bool TryEnterWrite(out LinkedListNode<LockWaiterEvent> node)
            {
                node = null;

                return false;
            }

            public virtual bool TryEnterRead(out LinkedListNode<LockWaiterEvent> node)
            {
                node = null;

                return false;
            }

            public virtual void OnTheFirstWriterDequeue()
            {

            }

            public virtual void OnReadingListBecomeEmpty()
            {

            }
        }

        private sealed class NoneState : StateBase
        {
            public NoneState(AsyncReaderWriterLock owner) : base(owner)
            {
                Contract.Assert(owner._pendingReaderList.Count == 0);
                Contract.Assert(owner._pendingWriterList.Count == 0);
                Contract.Assert(owner._readingList.Count == 0);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForRead()
            {
                var waiter = new LockWaiterEvent();

                waiter.SetResult(LockStatus.Activated);

                var node = Owner._readingList.AddLast(waiter);
                Owner._currentState = new ReadingState(Owner);

                return node;
            }

            public override LinkedListNode<LockWaiterEvent> WaitForWrite()
            {
                var waiter = new LockWaiterEvent();
                var node = Owner._pendingWriterList.AddLast(waiter);

                waiter.SetResult(LockStatus.Activated);

                Owner._currentState = new WritingState(Owner);

                return node;
            }

            public override bool TryEnterRead(out LinkedListNode<LockWaiterEvent> node)
            {
                node = WaitForRead();

                return true;
            }

            public override bool TryEnterWrite(out LinkedListNode<LockWaiterEvent> node)
            {
                node = WaitForWrite();

                return true;
            }

            public override void OnReadingListBecomeEmpty()
            {
                throw new InvalidOperationException("m_readingList should be empty.");
            }

            public override void OnTheFirstWriterDequeue()
            {
                throw new InvalidOperationException("m_pendingWriterList should be empty.");
            }
        }

        private sealed class ReadingState : StateBase
        {
            public ReadingState(AsyncReaderWriterLock owner) : base(owner)
            {
                Contract.Assert(owner._pendingReaderList.Count == 0);
                Contract.Assert(owner._pendingWriterList.Count == 0);
                Contract.Assert(owner._readingList.Count != 0);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForRead()
            {
                var waiter = new LockWaiterEvent();

                waiter.SetResult(LockStatus.Activated);

                return Owner._readingList.AddLast(waiter);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForWrite()
            {
                var waiter = new LockWaiterEvent();
                var node = Owner._pendingWriterList.AddLast(waiter);
                Owner._currentState = new PendingWriteState(Owner);

                return node;
            }

            public override bool TryEnterRead(out LinkedListNode<LockWaiterEvent> node)
            {
                node = WaitForRead();

                return true;
            }

            public override void OnReadingListBecomeEmpty()
            {
                Owner._currentState = new NoneState(Owner);
            }

            public override void OnTheFirstWriterDequeue()
            {
                throw new InvalidOperationException("_pendingWriterList should be empty.");
            }
        }

        private sealed class WritingState : StateBase
        {
            public WritingState(AsyncReaderWriterLock owner) : base(owner)
            {
                Contract.Assert(owner._pendingWriterList.Count != 0);
                Contract.Assert(owner._readingList.Count == 0);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForRead()
            {
                var waiter = new LockWaiterEvent();

                return Owner._pendingReaderList.AddLast(waiter);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForWrite()
            {
                var waiter = new LockWaiterEvent();

                return Owner._pendingWriterList.AddLast(waiter);
            }

            public override void OnTheFirstWriterDequeue()
            {
                if (Owner._pendingWriterList.Count > 0)
                {
                    Owner._pendingWriterList.First?.Value.TrySetResult(LockStatus.Activated);
                }
                else if (Owner._pendingReaderList.Count > 0)
                {
                    UnsafeActivatePendingReaders();
                }
                else
                {
                    Owner._currentState = new NoneState(Owner);
                }
            }

            public override void OnReadingListBecomeEmpty()
            {
                throw new InvalidOperationException("_readingList should be empty.");
            }

            private void UnsafeActivatePendingReaders()
            {
                var tmp = Owner._readingList;

                Owner._readingList = Owner._pendingReaderList;
                Owner._pendingReaderList = tmp;
                Owner._currentState = new ReadingState(Owner);

                foreach (var pendingWaiter in Owner._readingList.ToArray())
                {
                    pendingWaiter.TrySetResult(LockStatus.Activated);
                }
            }
        }

        private sealed class PendingWriteState : StateBase
        {
            public PendingWriteState(AsyncReaderWriterLock owner) : base(owner)
            {
                Contract.Assert(owner._pendingWriterList.Count != 0);
                Contract.Assert(owner._readingList.Count != 0);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForRead()
            {
                var waiter = new LockWaiterEvent();

                return Owner._pendingReaderList.AddLast(waiter);
            }

            public override LinkedListNode<LockWaiterEvent> WaitForWrite()
            {
                var waiter = new LockWaiterEvent();

                return Owner._pendingWriterList.AddLast(waiter);
            }

            public override void OnReadingListBecomeEmpty()
            {
                Owner._currentState = new WritingState(Owner);

                Owner._pendingWriterList.First?.Value.TrySetResult(LockStatus.Activated);
            }

            public override void OnTheFirstWriterDequeue()
            {
                if (Owner._pendingWriterList.Count > 0)
                    return;

                var evtList = new List<LockWaiterEvent>();

                while (Owner._pendingReaderList.Count != 0)
                {
                    var first = Owner._pendingReaderList.First;

                    Owner._pendingReaderList.Remove(first);
                    Owner._readingList.AddLast(first);
                    evtList.Add(first.Value);
                }

                Owner._currentState = new ReadingState(Owner);

                foreach (var evt in evtList)
                {
                    evt.TrySetResult(LockStatus.Activated);
                }
            }
        }

        private LinkedList<LockWaiterEvent> _readingList = new LinkedList<LockWaiterEvent>();
        private LinkedList<LockWaiterEvent> _pendingReaderList = new LinkedList<LockWaiterEvent>();
        private readonly LinkedList<LockWaiterEvent> _pendingWriterList = new LinkedList<LockWaiterEvent>();
        private readonly object _currentStateObjLock;
        private StateBase _currentState;

        public AsyncReaderWriterLock()
        {
            _currentStateObjLock = new object();
            _currentState = new NoneState(this);
        }

        public async Task<IDisposable> WaitForReadAsync(CancellationToken cancellation = default)
        {
            LinkedListNode<LockWaiterEvent> node;

            lock(_currentStateObjLock)
            {
                node = _currentState.WaitForRead();
            }

            var waiter = node.Value;
            var disposer = new OnceDisposer<LinkedListNode<LockWaiterEvent>>(ExitReadLockInternal, node);

            await using (cancellation.Register(() => waiter.TrySetResult(LockStatus.Cancelled)))
            {
                if (cancellation.IsCancellationRequested)
                    waiter.TrySetResult(LockStatus.Cancelled);

                var status = await waiter.Task.ConfigureAwait(false);

                if (status != LockStatus.Cancelled)
                    return disposer;

                disposer.Dispose();

                throw new OperationCanceledException(cancellation);
            }
        }

        public async Task<IDisposable> WaitForWriteAsync(CancellationToken cancellation = default)
        {
            LinkedListNode<LockWaiterEvent> node;

            lock (_currentStateObjLock)
            {
                node = _currentState.WaitForWrite();
            }

            var waiter = node.Value;
            var disposer = new OnceDisposer<LinkedListNode<LockWaiterEvent>>(ExitWriteLockInternal, node);

            await using (cancellation.Register(() => waiter.TrySetResult(LockStatus.Cancelled)))
            {
                if (cancellation.IsCancellationRequested)
                    waiter.TrySetResult(LockStatus.Cancelled);

                var status = await waiter.Task.ConfigureAwait(false);

                if (status != LockStatus.Cancelled)
                    return disposer;

                disposer.Dispose();

                throw new OperationCanceledException(cancellation);
            }
        }

        public bool TryEnterRead(out IDisposable disposer)
        {
            disposer = null;

            lock (_currentStateObjLock)
            {
                if (!_currentState.TryEnterRead(out var node))
                    return false;

                disposer = new OnceDisposer<LinkedListNode<LockWaiterEvent>>(ExitReadLockInternal, node);

                return true;
            }
        }

        public bool TryEnterWrite(out IDisposable disposer)
        {
            disposer = null;

            lock (_currentStateObjLock)
            {
                if (!_currentState.TryEnterWrite(out var node))
                    return false;

                disposer = new OnceDisposer<LinkedListNode<LockWaiterEvent>>(ExitWriteLockInternal, node);

                return true;
            }
        }

        private void ExitReadLockInternal(LinkedListNode<LockWaiterEvent> node)
        {
            lock (_currentStateObjLock)
            {
                var shouldActiveNext = (node.List == _readingList) && (_readingList.Count == 1);

                node.List?.Remove(node);

                if (!shouldActiveNext)
                {
                    return;
                }

                _currentState.OnReadingListBecomeEmpty();
            }
        }

        private void ExitWriteLockInternal(LinkedListNode<LockWaiterEvent> node)
        {
            lock (_currentStateObjLock)
            {
                var shouldActiveNext = _pendingWriterList.First == node;

                _pendingWriterList.Remove(node);

                if (!shouldActiveNext)
                {
                    return;
                }

                _currentState.OnTheFirstWriterDequeue();
            }
        }
    }
}
