// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RIS.Collections.Chunked;

namespace RIS.Collections.Nestable
{
    public class NestableArrayCAL<T> : INestableArray<T>
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



        // ReSharper disable once StaticMemberInGenericType
        private static readonly NestableCollectionType CollectionTypeStatic;



        private ChunkedArrayL<NestedElement<T>> ValuesCollection { get; }

        public NestableCollectionType CollectionType
        {
            get
            {
                return CollectionTypeStatic;
            }
        }
        public int Length { get; private set; }
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }
        int ICollection.Count
        {
            get
            {
                return Length;
            }
        }



        static NestableArrayCAL()
        {
            CollectionTypeStatic = NestableCollectionType.NestableArrayCAL;
        }



        public NestableArrayCAL()
            : this(0)
        {

        }
        public NestableArrayCAL(string represent)
            : this(0)
        {
            if (string.IsNullOrEmpty(represent))
                return;

            FromStringRepresent(represent);
        }
        public NestableArrayCAL(int length)
        {
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(length),
                    "Длина коллекции не может быть меньше 0");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            SyncRoot = new object();
            IsSynchronized = false;

            Length = length;

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
            return ValuesCollection
                .GetRef(index);
        }
        public NestedElement<T> Get(int chunkIndex, uint valueIndex)
        {
            return ValuesCollection
                .GetRef(chunkIndex, valueIndex);
        }

        public ref NestedElement<T> GetRef(int index)
        {
            return ref ValuesCollection
                .GetRef(index);
        }
        public ref NestedElement<T> GetRef(int chunkIndex, uint valueIndex)
        {
            return ref ValuesCollection
                .GetRef(chunkIndex, valueIndex);
        }

        public void Set(int index, NestedElement<T> value)
        {
            ValuesCollection
                .GetRef(index)
                .Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, NestedElement<T> value)
        {
            ValuesCollection
                .GetRef(chunkIndex, valueIndex)
                .Set(value);
        }
        public void Set(int index, T value)
        {
            ValuesCollection
                .GetRef(index)
                .Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, T value)
        {
            ValuesCollection
                .GetRef(chunkIndex, valueIndex)
                .Set(value);
        }
        public void Set(int index, T[] value)
        {
            ValuesCollection
                .GetRef(index)
                .Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, T[] value)
        {
            ValuesCollection
                .GetRef(chunkIndex, valueIndex)
                .Set(value);
        }
        public void Set(int index, INestableCollection<T> value)
        {
            ValuesCollection
                .GetRef(index)
                .Set(value);
        }
        public void Set(int chunkIndex, uint valueIndex, INestableCollection<T> value)
        {
            ValuesCollection
                .GetRef(chunkIndex, valueIndex)
                .Set(value);
        }

        public string ToStringRepresent()
        {
            return NestableHelper.ToStringRepresent<T>(
                this);
        }

        INestableCollection INestableCollection.FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(
                represent, this);

            return this;
        }
        INestableCollection<T> INestableCollection<T>.FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(
                represent, this);

            return this;
        }

        INestableArray INestableArray.FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(
                represent, this);

            return this;
        }
        INestableArray<T> INestableArray<T>.FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(
                represent, this);

            return this;
        }

        public NestableArrayCAL<T> FromStringRepresent(string represent)
        {
            NestableHelper.FromStringRepresent<T>(
                represent, this);

            return this;
        }

        public IEnumerable<T> Enumerate()
        {
            return NestableHelper.Enumerate<T>(
                this);
        }

        IEnumerator<NestedElement<T>> IEnumerable<NestedElement<T>>.GetEnumerator()
        {
            return ValuesCollection
                .GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ValuesCollection
                .GetEnumerator();
        }

        public bool Add(NestedElement<T> value)
        {
            if (!ValuesCollection.Add(value))
                return false;

            ++Length;

            return true;
        }
        public bool Add(T value)
        {
            if (!ValuesCollection.Add(new NestedElement<T>(value)))
                return false;

            ++Length;

            return true;
        }
        public bool Add(T[] value)
        {
            if (!ValuesCollection.Add(new NestedElement<T>(value)))
                return false;

            ++Length;

            return true;
        }
        public bool Add(INestableCollection<T> value)
        {
            if (!ValuesCollection.Add(new NestedElement<T>(value)))
                return false;

            ++Length;

            return true;
        }

        public bool Remove()
        {
            if (!ValuesCollection.Remove())
                return false;

            --Length;

            return true;
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
                var exception = new RankException(
                    "Копирование в многомерные массивы не поддерживается");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
            if (array.GetType().GetElementType() != typeof(T))
            {
                var exception = new ArrayTypeMismatchException(
                    "Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var thisCollection = Enumerate()
                .ToList();

            if (array.Length - index < thisCollection.Count)
            {
                var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var arrayIndex = index;

            for (int i = 0; i < thisCollection.Count; ++i)
            {
                array.SetValue(thisCollection[i], arrayIndex);

                ++arrayIndex;
            }
        }
        public void CopyTo(ICollection<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(
                    nameof(collection),
                    "Целевая коллекция не может быть равна null");
                Events.OnError(this,
                    new RErrorEventArgs(exception, exception.Message));
                OnError(
                    new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            var thisCollection = Enumerate()
                .ToList();

            if (clearBeforeCopy)
                collection.Clear();

            for (int i = 0; i < thisCollection.Count; ++i)
            {
                collection.Add(thisCollection[i]);
            }
        }
    }
}
