// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.Chunked
{
    public class SyncChunkedArrayL<T> : ISyncChunkedArray<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event EventHandler<RInformationEventArgs> Information;
        public event EventHandler<RWarningEventArgs> Warning;
        public event EventHandler<RErrorEventArgs> Error;

        public T this[long index]
        {
            get
            {
                return GetRef(index);
            }
            set
            {
                Set(value, index);
            }
        }
        public T this[int chunkIndex, uint valueIndex]
        {
            get
            {
                return GetRef(chunkIndex, valueIndex);
            }
            set
            {
                Set(value, chunkIndex, valueIndex);
            }
        }

        private List<T[]> Chunks { get; }
        private ChunkedArrayD<object> ChunksSyncRoots { get; }

        public int ChunksCount
        {
            get
            {
                return Chunks.Count;
            }
        }
        public uint ChunkSize { get; }
        public long Length { get; private set; }
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }
        int ICollection.Count
        {
            get
            {
                if (Length > int.MaxValue)
                {
                    var exception = new Exception("Количество элементов коллекции больше int.MaxValue и не может быть возвращено свойством Count");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                return (int)Length;
            }
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly SyncChunkedArrayL<T> _list;
            private long _index;

            public T Current { get; private set; }

            internal Enumerator(SyncChunkedArrayL<T> list)
            {
                _list = list;
                _index = 0;
                Current = default(T);
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                SyncChunkedArrayL<T> list = _list;
                if (_index >= list.Length)
                {
                    _index = _list.Length + 1;
                    Current = default(T);
                    return false;
                }
                Current = list[_index];
                ++_index;
                return true;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Length + 1)
                    {
                        var exception = new InvalidOperationException("Перечисление не может выполниться");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        _list.OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }
                    return (object)Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                Current = default(T);
            }
        }

        public SyncChunkedArrayL()
        {
            ChunkSize = RIS.Environment.GCLOHThresholdSize / RIS.Environment.GetSize<T>();
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = true;

            Length = 0;
            Chunks = new List<T[]>();

            ChunksSyncRoots = new ChunkedArrayD<object>();
        }
        public SyncChunkedArrayL(long length)
        {
            ChunkSize = RIS.Environment.GCLOHThresholdSize / RIS.Environment.GetSize<T>();
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина массива не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = true;

            Length = length;
            Chunks = new List<T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            ChunksSyncRoots = new ChunkedArrayD<object>();

            AddChunk(chunksCount, true);
        }
        public SyncChunkedArrayL(long length, uint chunkSize)
        {
            ChunkSize = chunkSize;
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина массива не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = true;

            Length = length;
            Chunks = new List<T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            ChunksSyncRoots = new ChunkedArrayD<object>();

            AddChunk(chunksCount, true);
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

        private T Get(long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index / ChunkSize);
            uint valueIndex = (uint)(index % ChunkSize);

            lock (ChunksSyncRoots[chunkIndex])
            {
                return Chunks[chunkIndex][valueIndex];
            }
        }
        private T Get(int chunkIndex, uint valueIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (valueIndex > ChunkSize - 1u)
            {
                var exception = new IndexOutOfRangeException("Индекс значения не может быть больше размера чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex == ChunksCount - 1 && valueIndex > Length - ChunkSize * (ChunksCount - 1) - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс значения в последнем чанке не может быть больше длины последнего чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (ChunksSyncRoots[chunkIndex])
            {
                return Chunks[chunkIndex][valueIndex];
            }
        }

        public ref T GetRef(long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index / ChunkSize);
            uint valueIndex = (uint)(index % ChunkSize);

            lock (ChunksSyncRoots[chunkIndex])
            {
                return ref Chunks[chunkIndex][valueIndex];
            }
        }
        public ref T GetRef(int chunkIndex, uint valueIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (valueIndex > ChunkSize - 1u)
            {
                var exception = new IndexOutOfRangeException("Индекс значения не может быть больше размера чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex == ChunksCount - 1 && valueIndex > Length - ChunkSize * (ChunksCount - 1) - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс значения в последнем чанке не может быть больше длины последнего чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (ChunksSyncRoots[chunkIndex])
            {
                return ref Chunks[chunkIndex][valueIndex];
            }
        }

        private void Set(T value, long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index / ChunkSize);
            uint valueIndex = (uint)(index % ChunkSize);

            lock (ChunksSyncRoots[chunkIndex])
            {
                Chunks[chunkIndex][valueIndex] = value;
            }
        }
        private void Set(T value, int chunkIndex, uint valueIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (valueIndex > ChunkSize - 1u)
            {
                var exception = new IndexOutOfRangeException("Индекс значения не может быть больше размера чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex == ChunksCount - 1 && valueIndex > Length - ChunkSize * (ChunksCount - 1) - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс значения в последнем чанке не может быть больше длины последнего чанка");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (ChunksSyncRoots[chunkIndex])
            {
                Chunks[chunkIndex][valueIndex] = value;
            }
        }

        private void AddChunk(bool synchronize = true)
        {
            if (synchronize)
            {
                lock (SyncRoot)
                {
                    if (Chunks.Count == int.MaxValue)
                    {
                        var exception =
                            new Exception("Нельзя добавить чанк, так как коллекция уже содержит максимальное их количество");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }

                    Chunks.Add(new T[ChunkSize]);
                    ChunksSyncRoots.Add(new object());
                }
            }
            else
            {
                if (Chunks.Count == int.MaxValue)
                {
                    var exception =
                        new Exception("Нельзя добавить чанк, так как коллекция уже содержит максимальное их количество");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                Chunks.Add(new T[ChunkSize]);
                ChunksSyncRoots.Add(new object());
            }
        }
        private void AddChunk(int count, bool synchronize = true)
        {
            if (synchronize)
            {
                lock (SyncRoot)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        AddChunk(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    AddChunk(false);
                }
            }
        }

        private void RemoveChunk(bool synchronize = true)
        {
            if (synchronize)
            {
                lock (SyncRoot)
                {
                    if (Chunks.Count == 0)
                    {
                        var exception = new IndexOutOfRangeException("Нельзя удалить чанк, так как массив пуст");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }

                    Chunks.RemoveAt(ChunksCount - 1);
                    ChunksSyncRoots.Remove();
                }
            }
            else
            {
                if (Chunks.Count == 0)
                {
                    var exception = new IndexOutOfRangeException("Нельзя удалить чанк, так как массив пуст");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                Chunks.RemoveAt(ChunksCount - 1);
                ChunksSyncRoots.Remove();
            }
        }
        private void RemoveChunk(int count, bool synchronize = true)
        {
            if (synchronize)
            {
                for (int i = 0; i < count; ++i)
                {
                    RemoveChunk(false);
                }
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    RemoveChunk(false);
                }
            }
        }

        public object GetChunkSyncRootForElement(long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index / ChunkSize);

            return ChunksSyncRoots[chunkIndex];
        }
        public object GetChunkSyncRoot(int chunkIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ChunksSyncRoots[chunkIndex];
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public bool Add(T value)
        {
            lock (SyncRoot)
            {
                if (Length == long.MaxValue)
                {
                    var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                ++Length;

                int needChunksCount = (int) (Length / ChunkSize);
                if (Length % ChunkSize != 0)
                    ++needChunksCount;

                if (ChunksCount < needChunksCount)
                    AddChunk(false);

                Set(value, Length - 1);
            }

            return true;
        }

        public bool Remove()
        {
            lock (SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                Set(default(T), Length - 1);

                --Length;

                int needChunksCount = (int) (Length / ChunkSize);
                if (Length % ChunkSize != 0)
                    ++needChunksCount;

                if (ChunksCount > needChunksCount)
                    RemoveChunk(false);
            }

            return true;
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                Length = 0;
                Chunks.Clear();
                ChunksSyncRoots.Clear();
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя скопировать пустую коллекцию");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (array != null && array.Rank != 1)
                {
                    var exception = new RankException("Копирование в многомерные массивы не поддерживается");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if ((long)array.Length - index < Length)
                {
                    var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (array.GetValue(0).GetType() != typeof(T))
                {
                    var exception = new ArrayTypeMismatchException("Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                long arrayIndex = index;
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    lock (ChunksSyncRoots[thisChunkIndex])
                    {
                        for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                        {
                            array.SetValue(this[thisChunkIndex, thisValueIndex], arrayIndex);
                            ++arrayIndex;
                        }
                    }
                }
                lock (ChunksSyncRoots[ChunksCount - 1])
                {
                    for (uint thisValueIndex = 0;
                        thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                        ++thisValueIndex)
                    {
                        array.SetValue(this[ChunksCount - 1, thisValueIndex], arrayIndex);
                        ++arrayIndex;
                    }
                }
            }
        }
        public void CopyTo(IChunkedCollection<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя скопировать пустую коллекцию");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (collection.Length < Length)
                {
                    var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (clearBeforeCopy)
                    collection.Clear();

                if (ChunkSize == collection.ChunkSize)
                {
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                    {
                        lock (ChunksSyncRoots[thisChunkIndex])
                        {
                            for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                            {
                                collection[thisChunkIndex, thisValueIndex] =
                                    this[thisChunkIndex, thisValueIndex];
                            }
                        }
                    }
                    lock (ChunksSyncRoots[ChunksCount - 1])
                    {
                        for (uint thisValueIndex = 0;
                            thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                            ++thisValueIndex)
                        {
                            collection[ChunksCount - 1, thisValueIndex] =
                                this[ChunksCount - 1, thisValueIndex];
                        }
                    }
                }
                else
                {
                    long collectionIndex = 0;
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                    {
                        lock (ChunksSyncRoots[thisChunkIndex])
                        {
                            for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                            {
                                collection[collectionIndex] =
                                    this[thisChunkIndex, thisValueIndex];
                                ++collectionIndex;
                            }
                        }
                    }
                    lock (ChunksSyncRoots[ChunksCount - 1])
                    {
                        for (uint thisValueIndex = 0;
                            thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                            ++thisValueIndex)
                        {
                            collection[collectionIndex] =
                                this[ChunksCount - 1, thisValueIndex];
                            ++collectionIndex;
                        }
                    }
                }
            }
        }
        public void CopyTo(ISyncChunkedCollection<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (SyncRoot)
            {
                lock (collection.SyncRoot)
                {
                    if (Length < 1)
                    {
                        var exception = new Exception("Нельзя скопировать пустую коллекцию");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }
                    else if (collection.Length < Length)
                    {
                        var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                        Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                        throw exception;
                    }

                    if (clearBeforeCopy)
                        collection.Clear();

                    if (ChunkSize == collection.ChunkSize)
                    {
                        for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                        {
                            lock (ChunksSyncRoots[thisChunkIndex])
                            {
                                lock (collection.GetChunkSyncRoot(thisChunkIndex))
                                {
                                    for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                                    {
                                        collection[thisChunkIndex, thisValueIndex] =
                                            this[thisChunkIndex, thisValueIndex];
                                    }
                                }
                            }
                        }
                        lock (ChunksSyncRoots[ChunksCount - 1])
                        {
                            lock (collection.GetChunkSyncRoot(ChunksCount - 1))
                            {
                                for (uint thisValueIndex = 0;
                                    thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                                    ++thisValueIndex)
                                {
                                    collection[ChunksCount - 1, thisValueIndex] =
                                        this[ChunksCount - 1, thisValueIndex];
                                }
                            }
                        }
                    }
                    else
                    {
                        long collectionIndex = 0;
                        for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                        {
                            lock (ChunksSyncRoots[thisChunkIndex])
                            {
                                for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                                {
                                    lock (collection.GetChunkSyncRootForElement(collectionIndex))
                                    {
                                        collection[collectionIndex] =
                                            this[thisChunkIndex, thisValueIndex];
                                    }
                                    ++collectionIndex;
                                }
                            }
                        }
                        lock (ChunksSyncRoots[ChunksCount - 1])
                        {
                            for (uint thisValueIndex = 0;
                                thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                                ++thisValueIndex)
                            {
                                lock (collection.GetChunkSyncRootForElement(collectionIndex))
                                {
                                    collection[collectionIndex] =
                                        this[ChunksCount - 1, thisValueIndex];
                                }
                                ++collectionIndex;
                            }
                        }
                    }
                }
            }
        }
        public void CopyTo(IList<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя скопировать пустую коллекцию");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (collection.Count < Length)
                {
                    var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    OnError(new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (clearBeforeCopy)
                    collection.Clear();

                int collectionIndex = 0;
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    lock (ChunksSyncRoots[thisChunkIndex])
                    {
                        for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                        {
                            collection[collectionIndex] =
                                this[thisChunkIndex, thisValueIndex];
                            ++collectionIndex;
                        }
                    }
                }
                lock (ChunksSyncRoots[ChunksCount - 1])
                {
                    for (uint thisValueIndex = 0;
                        thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                        ++thisValueIndex)
                    {
                        collection[collectionIndex] =
                            this[ChunksCount - 1, thisValueIndex];
                        ++collectionIndex;
                    }
                }
            }
        }
    }
}
