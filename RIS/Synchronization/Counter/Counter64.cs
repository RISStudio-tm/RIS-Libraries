// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class Counter64 : CounterBase
    {
        private sealed class Cell
        {
            [StructLayout(LayoutKind.Explicit, Size = (CACHE_LINE * 2) - OBJ_HEADER_SIZE)]
            public struct SpacedCounter
            {
                [FieldOffset(CACHE_LINE - OBJ_HEADER_SIZE)]
                public long Cnt;
            }

            public SpacedCounter Counter;
        }

        private Cell[] _cells;
        private long _cnt;
        private long _lastCnt;

        public long Value
        {
            get
            {
                var count = _cnt;
                var cells = this._cells;

                if (cells != null)
                {
                    for (int i = 0; i < cells.Length; i++)
                    {
                        var cell = cells[i];
                        if (cell != null)
                        {
                            count += cell.Counter.Cnt;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return count;
            }
        }
        public long EstimatedValue
        {
            get
            {
                if (cellCount == 0)
                {
                    return Value;
                }

                var curTicks = (uint) System.Environment.TickCount;
                if (curTicks != lastCntTicks)
                {
                    _lastCnt = Value;
                    lastCntTicks = curTicks;
                }

                return _lastCnt;
            }
        }

        public Counter64()
        {

        }

        public void Increment()
        {
            int curCellCount = cellCount;
            var drift = Increment(ref GetCntRef(curCellCount));

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static long Increment(ref long val)
        {
            return -val - 1 + Interlocked.Increment(ref val);
        }

        public void Decrement()
        {
            int curCellCount = cellCount;
            var drift = Decrement(ref GetCntRef(curCellCount));

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static long Decrement(ref long val)
        {
            return val - 1 - Interlocked.Decrement(ref val);
        }

        public void Add(int value)
        {
            int curCellCount = cellCount;
            var drift = Add(ref GetCntRef(curCellCount), value);

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static long Add(ref long val, int inc)
        {
            return -val - inc + Interlocked.Add(ref val, inc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref long GetCntRef(int curCellCount)
        {
            ref var cntRef = ref _cnt;

            if (_cells != null && curCellCount > 1)
            {
                var cell = _cells[GetIndex((uint)curCellCount)];

                if (cell != null)
                {
                    cntRef = ref cell.Counter.Cnt;
                }
            }

            return ref cntRef;
        }

        private void TryAddCell(int curCellCount)
        {
            if (curCellCount < MAX_CELL_COUNT)
            {
                TryAddCellCore(curCellCount);
            }
        }
        private void TryAddCellCore(int curCellCount)
        {
            var cells = this._cells;
            if (cells == null)
            {
                var newCells = new Cell[MAX_CELL_COUNT];
                cells = Interlocked.CompareExchange(ref this._cells, newCells, null) ?? newCells;
            }

            if (cells[curCellCount] == null)
            {
                Interlocked.CompareExchange(ref cells[curCellCount], new Cell(), null);
            }

            if (cellCount == curCellCount)
            {
                Interlocked.CompareExchange(ref cellCount, curCellCount + 1, curCellCount);
            }
        }
    }
}
