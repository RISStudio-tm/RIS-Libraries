// Copyright (c) RISStudio, 2020. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RIS.Tasks
{
    public sealed class TaskCompletionSource
    {
        private readonly TaskCompletionSource<object> _tcs;

        public Task Task
        {
            get
            {
                return _tcs.Task;
            }
        }

        public TaskCompletionSource()
        {
            _tcs = new TaskCompletionSource<object>();
        }
        public TaskCompletionSource(object state)
        {
            _tcs = new TaskCompletionSource<object>(state);
        }
        public TaskCompletionSource(TaskCreationOptions creationOptions)
        {
            _tcs = new TaskCompletionSource<object>(creationOptions);
        }
        public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
        {
            _tcs = new TaskCompletionSource<object>(state, creationOptions);
        }

        public void SetCanceled()
        {
            _tcs.SetCanceled();
        }

        public bool TrySetCanceled()
        {
            return _tcs.TrySetCanceled();
        }

        public void SetException(Exception exception)
        {
            _tcs.SetException(exception);
        }
        public void SetException(IEnumerable<Exception> exceptions)
        {
            _tcs.SetException(exceptions);
        }

        public bool TrySetException(Exception exception)
        {
            return _tcs.TrySetException(exception);
        }
        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            return _tcs.TrySetException(exceptions);
        }

        public void SetResult()
        {
            _tcs.SetResult(null);
        }

        public bool TrySetResult()
        {
            return _tcs.TrySetResult(null);
        }
    }
}
