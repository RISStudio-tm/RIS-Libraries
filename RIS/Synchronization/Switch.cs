// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information. 

using System;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class Switch
    {
        private const int ON_VALUE = 1;
        private const int OFF_VALUE = 0;
        private const int INVALID_VALUE = -1;

        private int _value;

        public bool IsSet
        {
            get
            {
                var oldValue = Interlocked.CompareExchange(ref _value, INVALID_VALUE, INVALID_VALUE);
                return oldValue == ON_VALUE;
            }
        }

        public Switch()
        {
            _value = OFF_VALUE;
        }

        public bool TrySet()
        {
            var oldValue = Interlocked.CompareExchange(ref _value, ON_VALUE, OFF_VALUE);
            return oldValue == OFF_VALUE;
        }

        public bool TryReset()
        {
            var oldValue = Interlocked.CompareExchange(ref _value, OFF_VALUE, ON_VALUE);
            return oldValue == ON_VALUE;
        }
    }
}
