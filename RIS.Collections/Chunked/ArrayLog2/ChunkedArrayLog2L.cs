using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.Chunked
{
    public class ChunkedArrayLog2L<T> : IChunkedArray<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event EventHandler<RMessageEventArgs> ShowMessage;
        public event EventHandler<RErrorEventArgs> ShowError;

        public T this[long index]
        {
            get
            {
                return GetRef(index);
            }
            set
            {
                GetRef(index) = value;
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
                GetRef(chunkIndex, valueIndex) = value;
            }
        }

        private List<T[]> Chunks { get; }

        private byte Offset { get; }
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
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                return (int)Length;
            }
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly ChunkedArrayLog2L<T> _list;
            private long _index;

            public T Current { get; private set; }

            internal Enumerator(ChunkedArrayLog2L<T> list)
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
                ChunkedArrayLog2L<T> list = _list;
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
                        Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                        _list.ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
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

        public ChunkedArrayLog2L()
        {
            ChunkSize = RIS.Environment.GCLOHThresholdSize / RIS.Environment.GetSize<T>();
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)System.Math.Log(ChunkSize, 2);
            ChunkSize = (uint)System.Math.Pow(2, Offset);
            Length = 0;
            Chunks = new List<T[]>();
        }
        public ChunkedArrayLog2L(long length)
        {
            ChunkSize = RIS.Environment.GCLOHThresholdSize / RIS.Environment.GetSize<T>();
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина массива не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)System.Math.Log(ChunkSize, 2);
            ChunkSize = (uint)System.Math.Pow(2, Offset);
            Length = length;
            Chunks = new List<T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            AddChunk(chunksCount);
        }
        public ChunkedArrayLog2L(long length, uint chunkSize)
        {
            ChunkSize = chunkSize;
            if (ChunkSize < 1)
            {
                var exception = new Exception("Размер чанка не может быть меньше 1");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (length < 0)
            {
                var exception = new ArgumentOutOfRangeException(nameof(length), "Длина массива не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (default(T) is double)
                if (ChunkSize > 999)
                    ChunkSize = 999;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)System.Math.Log(ChunkSize, 2);
            ChunkSize = (uint)System.Math.Pow(2, Offset);
            Length = length;
            Chunks = new List<T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            AddChunk(chunksCount);
        }

        private T Get(long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index >> Offset);
            uint valueIndex = (uint)(index & (ChunkSize - 1));
            return Chunks[chunkIndex][valueIndex];
        }
        private T Get(int chunkIndex, uint valueIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (valueIndex > ChunkSize - 1u)
            {
                var exception = new IndexOutOfRangeException("Индекс значения не может быть больше размера чанка");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex == ChunksCount - 1 && valueIndex > Length - ChunkSize * (ChunksCount - 1) - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс значения в последнем чанке не может быть больше длины последнего чанка");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return Chunks[chunkIndex][valueIndex];
        }

        public ref T GetRef(long index)
        {
            if (index < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (index > Length - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс не может быть больше длины коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            int chunkIndex = (int)(index >> Offset);
            uint valueIndex = (uint)(index & (ChunkSize - 1));
            return ref Chunks[chunkIndex][valueIndex];
        }
        public ref T GetRef(int chunkIndex, uint valueIndex)
        {
            if (chunkIndex < 0)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть меньше 0");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex > ChunksCount - 1)
            {
                var exception = new IndexOutOfRangeException("Индекс чанка не может быть больше количества чанков");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (valueIndex > ChunkSize - 1u)
            {
                var exception = new IndexOutOfRangeException("Индекс значения не может быть больше размера чанка");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (chunkIndex == ChunksCount - 1 && valueIndex > Length - ChunkSize * (ChunksCount - 1) - 1L)
            {
                var exception = new IndexOutOfRangeException("Индекс значения в последнем чанке не может быть больше длины последнего чанка");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            return ref Chunks[chunkIndex][valueIndex];
        }

        private void AddChunk()
        {
            if (Chunks.Count == int.MaxValue)
            {
                var exception = new Exception("Нельзя добавить чанк, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            Chunks.Add(new T[ChunkSize]);
        }
        private void AddChunk(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                AddChunk();
            }
        }

        private void RemoveChunk()
        {
            if (Chunks.Count == 0)
            {
                var exception = new IndexOutOfRangeException("Нельзя удалить чанк, так как массив пуст");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            Chunks.RemoveAt(ChunksCount - 1);
        }
        private void RemoveChunk(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                RemoveChunk();
            }
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
            if (Length == long.MaxValue)
            {
                var exception = new Exception("Нельзя добавить элемент, так как коллекция уже содержит максимальное их количество");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            ++Length;

            int needChunksCount = (int)(Length >> Offset);
            if ((Length & (ChunkSize - 1)) != 0)
                ++needChunksCount;

            if (ChunksCount < needChunksCount)
                AddChunk();

            GetRef(Length - 1) = value;

            return true;
        }

        public bool Remove()
        {
            if (Length < 1)
            {
                var exception = new Exception("Нельзя удалить элемент, так как коллекция уже пустая");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            GetRef(Length - 1) = default(T);
            --Length;

            int needChunksCount = (int)(Length >> Offset);
            if ((Length & (ChunkSize - 1)) != 0)
                ++needChunksCount;

            if (ChunksCount > needChunksCount)
                RemoveChunk();

            return true;
        }

        public void Clear()
        {
            Length = 0;
            Chunks.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            if (Length < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array != null && array.Rank != 1)
            {
                var exception = new RankException("Копирование в многомерные массивы не поддерживается");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if ((long)array.Length - index < Length)
            {
                var exception = new Exception("Для копирования длина целевого массива, начиная с указанного индекса, не может быть меньше длины текущей коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            if (array.GetValue(0).GetType() != typeof(T))
            {
                var exception = new ArrayTypeMismatchException("Для копирования тип целевого массива не может отличаться от типа текущей коллекции");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            long arrayIndex = index;
            for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
            {
                for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                {
                    array.SetValue(this[thisChunkIndex, thisValueIndex], arrayIndex);
                    ++arrayIndex;
                }
            }
            for (uint thisValueIndex = 0;
                thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                ++thisValueIndex)
            {
                array.SetValue(this[ChunksCount - 1, thisValueIndex], arrayIndex);
                ++arrayIndex;
            }
        }
        public void CopyTo(IChunkedCollection<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (Length < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (collection.Length < Length)
            {
                var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (clearBeforeCopy)
                collection.Clear();

            if (collection.ChunkSize == ChunkSize)
            {
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                    {
                        collection[thisChunkIndex, thisValueIndex] =
                            this[thisChunkIndex, thisValueIndex];
                    }
                }
                for (uint thisValueIndex = 0;
                    thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                    ++thisValueIndex)
                {
                    collection[ChunksCount - 1, thisValueIndex] =
                        this[ChunksCount - 1, thisValueIndex];
                }
            }
            else
            {
                long collectionIndex = 0;
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                    {
                        collection[collectionIndex] =
                            this[thisChunkIndex, thisValueIndex];
                        ++collectionIndex;
                    }
                }
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
        public void CopyTo(ISyncChunkedCollection<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (collection.SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя скопировать пустую коллекцию");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (collection.Length < Length)
                {
                    var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (clearBeforeCopy)
                    collection.Clear();

                if (collection.ChunkSize == ChunkSize)
                {
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
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
                else
                {
                    long collectionIndex = 0;
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
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
        public void CopyTo(IList<T> collection, bool clearBeforeCopy)
        {
            if (collection == null)
            {
                var exception = new ArgumentNullException(nameof(collection), "Целевая коллекция не может быть равна null");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (Length < 1)
            {
                var exception = new Exception("Нельзя скопировать пустую коллекцию");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }
            else if (collection.Count < Length)
            {
                var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (clearBeforeCopy)
                collection.Clear();

            int collectionIndex = 0;
            for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
            {
                for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                {
                    collection[collectionIndex] =
                        this[thisChunkIndex, thisValueIndex];
                    ++collectionIndex;
                }
            }
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
