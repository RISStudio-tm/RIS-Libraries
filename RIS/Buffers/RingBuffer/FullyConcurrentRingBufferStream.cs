// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Buffers
{
    public class FullyConcurrentRingBufferStream : RingBufferStream
    {
        protected FullyConcurrentRingBufferStream(int capacity, int? parallelism = null)
            : base(new FullyConcurrentRingBuffer(capacity, null, parallelism))
        {

        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await ((FullyConcurrentRingBuffer) _ringBuffer).Take(buffer, offset, count, cancellationToken).ConfigureAwait(false);

            return count;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ((FullyConcurrentRingBuffer) _ringBuffer).Put(buffer, offset, count, cancellationToken);
        }
    }
}
