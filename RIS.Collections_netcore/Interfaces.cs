using System;
using System.Collections.Generic;

namespace RIS.Collections
{
    public interface IChunkedCollection<T>
    {
        T this[long index] { get; set; }
        T this[int chunkIndex, uint valueIndex] { get; set; }

        int ChunksCount { get; }
        uint ChunkSize { get; }
        long Length { get; }

        ref T GetRef(long index);
        ref T GetRef(int chunkIndex, uint valueIndex);

        bool Add(T value);

        bool Remove();

        void Clear();
    }

    public interface ISynchronizedChunkedCollection<T>
    {
        T this[long index] { get; set; }
        T this[int chunkIndex, uint valueIndex] { get; set; }

        int ChunksCount { get; }
        uint ChunkSize { get; }
        long Length { get; }
        object SyncRoot { get; }

        object GetChunkSyncRootForElement(long index);
        object GetChunkSyncRoot(int chunkIndex);

        void SetValue(T value, long index);
        void SetValue(T value, int chunkIndex, uint valueIndex);

        ref T GetRef(long index);
        ref T GetRef(int chunkIndex, uint valueIndex);

        bool Add(T value);

        bool Remove();

        void Clear();
    }

    public interface INestableCollection<T>
    {
        NestedElement<T> this[int index] { get; set; }

        int Length { get; }

        NestedElement<T> Get(int index);

        void Set(int index, NestedElement<T> value);
        void Set(int index, T value);
        void Set(int index, T[] value);
        void Set(int index, INestableCollection<T> value);

        string ToStringRepresent();
        
        void FromStringRepresent(string represent);
        void FromStringRepresent<TC>(string represent)
            where TC : INestableCollection<T>, new();

        IEnumerable<T> Enumerate();

        bool Add(NestedElement<T> value);
        bool Add(T value);
        bool Add(T[] value);
        bool Add(INestableCollection<T> value);

        bool Remove();

        void Clear();
    }
}
