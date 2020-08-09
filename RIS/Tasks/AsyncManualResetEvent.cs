using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Tasks
{
    public sealed class AsyncManualResetEvent
    {
        private static class IdManager<T>
        {
            private static int _lastId;
            private static T _type;

            public static int GetId(ref int id)
            {
                if (id != 0)
                    return id;

                int newId;

                do
                {
                    newId = Interlocked.Increment(ref _lastId);
                }
                while (newId == 0);

                Interlocked.CompareExchange(ref id, newId, 0);

                return id;
            }
        }

        private readonly object _lockObj;
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
            _lockObj = new object();
            _tcs = new TaskCompletionSource();
            if (set) {
                _tcs.SetResult();
            }
        }

        public void Wait()
        {
            Task task = WaitAsync();

            if (task.IsCompleted)
                return;

            task.Wait();
        }
        public void Wait(CancellationToken cancellationToken)
        {
            Task task = WaitAsync();

            if (task.IsCompleted)
                return;

            task.Wait(cancellationToken);
        }
        public Task WaitAsync()
        {
            lock (_lockObj)
            {
                return _tcs.Task;
            }
        }

        public void Set()
        {
            lock (_lockObj)
            {
                _tcs.TrySetResultWithBackgroundContinuations();
            }
        }

        public void Reset()
        {
            lock (_lockObj)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = new TaskCompletionSource();
            }
        }
    }
}
