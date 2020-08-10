// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Collections.Generic;

namespace RIS.Collections.Chunked
{
    public interface IChunkedCollection<T>
    {
        T this[long index] { get; set; }
        T this[int chunkIndex, uint valueIndex] { get; set; }

        int ChunksCount { get; }
        uint ChunkSize { get; }
        long Length { get; }

        bool Add(T value);

        bool Remove();

        void Clear();

        void CopyTo(IChunkedCollection<T> collection, bool clearBeforeCopy);
        void CopyTo(ISyncChunkedCollection<T> collection, bool clearBeforeCopy);
        void CopyTo(IList<T> collection, bool clearBeforeCopy);
    }

    public interface IChunkedArray<T> : IChunkedCollection<T>
    {
        ref T GetRef(long index);
        ref T GetRef(int chunkIndex, uint valueIndex);
    }

    public interface ISyncChunkedCollection<T> : IChunkedCollection<T>
    {
        object SyncRoot { get; }

        object GetChunkSyncRootForElement(long index);
        object GetChunkSyncRoot(int chunkIndex);
    }

    public interface ISyncChunkedArray<T> : ISyncChunkedCollection<T>
    {
        ref T GetRef(long index);
        ref T GetRef(int chunkIndex, uint valueIndex);
    }
}
