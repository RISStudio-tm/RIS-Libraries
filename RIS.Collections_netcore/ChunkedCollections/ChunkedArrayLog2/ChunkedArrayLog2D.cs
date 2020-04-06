using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.ChunkedCollections
{
    public class ChunkedArrayLog2D<T> : IChunkedCollection<T>, ICollection, IEnumerable<T>, IEnumerable
    {
        public event RMessageHandler ShowMessage;
        public event RErrorHandler ShowError;

        public T this[long index]
        {
            get
            {
                return GetRefByIndex(index);
            }
            set
            {
                GetRefByIndex(index) = value;
            }
        }
        public T this[int chunkIndex, uint valueIndex]
        {
            get
            {
                return GetRefByIndex(chunkIndex, valueIndex);
            }
            set
            {
                GetRefByIndex(chunkIndex, valueIndex) = value;
            }
        }

        private Dictionary<int, T[]> Chunks { get; }
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

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private ChunkedArrayLog2D<T> _list;
            private long _index;
            public T Current { get; private set; }

            internal Enumerator(ChunkedArrayLog2D<T> list)
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
                ChunkedArrayLog2D<T> list = _list;
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

        public ChunkedArrayLog2D()
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
                ChunkSize = 512;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)Math.Log(ChunkSize, 2);
            ChunkSize = (uint)Math.Pow(2, Offset);
            Length = 0;
            Chunks = new Dictionary<int, T[]>();
        }
        public ChunkedArrayLog2D(long length)
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
                ChunkSize = 512;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)Math.Log(ChunkSize, 2);
            ChunkSize = (uint)Math.Pow(2, Offset);
            Length = length;
            Chunks = new Dictionary<int, T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            AddChunk(chunksCount);
        }
        public ChunkedArrayLog2D(long length, uint chunkSize)
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
                ChunkSize = 512;

            SyncRoot = new object();
            IsSynchronized = false;

            Offset = (byte)Math.Log(ChunkSize, 2);
            ChunkSize = (uint)Math.Pow(2, Offset);
            Length = length;
            Chunks = new Dictionary<int, T[]>();
            int chunksCount = (int)(length / ChunkSize);
            if (length % ChunkSize != 0)
                chunksCount += 1;

            AddChunk(chunksCount);
        }

        private T GetByIndex(long index)
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
        private T GetByIndex(int chunkIndex, uint valueIndex)
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

        private ref T GetRefByIndex(long index)
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
        private ref T GetRefByIndex(int chunkIndex, uint valueIndex)
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
            Chunks.Add(Chunks.Count, new T[ChunkSize]);
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

            Chunks.Remove(ChunksCount - 1);
        }
        private void RemoveChunk(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                RemoveChunk();
            }
        }

        public ref T GetRef(long index)
        {
            return ref GetRefByIndex(index);
        }
        public ref T GetRef(int chunkIndex, uint valueIndex)
        {
            return ref GetRefByIndex(chunkIndex, valueIndex);
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
                needChunksCount += 1;

            if (ChunksCount < needChunksCount)
                AddChunk();

            GetRefByIndex(Length - 1) = value;

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

            GetRefByIndex(Length - 1) = default(T);
            --Length;

            int needChunksCount = (int)(Length >> Offset);
            if ((Length & (ChunkSize - 1)) != 0)
                needChunksCount += 1;

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

        public void CopyTo(IChunkedCollection<T> chunkedCollection)
        {
            if (chunkedCollection == null)
            {
                var exception = new ArgumentNullException(nameof(chunkedCollection), "Целевая коллекция не может быть равна null");
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
            else if (chunkedCollection.Length < Length)
            {
                var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            if (chunkedCollection.ChunkSize == ChunkSize)
            {
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                    {
                        chunkedCollection[thisChunkIndex, thisValueIndex] =
                            this[thisChunkIndex, thisValueIndex];
                    }
                }
                for (uint thisValueIndex = 0;
                    thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                    ++thisValueIndex)
                {
                    chunkedCollection[ChunksCount - 1, thisValueIndex] =
                        this[ChunksCount - 1, thisValueIndex];
                }
            }
            else
            {
                long chunkedCollectionIndex = 0;
                for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                {
                    for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                    {
                        chunkedCollection[chunkedCollectionIndex] =
                            this[thisChunkIndex, thisValueIndex];
                        ++chunkedCollectionIndex;
                    }
                }
                for (uint thisValueIndex = 0;
                    thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                    ++thisValueIndex)
                {
                    chunkedCollection[chunkedCollectionIndex] =
                        this[ChunksCount - 1, thisValueIndex];
                    ++chunkedCollectionIndex;
                }
            }
        }

        public void CopyTo(ISynchronizedChunkedCollection<T> chunkedCollection)
        {
            if (chunkedCollection == null)
            {
                var exception = new ArgumentNullException(nameof(chunkedCollection), "Целевая коллекция не может быть равна null");
                Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                throw exception;
            }

            lock (chunkedCollection.SyncRoot)
            {
                if (Length < 1)
                {
                    var exception = new Exception("Нельзя скопировать пустую коллекцию");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }
                else if (chunkedCollection.Length < Length)
                {
                    var exception = new Exception("Для копирования длина целевой коллекции не может быть меньше длины текущей");
                    Events.DShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    ShowError?.Invoke(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                if (chunkedCollection.ChunkSize == ChunkSize)
                {
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                    {
                        lock (chunkedCollection.GetChunkSyncRoot(thisChunkIndex))
                        {
                            for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                            {
                                chunkedCollection[thisChunkIndex, thisValueIndex] =
                                    this[thisChunkIndex, thisValueIndex];
                            }
                        }
                    }
                    lock (chunkedCollection.GetChunkSyncRoot(ChunksCount - 1))
                    {
                        for (uint thisValueIndex = 0;
                            thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                            ++thisValueIndex)
                        {
                            chunkedCollection[ChunksCount - 1, thisValueIndex] =
                                this[ChunksCount - 1, thisValueIndex];
                        }
                    }
                }
                else
                {
                    long chunkedCollectionIndex = 0;
                    for (int thisChunkIndex = 0; thisChunkIndex < ChunksCount - 1; ++thisChunkIndex)
                    {
                        for (uint thisValueIndex = 0; thisValueIndex < ChunkSize; ++thisValueIndex)
                        {
                            lock (chunkedCollection.GetChunkSyncRootForElement(chunkedCollectionIndex))
                            {
                                chunkedCollection[chunkedCollectionIndex] =
                                    this[thisChunkIndex, thisValueIndex];
                            }
                            ++chunkedCollectionIndex;
                        }
                    }
                    for (uint thisValueIndex = 0;
                        thisValueIndex < Length - ChunkSize * (ChunksCount - 1);
                        ++thisValueIndex)
                    {
                        lock (chunkedCollection.GetChunkSyncRootForElement(chunkedCollectionIndex))
                        {
                            chunkedCollection[chunkedCollectionIndex] =
                                this[ChunksCount - 1, thisValueIndex];
                        }
                        ++chunkedCollectionIndex;
                    }
                }
            }
        }

        public void CopyTo(IList<T> collection)
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
