// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;

namespace RIS.Synchronization
{
    public class SimpleCounter
    {
        private long _value = 0;

        public long Current
        {
            get
            {
                return Interlocked.Read(ref _value);
            }
        }

        public long Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        public long Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }
    }
}