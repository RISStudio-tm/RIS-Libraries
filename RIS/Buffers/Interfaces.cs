// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Buffers
{
    public interface IRingBuffer
    {
        int MaximumCapacity { get; }
        int CurrentLength { get; }
        int SpareLength { get; }
        bool Overwritable { get; }

        void Put(byte input);
        void Put(byte[] buffer);
        void Put(byte[] buffer, int offset, int count);

        int PutFrom(Stream source, int count);
        Task<int> PutFromAsync(Stream source, int count, CancellationToken cancellationToken);

        void PutExactlyFrom(Stream source, int count);
        Task PutExactlyFromAsync(Stream source, int count, CancellationToken cancellationToken);

        byte Take();
        byte[] Take(int count);
        void Take(byte[] buffer);
        void Take(byte[] buffer, int offset, int count);

        void TakeTo(Stream destination, int count);
        Task TakeToAsync(Stream destination, int count, CancellationToken cancellationToken);

        void Skip(int count);

        void Reset();

        byte[] ToArray();
    }
}
