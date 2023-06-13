// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Synchronization
{
    internal sealed class NonBlockedAwaiter : AwaiterBase
    {
        public override bool IsCompleted
        {
            get
            {
                return true;
            }
        }

        public NonBlockedAwaiter(LightAsyncLock @lock)
            : base(@lock)
        {

        }

        public override string ToString()
        {
            return "NonBlockingAwaiter: " + base.ToString();
        }

        protected override void OnCompleted(Action continuation, bool captureExecutionContext)
        {
            continuation?.Invoke();
        }
    }
}