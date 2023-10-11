// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class Switch
    {
        private const int OnValue = 1;
        private const int OffValue = 0;
        private const int InvalidValue = -1;

        private int _value;

        public bool IsSet
        {
            get
            {
                var oldValue = Interlocked.CompareExchange(ref _value, InvalidValue, InvalidValue);
                return oldValue == OnValue;
            }
        }

        public Switch()
        {
            _value = OffValue;
        }

        public bool TrySet()
        {
            var oldValue = Interlocked.CompareExchange(ref _value, OnValue, OffValue);
            return oldValue == OffValue;
        }

        public bool TryReset()
        {
            var oldValue = Interlocked.CompareExchange(ref _value, OffValue, OnValue);
            return oldValue == OnValue;
        }
    }
}
