using System;
using System.Threading;

namespace RIS.Collections.Concurrent
{
    internal enum ConcurrentOneLinkedListNodeState : byte
    {
        Ins = 0,
        Rem = 1,
        Dat = 2,
        Inv = 3
    }

    internal readonly struct ThreadState<T>
    {
        public readonly int Phase;
        public readonly bool Pending;
        public readonly ConcurrentOneLinkedListNode<T> Node;

        public ThreadState(int phase, bool pending, ConcurrentOneLinkedListNode<T> node)
        {
            Phase = phase;
            Pending = pending;
            Node = node;
        }
    }

    public sealed class ConcurrentOneLinkedListNode<T>
    {
        private int _state;
        private readonly bool _isDummy;

        internal ConcurrentOneLinkedListNode<T> Previous;
        internal int ThreadId;
        internal ConcurrentOneLinkedListNodeState State
        {
            get
            {
                return (ConcurrentOneLinkedListNodeState)_state;
            }
            set
            {
                _state = (int)value;
            }
        }
        public T Value;
        public ConcurrentOneLinkedListNode<T> Next;
        internal bool IsDummy
        {
            get
            {
                return _isDummy;
            }
        }

        internal ConcurrentOneLinkedListNode()
        {
            _isDummy = true;
            Value = default(T);
        }
        internal ConcurrentOneLinkedListNode(T value, ConcurrentOneLinkedListNodeState state, int threadId)
        {
            Value = value;
            ThreadId = threadId;
            _state = (int)state;
            _isDummy = false;
        }

        internal ConcurrentOneLinkedListNodeState AtomicCompareAndExchangeState(ConcurrentOneLinkedListNodeState value, ConcurrentOneLinkedListNodeState compare)
        {
            return (ConcurrentOneLinkedListNodeState)Interlocked.CompareExchange(ref _state, (int)value, (int)compare);
        }
    }
}
