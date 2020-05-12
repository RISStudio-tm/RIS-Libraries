using System;
using System.Threading;

namespace RIS.Synchronization
{
    internal sealed class AsyncLockWaiter : WaiterBase
    {
        private sealed class ContextAction
        {
            public ExecutionContext Context { get; }
            public Action Continuation { get; }

            public ContextAction(ExecutionContext context, Action continuation)
            {
                Context = context;
                Continuation = continuation;
            }
        }

        private static readonly Action Marker = () => { };
        private Action _continuation;
        private ExecutionContext _executionContext;

        public override bool IsCompleted
        {
            get
            {
                return false;
            }
        }

        public AsyncLockWaiter(LightAsyncLock @lock)
            : base(@lock)
        {

        }

        public override void Ready()
        {
            var continuation = Interlocked.Exchange(ref _continuation, Marker);

            ScheduleContinuation(_executionContext, continuation);
        }

        public override string ToString()
        {
            return "AsyncWaiter: " + base.ToString();
        }

        protected override void OnCompleted(Action continuation, bool captureExecutionContext)
        {
            if (captureExecutionContext)
            {
                _executionContext = ExecutionContext.Capture();
            }
            else
            {
                _executionContext = null;
            }

            var placeholder = Interlocked.Exchange(ref _continuation, continuation);

            if (placeholder == Marker)
                ScheduleContinuation(_executionContext, continuation);
        }

        private static void ContinuationCallback(object state)
        {
            var c = (ContextAction) state;

            if (c.Context != null)
            {
                ExecutionContext.Run(c.Context, x => ((Action)x)?.Invoke(), c.Continuation);
            }
            else
            {
                c.Continuation();
            }
        }

        private static void ScheduleContinuation(ExecutionContext executionContext, Action continuation)
        {
            if (continuation == null || continuation == Marker)
                return;

            var callbackState = new ContextAction(executionContext, continuation);

            ThreadPool.QueueUserWorkItem(ContinuationCallback, callbackState);
        }
    }
}