﻿// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace RIS.Synchronization
{
    public sealed class Counter32: CounterBase
    {
        private sealed class Cell
        {
            [StructLayout(LayoutKind.Explicit, Size = (CacheLine * 2) - ObjHeaderSize)]
            public struct SpacedCounter
            {
                [FieldOffset(CacheLine - ObjHeaderSize)]
                public int Cnt;
            }

            public SpacedCounter Counter;
        }

        private Cell[] _cells;
        private int _cnt;
        private int _lastCnt;

        public int Value
        {
            get
            {
                var count = _cnt;
                var cells = _cells;

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
        public int EstimatedValue
        {
            get
            {
                if (_cells == null)
                {
                    return _cnt;
                }

                var curTicks = (uint)System.Environment.TickCount;
                if (curTicks != LastCntTicks)
                {
                    _lastCnt = Value;
                    LastCntTicks = curTicks;
                }

                return _lastCnt;
            }
        }

        public Counter32()
        {

        }

        public void Increment()
        {
            int curCellCount = CellCount;
            var drift = Increment(ref GetCntRef(curCellCount));

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static int Increment(ref int val)
        {
            return -val - 1 + Interlocked.Increment(ref val);
        }

        public void Decrement()
        {
            int curCellCount = CellCount;
            var drift = Decrement(ref GetCntRef(curCellCount));

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static int Decrement(ref int val)
        {
            return val - 1 - Interlocked.Decrement(ref val);
        }

        public void Add(int value)
        {
            int curCellCount = CellCount;
            var drift = Add(ref GetCntRef(curCellCount), value);

            if (drift != 0)
            {
                TryAddCell(curCellCount);
            }
        }
        private static int Add(ref int val, int inc)
        {
            return -val - inc + Interlocked.Add(ref val, inc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetCntRef(int curCellCount)
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
            if (curCellCount < MaxCellCount)
            {
                TryAddCellCore(curCellCount);
            }
        }
        private void TryAddCellCore(int curCellCount)
        {
            var cells = _cells;
            if (cells == null)
            {
                var newCells = new Cell[MaxCellCount];
                cells = Interlocked.CompareExchange(ref _cells, newCells, null) ?? newCells;
            }

            if (cells[curCellCount] == null)
            {
                Interlocked.CompareExchange(ref cells[curCellCount], new Cell(), null);
            }

            if (CellCount == curCellCount)
            {
                Interlocked.CompareExchange(ref CellCount, curCellCount + 1, curCellCount);
            }
        }
    }
}
