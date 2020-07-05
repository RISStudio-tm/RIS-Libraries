using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            return new TaskCompletionSource<bool>()
                .CancelWith(cancellationToken)
                .Task;
        }
    }
}
