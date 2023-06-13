// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System.Threading;

namespace RIS.Synchronization
{
    internal sealed class LeftRightVersion
    {
        public const int RESET_VALUE = 1;
        public const int SET_VALUE = 0;
        private const int INVALID_VALUE = int.MinValue;

        private int _counter;
        private readonly ManualResetEventSlim _waitEvent;

        public bool IsEmpty
        {
            get
            {
                return Interlocked.CompareExchange(ref _counter, INVALID_VALUE, INVALID_VALUE) == 0;
            }
        }

        public LeftRightVersion()
        {
            _counter = 0;
            _waitEvent = new ManualResetEventSlim(true);
        }

        public void Arrive()
        {
            var newValue = Interlocked.Increment(ref _counter);
            if (newValue == RESET_VALUE)
                _waitEvent.Reset();
        }

        public void Depart()
        {
            var newValue = Interlocked.Decrement(ref _counter);
            if (newValue == SET_VALUE)
                _waitEvent.Set();
        }

        public void WaitForEmptyVersion()
        {
            _waitEvent.Wait();
        }
    }
}