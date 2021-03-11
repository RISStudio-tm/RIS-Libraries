// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RIS.Collections.Chunked;

namespace RIS.Collections.Nestable
{
    public class NestableArrayCAL<T> : INestableArray<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public NestedElement<T> this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
            }
        }
        public NestedElement<T> this[int chunkIndex, uint valueIndex]
        {
            get
            {
                return Get(chunkIndex, valueIndex);
            }
            set
            {
                Set(chunkIndex, valueIndex, value);
            }
        }

        private ChunkedArrayL<NestedElement<T>> ValuesCollection { get; }

        public int Length { get; private set; }
        public NestableCollectionType CollectionType { get; }
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }
        int ICollection.Count
        {
            get
            {
                return Length;
            }
        }

        public NestableArrayCAL()
            : this(0)
        {

        }
        public NestableArrayCAL(int length)
        {
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина коллекции не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            SyncRoot = new object();
            IsSynchronized = false;

            Length = length;
            CollectionType = NestableHelper.GetCollectionType(GetType().Name);

            ValuesCollection = new ChunkedArrayL<NestedElement<T>>(length);
        }

        public void OnInformation(RInformationEventArgs e)
        {
            OnInformation(this, e);
        }
        public void OnInformation(object sender, RInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public void OnWarning(RWarningEventArgs e)
        {
            OnWarning(this, e);
        }
        public void OnWarning(object sender, RWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public void OnError(RErrorEventArgs e)
        {
            OnError(this, e);
        }
        public void OnError(object sender, RErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }

        public NestedElement<T> Get(int index)
        {
            return ValuesCollection.GetRef(index);
        }
        public NestedElement<T> Get(int chunkIndex, uint valueIndex)
        {
            return ValuesCollection.GetRef(chunkIndex, valueIndex);
        }

        public ref NestedElement<T> GetRef(int index)
        {
            return ref ValuesCollection.GetRef(index);
        }
        public ref NestedElement<T> GetRef(int chunkIndex, uint valueIndex)
        {
            return ref ValuesCollection.GetRef(chunkIndex, valueIndex);
        }

        public void Set(int index, NestedElement<T> value)
        {
            ValuesCollection.GetRef(index).Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, NestedElement<T> value)
        {
            ValuesCollection.GetRef(chunkIndex, valueIndex).Set(value);
        }
        public void Set(int index, T value)
        {
            ValuesCollection.GetRef(index).Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, T value)
        {
            ValuesCollection.GetRef(chunkIndex, valueIndex).Set(value);
        }
        public void Set(int index, T[] value)
        {
            ValuesCollection.GetRef(index).Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, T[] value)
        {
            ValuesCollection.GetRef(chunkIndex, valueIndex).Set(value);
        }
        public void Set(int index, INestableCollection<T> value)
        {
            ValuesCollection.GetRef(index).Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, INestableCollection<T> value)
        {
            ValuesCollection.GetRef(chunkIndex, valueIndex).Set(value);
        }

        public string ToStringRepresent()
        {
            return NestableHelper.ToStringRepresent<T>(this);
        }

        public void FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(represent, this);
        }

        public IEnumerable<T> Enumerate()
        {
            return NestableHelper.Enumerate<T>(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable<T> value = NestableHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable<T> value = NestableHelper.Enumerate<T>(this);

            foreach (var element in value)
            {
                yield return element;
            }
        }

        public bool Add(NestedElement<T> value)
        {
            if (ValuesCollection.Add(value))
            {
                ++Length;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Add(T value)
        {
            if (ValuesCollection.Add(new NestedElement<T>(value)))
            {
                ++Length;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Add(T[] value)
        {
            if (ValuesCollection.Add(new NestedElement<T>(value)))
            {
                ++Length;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Add(INestableCollection<T> value)
        {
            if (ValuesCollection.Add(new NestedElement<T>(value)))
            {
                ++Length;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove()
        {
            if (ValuesCollection.Remove())
            {
                --Length;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            Length = 0;
            ValuesCollection.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            if (array != null && array.Rank != 1)
            {
                var exception = new RankException("Копирование в многомерные массивы не поддерживается");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.GetValue(0).GetType() != typeof(T))
            {
                var exception = new ArrayTypeMismatchException("Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.Length - index < thisCollection.Count)
            {
                var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            int arrayIndex = index;
            for (int i = 0; i < thisCollection.Count; ++i)
            {
                array.SetValue(thisCollection[i], arrayIndex);
                ++arrayIndex;
            }

            //int arrayIndex = index;
            //for (int thisChunkIndex = 0; thisChunkIndex < ValuesCollection.ChunksCount - 1; ++thisChunkIndex)
            //{
            //    for (uint thisValueIndex = 0; thisValueIndex < ValuesCollection.ChunkSize; ++thisValueIndex)
            //    {
            //        array.SetValue(this[thisChunkIndex, thisValueIndex], arrayIndex);
            //        ++arrayIndex;
            //    }
            //}
            //for (uint thisValueIndex = 0;
            //    thisValueIndex < Length - ValuesCollection.ChunkSize * (ValuesCollection.ChunksCount - 1);
            //    ++thisValueIndex)
            //{
            //    array.SetValue(this[ValuesCollection.ChunksCount - 1, thisValueIndex], arrayIndex);
            //    ++arrayIndex;
            //}
        }
        public void CopyTo(IList<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            List<T> thisCollection = Enumerate().ToList();

            if (thisCollection.Count < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.OnError(this, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            if (clearBeforeCopy)
                collection.Clear();

            for (int i = 0; i < thisCollection.Count; ++i)
            {
                collection.Add(thisCollection[i]);
            }

            //int collectionIndex = 0;
            //for (int thisChunkIndex = 0; thisChunkIndex < ValuesCollection.ChunksCount - 1; ++thisChunkIndex)
            //{
            //    for (uint thisValueIndex = 0; thisValueIndex < ValuesCollection.ChunkSize; ++thisValueIndex)
            //    {
            //        collection[collectionIndex] = this[thisChunkIndex, thisValueIndex];
            //        ++collectionIndex;
            //    }
            //}
            //for (uint thisValueIndex = 0;
            //    thisValueIndex < Length - ValuesCollection.ChunkSize * (ValuesCollection.ChunksCount - 1);
            //    ++thisValueIndex)
            //{
            //    collection[collectionIndex] = this[ValuesCollection.ChunksCount - 1, thisValueIndex];
            //    ++collectionIndex;
            //}
        }
    }
}
