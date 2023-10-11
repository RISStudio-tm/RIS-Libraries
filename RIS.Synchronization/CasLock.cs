// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class CasLock : LockBase
    {
        private const int STA_FREE = 0;
        private const int STA_BLOCKING = 1;
        private const int YIELD_THRESHOLD = 10;
        private const int SLEEP_0_EVERY_HOW_MANY_TIMES = 5;
        private const int SLEEP_1_EVERY_HOW_MANY_TIMES = 20;
        private volatile int _status;
        private static bool IsSingleProcessor { get; }

        static CasLock()
        {
            IsSingleProcessor = System.Environment.ProcessorCount == 1;
        }

        protected override async Task EnterLockAsync(CancellationToken cancellation)
        {
            int count = 0;

            while (true)
            {
                cancellation.ThrowIfCancellationRequested();

                if (Interlocked.CompareExchange(ref _status, STA_BLOCKING, STA_FREE) != STA_FREE)
                {
                    if (count > YIELD_THRESHOLD || IsSingleProcessor)
                    {
                        int yieldsSoFar = (count >= YIELD_THRESHOLD ? count - YIELD_THRESHOLD : count);

                        if ((yieldsSoFar % SLEEP_1_EVERY_HOW_MANY_TIMES) == (SLEEP_1_EVERY_HOW_MANY_TIMES - 1))
                        {
                            await Task.Delay(1, cancellation).ConfigureAwait(false);
                        }
                        else if ((yieldsSoFar % SLEEP_0_EVERY_HOW_MANY_TIMES) == (SLEEP_0_EVERY_HOW_MANY_TIMES - 1))
                        {
                            await Task.Delay(0, cancellation).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Yield();
                        }
                    }

                    ++count;
                }
                else
                {
                    break;
                }
            }
        }

        protected override bool TryEnterLock()
        {
            return Interlocked.CompareExchange(ref _status, STA_BLOCKING, STA_FREE) == STA_FREE;
        }

        protected override void ExitLock()
        {
            _status = STA_FREE;
        }
    }
}
