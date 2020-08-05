using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RIS.Collections.Concurrent
{
    public sealed class ConcurrentOneLinkedList<T>
    {
        private int _counter;
        private readonly ConcurrentOneLinkedListNode<T> _dummy;
        private readonly ConcurrentDictionary<int, ThreadState<T>> _threads;

        internal ConcurrentOneLinkedListNode<T> HeadNode;
        public ConcurrentOneLinkedListNode<T> Head
        {
            get
            {
                return HeadNode;
            }
        }

        public ConcurrentOneLinkedList()
        {
            _counter = 0;
            _dummy = new ConcurrentOneLinkedListNode<T>();
            _threads = new ConcurrentDictionary<int, ThreadState<T>>();
            HeadNode = new ConcurrentOneLinkedListNode<T>(default(T), ConcurrentOneLinkedListNodeState.Rem, -1);
        }

        public bool TryAdd(T value)
        {
            var node = new ConcurrentOneLinkedListNode<T>(value, (int) ConcurrentOneLinkedListNodeState.Ins, Thread.CurrentThread.ManagedThreadId);

            Enlist(node);

            var insertionResult = HelpInsert(node, value);
            var originalValue = node.AtomicCompareAndExchangeState(insertionResult ? ConcurrentOneLinkedListNodeState.Dat : ConcurrentOneLinkedListNodeState.Inv, ConcurrentOneLinkedListNodeState.Ins);

            if (originalValue != ConcurrentOneLinkedListNodeState.Ins)
            {
                HelpRemove(node, value, out _);
                node.State = ConcurrentOneLinkedListNodeState.Inv;
            }

            return insertionResult;
        }

        public bool Remove(T value, out T result)
        {
            var node = new ConcurrentOneLinkedListNode<T>(value, ConcurrentOneLinkedListNodeState.Rem, Thread.CurrentThread.ManagedThreadId);

            Enlist(node);

            var removeResult = HelpRemove(node, value, out result);
            node.State = ConcurrentOneLinkedListNodeState.Inv;

            return removeResult;
        }

        public bool Contains(T value)
        {
            var current = HeadNode;

            while (current != null)
            {
                if (current.Value == null || current.Value.Equals(value))
                {
                    var state = current.State;

                    if (state != ConcurrentOneLinkedListNodeState.Inv)
                    {
                        return state == ConcurrentOneLinkedListNodeState.Ins || state == ConcurrentOneLinkedListNodeState.Dat;
                    }
                }

                current = current.Next;
            }

            return false;
        }

        private static bool HelpInsert(ConcurrentOneLinkedListNode<T> node, T value)
        {
            var previous = node;
            var current = previous.Next;

            while (current != null)
            {
                var state = current.State;

                if (state == ConcurrentOneLinkedListNodeState.Inv)
                {
                    var successor = current.Next;
                    previous.Next = successor;
                    current = successor;
                }
                else if (current.Value != null && !current.Value.Equals(value))
                {
                    previous = current;
                    current = current.Next;
                }
                else if (state == ConcurrentOneLinkedListNodeState.Rem)
                {
                    return true;
                }
                else if (state == ConcurrentOneLinkedListNodeState.Ins || state == ConcurrentOneLinkedListNodeState.Dat)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HelpRemove(ConcurrentOneLinkedListNode<T> node, T value, out T result)
        {
            result = default(T);
            var previous = node;
            var current = previous.Next;

            while (current != null)
            {
                var state = current.State;

                if (state == ConcurrentOneLinkedListNodeState.Inv)
                {
                    var successor = current.Next;
                    previous.Next = successor;
                    current = successor;
                }
                else if (current.Value != null && !current.Value.Equals(value))
                {
                    previous = current;
                    current = current.Next;
                }
                else if (state == ConcurrentOneLinkedListNodeState.Rem)
                {
                    return false;
                }
                else if (state == ConcurrentOneLinkedListNodeState.Ins)
                {
                    var originalValue = current.AtomicCompareAndExchangeState(ConcurrentOneLinkedListNodeState.Rem, ConcurrentOneLinkedListNodeState.Ins);

                    if (originalValue == ConcurrentOneLinkedListNodeState.Ins)
                    {
                        result = current.Value;

                        return true;
                    }
                }
                else if (state == ConcurrentOneLinkedListNodeState.Dat)
                {
                    result = current.Value;
                    current.State = ConcurrentOneLinkedListNodeState.Inv;

                    return true;
                }
            }

            return false;
        }

        private void Enlist(ConcurrentOneLinkedListNode<T> node)
        {
            var phase = Interlocked.Increment(ref _counter);
            var threadState = new ThreadState<T>(phase, true, node);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            _threads.AddOrUpdate(currentThreadId, threadState, (key, value) => threadState);

            foreach (var threadId in _threads.Keys)
            {
                HelpEnlist(threadId, phase);
            }

            HelpFinish();
        }

        private void HelpEnlist(int threadId, int phase)
        {
            while (IsPending(threadId, phase))
            {
                var current = HeadNode;

                if (current.Equals(HeadNode))
                {
                    if (current.Previous == null)
                    {
                        if (IsPending(threadId, phase))
                        {
                            var node = _threads[threadId].Node;
                            var original = Interlocked.CompareExchange(ref current.Previous, node, null);

                            if (original is null)
                            {
                                HelpFinish();
                                return;
                            }
                        }
                    }
                    else
                    {
                        HelpFinish();
                    }
                }
            }
        }

        private void HelpFinish()
        {
            var current = HeadNode;
            var previous = current.Previous;

            if (previous?.IsDummy == false)
            {
                var threadId = previous.ThreadId;
                var threadState = _threads[threadId];

                if (current.Equals(HeadNode) && previous.Equals(threadState.Node))
                {
                    var currentState = _threads[threadId];
                    var updatedState = new ThreadState<T>(threadState.Phase, false, threadState.Node);

                    _threads.TryUpdate(threadId, updatedState, currentState);

                    previous.Next = current;

                    Interlocked.CompareExchange(ref HeadNode, previous, current);

                    current.Previous = _dummy;
                }
            }
        }

        private bool IsPending(int threadId, int phase)
        {
            var threadState = _threads[threadId];
            return threadState.Pending && threadState.Phase <= phase;
        }
    }
}