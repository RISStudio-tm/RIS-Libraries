// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class MonitorLock : LockBase
    {
        private readonly object _lockObj = new object();

        protected override Task EnterLockAsync(CancellationToken cancellation)
        {
            Monitor.Enter(_lockObj);

            return Task.CompletedTask;
        }

        protected override bool TryEnterLock()
        {
            return Monitor.TryEnter(_lockObj);
        }

        protected override void ExitLock()
        {
            Monitor.Exit(_lockObj);
        }
    }
}
