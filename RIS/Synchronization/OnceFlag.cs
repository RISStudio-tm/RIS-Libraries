// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class OnceFlag
    {
        private const int TRUE = 1;
        private const int FALSE = 0;
        private volatile int _flag = FALSE;

        public bool IsSet
        {
            get
            {
                return _flag == TRUE;
            }
        }

        public bool TrySet()
        {
            if (IsSet)
                return false;

            return Interlocked.CompareExchange(ref _flag, TRUE, FALSE) == FALSE;
        }
    }
}