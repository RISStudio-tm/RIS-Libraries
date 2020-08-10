// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;

namespace RIS.Synchronization
{
    public struct DetachSynchronizationContextAwaiter : IAwaiter
    {
        public bool IsCompleted
        {
            get
            {
                return SynchronizationContext.Current == null;
            }
        }

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(_ => continuation());
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ => continuation(), null);
        }

        public void GetResult()
        {

        }

        public DetachSynchronizationContextAwaiter GetAwaiter()
        {
            return this;
        }
    }
}
