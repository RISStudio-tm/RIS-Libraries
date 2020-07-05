using System;

namespace RIS.Extensions
{
    public static class TimeSpanExtensions
    {
        internal static void ThrowIfNotValidTaskTimeout(this TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;

            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "Task timeout is not valid");
            }
        }
    }
}
