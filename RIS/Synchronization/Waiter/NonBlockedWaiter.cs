using System;

namespace RIS.Synchronization
{
    internal sealed class NonBlockedWaiter : WaiterBase
    {
        public override bool IsCompleted
        {
            get
            {
                return true;
            }
        }

        public NonBlockedWaiter(LightAsyncLock @lock)
            : base(@lock)
        {

        }

        public override string ToString()
        {
            return "NonBlockingWaiter: " + base.ToString();
        }

        protected override void OnCompleted(Action continuation, bool captureExecutionContext)
        {
            continuation?.Invoke();
        }
    }
}