// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RIS.Synchronization
{
    public sealed class AsyncManualResetEvent
    {
        private static class IdManager
        {
            private static int _lastId;

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
        private TaskCompletionSource<object> _tcs;

        public int Id
        {
            get
            {
                return IdManager.GetId(ref _id);
            }
        }



        public AsyncManualResetEvent()
            : this(false)
        {

        }
        public AsyncManualResetEvent(bool set)
        {
            _lockObj = new object();
            _tcs = new TaskCompletionSource<object>();

            if (set)
                _tcs.SetResult(null);
        }



        public void Wait()
        {
            var task = WaitAsync();

            if (task.IsCompleted)
                return;

            task.Wait();
        }
        public void Wait(CancellationToken cancellationToken)
        {
            var task = WaitAsync();

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
                Task.Run(() =>
                    _tcs.TrySetResult(null));

                _tcs.Task.Wait();
            }
        }



        public void Reset()
        {
            lock (_lockObj)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = new TaskCompletionSource<object>();
            }
        }
    }
}
