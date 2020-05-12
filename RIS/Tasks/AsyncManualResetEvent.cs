using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Tasks
{
    public sealed class AsyncManualResetEvent
    {
        private readonly object _sync;
        private int _id;
        private TaskCompletionSource _tcs;

        public int Id
        {
            get
            {
                return IdManager<AsyncManualResetEvent>.GetId(ref _id);
            }
        }

        public AsyncManualResetEvent()
            : this(false)
        {

        }
        public AsyncManualResetEvent(bool set)
        {
            _sync = new object();
            _tcs = new TaskCompletionSource();
            if (set) {
                _tcs.SetResult();
            }
        }

        public void Wait()
        {
            WaitAsync().Wait();
        }
        public void Wait(CancellationToken cancellationToken)
        {
            Task ret = WaitAsync();

            if (ret.IsCompleted)
                return;

            ret.Wait(cancellationToken);
        }
        public Task WaitAsync()
        {
            lock (_sync)
            {
                return _tcs.Task;
            }
        }

        public void Set()
        {
            lock (_sync)
            {
                _tcs.TrySetResultWithBackgroundContinuations();
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = new TaskCompletionSource();
            }
        }
    }
}
