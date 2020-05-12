using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RIS.Collections.ConcurrentCollections
{
    public sealed class ConcurrentDeque<T> : IProducerConsumerCollection<T>
    {
        private enum DequeStatus
        {
            Stable,
            LeftPush,
            RightPush
        };

        private sealed class Anchor
        {
            internal readonly Node _left;
            internal readonly Node _right;
            internal readonly DequeStatus _status;

            internal Anchor()
            {
                _right = _left = null;
                _status = DequeStatus.Stable;
            }
            internal Anchor(Node left, Node right, DequeStatus status)
            {
                _left = left;
                _right = right;
                _status = status;
            }
        }

        private sealed class Node
        {
            internal volatile Node _left;
            internal volatile Node _right;
            internal readonly T _value;

            internal Node(T value)
            {
                _value = value;
            }
        }

        public static event EventHandler<RMessageEventArgs> ShowMessage;
        public static event EventHandler<RErrorEventArgs> ShowError;

        private volatile Anchor _anchor;

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");
            }
        }
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return _anchor._left == null;
            }
        }
        public int Count
        {
            get
            {
                return ToList().Count;
            }
        }

        public ConcurrentDeque()
        {
            _anchor = new Anchor();
        }
        public ConcurrentDeque(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            using (var iterator = collection.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    Node first = new Node(iterator.Current);
                    Node last = first;

                    while (iterator.MoveNext())
                    {
                        Node newLast = new Node(iterator.Current)
                        {
                            _left = last
                        };
                        last._right = newLast;

                        last = newLast;
                    }

                    _anchor = new Anchor(first, last, DequeStatus.Stable);
                }
                else
                {
                    _anchor = new Anchor();
                }
            }
        }

        public void PushRight(T item)
        {
            var newNode = new Node(item);
            var spinner = new SpinWait();

            while (true)
            {
                var anchor = _anchor;

                if (anchor._right == null)
                {
                    var newAnchor = new Anchor(newNode, newNode, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        return;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    newNode._left = anchor._right;
                    var newAnchor = new Anchor(anchor._left, newNode, DequeStatus.RightPush);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                    {
                        StabilizeRight(newAnchor);

                        return;
                    }
                }
                else
                {
                    Stabilize(anchor);
                }

                spinner.SpinOnce();
            }
        }
        public void PushLeft(T item)
        {
            var newNode = new Node(item);
            var spinner = new SpinWait();

            while (true)
            {
                var anchor = _anchor;

                if (anchor._left == null)
                {
                    var newAnchor = new Anchor(newNode, newNode, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        return;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    newNode._right = anchor._left;
                    var newAnchor = new Anchor(newNode, anchor._right, DequeStatus.LeftPush);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                    {
                        StabilizeLeft(newAnchor);

                        return;
                    }
                }
                else
                {
                    Stabilize(anchor);
                }

                spinner.SpinOnce();
            }
        }

        public bool TryPopRight(out T item)
        {
            Anchor anchor;
            var spinner = new SpinWait();

            while (true)
            {
                anchor = _anchor;

                if (anchor._right == null)
                {
                    item = default(T);

                    return false;
                }
                if (anchor._right == anchor._left)
                {
                    var newAnchor = new Anchor();

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    var prev = anchor._right._left;
                    var newAnchor = new Anchor(anchor._left, prev, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else
                {
                    Stabilize(anchor);
                }

                spinner.SpinOnce();
            }

            var node = anchor._right;
            item = node._value;
            var rightmostNode = node._left;

            if (rightmostNode != null)
                Interlocked.CompareExchange(ref rightmostNode._right, null, node);

            return true;
        }
        public bool TryPopLeft(out T item)
        {
            Anchor anchor;
            var spinner = new SpinWait();

            while (true)
            {
                anchor = _anchor;

                if (anchor._left == null)
                {
                    item = default(T);

                    return false;
                }
                if (anchor._right == anchor._left)
                {
                    var newAnchor = new Anchor();

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    var prev = anchor._left._right;
                    var newAnchor = new Anchor(prev, anchor._right, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else
                {
                    Stabilize(anchor);
                }

                spinner.SpinOnce();
            }

            var node = anchor._left;
            item = node._value;
            var leftmostNode = node._right;

            if (leftmostNode != null)
                Interlocked.CompareExchange(ref leftmostNode._left, null, node);

            return true;
        }

        public bool TryPeekRight(out T item)
        {
            var rightmostNode = _anchor._right;

            if (rightmostNode != null)
            {
                item = rightmostNode._value;
                return true;
            }

            item = default(T);

            return false;
        }
        public bool TryPeekLeft(out T item)
        {
            var leftmostNode = _anchor._left;

            if (leftmostNode != null)
            {
                item = leftmostNode._value;
                return true;
            }

            item = default(T);

            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            PushRight(item);

            return true;
        }
        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryPopLeft(out item);
        }

        private void Stabilize(Anchor anchor)
        {
            if (anchor._status == DequeStatus.RightPush)
                StabilizeRight(anchor);
            else
                StabilizeLeft(anchor);
        }
        private void StabilizeRight(Anchor anchor)
        {
            if (_anchor != anchor)
                return;

            var newNode = anchor._right;
            var prev = newNode._left;

            if (prev == null)
                return;

            var prevNext = prev._right;

            if (prevNext != newNode)
            {
                if (_anchor != anchor)
                    return;

                if (Interlocked.CompareExchange(ref prev._right, newNode, prevNext) != prevNext)
                    return;
            }

            var newAnchor = new Anchor(anchor._left, anchor._right, DequeStatus.Stable);

            Interlocked.CompareExchange(ref _anchor, newAnchor, anchor);
        }
        private void StabilizeLeft(Anchor anchor)
        {
            if (_anchor != anchor)
                return;

            var newNode = anchor._left;
            var prev = newNode._right;

            if (prev == null)
                return;

            var prevNext = prev._left;

            if (prevNext != newNode)
            {

                if (_anchor != anchor)
                    return;

                if (Interlocked.CompareExchange(ref prev._left, newNode, prevNext) != prevNext)
                    return;
            }

            var newAnchor = new Anchor(anchor._left, anchor._right, DequeStatus.Stable);

            Interlocked.CompareExchange(ref _anchor, newAnchor, anchor);
        }

        public void Clear()
        {
            _anchor = new Anchor();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ((ICollection) ToList()).CopyTo(array, index);
        }
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ToList().CopyTo(array, index);
        }

        public T[] ToArray()
        {
            return ToList().ToArray();
        }
        private List<T> ToList()
        {
            Anchor anchor = _anchor;

            if (anchor._status != DequeStatus.Stable)
            {
                var spinner = new SpinWait();

                do
                {
                    anchor = _anchor;

                    spinner.SpinOnce();
                } while (anchor._status != DequeStatus.Stable);
            }

            var left = anchor._left;
            var right = anchor._right;

            if(left == null)
                return new List<T>();

            if (left == right)
                return new List<T> {left._value};

            var leftPath = new List<Node>();
            var current = left;

            while (current != null
                   && current != right)
            {
                leftPath.Add(current);

                current = current._right;
            }

            if (current == right)
            {
                leftPath.Add(current);

                return leftPath.Select(node => node._value).ToList();
            }

            current = right;
            var leftPathLast = leftPath.Last();
            var rightPath = new Stack<Node>();

            while (current._left != null
                   && current._left._right != current
                   && current != leftPathLast)
            {
                rightPath.Push(current);

                current = current._left;
            }

            var common = current;

            rightPath.Push(common);

            var leftRightSequence = leftPath
                .TakeWhile(node => node != common)
                .Select(node => node._value)
                .Concat(rightPath.Select(node => node._value));

            return leftRightSequence.ToList();
        }
    }
}
