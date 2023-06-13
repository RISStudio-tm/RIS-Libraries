// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Math = RIS.Mathematics.Math;

namespace RIS.Synchronization
{
    public class CounterBase
    {
        protected const int CACHE_LINE = 64;
        protected const int OBJ_HEADER_SIZE = 8;

        protected static readonly int MAX_CELL_COUNT;

        private protected int cellCount;
        protected uint lastCntTicks;

        static CounterBase()
        {
            MAX_CELL_COUNT = (int) Math.NextPowerOfTwo((uint) System.Environment.ProcessorCount) + 1;
        }
        protected CounterBase()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private protected static unsafe int GetIndex(uint cellCount)
        {
            if (IntPtr.Size == 4)
            {
                uint addr = (uint)&cellCount;
                return (int)(addr % cellCount);
            }
            else
            {
                ulong addr = (ulong)&cellCount;
                return (int)(addr % cellCount);
            }
        }
    }
}
