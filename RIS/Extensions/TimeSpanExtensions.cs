// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;

namespace RIS.Extensions
{
    public static class TimeSpanExtensions
    {
        internal static void ThrowIfNotValidTaskTimeout(this TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;

            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                var exception = new ArgumentOutOfRangeException(
                    nameof(timeout), "Task timeout is not valid");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }
        }
    }
}
