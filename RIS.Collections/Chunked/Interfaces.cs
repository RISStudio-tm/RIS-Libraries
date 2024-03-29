﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIS.Collections.Chunked
{
    public interface IChunkedCollection : ICollection, IEnumerable
    {
        int ChunksCount { get; }
        uint ChunkSize { get; }
        long Length { get; }

        bool Remove();

        void Clear();
    }

    public interface IChunkedCollection<T> : IChunkedCollection, IEnumerable<T>
    {
        T this[long index] { get; set; }
        T this[int chunkIndex, uint valueIndex] { get; set; }

        bool Add(T value);

        void CopyTo(IChunkedCollection<T> collection, bool clearBeforeCopy);
        void CopyTo(ISyncChunkedCollection<T> collection, bool clearBeforeCopy);
        void CopyTo(IList<T> collection, bool clearBeforeCopy);
    }

    public interface IChunkedArray : IChunkedCollection
    {

    }

    public interface IChunkedArray<T> : IChunkedArray, IChunkedCollection<T>
    {
        ref T GetRef(long index);
        ref T GetRef(int chunkIndex, uint valueIndex);
    }

    public interface ISyncChunkedCollection : IChunkedCollection
    {
        object GetChunkSyncRootForElement(long index);
        object GetChunkSyncRoot(int chunkIndex);
    }

    public interface ISyncChunkedCollection<T> : ISyncChunkedCollection, IChunkedCollection<T>
    {

    }

    public interface ISyncChunkedArray : IChunkedArray, ISyncChunkedCollection
    {

    }

    public interface ISyncChunkedArray<T> : ISyncChunkedArray, IChunkedArray<T>, ISyncChunkedCollection<T>
    {

    }
}
