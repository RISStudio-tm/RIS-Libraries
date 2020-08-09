using System;

namespace RIS.Synchronization
{
    internal sealed class NonBlockedAwaiter : AwaiterBase
    {
        public override bool IsCompleted
        {
            get
            {
                return true;
            }
        }

        public NonBlockedAwaiter(LightAsyncLock @lock)
            : base(@lock)
        {

        }

        public override string ToString()
        {
            return "NonBlockingAwaiter: " + base.ToString();
        }

        protected override void OnCompleted(Action continuation, bool captureExecutionContext)
        {
            continuation?.Invoke();
        }
    }
}