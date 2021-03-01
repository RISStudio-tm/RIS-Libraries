// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RIS.Collections.Queues
{
    public sealed class Deque<T> : ICollection<T>, ICollection
    {
        public struct Enumerator : IEnumerator<T>
        {
            private readonly Deque<T> _deque;
            private readonly int _version;
            private T _current;
            private int _virtualIndex;

            internal Enumerator(Deque<T> deque)
            {
                _deque = deque;
                _version = _deque._version;
                _current = default(T);
                _virtualIndex = -1;
            }

            public bool MoveNext()
            {
                Validate();

                if (_virtualIndex == _deque.Count - 1)
                    return false;

                _virtualIndex++;
                _current = _deque[_virtualIndex];
                return true;
            }

            void IEnumerator.Reset()
            {
                Validate();

                _virtualIndex = -1;
                _current = default(T);
            }

            public T Current
            {
                get
                {
                    return _current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {

            }

            private void Validate()
            {
                if (_version != _deque._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
            }
        }

        public static event EventHandler<RInformationEventArgs> Information;
        public static event EventHandler<RErrorEventArgs> ShowError;

        public T this[int index]
        {
            get
            {
                return _buffer[VirtualIndexToBufferIndex(index)];
            }
            set
            {
                _buffer[VirtualIndexToBufferIndex(index)] = value;
            }
        }

        private const int DefaulTCapacity = 4;
        private int _version;
        private object _syncRoot;
        private T[] _buffer;
        private int _leftIndex;
        private bool LoopsAround
        {
            get
            {
                return Count > (Capacity - LeftIndex);
            }
        }
        private int LeftIndex
        {
            get
            {
                return _leftIndex;
            }
            set
            {
                _leftIndex = CalculateIndex(value);
            }
        }
        private T Left
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }
        private T Right
        {
            get
            {
                return this[Count - 1];
            }
            set
            {
                this[Count - 1] = value;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public int Count { get; private set; }
        public int Capacity
        {
            get
            {
                return _buffer.Length;
            }
            set
            {
                if (value < Count)
                    throw new ArgumentOutOfRangeException(nameof(value), "capacity was less than the current size.");

                if (value == Capacity)
                    return;

                T[] newBuffer = new T[value];

                CopyTo(newBuffer, 0);

                LeftIndex = 0;
                _buffer = newBuffer;
            }
        }

        public Deque()
        {
            _buffer = Array.Empty<T>();
        }
        public Deque(int capacity)
        {
            if(capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "capacity was less than zero.");

            _buffer = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }
        public Deque(IEnumerable<T> collection)
        {
            if(collection == null)
                throw new ArgumentNullException(nameof(collection));

            var collectionLocal = collection as ICollection<T> ?? collection.ToArray();
            var count = collectionLocal.Count;

            if (count == 0)
            {
                _buffer = Array.Empty<T>();
            }
            else
            {
                _buffer = new T[count];
                collectionLocal.CopyTo(_buffer, 0);
                Count = count;
            }
        }

        public void PushRight(T item)
        {
            EnsureCapacity(Count + 1);

            Count ++;

            Right = item;

            _version++;
        }
        public void PushLeft(T item)
        {
            EnsureCapacity(Count + 1);

            LeftIndex --;
            Count++;

            Left = item;

            _version++;
        }

        public T PopRight()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            var right = Right;
            Right = default(T);

            Count--;
            _version++;

            return right;
        }
        public T PopLeft()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            var left = Left;
            Left = default(T);

            LeftIndex++;
            Count--;
            _version++;

            return left;
        }

        public T PeekRight()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            return Right;
        }
        public T PeekLeft()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            return Left;
        }

        void ICollection<T>.Add(T item)
        {
            PushRight(item);
        }
        bool ICollection<T>.Remove(T item)
        {
            var comp = EqualityComparer<T>.Default;
            int virtualIndex = -1;
            int counter = 0;

            foreach (var dequeItem in this)
            {
                if (comp.Equals(item, dequeItem))
                {
                    virtualIndex = counter;
                    break;
                }

                counter++;
            }

            if (virtualIndex == -1)
                return false;

            if (virtualIndex == 0)
            {
                PopLeft();
            }
            else if (virtualIndex == Count - 1)
            {
                PopRight();
            }
            else
            {
                if (virtualIndex < Count / 2)
                {
                    for (int i = virtualIndex - 1; i >= 0; i--)
                        this[i + 1] = this[i];

                    Left = default(T);

                    LeftIndex++;
                    Count--;
                }
                else
                {
                    for (int i = virtualIndex + 1; i < Count; i++)
                        this[i - 1] = this[i];

                    Right = default(T);

                    Count--;
                }

                _version++;
            }

            return true;
        }

        public bool Contains(T item)
        {
            return this.Contains(item, EqualityComparer<T>.Default);
        }

        public void TrimExcess()
        {
            Capacity = Count;
        }

        private void EnsureCapacity(int min)
        {
            if (Capacity < min)
            {
                var newCapacity = Capacity == 0 ? DefaulTCapacity : Capacity*2;
                newCapacity = Math.Max(newCapacity, min);
                Capacity = newCapacity;
            }
        }

        private int CalculateIndex(int position)
        {
            if (Capacity != 0)
                return Mod(position, Capacity);

            Debug.Assert(_leftIndex == 0);

            return 0;
        }

        private int VirtualIndexToBufferIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the collection.");

            return CalculateIndex(LeftIndex + index);
        }

        public void Clear()
        {
            if (LoopsAround)
            {
                Array.Clear(_buffer, LeftIndex, Capacity - LeftIndex);
                Array.Clear(_buffer, 0, LeftIndex + (Count - Capacity));
            }
            else
            {
                Array.Clear(_buffer, LeftIndex, Count);
            }

            Count = 0;
            LeftIndex = 0;
            _version++;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection)this).CopyTo(array, arrayIndex);
        }
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1)
                throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index was less than the array's lower bound.");

            if (arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index was greater than the array's upper bound.");

            if (Count == 0)
                return;

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Destination array was not long enough");

            try
            {
                if (!LoopsAround)
                {
                    Array.Copy(_buffer, LeftIndex, array, arrayIndex, Count);
                }
                else
                {
                    Array.Copy(_buffer, LeftIndex, array, arrayIndex, Capacity - LeftIndex);
                    Array.Copy(_buffer, 0, array, arrayIndex + Capacity - LeftIndex, LeftIndex + (Count - Capacity));
                }
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");
            }
        }

        private static int Mod(int a, int n)
        {
            if (n == 0)
                throw new ArgumentOutOfRangeException("n", "(a mod 0) is undefined.");

            int remainder = a % n;

            if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0))
                return remainder + n;

            return remainder;
        }
    }
}
