// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    internal interface ILockDisposable
    {
        Task InternalUnlockAsync();
    }

    public interface IAsyncLock
    {
        Task<IAsyncDisposable> LockAsync(CancellationToken cancellation = default);

        bool TryLock(out IAsyncDisposable lockDisposer);
    }
}
