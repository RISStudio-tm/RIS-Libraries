// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RIS.Collections.Queues
{
    public sealed class PriorityQueue<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private T[] _heap;
        private int _version;

        public int Count { get; private set; }
        public IComparer<T> Comparer { get; }
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public PriorityQueue(IComparer<T> comparer = null)
        {
            Comparer = comparer ?? Comparer<T>.Default;
            _heap = Array.Empty<T>();
        }
        public PriorityQueue(int initialCapacity, IComparer<T> comparer = null)
            : this(comparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "InitialCapacity must be positive");
            }

            if (initialCapacity > 0)
                _heap = new T[initialCapacity];
        }
        public PriorityQueue(IEnumerable<T> items, IComparer<T> comparer = null)
            : this(comparer)
        {
            Enqueue(items);
        }

        public void Enqueue(T item)
        {
            if (_heap.Length == Count)
                Expand(Count + 1);

            int lastIndex = Count;

            ++Count;

            Swim(lastIndex, item);

            unchecked
            {
                ++_version;
            }
        }
        public void Enqueue(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            int initialCount = Count;

            AppendItems(items);

            if (Count == initialCount)
                return;

            if (Count - initialCount > initialCount)
            {
                Heapify();
            }
            else
            {
                for (int i = initialCount; i < Count; ++i)
                {
                    Swim(i, _heap[i]);
                }
            }

            unchecked
            {
                ++_version;
            }
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty");
            }

            T result = _heap[0];

            --Count;

            if (Count > 0)
            {
                T last = _heap[Count];
                _heap[Count] = default(T);

                Sink(0, last);
            }
            else
            {
                _heap[0] = default(T);
            }

            unchecked
            {
                ++_version;
            }

            return result;
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty");
            }

            return _heap[0];
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public bool Remove(T item)
        {
            int? itemIndex = Find(item);

            if (itemIndex.HasValue)
            {
                int indexToRemove = itemIndex.Value;

                --Count;

                T lastItem = _heap[Count];
                _heap[Count] = default(T);

                if (indexToRemove != Count)
                    Sink(indexToRemove, lastItem);

                unchecked
                {
                    ++_version;
                }

                return true;
            }

            return false;
        }

        public bool Contains(T item)
        {
            return Find(item).HasValue;
        }

        public T DownElement()
        {
            return DownElement(1)[0];
        }
        public T[] DownElement(int count)
        {
            PriorityQueue<T> result = new PriorityQueue<T>();

            foreach (T element in this)
            {
                result.Enqueue(element);

                if (result.Count > count)
                    result.Dequeue();
            }

            return result.OrderByDescending(element => element).ToArray();
        }

        public T TopElement()
        {
            return Peek();
        }
        public T[] TopElement(int count)
        {
            PriorityQueue<T> result = new PriorityQueue<T>();

            foreach (T element in this)
            {
                result.Enqueue(element);

                if (result.Count > count)
                    break;
            }

            return result.OrderBy(element => element).ToArray();
        }

        private void Sink(int index, T item)
        {
            int half = Count >> 1;
            int i = index;

            while (i < half)
            {
                int childIndex = (i << 1) + 1;
                T childItem = _heap[childIndex];
                int righTChildIndex = childIndex + 1;

                if (righTChildIndex < Count && Comparer.Compare(childItem, _heap[righTChildIndex]) > 0)
                    childItem = _heap[childIndex = righTChildIndex];

                if (Comparer.Compare(item, childItem) <= 0)
                    break;

                _heap[i] = childItem;
                i = childIndex;
            }

            _heap[i] = item;
        }

        private void Swim(int index, T item)
        {
            int i = index;

            while (i > 0)
            {
                int parentIndex = (i - 1) >> 1;
                T parentItem = _heap[parentIndex];

                if (Comparer.Compare(item, parentItem) >= 0)
                    break;

                _heap[i] = parentItem;
                i = parentIndex;
            }

            _heap[i] = item;
        }

        private void Heapify()
        {
            for (int i = (Count >> 1) - 1; i >= 0; --i)
            {
                Sink(i, _heap[i]);
            }
        }

        private void AppendItems(IEnumerable<T> items)
        {
            ICollection<T> itemsCollection = items as ICollection<T>;

            if (itemsCollection != null)
            {
                int itemsCollectionCount = itemsCollection.Count;

                if (itemsCollectionCount <= 0)
                    return;

                int newCount = checked(Count + itemsCollectionCount);

                if (newCount > _heap.Length)
                    Expand(newCount);

                itemsCollection.CopyTo(_heap, Count);

                Count = newCount;

                return;
            }

            IReadOnlyCollection<T> readOnlyItemsCollection = items as IReadOnlyCollection<T>;

            if (readOnlyItemsCollection != null)
            {
                int readOnlyItemsCollectionCount = readOnlyItemsCollection.Count;

                if (readOnlyItemsCollectionCount <= 0)
                    return;

                int newCount = checked(Count + readOnlyItemsCollectionCount);

                if (newCount > _heap.Length)
                    Expand(newCount);

                foreach (T item in readOnlyItemsCollection)
                {
                    _heap[Count++] = item;
                }

                return;
            }

            foreach (T item in items)
            {
                if (Count == _heap.Length)
                    Expand(Count + 1);

                _heap[Count++] = item;
            }
        }

        private void Expand(int requiredCapacity)
        {
            const int MaxArrayLength = 0x7FEFFFFF;
            const int InitialCapacity = 16;

            if (requiredCapacity > MaxArrayLength)
            {
                throw new InvalidOperationException("Queue cannot be further expanded");
            }

            int currenTCapacity = _heap.Length;
            int nextNaturalGrowthCapacity = unchecked((int)(
                    currenTCapacity < InitialCapacity
                        ? InitialCapacity
                        : currenTCapacity < 64
                            ? 2 * (currenTCapacity + 1)
                            : 3 * (currenTCapacity / 2.0))
            );

            int newCapacity = nextNaturalGrowthCapacity < 0
                ? MaxArrayLength
                : nextNaturalGrowthCapacity < requiredCapacity
                    ? requiredCapacity
                    : nextNaturalGrowthCapacity;

            Array.Resize(ref _heap, newCapacity);
        }

        private int? Find(T item)
        {
            return Count > 0 ? FindHelper(item, 0) : null;
        }

        private int? FindHelper(T item, int startIndex)
        {
            int cmp = Comparer.Compare(item, _heap[startIndex]);

            if (cmp == 0)
                return startIndex;
            else if (cmp < 0)
                return null;

            int lefTChildIndex = (startIndex << 1) + 1;

            return lefTChildIndex >= Count || lefTChildIndex < 0
                ? null
                : (FindHelper(item, lefTChildIndex) ?? (lefTChildIndex == Count - 1
                       ? null
                       : FindHelper(item, lefTChildIndex + 1)));
        }

        public void Clear()
        {
            const int maxRetainedCapacity = 1024;

            if (_heap.Length > maxRetainedCapacity)
                _heap = Array.Empty<T>();
            else
                Array.Clear(_heap, 0, Count);

            Count = 0;

            unchecked
            {
                ++_version;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int version = _version;

            for (int i = 0; i < Count; ++i)
            {
                if (_version != version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                yield return _heap[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_heap, 0, array, arrayIndex, Count);
        }
    }
}
